namespace AirlineManagement
{
    partial class Home
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Home));
            this.label2 = new System.Windows.Forms.Label();
            this.lblExit = new System.Windows.Forms.Label();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblUserInfo = new System.Windows.Forms.Label();
            this.btnViewFlights = new System.Windows.Forms.Button();
            this.btnFlights = new System.Windows.Forms.Button();
            this.btnPassengers = new System.Windows.Forms.Button();
            this.btnTickets = new System.Windows.Forms.Button();
            this.btnCancellation = new System.Windows.Forms.Button();
            this.btnLogout = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.BackColor = System.Drawing.Color.Transparent;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Name = "label2";
            // 
            // lblExit
            // 
            resources.ApplyResources(this.lblExit, "lblExit");
            this.lblExit.BackColor = System.Drawing.Color.Red;
            this.lblExit.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblExit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblExit.ForeColor = System.Drawing.Color.MistyRose;
            this.lblExit.Name = "lblExit";
            this.lblExit.Click += new System.EventHandler(this.lblExit_Click);
            // 
            // pictureBox2
            // 
            resources.ApplyResources(this.pictureBox2, "pictureBox2");
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.TabStop = false;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.RoyalBlue;
            this.panel1.Controls.Add(this.pictureBox2);
            this.panel1.Controls.Add(this.lblExit);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.lblUserInfo);
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // lblUserInfo
            // 
            this.lblUserInfo.ForeColor = System.Drawing.Color.White;
            resources.ApplyResources(this.lblUserInfo, "lblUserInfo");
            this.lblUserInfo.Name = "lblUserInfo";
            // 
            // btnViewFlights
            // 
            this.btnViewFlights.BackColor = System.Drawing.Color.CornflowerBlue;
            this.btnViewFlights.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.btnViewFlights, "btnViewFlights");
            this.btnViewFlights.ForeColor = System.Drawing.Color.Black;
            this.btnViewFlights.Name = "btnViewFlights";
            this.btnViewFlights.UseVisualStyleBackColor = false;
            this.btnViewFlights.Click += new System.EventHandler(this.btnViewFlights_Click);
            // 
            // btnFlights
            // 
            this.btnFlights.BackColor = System.Drawing.Color.CornflowerBlue;
            this.btnFlights.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.btnFlights, "btnFlights");
            this.btnFlights.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnFlights.Name = "btnFlights";
            this.btnFlights.UseVisualStyleBackColor = false;
            this.btnFlights.Click += new System.EventHandler(this.btnFlights_Click);
            // 
            // btnPassengers
            // 
            this.btnPassengers.BackColor = System.Drawing.Color.CornflowerBlue;
            this.btnPassengers.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.btnPassengers, "btnPassengers");
            this.btnPassengers.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnPassengers.Name = "btnPassengers";
            this.btnPassengers.UseVisualStyleBackColor = false;
            this.btnPassengers.Click += new System.EventHandler(this.btnPassengers_Click);
            // 
            // btnTickets
            // 
            this.btnTickets.BackColor = System.Drawing.Color.CornflowerBlue;
            this.btnTickets.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.btnTickets, "btnTickets");
            this.btnTickets.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnTickets.Name = "btnTickets";
            this.btnTickets.UseVisualStyleBackColor = false;
            this.btnTickets.Click += new System.EventHandler(this.btnTickets_Click);
            // 
            // btnCancellation
            // 
            this.btnCancellation.BackColor = System.Drawing.Color.CornflowerBlue;
            this.btnCancellation.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.btnCancellation, "btnCancellation");
            this.btnCancellation.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnCancellation.Name = "btnCancellation";
            this.btnCancellation.UseVisualStyleBackColor = false;
            this.btnCancellation.Click += new System.EventHandler(this.btnCancellation_Click);
            // 
            // btnLogout
            // 
            this.btnLogout.BackColor = System.Drawing.Color.IndianRed;
            this.btnLogout.Cursor = System.Windows.Forms.Cursors.Hand;
            resources.ApplyResources(this.btnLogout, "btnLogout");
            this.btnLogout.ForeColor = System.Drawing.Color.Transparent;
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.UseVisualStyleBackColor = false;
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);
            // 
            // Home
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.MistyRose;
            this.Controls.Add(this.btnCancellation);
            this.Controls.Add(this.btnViewFlights);
            this.Controls.Add(this.btnTickets);
            this.Controls.Add(this.btnPassengers);
            this.Controls.Add(this.btnFlights);
            this.Controls.Add(this.btnLogout);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Home";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblExit;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblUserInfo;
        private System.Windows.Forms.Button btnViewFlights;
        private System.Windows.Forms.Button btnMyBookings;
        private System.Windows.Forms.Button btnFlights;
        private System.Windows.Forms.Button btnPassengers;
        private System.Windows.Forms.Button btnTickets;
        private System.Windows.Forms.Button btnCancellation;
        private System.Windows.Forms.Button btnLogout;
    }
}