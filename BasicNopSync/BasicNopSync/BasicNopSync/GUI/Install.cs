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

            string repMercValue = repMerc.GetOptionValue("NOP_REP_M")?.ToString();
            string journalValue = journal.GetOptionValue("NOP_JOURN")?.ToString();

            txtRepMercator.Text = repMercValue?.ToString()?.TrimEnd() ?? "";
            txtJournal.Text = journalValue?.ToString()?.TrimEnd() ?? "";

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

        private void syncBtn_Click(object sender, EventArgs e)
        {
            //string syncChoice = syncComboBox.Text;
            Cursor.Current = Cursors.WaitCursor;
            successLabel.Text = "Syncing...";
            errorLabel.Text = "";

            //switch (syncChoice)
            //{
            //    case "RFS":
            //        if (RFS.SyncRfs("Rayons"))
            //         {

            //            if (RFS.SyncRfs("Familles"))
            //            {   
            //                if (RFS.SyncRfs("SousFamilles"))
            //                {
            //                    successLabel.Text = "RSF succesfully sync";
            //                }
            //                else
            //                {
            //                    errorLabel.Text = "Sous-Familles - Sync Failed";                        
            //                }
            //            }
            //            else
            //            {
            //                errorLabel.Text = "Familles - Sync Failed";                        
            //            }
            //        } else
            //         {
            //            errorLabel.Text = "Rayons - Sync Failed";
            //            successLabel.Text = "";
            //        }

            //        break;

            //    case "Catégories Articles":
            //        if (SCat.SyncSCat())
            //        {
            //            successLabel.Text = "Catégories successfully sync";
            //        } else
            //        {
            //            errorLabel.Text = "Catégories - Sync Failed";
            //            successLabel.Text = "";
            //        }
            //        break;

            //    case "Tarifs Articles":
            //        if (Tarifs.SyncTarifs())
            //        {
            //            successLabel.Text = "Tarifs successfully sync";
            //        }
            //        else
            //        {
            //            errorLabel.Text = "Tarifs - Sync Failed";
            //            successLabel.Text = "";
            //        }
            //        break;

            //    case "Produits":
            //        if (Stock.SyncStock())
            //        {
            //            successLabel.Text = "Produits succesfully sync";
            //        }
            //        else
            //        {
            //            errorLabel.Text = "Produits - Sync Failed";
            //            successLabel.Text = "";
            //        }
            //        break;

            //    //case "Baremes":
            //    //    if(Baremes.syncBaremes())
            //    //    {
            //    //        successLabel.Text = "Baremes successfully sync";
            //    //    } else
            //    //    {
            //    //        errorLabel.Text = "Baremes - Sync Failed";
            //    //        successLabel.Text = "";
            //    //    }
            //    //    break;
            //    default:
            //        successLabel.Text = "";
            //        errorLabel.Text = "Please chose a table to sync";
            //        break;
            //}

            //TODO DISPO
            //if (Dispo.SyncDispo())
            //{
            //    Console.WriteLine("Update des stocks - OK");
            //}
            //else
            //{
            //    Console.WriteLine("Une erreur est survenue durant l'update des stocks");
            //}
            RFSSyncer rfs = new RFSSyncer();
            rfs.Sync();           

            Console.WriteLine("Syncing SCat...");
            SCatSyncer scat = new SCatSyncer();
            if (scat.Sync())
            {
                Console.WriteLine("SCat Sync Ok");
            }
            else
            {
                Console.WriteLine("SCat - Sync Failed");
            }
            Console.WriteLine("Syncing Tarifs...");
            //if (Tarifs.SyncTarifs())
            //{
            //    Console.WriteLine("Tarifs Sync Ok");
            //}
            //else
            //{
            //    Console.WriteLine("Tarifs - Sync Failed");
            //}

            Console.WriteLine("Syncing Products...");
            StockSyncer s = new StockSyncer();
            if (s.Sync())
            {
                Console.WriteLine("Product Sync Ok");
                successLabel.Text = "Sync OK";
            }
            else
            {
                Console.WriteLine("Product - Sync Failed");

            }

            Console.WriteLine("Syncing Additional Infos...");
            //Additional infos : promos folders, catalogues, ...


            Cursor.Current = Cursors.Default;
            //Program.createLog();
        }

        private void syncBtn2_Click(object sender, EventArgs e)
        {
            //string syncChoice = syncComboBox2.Text;
            Cursor.Current = Cursors.WaitCursor;
            successLabel.Text = "Syncing...";
            errorLabel.Text = "";

            //Console.WriteLine("Syncing Clients Mercator...");
            //Clients Mercator -> Nop
            //if (Client.syncClientMercatorNop())
            //{
            //    Console.WriteLine("Clients Sync Mercator->Nop Ok");
            //}
            //else
            //{
            //    Console.WriteLine("Clients Sync Mercator -> Nop Sync Failed");
            //}

            //Console.WriteLine("Syncing Commandes...");
            //Commandes
            //if (Commande.syncCommande())
            //{
            //    Console.WriteLine("Commandes Sync Ok");
            //}
            //else
            //{
            //    Console.WriteLine("Commandes - Sync Failed");
            //}           

            Cursor.Current = Cursors.Default;
            Program.createLog();
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
