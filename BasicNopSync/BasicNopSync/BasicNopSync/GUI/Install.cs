using BasicNopSync.Database;
using BasicNopSync.WebAPi.Controllers;
using MercatorORM;
using System;
using System.Drawing;
using System.Windows.Forms;
using BasicNopSync.Syncers.MercatorToNop;
using BasicNopSync.Model.Mercator;
using BasicNopSync.WebApi.Datas;

namespace BasicNopSync.GUI
{
    public partial class Install : Form
    {   
        public Install()
        {   
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            OptionsMercator repMerc = new OptionsMercator();
            OptionsMercator journal = new OptionsMercator();
            OptionsMercator genericArticle = new OptionsMercator();

            string repMercValue = repMerc.GetOptionValue("NOP_REP_M")?.ToString();
            string journalValue = journal.GetOptionValue("NOP_JOURN")?.ToString();
            string genericArticleValue = genericArticle.GetOptionValue("NOP_GEN_A")?.ToString()?.TrimEnd();

            txtRepMercator.Text = repMercValue?.ToString()?.TrimEnd() ?? "";
            txtJournal.Text = journalValue?.ToString()?.TrimEnd() ?? "";
            boxGenericArticle.Checked = genericArticleValue == "1";//genericArticleValue;

            if (DatabaseManager.CheckTableExistence("WEB_API_CREDENTIALS"))
            {
                AuthParameters ap = AuthorizationController.GetAuthParams();

                if (ap != null)
                {
                    txtPublicToken.Text = ap.PublicToken;
                    txtSecretToken.Text = ap.SecretToken;
                    txtStoreAddress.Text = ap.StoreAddress;
                    txtUser.Text = ap.ClientName;
                }
            }
        }
        
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Control && e.Shift)
            {
                Console.WriteLine("Greetings Professor Falken");
                Program.Run("");

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {           

            //Program.UpdateConfig("FirstLaunch", "False");
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e)
        {

            if (String.IsNullOrEmpty(txtUser.Text))
            {
                txtUser.Text = "Remplissez ce champ!";
                txtUser.ForeColor = Color.Red;
                return;
            }
            if (String.IsNullOrEmpty(txtPublicToken.Text))
            {
                txtPublicToken.Text = "Remplissez ce champ!";
                txtPublicToken.ForeColor = Color.Red;
                return;
            }

            if (String.IsNullOrEmpty(txtSecretToken.Text))
            {
                txtSecretToken.Text = "Remplissez ce champ!";
                txtSecretToken.ForeColor = Color.Red;
                return;
            }

            if (String.IsNullOrEmpty(txtStoreAddress.Text))
            {
                txtStoreAddress.Text = "Remplissez ce champ!";
                txtStoreAddress.ForeColor = Color.Red;
                return;
            }

            if (String.IsNullOrEmpty(txtJournal.Text))
            {
                txtJournal.Text = "Remplissez ce champ!";
                txtJournal.ForeColor = Color.Red;
                return;
            }

            if (String.IsNullOrEmpty(txtRepMercator.Text))
            {
                txtRepMercator.Text = "Remplissez ce champ!";
                txtRepMercator.ForeColor = Color.Red;
                return;
            }

            AuthorizationController controller = new AuthorizationController();            
            DataSettings ds = DatabaseManager.LoadSettings();

            //AuthParameters authParams = controller.InitiateAuthorization(txtServerUrl.Text,txtClientId.Text,txtClientSecret.Text);
            InstallerDatas install = new InstallerDatas();
            install.JournalMercator = txtJournal.Text;
            install.RepMercator = txtRepMercator.Text;
            install.ConnectionString = ds.DataConnectionString;
            install.UseGenericArticles = boxGenericArticle.Checked;

            AuthParameters authParams = new AuthParameters();
            authParams.ClientName = txtUser.Text;
            authParams.PublicToken = txtPublicToken.Text;
            authParams.SecretToken = txtSecretToken.Text;
            authParams.StoreAddress = txtStoreAddress.Text;

            install.authParameters = authParams;

            InstallerProgressBar installPB = new InstallerProgressBar(install);
            installPB.Show();

            this.Hide();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();

            txtRepMercator.Text = folderBrowserDialog1.SelectedPath;
        }
    }
}
