using MercatorORM;
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
    public partial class DataSettingsForm : Form
    {
        public DataSettingsForm(DataSettings settings)
        {
            InitializeComponent();
            if(settings != null)
            {
                settings.ExtractDatasFromConnectionString();
                txtServer.Text = settings.DataSource;
                txtDbName.Text = settings.Catalog;
                txtUser.Text = settings.User;
                txtPwd.Text = settings.Password;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
            //Application.Exit();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(txtDbName.Text))
            {
                txtDbName.Text = "Remplissez ce champ!";
                txtDbName.ForeColor = Color.Red;
                return;
            }
            if (String.IsNullOrEmpty(txtServer.Text))
            {
                txtServer.Text = "Remplissez ce champ!";
                txtServer.ForeColor = Color.Red;
                return;
            }

            if (String.IsNullOrEmpty(txtUser.Text))
            {
                txtUser.Text = "Remplissez ce champ!";
                txtUser.ForeColor = Color.Red;
                return;
            }

            if (String.IsNullOrEmpty(txtPwd.Text))
            {
                txtPwd.Text = "Remplissez ce champ!";
                txtPwd.ForeColor = Color.Red;
                return;
            }

            DatabaseManager dm = new DatabaseManager();
            DataSettings ds = new DataSettings();
            ds.BuildConnectionString(txtServer.Text, txtDbName.Text, txtUser.Text, txtPwd.Text);

            bool success = TestConnection(ds);

            if (!success)
            {
                lblError.Text = "Invalid database";
                return;
            }
            try
            {
                dm.SaveSettings(ds);
            }catch(Exception ex)
            {   
                Program.log(ex);
            }

            Install i = new Install();
            i.Show();
            this.Hide();
        }

        private bool TestConnection(DataSettings ds)
        {
            try
            {
                return DatabaseManager.CheckTableExistence(ds.DataConnectionString,"STOCK");
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
