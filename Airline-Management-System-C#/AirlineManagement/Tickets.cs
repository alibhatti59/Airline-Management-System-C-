using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using AirlineManagement.Database;

namespace AirlineManagement
{
    public partial class Tickets : Form
    {
        private SqlConnection Con = Db.CreateConnection();
        private bool _passengerExists = false;
        private ToolTip _toolTip;

        public Tickets()
        {
            InitializeComponent();
            PopulateFlightCodes();
            PopulatePassengerIds();
            PopulateDataGridView();

            // default class
            if (cmbClass.Items.Count > 0)
                cmbClass.SelectedIndex = 0;

            // initial state
            btnBook.Enabled = false;

            // wire up handlers for UX improvements
            cmbPassId.SelectedIndexChanged += cmbPassId_SelectedIndexChanged;
            cmbPassId.TextChanged += cmbPassId_TextChanged;
            cmbFlightCode.TextChanged += (s, e) => { UpdateBookButtonState(); UpdateFieldVisuals(); UpdateAmount(); };
            cmbClass.SelectedIndexChanged += (s, e) => { UpdateAmount(); UpdateBookButtonState(); };

            // tooltip for guidance
            _toolTip = new ToolTip();
            _toolTip.AutoPopDelay = 5000;
            _toolTip.InitialDelay = 500;
            _toolTip.ReshowDelay = 200;
            _toolTip.ShowAlways = true; // show even when disabled

            UpdateFieldVisuals();

            // If logged in as a passenger, preselect their ID and restrict changing it
            try
            {
                var role = Session.Role ?? "Passenger";
                if (string.Equals(role, "Passenger", StringComparison.OrdinalIgnoreCase))
                {
                    // prefer PassengerId stored in session (created at registration)
                    if (!string.IsNullOrWhiteSpace(Session.PassengerId))
                    {
                        cmbPassId.Text = Session.PassengerId;
                        // attempt to autofill
                        TryLoadPassengerFromCombo();
                    }
                    else if (!string.IsNullOrWhiteSpace(Session.Username))
                    {
                        // fallback: use username only if PassengerId not available
                        cmbPassId.Text = Session.Username;
                        TryLoadPassengerFromCombo();
                    }
                    // prevent passengers from selecting other passenger IDs
                    cmbPassId.Enabled = false;
                }
            }
            catch { }
        }

        private void UpdateAmount()
        {
            // fixed pricing per class as requested
            var cls = cmbClass.SelectedItem != null ? cmbClass.SelectedItem.ToString() : cmbClass.Text;
            decimal amount = 0m;
            if (string.Equals(cls, "VIP", StringComparison.OrdinalIgnoreCase))
                amount = 50000m;
            else if (string.Equals(cls, "Business", StringComparison.OrdinalIgnoreCase))
                amount = 30000m;
            else // Economy or default
                amount = 15000m;

            txtAmount.Text = amount.ToString("F2");
        }

        private double GetBaseFareForFlight(string flightCode)
        {
            if (string.IsNullOrWhiteSpace(flightCode))
                return 0;

            try
            {
                using (var con = Db.CreateConnection())
                {
                    con.Open();

                    // ensure Tickets table has required columns (TicketClass / Amount) before insert
                    try
                    {
                        EnsureTicketsColumns(con);
                    }
                    catch
                    {
                        // ignore - will surface during insert if fails
                    }

                    // ensure Tickets table has required columns; add if missing
                    try
                    {
                        EnsureTicketsColumns(con);
                    }
                    catch (Exception exSchema)
                    {
                        // non-fatal: inform user but continue; insert may still fail
                        MessageBox.Show("Warning checking DB schema: " + exSchema.Message, "Schema Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    using (var cmd = new SqlCommand("SELECT BaseFare FROM Flights WHERE FlightCode = @fc", con))
                    {
                        cmd.Parameters.AddWithValue("@fc", flightCode);
                        var obj = cmd.ExecuteScalar();
                        if (obj != null && obj != DBNull.Value)
                        {
                            double val;
                            if (double.TryParse(obj.ToString(), out val))
                                return val;
                        }
                    }
                }
            }
            catch
            {
                // ignore and fall through to default
            }

            return 100.0; // default base fare
        }

        private void PopulateFlightCodes()
        {
            try
            {
                Con.Open();
                string query = "SELECT FlightCode FROM Flights WHERE Seats > 0";
                SqlCommand cmd = new SqlCommand(query, Con);
                SqlDataReader rdr = cmd.ExecuteReader();
                cmbFlightCode.Items.Clear();
                while (rdr.Read())
                {
                    cmbFlightCode.Items.Add(rdr["FlightCode"].ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while populating flight codes: " + ex.Message);
            }
            finally
            {
                Con.Close();
            }
        }

        private void cmbPassId_SelectedIndexChanged(object sender, EventArgs e)
        {
            // when the user selects from the dropdown
            TryLoadPassengerFromCombo();
            UpdateBookButtonState();
        }

        private void cmbPassId_TextChanged(object sender, EventArgs e)
        {
            // when the user types an id
            TryLoadPassengerFromCombo();
            UpdateBookButtonState();
        }

        private void TryLoadPassengerFromCombo()
        {
            var pid = (cmbPassId.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(pid))
            {
                _passengerExists = false;
                ClearPassengerFields(keepId: true);
                return;
            }

            // lookup passenger by id (string)
            try
            {
                using (var con = Db.CreateConnection())
                {
                    con.Open();
                    using (var cmd = new SqlCommand("SELECT PassengerName, Passport, PassengerNationality FROM PassengerTbl WHERE PassengerId = @id", con))
                    {
                        cmd.Parameters.AddWithValue("@id", pid);
                        using (var rdr = cmd.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                txtName.Text = rdr["PassengerName"] != DBNull.Value ? rdr["PassengerName"].ToString() : string.Empty;
                                txtPassport.Text = rdr["Passport"] != DBNull.Value ? rdr["Passport"].ToString() : string.Empty;
                                cmbNationality.Text = rdr["PassengerNationality"] != DBNull.Value ? rdr["PassengerNationality"].ToString() : string.Empty;
                                _passengerExists = true;
                                return;
                            }
                        }
                    }
                }
            }
            catch
            {
                // ignore lookup errors here; treat as not found
            }

            _passengerExists = false;
            ClearPassengerFields(keepId: true);
        }

        private void ClearPassengerFields(bool keepId = false)
        {
            if (!keepId) cmbPassId.Text = string.Empty;
            txtName.Text = string.Empty;
            txtPassport.Text = string.Empty;
            cmbNationality.SelectedItem = null;
            txtAmount.Text = string.Empty;
            // reset visuals
            UpdateFieldVisuals();
        }

        private void UpdateBookButtonState()
        {
            // allow booking if passenger exists OR if user provided manual passenger details
            bool manualReady = false;
            var passIdText = (cmbPassId.Text ?? string.Empty).Trim();
            if (!_passengerExists)
            {
                // require id + name + passport + nationality
                manualReady = !string.IsNullOrWhiteSpace(passIdText)
                    && !string.IsNullOrWhiteSpace(txtName.Text)
                    && !string.IsNullOrWhiteSpace(txtPassport.Text)
                    && !string.IsNullOrWhiteSpace(cmbNationality.Text);
            }

            btnBook.Enabled = !string.IsNullOrWhiteSpace(cmbFlightCode.Text) && (_passengerExists || manualReady);
            UpdateFieldVisuals();
        }

        private void UpdateFieldVisuals()
        {
            // color required fields red when missing
            var missingBrush = SystemColors.Info; // light background
            var okBrush = SystemColors.Window;

            // passenger id / name
            if (!_passengerExists)
            {
                cmbPassId.BackColor = Color.MistyRose;
                txtName.BackColor = Color.MistyRose;
                txtPassport.BackColor = Color.MistyRose;
                cmbNationality.BackColor = Color.MistyRose;
            }
            else
            {
                cmbPassId.BackColor = okBrush;
                txtName.BackColor = okBrush;
                txtPassport.BackColor = okBrush;
                cmbNationality.BackColor = okBrush;
            }

            // flight
            cmbFlightCode.BackColor = string.IsNullOrWhiteSpace(cmbFlightCode.Text) ? Color.MistyRose : okBrush;

            // tooltips for missing fields
            if (!_passengerExists)
            {
                _toolTip.SetToolTip(cmbPassId, "Passenger not found. Type or select a valid Passenger ID.");
                _toolTip.SetToolTip(txtName, "Passenger data will autofill when a valid Passenger ID is selected.");
                _toolTip.SetToolTip(txtPassport, "Passenger data will autofill when a valid Passenger ID is selected.");
                _toolTip.SetToolTip(cmbNationality, "Passenger nationality will autofill when a valid Passenger ID is selected.");
            }
            else
            {
                _toolTip.SetToolTip(cmbPassId, null);
                _toolTip.SetToolTip(txtName, null);
                _toolTip.SetToolTip(txtPassport, null);
                _toolTip.SetToolTip(cmbNationality, null);
            }

            if (string.IsNullOrWhiteSpace(cmbFlightCode.Text))
                _toolTip.SetToolTip(cmbFlightCode, "Select or type a flight code before booking.");
            else
                _toolTip.SetToolTip(cmbFlightCode, null);

            // available flights are populated on load; do not repopulate here to avoid clearing user selection

            // Book button tooltip explaining why it is disabled
            if (!btnBook.Enabled)
            {
                string reason = !_passengerExists ? "No valid passenger selected." : string.Empty;
                if (string.IsNullOrWhiteSpace(reason) && string.IsNullOrWhiteSpace(cmbFlightCode.Text))
                    reason = "No flight code selected.";
                else if (!string.IsNullOrWhiteSpace(reason) && string.IsNullOrWhiteSpace(cmbFlightCode.Text))
                    reason += " Also no flight code selected.";

                _toolTip.SetToolTip(btnBook, "Book disabled: " + (string.IsNullOrWhiteSpace(reason) ? "Complete required fields." : reason));
            }
            else
            {
                _toolTip.SetToolTip(btnBook, "Click to book the ticket.");
            }
            // enable/disable book tooltip
            btnBook.Enabled = btnBook.Enabled; // keep current
        }

        public void PopulatePassengerIds()
        {
            try
            {
                Con.Open();
                string query = "SELECT PassengerId FROM PassengerTbl";
                SqlCommand cmd = new SqlCommand(query, Con);
                SqlDataReader rdr = cmd.ExecuteReader();
                cmbPassId.Items.Clear();
                while (rdr.Read())
                {
                    cmbPassId.Items.Add(rdr["PassengerId"].ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while populating passenger IDs: " + ex.Message);
            }
            finally
            {
                Con.Close();
            }
        }

        // Refresh passenger list and optionally select a newly added id
        public void RefreshPassengersAndSelect(string passengerId)
        {
            PopulatePassengerIds();
            if (!string.IsNullOrWhiteSpace(passengerId))
            {
                cmbPassId.Text = passengerId;
                TryLoadPassengerFromCombo();
                UpdateBookButtonState();
            }
        }

        private void PopulateDataGridView()
        {
            try
            {
                Con.Open();
                string query = "SELECT * FROM Tickets";
                SqlDataAdapter sda = new SqlDataAdapter(query, Con);
                DataTable dt = new DataTable();
                sda.Fill(dt);
                dgvTickets.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while populating DataGridView: " + ex.Message);
            }
            finally
            {
                Con.Close();
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            // age field removed
            txtAmount.Text = "";
            txtName.Text = "";
            txtPassport.Text = "";
            //txtTicketId.Text = "";
            cmbFlightCode.SelectedItem = null;
            cmbNationality.SelectedItem = null;
            cmbPassId.SelectedItem = null;
        }

        private void btnBook_Click_1(object sender, EventArgs e)
        {
            // Validate selection
            if (string.IsNullOrWhiteSpace(cmbPassId.Text))
            {
                MessageBox.Show("Please select a passenger ID before booking a ticket.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(cmbFlightCode.Text))
            {
                MessageBox.Show("Please select a flight code before booking a ticket.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // passenger id as string
            var passId = (cmbPassId.Text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(passId))
            {
                MessageBox.Show("Selected Passenger ID is invalid.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // if session has PassengerId and current user is passenger, force use of that id
            if (string.Equals(Session.Role, "Passenger", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(Session.PassengerId))
            {
                passId = Session.PassengerId;
                // ensure UI shows it
                cmbPassId.Text = passId;
            }

            try
            {
                using (var con = Db.CreateConnection())
                {
                    con.Open();

                    // Ensure passenger exists (avoid FK violation). If not, insert from provided fields.
                    using (var chk = new SqlCommand("SELECT COUNT(1) FROM PassengerTbl WHERE PassengerId = @id", con))
                    {
                        chk.Parameters.AddWithValue("@id", passId);
                        var exists = (int)chk.ExecuteScalar() > 0;
                        if (!exists)
                        {
                            // require manual details to create passenger record
                            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtPassport.Text) || string.IsNullOrWhiteSpace(cmbNationality.Text))
                            {
                                MessageBox.Show("Passenger not found. Please provide passenger details in the form to register before booking.", "Foreign Key", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            using (var ins = new SqlCommand("INSERT INTO PassengerTbl (PassengerId, PassengerName, PassengerAdress, Passport, PassengerNationality, PassengerGender, PassengerPhoneNumber) VALUES (@id,@name,@addr,@passport,@nat,@gender,@phone)", con))
                            {
                                ins.Parameters.AddWithValue("@id", passId);
                                ins.Parameters.AddWithValue("@name", txtName.Text.Trim());
                                ins.Parameters.AddWithValue("@addr", string.Empty);
                                ins.Parameters.AddWithValue("@passport", txtPassport.Text.Trim());
                                ins.Parameters.AddWithValue("@nat", cmbNationality.Text ?? string.Empty);
                                ins.Parameters.AddWithValue("@gender", string.Empty);
                                ins.Parameters.AddWithValue("@phone", string.Empty);
                                ins.ExecuteNonQuery();
                            }
                        }
                    }

                    // Optionally ensure flight exists
                    using (var chkf = new SqlCommand("SELECT COUNT(1) FROM Flights WHERE FlightCode = @fc", con))
                    {
                        chkf.Parameters.AddWithValue("@fc", cmbFlightCode.Text);
                        var fexists = (int)chkf.ExecuteScalar() > 0;
                        if (!fexists)
                        {
                            MessageBox.Show("Selected flight does not exist.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    // ensure there are seats available and perform insert + decrement in a transaction
                    using (var tran = con.BeginTransaction())
                    {
                        // check seats
                        using (var chkSeats = new SqlCommand("SELECT Seats FROM Flights WHERE FlightCode = @fc", con, tran))
                        {
                            chkSeats.Parameters.AddWithValue("@fc", cmbFlightCode.Text);
                            var seatsObj = chkSeats.ExecuteScalar();
                            if (seatsObj == null || seatsObj == DBNull.Value || Convert.ToInt32(seatsObj) <= 0)
                            {
                                tran.Rollback();
                                MessageBox.Show("No seats available for selected flight.", "Booking Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }

                        string insertQuery = "INSERT INTO Tickets (FlightCode, TicketClass, PassId, PassName, PassPassport, PassNationality, Amount) VALUES (@FlightCode, @TicketClass, @PassId, @PassName, @PassPassport, @PassNationality, @Amount)";
                        using (SqlCommand insertCmd = new SqlCommand(insertQuery, con, tran))
                        {
                            insertCmd.Parameters.AddWithValue("@FlightCode", cmbFlightCode.Text);
                            // ensure class value is provided
                            var classVal = cmbClass.SelectedItem != null ? cmbClass.SelectedItem.ToString() : cmbClass.Text;
                            if (string.IsNullOrWhiteSpace(classVal)) classVal = "Economy";
                            insertCmd.Parameters.AddWithValue("@TicketClass", classVal);
                            insertCmd.Parameters.AddWithValue("@PassId", passId);
                            insertCmd.Parameters.AddWithValue("@PassName", txtName.Text.Trim());
                            insertCmd.Parameters.AddWithValue("@PassPassport", txtPassport.Text.Trim());
                            insertCmd.Parameters.AddWithValue("@PassNationality", cmbNationality.Text ?? string.Empty);
                            // store amount as decimal
                            decimal amt = 0m;
                            decimal.TryParse(txtAmount.Text, out amt);
                            insertCmd.Parameters.AddWithValue("@Amount", amt);

                            insertCmd.ExecuteNonQuery();
                        }

                        // decrement seats
                        using (var upd = new SqlCommand("UPDATE Flights SET Seats = Seats - 1 WHERE FlightCode = @fc AND Seats > 0", con, tran))
                        {
                            upd.Parameters.AddWithValue("@fc", cmbFlightCode.Text);
                            int updated = upd.ExecuteNonQuery();
                            if (updated == 0)
                            {
                                tran.Rollback();
                                MessageBox.Show("Failed to reserve seat. It may have been taken by another booking.", "Booking Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }

                        tran.Commit();

                        MessageBox.Show("Ticket booked successfully!");
                        // refresh UI
                        PopulateDataGridView();
                        PopulateFlightCodes();
                        UpdateAmount();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while booking ticket: " + ex.Message);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();

            Home homeForm = new Home();
            homeForm.Show();
        }

        private void txtAmount_TextChanged(object sender, EventArgs e)
        {

        }

        private void EnsureTicketsColumns(SqlConnection con)
        {
            // ensure TicketClass and Amount exist
            var checkCmd = new SqlCommand("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Tickets'", con);
            using (var rdr = checkCmd.ExecuteReader())
            {
                var cols = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
                while (rdr.Read()) cols.Add(rdr.GetString(0));
                rdr.Close();

                if (!cols.Contains("TicketClass"))
                {
                    using (var add = new SqlCommand("ALTER TABLE Tickets ADD TicketClass NVARCHAR(50) NULL", con))
                        add.ExecuteNonQuery();
                }

                if (!cols.Contains("Amount"))
                {
                    using (var add = new SqlCommand("ALTER TABLE Tickets ADD Amount DECIMAL(18,2) NULL", con))
                        add.ExecuteNonQuery();
                }
            }
        }
    }
}
