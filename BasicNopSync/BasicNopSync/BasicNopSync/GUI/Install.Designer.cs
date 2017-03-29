namespace BasicNopSync.GUI
{
    partial class Install
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Install));
            this.errorLabel = new System.Windows.Forms.Label();
            this.successLabel = new System.Windows.Forms.Label();
            this.lblClientId = new System.Windows.Forms.Label();
            this.lblClientSecret = new System.Windows.Forms.Label();
            this.redirectUrl = new System.Windows.Forms.Label();
            this.txtPublicToken = new System.Windows.Forms.TextBox();
            this.txtSecretToken = new System.Windows.Forms.TextBox();
            this.txtStoreAddress = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.txtUser = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtRepMercator = new System.Windows.Forms.TextBox();
            this.lblRepMercator = new System.Windows.Forms.Label();
            this.lblJournal = new System.Windows.Forms.Label();
            this.txtJournal = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.boxGenericArticle = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // errorLabel
            // 
            this.errorLabel.AutoSize = true;
            this.errorLabel.ForeColor = System.Drawing.Color.Red;
            this.errorLabel.Location = new System.Drawing.Point(10, 37);
            this.errorLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.errorLabel.Name = "errorLabel";
            this.errorLabel.Size = new System.Drawing.Size(0, 13);
            this.errorLabel.TabIndex = 2;
            // 
            // successLabel
            // 
            this.successLabel.AutoSize = true;
            this.successLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.successLabel.Location = new System.Drawing.Point(10, 37);
            this.successLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.successLabel.Name = "successLabel";
            this.successLabel.Size = new System.Drawing.Size(0, 13);
            this.successLabel.TabIndex = 3;
            // 
            // lblClientId
            // 
            this.lblClientId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblClientId.AutoSize = true;
            this.lblClientId.Location = new System.Drawing.Point(8, 215);
            this.lblClientId.Name = "lblClientId";
            this.lblClientId.Size = new System.Drawing.Size(68, 13);
            this.lblClientId.TabIndex = 6;
            this.lblClientId.Text = "Clé publique:";
            // 
            // lblClientSecret
            // 
            this.lblClientSecret.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblClientSecret.AutoSize = true;
            this.lblClientSecret.Location = new System.Drawing.Point(8, 254);
            this.lblClientSecret.Name = "lblClientSecret";
            this.lblClientSecret.Size = new System.Drawing.Size(63, 13);
            this.lblClientSecret.TabIndex = 7;
            this.lblClientSecret.Text = "Clé secrète:";
            // 
            // redirectUrl
            // 
            this.redirectUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.redirectUrl.AutoSize = true;
            this.redirectUrl.Location = new System.Drawing.Point(8, 293);
            this.redirectUrl.Name = "redirectUrl";
            this.redirectUrl.Size = new System.Drawing.Size(57, 13);
            this.redirectUrl.TabIndex = 8;
            this.redirectUrl.Text = "Url du site:";
            // 
            // txtPublicToken
            // 
            this.txtPublicToken.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPublicToken.Location = new System.Drawing.Point(11, 231);
            this.txtPublicToken.Name = "txtPublicToken";
            this.txtPublicToken.Size = new System.Drawing.Size(363, 20);
            this.txtPublicToken.TabIndex = 9;
            // 
            // txtSecretToken
            // 
            this.txtSecretToken.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSecretToken.Location = new System.Drawing.Point(11, 270);
            this.txtSecretToken.Name = "txtSecretToken";
            this.txtSecretToken.Size = new System.Drawing.Size(363, 20);
            this.txtSecretToken.TabIndex = 10;
            // 
            // txtStoreAddress
            // 
            this.txtStoreAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtStoreAddress.Location = new System.Drawing.Point(11, 309);
            this.txtStoreAddress.Name = "txtStoreAddress";
            this.txtStoreAddress.Size = new System.Drawing.Size(365, 20);
            this.txtStoreAddress.TabIndex = 11;
            // 
            // btnCancel
            // 
            this.btnCancel.AllowDrop = true;
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.Location = new System.Drawing.Point(11, 342);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(65, 23);
            this.btnCancel.TabIndex = 15;
            this.btnCancel.Text = "Annuler";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // txtUser
            // 
            this.txtUser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtUser.Location = new System.Drawing.Point(11, 192);
            this.txtUser.Name = "txtUser";
            this.txtUser.Size = new System.Drawing.Size(363, 20);
            this.txtUser.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 176);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Utilisateur:";
            // 
            // btnNext
            // 
            this.btnNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNext.Location = new System.Drawing.Point(302, 342);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(75, 23);
            this.btnNext.TabIndex = 12;
            this.btnNext.Text = "Installer";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(309, 77);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(65, 23);
            this.btnBrowse.TabIndex = 3;
            this.btnBrowse.Text = "Parcourir";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtRepMercator
            // 
            this.txtRepMercator.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRepMercator.Location = new System.Drawing.Point(11, 77);
            this.txtRepMercator.Name = "txtRepMercator";
            this.txtRepMercator.Size = new System.Drawing.Size(289, 20);
            this.txtRepMercator.TabIndex = 2;
            // 
            // lblRepMercator
            // 
            this.lblRepMercator.AutoSize = true;
            this.lblRepMercator.Location = new System.Drawing.Point(8, 61);
            this.lblRepMercator.Name = "lblRepMercator";
            this.lblRepMercator.Size = new System.Drawing.Size(101, 13);
            this.lblRepMercator.TabIndex = 18;
            this.lblRepMercator.Text = "Répertoire Mercator";
            // 
            // lblJournal
            // 
            this.lblJournal.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblJournal.AutoSize = true;
            this.lblJournal.Location = new System.Drawing.Point(8, 22);
            this.lblJournal.Name = "lblJournal";
            this.lblJournal.Size = new System.Drawing.Size(167, 13);
            this.lblJournal.TabIndex = 17;
            this.lblJournal.Text = "Journal de commandes Mercator :";
            // 
            // txtJournal
            // 
            this.txtJournal.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtJournal.Location = new System.Drawing.Point(13, 38);
            this.txtJournal.Name = "txtJournal";
            this.txtJournal.Size = new System.Drawing.Size(360, 20);
            this.txtJournal.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 13);
            this.label2.TabIndex = 21;
            this.label2.Text = "MERCATOR";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 151);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(143, 13);
            this.label3.TabIndex = 22;
            this.label3.Text = "NOPCOMMERCE : WEBAPI";
            // 
            // boxGenericArticle
            // 
            this.boxGenericArticle.AutoSize = true;
            this.boxGenericArticle.Location = new System.Drawing.Point(11, 114);
            this.boxGenericArticle.Name = "boxGenericArticle";
            this.boxGenericArticle.Size = new System.Drawing.Size(182, 17);
            this.boxGenericArticle.TabIndex = 23;
            this.boxGenericArticle.Text = "Utilisation des articles génériques";
            this.boxGenericArticle.UseVisualStyleBackColor = true;
            // 
            // Install
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(384, 376);
            this.Controls.Add(this.boxGenericArticle);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtRepMercator);
            this.Controls.Add(this.lblRepMercator);
            this.Controls.Add(this.lblJournal);
            this.Controls.Add(this.txtJournal);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.txtUser);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.txtStoreAddress);
            this.Controls.Add(this.txtSecretToken);
            this.Controls.Add(this.txtPublicToken);
            this.Controls.Add(this.redirectUrl);
            this.Controls.Add(this.lblClientSecret);
            this.Controls.Add(this.lblClientId);
            this.Controls.Add(this.successLabel);
            this.Controls.Add(this.errorLabel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Install";
            this.Text = "NopSync";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }



        #endregion
        private System.Windows.Forms.Label errorLabel;
        private System.Windows.Forms.Label successLabel;
        private System.Windows.Forms.Label lblClientId;
        private System.Windows.Forms.Label lblClientSecret;
        private System.Windows.Forms.Label redirectUrl;
        private System.Windows.Forms.TextBox txtPublicToken;
        private System.Windows.Forms.TextBox txtSecretToken;
        private System.Windows.Forms.TextBox txtStoreAddress;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox txtUser;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtRepMercator;
        private System.Windows.Forms.Label lblRepMercator;
        private System.Windows.Forms.Label lblJournal;
        private System.Windows.Forms.TextBox txtJournal;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.CheckBox boxGenericArticle;
    }
}

