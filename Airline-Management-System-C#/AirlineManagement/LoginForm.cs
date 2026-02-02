using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AirlineManagement.Database;

namespace AirlineManagement
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
            txtPassword.PasswordChar = '*';
        }

        private void lblExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = (txtUsername.Text ?? string.Empty).Trim();
            string password = (txtPassword.Text ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter username and password.");
                return;
            }

            try
            {
                bool authenticated = false;
                string role = null;
                using (var conn = Db.CreateConnection())
                {
                    conn.Open();
                    // simpler authentication query: return Role if username+password match
                    string query = "SELECT Role FROM Users WHERE Username = @u AND Password = @p";
                    using (var cmd = new System.Data.SqlClient.SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@u", username);
                        cmd.Parameters.AddWithValue("@p", password);
                        var roleObj = cmd.ExecuteScalar();
                        if (roleObj != null && roleObj != DBNull.Value)
                        {
                            role = roleObj.ToString();
                            authenticated = true;
                        }
                    }
                }

                // fallback to default admin/admin if no user found
                if (!authenticated && username == "admin" && password == "admin")
                {
                    authenticated = true;
                    role = "Staff";
                }

                if (authenticated)
                {
                    // store in session
                    Session.Username = username;
                    Session.Role = string.IsNullOrWhiteSpace(role) ? "Passenger" : role;

                    // if passenger, also load PassengerId from Users table (PassengerId may be null)
                    if (string.Equals(Session.Role, "Passenger", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            using (var conn2 = Db.CreateConnection())
                            {
                                conn2.Open();
                                string pidQ = "SELECT PassengerId FROM Users WHERE Username = @u";
                                using (var cmd2 = new System.Data.SqlClient.SqlCommand(pidQ, conn2))
                                {
                                    cmd2.Parameters.AddWithValue("@u", username);
                                    var pidObj = cmd2.ExecuteScalar();
                                    if (pidObj != null && pidObj != DBNull.Value)
                                        Session.PassengerId = pidObj.ToString();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // show detailed info for troubleshooting
                            MessageBox.Show("Error loading passenger id: " + ex.ToString(), "Login error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                    try
                    {
                        Home home = new Home();
                        home.Show();
                        this.Hide();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error opening Home form: " + ex.ToString(), "Login error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Incorrect username or password. Please try again.", "Authentication Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                // show full exception for diagnostics
                MessageBox.Show("Login error: " + ex.ToString(), "Login error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtPassword.Text = "";
            txtUsername.Text = "";
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            txtPassword.PasswordChar = checkBox1.Checked ? '\0' : '*';
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            using (var reg = new RegisterForm())
            {
                var result = reg.ShowDialog(this);
                if (result == DialogResult.OK)
                {
                    // registration completed and session set by RegisterForm for passenger
                    try
                    {
                        Home home = new Home();
                        home.Show();
                        this.Hide();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error opening Home after registration: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
        }

        private void lblClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}

