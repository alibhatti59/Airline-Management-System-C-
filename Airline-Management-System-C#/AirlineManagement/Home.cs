using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AirlineManagement
{
    public partial class Home : Form
    {
        public Home()
        {
            InitializeComponent();
            // Presentation toggle from Session.PresentationMode
            try
            {
                if (Session.PresentationMode)
                    PresentationHelper.ApplyPresentation(this, baseFontSize: 18f, controlScale: 1.2f);
            }
            catch { }

            // Add a small presentation host button so user can open fullscreen host at runtime
            try
            {
                var btnPres = new Button();
                btnPres.Text = "Presentation";
                btnPres.AutoSize = true;
                btnPres.BackColor = System.Drawing.Color.DarkCyan;
                btnPres.ForeColor = System.Drawing.Color.White;
                btnPres.FlatStyle = FlatStyle.Flat;
                btnPres.Padding = new Padding(6);
                btnPres.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                btnPres.Location = new System.Drawing.Point(this.ClientSize.Width - 140, 10);
                btnPres.Click += (s, e) =>
                {
                    try
                    {
                        PresentationHost.ShowHost();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Unable to open presentation host: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };
                this.Controls.Add(btnPres);
            }
            catch { }
            // show current user info and apply role-based visibility
            try
            {
                if (lblUserInfo != null)
                    lblUserInfo.Text = $"Logged in as: {Session.Username ?? "(guest)"} ({Session.Role ?? "Passenger"})";
            }
            catch { }

            ApplyRoleBasedVisibility();
        }

        private void ApplyRoleBasedVisibility()
        {
            try
            {
                var role = Session.Role ?? "Passenger";
                // assuming button names from designer: btnFlights, btnPassengers, btnCancellation, btnViewFlights
                bool isStaff = string.Equals(role, "Staff", StringComparison.OrdinalIgnoreCase) || string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);

                // Staff can manage flights, passengers and cancellations
                btnFlights.Visible = isStaff;
                btnPassengers.Visible = isStaff;
                // cancellation should be visible to passengers as well
                btnCancellation.Visible = true;

                // Everyone can book tickets
                btnTickets.Visible = true;

                // Passengers can view flights but not add; show a dedicated view button for them
                if (btnViewFlights != null)
                    btnViewFlights.Visible = !isStaff;

                // show MyBookings only for passengers (not staff)
                if (btnMyBookings != null)
                    btnMyBookings.Visible = !isStaff;
            }
            catch
            {
                // ignore - designer controls may not be initialized in some contexts
            }
        }

        private void lblExit_Click(object sender, EventArgs e)
        {
            this.Close();

            LoginForm login = new LoginForm();
            login.Show();
        }

        private void btnFlights_Click(object sender, EventArgs e)
        {
            FlightsTbl flightsTblForm = new FlightsTbl();
            flightsTblForm.Show();

        }

        private void btnPassengers_Click(object sender, EventArgs e)
        {
            AddPassenger addPassengerForm = new AddPassenger();
            addPassengerForm.Show();
        }

        private void btnTickets_Click(object sender, EventArgs e)
        {
            Tickets ticketsForm = new Tickets();
            ticketsForm.Show();
        }

        private void btnCancellation_Click(object sender, EventArgs e)
        {
            CancellationTbl cancellationTblForm = new CancellationTbl();
            cancellationTblForm.Show();
        }

        private void btnViewFlights_Click(object sender, EventArgs e)
        {
            ViewFlights vf = new ViewFlights();
            vf.Show();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            try
            {
                Session.Username = null;
                Session.Role = "Passenger";
                var login = new LoginForm();
                login.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Logout error: " + ex.Message);
            }
        }

        // Presentation menu handler (called by designer injected event)
        private void presentationModeToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                var item = sender as ToolStripMenuItem;
                if (item == null) return;
                Session.PresentationMode = item.Checked;
                if (item.Checked)
                    PresentationHelper.ApplyPresentation(this, baseFontSize: 18f, controlScale: 1.2f);
                else
                    PresentationHelper.RevertPresentation(this);
            }
            catch { }
        }
    }
}
