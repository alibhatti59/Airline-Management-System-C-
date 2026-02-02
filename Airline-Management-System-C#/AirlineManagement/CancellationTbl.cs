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
    public partial class CancellationTbl : Form
    {
        SqlConnection Con = Db.CreateConnection();

        public CancellationTbl()
        {
            InitializeComponent();
            // Cancellation available to both staff and passengers; visibility controlled by Home
        }
        

        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtTicketId.Text))
                {
                    MessageBox.Show("Please enter the Ticket ID to cancel.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Con.Open();

                // read owner info for the ticket
                string ownerQuery = "SELECT PassId, PassName FROM Tickets WHERE TicketId = @TicketId";
                string passId = null;
                string passName = null;
                using (var ownerCmd = new SqlCommand(ownerQuery, Con))
                {
                    ownerCmd.Parameters.AddWithValue("@TicketId", txtTicketId.Text);
                    using (var r = ownerCmd.ExecuteReader())
                    {
                        if (!r.Read())
                        {
                            MessageBox.Show("Ticket not found.", "Not found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        passId = r["PassId"] != DBNull.Value ? r["PassId"].ToString() : null;
                        passName = r["PassName"] != DBNull.Value ? r["PassName"].ToString() : null;
                    }
                }

                // If current user is a passenger, ensure they own the ticket
                var role = Session.Role ?? "Passenger";
                bool isStaff = string.Equals(role, "Staff", StringComparison.OrdinalIgnoreCase) || string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
                if (!isStaff)
                {
                    var current = Session.Username ?? string.Empty;
                    // allow cancellation if session username matches PassId or PassName
                    // prefer PassengerId match if available
                    var sessionPassengerId = Session.PassengerId ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(sessionPassengerId))
                    {
                        if (!string.Equals(sessionPassengerId, passId, StringComparison.OrdinalIgnoreCase))
                        {
                            MessageBox.Show("You can only cancel your own tickets.", "Authorization", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                    else
                    {
                        if (!string.Equals(current, passId, StringComparison.OrdinalIgnoreCase) && !string.Equals(current, passName, StringComparison.OrdinalIgnoreCase))
                        {
                            MessageBox.Show("You can only cancel your own tickets.", "Authorization", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                    {
                        MessageBox.Show("You can only cancel your own tickets.", "Authorization", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // confirm
                var conf = MessageBox.Show("Are you sure you want to cancel this ticket?", "Confirm cancellation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (conf != DialogResult.Yes)
                    return;

                // fetch deleted row for display
                DataTable deletedRow = null;
                string selectQuery = "SELECT * FROM Tickets WHERE TicketId = @TicketId";
                using (var selectCmd = new SqlCommand(selectQuery, Con))
                {
                    selectCmd.Parameters.AddWithValue("@TicketId", txtTicketId.Text);
                    using (var reader = selectCmd.ExecuteReader())
                    {
                        deletedRow = new DataTable();
                        deletedRow.Load(reader);
                    }
                }

                // delete
                string deleteQuery = "DELETE FROM Tickets WHERE TicketId = @TicketId";
                using (var deleteCmd = new SqlCommand(deleteQuery, Con))
                {
                    deleteCmd.Parameters.AddWithValue("@TicketId", txtTicketId.Text);
                    deleteCmd.ExecuteNonQuery();
                }

                MessageBox.Show("Ticket cancelled successfully!", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dgvCancellations.DataSource = deletedRow;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while cancelling ticket: " + ex.Message);
            }
            finally
            {
                if (Con.State == ConnectionState.Open)
                    Con.Close();
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            txtTicketId.Text = "";
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();

            Home homeForm = new Home();
            homeForm.Show();
        }
    }
}
