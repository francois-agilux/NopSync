using BasicNopSync.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BasicNopSync.GUI
{
    public partial class WelcomeWindow : Form
    {
        DataSettings settings;

        public WelcomeWindow(DataSettings settings)
        {
            InitializeComponent();
            this.settings = settings;
        }

        private void WelcomeWindow_Load(object sender, EventArgs e)
        {
            // Construct an image object from a file in the local directory.
            // ... This file must exist in the solution.
            Image image = Image.FromFile("GUI/img/logo.png");
            // Set the PictureBox image property to this image.
            // ... Then, adjust its height and width properties.
            pictureBox1.Image = image;
            pictureBox1.Height = image.Height;
            pictureBox1.Width = image.Width;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
            //Application.Exit();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {   
            DataSettingsForm dsf = new DataSettingsForm(settings);
            dsf.Show();
            this.Hide();
        }
    }
}
