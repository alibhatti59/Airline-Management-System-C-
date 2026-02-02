using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using AirlineManagement.Database;

namespace AirlineManagement
{
    public partial class RegisterForm : Form
    {
        public RegisterForm()
        {
            InitializeComponent();
            // mask password by default
            txtPassword.PasswordChar = '*';
            lblPassError.Visible = false;
            // wire checkbox to toggle password visibility
            this.checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            // set default role
            try
            {
                if (cmbRole != null && cmbRole.Items.Count > 0)
                    cmbRole.SelectedIndex = 0; // Passenger
                // hide passenger id field for staff
                this.cmbRole.SelectedIndexChanged += CmbRole_SelectedIndexChanged;
                UpdatePassengerIdVisibility();
                // auto-generate passenger id into the textbox when creating passenger
                if (txtPassengerId != null)
                {
                    try
                    {
                        using (var conn = Db.CreateConnection())
                        {
                            conn.Open();
                            using (var genCmd = new SqlCommand("SELECT MAX(TRY_CAST(PassengerId AS INT)) FROM PassengerTbl", conn))
                            {
                                var obj = genCmd.ExecuteScalar();
                                int next = 1;
                                if (obj != null && obj != DBNull.Value)
                                {
                                    int mx;
                                    if (int.TryParse(obj.ToString(), out mx)) next = mx + 1;
                                }
                                txtPassengerId.Text = next.ToString();
                                txtPassengerId.Enabled = false; // user shouldn't edit generated id
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        private void CmbRole_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePassengerIdVisibility();
        }

        private void UpdatePassengerIdVisibility()
        {
            try
            {
                var role = cmbRole.SelectedItem != null ? cmbRole.SelectedItem.ToString() : "Passenger";
                var isStaff = string.Equals(role, "Staff", StringComparison.OrdinalIgnoreCase) || string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
                // if staff, hide PassengerId input
                if (this.txtPassengerId != null)
                    this.txtPassengerId.Visible = !isStaff;
                if (this.lblPassengerId != null)
                    this.lblPassengerId.Visible = !isStaff;
            }
            catch { }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            lblPassError.Visible = false;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblPassError.Text = "Please provide username and password.";
                lblPassError.Visible = true;
                return;
            }

            // enforce minimum password length
            if (password.Length < 6)
            {
                lblPassError.Text = "Password must be at least 6 characters long.";
                lblPassError.Visible = true;
                return;
            }

            try
            {
                using (var conn = Db.CreateConnection())
                {
                    conn.Open();
                    // if role is passenger, ensure a Passenger record exists and get its id
                    var roleForInsert = cmbRole != null && cmbRole.SelectedItem != null ? cmbRole.SelectedItem.ToString() : "Passenger";
                    string passengerIdForUser = null;
                    if (string.Equals(roleForInsert, "Passenger", StringComparison.OrdinalIgnoreCase))
                    {
                        // if user provided a PassengerId use it, otherwise auto-generate
                        var provided = (this.txtPassengerId != null) ? this.txtPassengerId.Text.Trim() : string.Empty;
                        using (var chk = new SqlCommand("SELECT COUNT(1) FROM PassengerTbl WHERE PassengerId = @id", conn))
                        {
                            if (!string.IsNullOrWhiteSpace(provided))
                            {
                                chk.Parameters.AddWithValue("@id", provided);
                                var exists = (int)chk.ExecuteScalar() > 0;
                                if (exists)
                                {
                                    // use provided
                                    passengerIdForUser = provided;
                                }
                                else
                                {
                                    // create passenger using provided id
                                    using (var ins = new SqlCommand("INSERT INTO PassengerTbl (PassengerId, PassengerName, PassengerAdress, Passport, PassengerNationality, PassengerGender, PassengerPhoneNumber) VALUES (@id,@name,'', '', '', '', '')", conn))
                                    {
                                        ins.Parameters.AddWithValue("@id", provided);
                                        ins.Parameters.AddWithValue("@name", username);
                                        ins.ExecuteNonQuery();
                                    }
                                    passengerIdForUser = provided;
                                }
                            }
                            else
                            {
                                // auto-generate numeric id by taking max numeric PassengerId + 1
                                using (var genCmd = new SqlCommand("SELECT MAX(TRY_CAST(PassengerId AS INT)) FROM PassengerTbl", conn))
                                {
                                    var obj = genCmd.ExecuteScalar();
                                    int next = 1;
                                    if (obj != null && obj != DBNull.Value)
                                    {
                                        int mx;
                                        if (int.TryParse(obj.ToString(), out mx)) next = mx + 1;
                                    }
                                    passengerIdForUser = next.ToString();
                                    using (var ins = new SqlCommand("INSERT INTO PassengerTbl (PassengerId, PassengerName, PassengerAdress, Passport, PassengerNationality, PassengerGender, PassengerPhoneNumber) VALUES (@id,@name,'', '', '', '', '')", conn))
                                    {
                                        ins.Parameters.AddWithValue("@id", passengerIdForUser);
                                        ins.Parameters.AddWithValue("@name", username);
                                        ins.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                    }

                    // validate username uniqueness
                    using (var checkUser = new SqlCommand("SELECT COUNT(1) FROM Users WHERE Username = @u", conn))
                    {
                        checkUser.Parameters.AddWithValue("@u", username);
                        var cnt = Convert.ToInt32(checkUser.ExecuteScalar());
                        if (cnt > 0)
                        {
                            MessageBox.Show("Username already exists. Choose a different username.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    // if passengerIdForUser assigned, ensure it's not already linked to another user
                    if (!string.IsNullOrWhiteSpace(passengerIdForUser))
                    {
                        using (var checkLink = new SqlCommand("SELECT Username FROM Users WHERE PassengerId = @pid", conn))
                        {
                            checkLink.Parameters.AddWithValue("@pid", passengerIdForUser);
                            var existing = checkLink.ExecuteScalar();
                            if (existing != null && existing != DBNull.Value)
                            {
                                MessageBox.Show($"Passenger ID {passengerIdForUser} is already linked to user '{existing}'. Choose a different Passenger ID.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }
                    }

                    string query = "INSERT INTO Users (Username, Password, Role, PassengerId) VALUES (@u, @p, @r, @passId)";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@u", username);
                        cmd.Parameters.AddWithValue("@p", password);
                        // role from UI selection
                        var role = "Passenger";
                        try { if (cmbRole != null && cmbRole.SelectedItem != null) role = cmbRole.SelectedItem.ToString(); } catch { }
                        cmd.Parameters.AddWithValue("@r", role);
                        var passIdValue = (object)DBNull.Value;
                        if (!string.IsNullOrWhiteSpace(passengerIdForUser)) passIdValue = passengerIdForUser;
                        cmd.Parameters.AddWithValue("@passId", passIdValue);
                        cmd.ExecuteNonQuery();

                        // if registered as passenger, sign in immediately and set session PassengerId
                        if (string.Equals(role, "Passenger", StringComparison.OrdinalIgnoreCase))
                        {
                            Session.Username = username;
                            Session.Role = "Passenger";
                            Session.PassengerId = passengerIdForUser;

                            // signal success to caller (do not close owner here)
                            this.DialogResult = DialogResult.OK;
                            return; // stop further processing in this method
                        }
                    }
                }

                MessageBox.Show("User registered successfully.");
                this.Close();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("SQL Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.PasswordChar = checkBox1.Checked ? '\0' : '*';
        }
    }
}
