using BasicNopSync.Model.Mercator;
using BasicNopSync.WebAPi.Controllers;
using BasicNopSync.Model.Mercator;
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
    public partial class InstallerProgressBar : Form
    {
        private InstallerDatas installer;        

        public InstallerProgressBar(InstallerDatas installer)
        {
            InitializeComponent();
            
            this.installer = installer;
        }

        private void InstallerProgressBar_Load(object sender, EventArgs e)
        {
            
         
        }

        private void InstallerProgressBar_Shown(object sender, EventArgs e)
        {
            //Force window to render components
            Application.DoEvents();

            bool success = FirstLaunch.Initiate(progressBar, lblProgressText, installer);
            if (success)
            {
                btnDone.Enabled = true;
                label1.Text = "Installation terminée";
            }
            else
            {
                label1.Text = "Une erreur est survenue, veuillez réessayer";
                label1.ForeColor = Color.Red;
            }
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);            
        }
    }
}
