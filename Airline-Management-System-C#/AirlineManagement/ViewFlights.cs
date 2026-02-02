using System;
using System.Data;
using System.Data.SqlClient;
using AirlineManagement.Database;
using System.Windows.Forms;

namespace AirlineManagement
{
    public partial class ViewFlights : Form
    {
        private SqlConnection Con = Db.CreateConnection();

        public ViewFlights()
        {
            InitializeComponent();
            // Presentation mode for large screens during demos
            try { PresentationHelper.ApplyPresentation(this, baseFontSize: 18f, controlScale: 1.15f); } catch { }
            PopulateDataGridView();
        }

        // Pre-fill form fields when a row is selected
        private void dgvFlights_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvFlights.Rows[e.RowIndex];
            try
            {
                txtFlightCode.Text = row.Cells["FlightCode"].Value?.ToString() ?? string.Empty;
                cmbSource1.Text = row.Cells["Source"].Value?.ToString() ?? string.Empty;
                cmbDestination.Text = row.Cells["Destination"].Value?.ToString() ?? string.Empty;
                var dtObj = row.Cells["TakeOffDate"].Value;
                if (dtObj != null && dtObj != DBNull.Value)
                {
                    DateTime dt;
                    if (DateTime.TryParse(dtObj.ToString(), out dt))
                        dtpdate.Value = dt;
                }
                txtNumberSeats.Text = row.Cells["Seats"].Value?.ToString() ?? string.Empty;
            }
            catch
            {
                // ignore parse errors
            }
        }

        private void PopulateDataGridView()
        {
            try
            {
                if (Con.State == ConnectionState.Closed)
                {
                    Con.Open();
                }

                string query = "SELECT * FROM Flights";
                SqlDataAdapter sda = new SqlDataAdapter(query, Con);
                DataTable dt = new DataTable();
                sda.Fill(dt);
                dgvFlights.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while populating DataGridView: " + ex.Message);
            }
            finally
            {
                if (Con.State == ConnectionState.Open)
                {
                    Con.Close();
                }
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            // return to Home instead of opening FlightsTbl directly
            this.Hide();
            Home home = new Home();
            home.Show();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            txtFlightCode.Text = "";
            txtNumberSeats.Text = "";
            cmbDestination.Text = "";
            cmbSource1.Text = "";
            dtpdate.Text = "";
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            string flightCodeToDelete = txtFlightCode.Text.Trim();
            // allow deletion only for staff
            var role = Session.Role ?? "Passenger";
            bool isStaff = string.Equals(role, "Staff", StringComparison.OrdinalIgnoreCase) || string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isStaff)
            {
                MessageBox.Show("You are in view-only mode. Delete is not available.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(flightCodeToDelete))
            {
                MessageBox.Show("Please enter a flight code to delete.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this flight?", "Confirmation", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            try
            {
                using (var conn = Db.CreateConnection())
                {
                    conn.Open();
                    string query = "DELETE FROM Flights WHERE FlightCode = @FlightCode";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@FlightCode", flightCodeToDelete);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Flight deleted successfully!");
                            PopulateDataGridView();
                        }
                        else
                        {
                            MessageBox.Show("Flight not found or could not be deleted.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            string flightCodeToUpdate = (txtFlightCode.Text ?? string.Empty).Trim();

            var role = Session.Role ?? "Passenger";
            bool isStaff = string.Equals(role, "Staff", StringComparison.OrdinalIgnoreCase) || string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isStaff)
            {
                MessageBox.Show("You are in view-only mode. Update is not available.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(flightCodeToUpdate))
            {
                MessageBox.Show("Please enter a flight code to update.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(cmbSource1.Text) || string.IsNullOrWhiteSpace(cmbDestination.Text))
            {
                MessageBox.Show("Please select source and destination.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtNumberSeats.Text, out int seats))
            {
                MessageBox.Show("Number of seats must be a valid integer.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var conn = Db.CreateConnection())
                {
                    conn.Open();
                    string query = "UPDATE Flights SET Source = @Source, Destination = @Destination, TakeOffDate = @TakeOffDate, Seats = @Seats WHERE FlightCode = @FlightCode";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Source", cmbSource1.Text);
                        cmd.Parameters.AddWithValue("@Destination", cmbDestination.Text);
                        cmd.Parameters.AddWithValue("@TakeOffDate", dtpdate.Value);
                        cmd.Parameters.AddWithValue("@Seats", seats);
                        cmd.Parameters.AddWithValue("@FlightCode", flightCodeToUpdate);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Flight updated successfully!");
                            PopulateDataGridView();
                        }
                        else
                        {
                            MessageBox.Show("Flight not found or could not be updated.", "Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Note: dgvFlights_CellContentClick implemented above to pre-fill fields when a row is selected.
    }
}
