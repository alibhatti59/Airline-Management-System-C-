using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AirlineManagement
{
    // Full-screen host that displays other forms inside a blank presentation background.
    // Usage: PresentationHost.ShowHost();
    public class PresentationHost : Form
    {
        private Panel hostPanel;
        private ToolStrip toolStrip;

        public PresentationHost()
        {
            Initialize();
        }

        private void Initialize()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.Black; // blank background
            this.StartPosition = FormStartPosition.CenterScreen;
            this.KeyPreview = true;

            // Host panel where forms will be embedded (double-buffered to reduce flicker)
            hostPanel = new DoubleBufferedPanel();
            hostPanel.Dock = DockStyle.Fill;
            // use the same blank background as the host so embedded forms appear on a uniform background
            hostPanel.BackColor = Color.Black;

            // Toolstrip on the left with buttons to open common forms
            toolStrip = new ToolStrip();
            toolStrip.Dock = DockStyle.Left;
            toolStrip.RenderMode = ToolStripRenderMode.System;
            toolStrip.BackColor = Color.Transparent;
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
            toolStrip.Padding = new Padding(6);
            toolStrip.Width = 140;

            AddToolButton("Home", OpenHome);
            AddToolButton("Flights (View)", OpenViewFlights);
            AddToolButton("Flights (Edit)", OpenFlightsTbl);
            AddToolButton("Tickets", OpenTickets);
            AddToolButton("Passengers", OpenViewPassengers);
            AddToolButton("Add Passenger", OpenAddPassenger);
            AddToolButton("Cancellations", OpenCancellation);
            AddToolButton("Close Host", (s, e) => this.Close());

            // add toolstrip first so dock layout reserves space, then add host panel to fill remaining area
            this.Controls.Add(toolStrip);
            this.Controls.Add(hostPanel);

            // Escape closes host
            this.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) this.Close(); };
        }

        // show the main form immediately so the host is not empty
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // start with the Login form inside the host so users must authenticate first
            ShowFormInHost(new LoginForm());
        }

        // small helper panel type with double buffering enabled
        private class DoubleBufferedPanel : Panel
        {
            public DoubleBufferedPanel()
            {
                this.DoubleBuffered = true;
            }
        }

        private void AddToolButton(string text, EventHandler onClick)
        {
            var btn = new ToolStripButton(text);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.DisplayStyle = ToolStripItemDisplayStyle.Text;
            btn.AutoSize = false;
            btn.Height = 36;
            btn.Width = 120;
            btn.Margin = new Padding(2);
            btn.Click += onClick;
            toolStrip.Items.Add(btn);
        }

        private void ShowFormInHost(Form f)
        {
            if (f == null) return;

            try
            {
                // Hide any previously shown owned forms so only one form appears on the blank background
                var owned = Application.OpenForms.Cast<Form>().Where(of => of.Owner == this).ToList();
                foreach (var of in owned)
                {
                    try { of.Hide(); } catch { }
                }

                // Show the form as a normal top-level window owned by the host so we don't change its style or size.
                f.TopLevel = true;

                // Set owner so the form stays in front of host but retains its own chrome and size
                f.Owner = this;

                // Position: center over hostPanel (blank background area)
                try
                {
                    var hostRect = hostPanel.RectangleToScreen(hostPanel.ClientRectangle);
                    int x = hostRect.Left + Math.Max(0, (hostRect.Width - f.Width) / 2);
                    int y = hostRect.Top + Math.Max(0, (hostRect.Height - f.Height) / 2);
                    f.StartPosition = FormStartPosition.Manual;
                    f.Location = new Point(x, y);
                }
                catch { }

                // Show without altering designer appearance
                if (!f.Visible)
                    f.Show(this);

                f.BringToFront();
                f.Activate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to show form in host: " + ex.Message, "Host Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetControlsVisibleRecursive(Control parent, bool visible)
        {
            foreach (Control c in parent.Controls)
            {
                try { c.Visible = visible; } catch { }
                if (c.HasChildren) SetControlsVisibleRecursive(c, visible);
            }
        }

        private void OpenHome(object sender, EventArgs e)
        {
            var f = new Home();
            // keep host background and hide Home's own maximize etc by embedding
            ShowFormInHost(f);
        }

        private void OpenViewFlights(object sender, EventArgs e)
        {
            var f = new ViewFlights();
            ShowFormInHost(f);
        }

        private void OpenFlightsTbl(object sender, EventArgs e)
        {
            var f = new FlightsTbl();
            ShowFormInHost(f);
        }

        private void OpenTickets(object sender, EventArgs e)
        {
            var f = new Tickets();
            ShowFormInHost(f);
        }

        private void OpenViewPassengers(object sender, EventArgs e)
        {
            var f = new ViewPassengers();
            ShowFormInHost(f);
        }

        private void OpenAddPassenger(object sender, EventArgs e)
        {
            var f = new AddPassenger();
            ShowFormInHost(f);
        }

        private void OpenCancellation(object sender, EventArgs e)
        {
            var f = new CancellationTbl();
            ShowFormInHost(f);
        }

        // Helper to show the host form modally/fullscreen
        public static void ShowHost()
        {
            try
            {
                var host = new PresentationHost();
                host.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Presentation host error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // If a PresentationHost is already open, use it to display a form inside the host.
        // Returns true if an existing host was found and used.
        public static bool TryShowInExistingHost(Form f)
        {
            if (f == null) return false;
            foreach (Form open in Application.OpenForms)
            {
                if (open is PresentationHost ph)
                {
                    try
                    {
                        ph.ShowFormInHost(f);
                        return true;
                    }
                    catch { return false; }
                }
            }
            return false;
        }

        // Expose a public method to show a form inside this host instance.
        public void ShowFormInHostPublic(Form f)
        {
            ShowFormInHost(f);
        }
    }
}