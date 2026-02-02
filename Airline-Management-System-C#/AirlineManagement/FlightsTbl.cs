using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AirlineManagement.Database;

namespace AirlineManagement
{
    public partial class FlightsTbl : Form
    {
        public FlightsTbl()
        {
            InitializeComponent();

            // enforce role: only Staff/Admin can open this form
            try
            {
                var role = Session.Role ?? "Passenger";
                bool isStaff = string.Equals(role, "Staff", StringComparison.OrdinalIgnoreCase) || string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
                if (!isStaff)
                {
                    MessageBox.Show("Access denied. Only staff can manage flights.", "Authorization", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.BeginInvoke((Action)(() => this.Close()));
                    return;
                }
            }
            catch { }
        }

        SqlConnection Con = Db.CreateConnection();

        private void btnRecord_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(txtFlightCode.Text) &&
                    !string.IsNullOrWhiteSpace(cmbSource.Text) &&
                    !string.IsNullOrWhiteSpace(cmbDestination.Text) &&
                    int.TryParse(txtNumberSeats.Text, out int seats))
                {
                    using (var con = Db.CreateConnection())
                    {
                        con.Open();
                        string query = "INSERT INTO Flights (FlightCode, Source, Destination, TakeOffDate, Seats) VALUES (@FlightCode, @Source, @Destination, @TakeOffDate, @Seats)";

                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@FlightCode", txtFlightCode.Text.Trim());
                            cmd.Parameters.AddWithValue("@Source", cmbSource.Text.Trim());
                            cmd.Parameters.AddWithValue("@Destination", cmbDestination.Text.Trim());
                            cmd.Parameters.AddWithValue("@TakeOffDate", dtpDate.Value);
                            cmd.Parameters.AddWithValue("@Seats", seats);

                            cmd.ExecuteNonQuery();

                            MessageBox.Show("Flight recorded successfully!");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please enter valid flight code, source, destination, and number of seats.");
                }
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

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtFlightCode.Text = "";
            txtNumberSeats.Text = "";
            cmbDestination.Text = "";
            cmbSource.Text = "";
        }

        private void btnFlights_Click(object sender, EventArgs e)
        {
            ViewFlights viewFlightsForm = new ViewFlights();

            viewFlightsForm.Show();
        }

        private void label7_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();

            Home homeForm = new Home();
            homeForm.Show();
        }
    }
}
