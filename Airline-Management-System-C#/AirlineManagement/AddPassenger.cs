using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using AirlineManagement.Database;

namespace AirlineManagement
{
    public partial class AddPassenger : Form
    {
        private SqlConnection Con = Db.CreateConnection();

        public AddPassenger()
        {
            InitializeComponent();

            // enforce role: only Staff/Admin can open this form
            try
            {
                var role = Session.Role ?? "Passenger";
                bool isStaff = string.Equals(role, "Staff", StringComparison.OrdinalIgnoreCase) || string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
                if (!isStaff)
                {
                    MessageBox.Show("Access denied. Only staff can add passengers.", "Authorization", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.BeginInvoke((Action)(() => this.Close()));
                    return;
                }
            }
            catch { }
        }

        private void lblExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnRecord_Click(object sender, EventArgs e)
        {
            // allow selecting from combo or typing text
            bool nationalityProvided = (cmbNationality.SelectedItem != null) || !string.IsNullOrWhiteSpace(cmbNationality.Text);
            bool genderProvided = (cmbGender.SelectedItem != null) || !string.IsNullOrWhiteSpace(cmbGender.Text);

            if (string.IsNullOrWhiteSpace(txtPassID.Text)
                || string.IsNullOrWhiteSpace(txtPassName.Text)
                || string.IsNullOrWhiteSpace(txtPassAdress.Text)
                || string.IsNullOrWhiteSpace(txtPassNumber.Text)
                || string.IsNullOrWhiteSpace(txtPhoneNumber.Text)
                || !nationalityProvided
                || !genderProvided)
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            try
            {
                Con.Open();
                string query = "INSERT INTO PassengerTbl VALUES (@PassID, @PassName, @PassAdress, @PassNumber, @Nationality, @Gender, @PhoneNumber)";
                SqlCommand cmd = new SqlCommand(query, Con);
                cmd.Parameters.AddWithValue("@PassID", txtPassID.Text.Trim());
                cmd.Parameters.AddWithValue("@PassName", txtPassName.Text.Trim());
                cmd.Parameters.AddWithValue("@PassAdress", txtPassAdress.Text.Trim());
                cmd.Parameters.AddWithValue("@PassNumber", txtPassNumber.Text.Trim());
                string nationality = cmbNationality.SelectedItem != null ? cmbNationality.SelectedItem.ToString() : cmbNationality.Text.Trim();
                string gender = cmbGender.SelectedItem != null ? cmbGender.SelectedItem.ToString() : cmbGender.Text.Trim();
                cmd.Parameters.AddWithValue("@Nationality", nationality);
                cmd.Parameters.AddWithValue("@Gender", gender);
                cmd.Parameters.AddWithValue("@PhoneNumber", txtPhoneNumber.Text.Trim());
                cmd.ExecuteNonQuery();
                MessageBox.Show("Passenger recorded successfully!");

                // If user typed a new nationality or gender, add to combo lists so it appears next time
                if (!string.IsNullOrWhiteSpace(nationality) && !cmbNationality.Items.Contains(nationality))
                    cmbNationality.Items.Add(nationality);
                if (!string.IsNullOrWhiteSpace(gender) && !cmbGender.Items.Contains(gender))
                    cmbGender.Items.Add(gender);

                // Try to refresh Tickets form passenger list if open
                foreach (Form open in Application.OpenForms)
                {
                    if (open is Tickets ticketsForm)
                    {
                        ticketsForm.RefreshPassengersAndSelect(txtPassID.Text.Trim());
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                Con.Close();
            }
        }

        private void btnViewPassengers_Click(object sender, EventArgs e)
        {
            ViewPassengers viewpass = new ViewPassengers();
            viewpass.Show();
            this.Hide();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtPassID.Text = "";
            txtPassName.Text = "";
            txtPassAdress.Text = "";
            txtPassNumber.Text = "";
            cmbNationality.SelectedItem = null;
            cmbGender.SelectedItem = null;
            txtPhoneNumber.Text = "";
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();

            Home homeForm = new Home();
            homeForm.Show();
        }

        private void AddPassenger_FormClosed(object sender, FormClosedEventArgs e)
        {
            Home homeForm = new Home();
            homeForm.Show();
            this.Dispose();
        }

        private void cmbNationality_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
