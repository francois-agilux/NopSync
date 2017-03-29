using BasicNopSync.Database;
using MercatorORM;
using BasicNopSync.Model.Mercator;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BasicNopSync
{
    class FirstLaunch
    {   

        private static ProgressBar pb;
        //private static string connectionString = ConfigurationManager.ConnectionStrings["Mercator"].ConnectionString;

        //Add new tables
        private static string addressTableQuery = @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ADRESSE_WEB' AND xtype='U')
                               CREATE TABLE [dbo].[ADRESSE_WEB](
                               [ID_ADDRESS] [int] NOT NULL CONSTRAINT [DF_ADRESSE_WEB_ID_ADDRESS]  DEFAULT ((0)),
                               [ID_COUNTRY] [int] NOT NULL CONSTRAINT [DF_Table_1_ID_ADDRESS3]  DEFAULT ((0)),
                               [ID_STATE] [int] NOT NULL CONSTRAINT [DF_Table_1_ID_ADDRESS2]  DEFAULT ((0)),
                               [ID_CUSTOMER] [int] NOT NULL CONSTRAINT [DF_Table_1_ID_ADDRESS1]  DEFAULT ((0)),
                               [COMPANY] [varchar](64) NOT NULL CONSTRAINT [DF_ADRESSE_WEB_COMPANY]  DEFAULT (''),
                               [LASTNAME] [varchar](32) NOT NULL CONSTRAINT [DF_Table_1_COMPANY8]  DEFAULT (''),
                               [FIRSTNAME] [varchar](32) NOT NULL CONSTRAINT [DF_Table_1_COMPANY7]  DEFAULT (''),
                               [ADDRESS1] [varchar](128) NOT NULL CONSTRAINT [DF_Table_1_COMPANY6]  DEFAULT (''),
                               [ADDRESS2] [varchar](128) NOT NULL CONSTRAINT [DF_Table_1_COMPANY5]  DEFAULT (''),
                               [POSTCODE] [varchar](12) NOT NULL CONSTRAINT [DF_Table_1_COMPANY4]  DEFAULT (''),
                               [CITY] [varchar](64) NOT NULL CONSTRAINT [DF_Table_1_COMPANY3]  DEFAULT (''),
                               [OTHER] [varchar](max) NOT NULL CONSTRAINT [DF_Table_1_COMPANY2]  DEFAULT (''),
                               [PHONE] [varchar](32) NOT NULL CONSTRAINT [DF_Table_1_COMPANY1]  DEFAULT (''),
                               [PHONE_MOBILE] [varchar](32) NOT NULL CONSTRAINT [DF_Table_1_PHONE1]  DEFAULT (''),
                                CONSTRAINT [PK_ADRESSE_WEB] PRIMARY KEY CLUSTERED 
                                (
                                       [ID_ADDRESS] ASC
                                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                                ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
                                ";

        private static bool paysSuccess = false;

        private static string paysTableQuery = @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PAYS' AND xtype='U')
                                CREATE TABLE [dbo].[PAYS](
                                [ISO] [varchar](3) NOT NULL CONSTRAINT [DF_PAYS_ISO]  DEFAULT (''),   
                                [NOM_F] [varchar](64) NOT NULL DEFAULT (''),
                                [NOM_E] [varchar](64) NOT NULL DEFAULT (''),                     
                                CONSTRAINT [PK_PAYS] PRIMARY KEY CLUSTERED 
                                (
                                [ISO] ASC
                                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                                ) ON [PRIMARY]
                                ";

       
        private static string webApiTableQuery = @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'WEB_API_CREDENTIALS' AND xtype = 'U') 
                                                   CREATE TABLE [dbo].[WEB_API_CREDENTIALS](
	                                                [ID] [int] IDENTITY(1,1) NOT NULL,
	                                                [ClientName] [varchar](max) NOT NULL,
	                                                [SecretToken] [varchar](max) NOT NULL,
	                                                [PublicToken] [varchar](max) NOT NULL,
                                                    [StoreAddress] [varchar](max) NOT NULL	                                                
                                                 CONSTRAINT [PK_WEB_API_CREDENTIALS] PRIMARY KEY CLUSTERED 
                                                (
	                                                [ID] ASC
                                                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                                                ) ON [PRIMARY]";


        //Add columns
        private static string addIntColumnToTable = @"IF COL_LENGTH('{0}', '{1}') IS NULL BEGIN ALTER TABLE [dbo].[{0}] ADD {1} int NOT NULL DEFAULT ((0)) END";
        private static string addBitColumnToTable = @"IF COL_LENGTH('{0}', '{1}') IS NULL BEGIN ALTER TABLE [dbo].[{0}] ADD {1} bit NOT NULL DEFAULT ((0)) END";
        private static string addVarcharColumnToTable = @"IF COL_LENGTH('{0}', '{1}') IS NULL BEGIN ALTER TABLE [dbo].[{0}] ADD {1} varchar({2}) NOT NULL DEFAULT (('')) END";
       

        //Insert values

        private static string insertIntoOptions = @"INSERT INTO [dbo].[OPTIONS] (TYPE,VALEUR,STEM,LIBELLE_F) values ('NOP_MDFTAG',0,'PARAMS','Dernier modiftag synchronisé')";
        private static string insertRepMercIntoOptions = @"INSERT INTO [dbo].[OPTIONS] (TYPE,VALEUR,STEM,LIBELLE_F) values ('NOP_REP_M',0,'PARAMS','Repertoire mercator')";
        private static string insertJournalIntoOptions = @"INSERT INTO [dbo].[OPTIONS] (TYPE,VALEUR,STEM,LIBELLE_F) values ('NOP_JOURN',0,'PARAMS','Journal choisi pour commandes nopSync')";
		private static string insertMargeIntoOptions = @"INSERT INTO [dbo].[OPTIONS] (TYPE,VALEUR,STEM,LIBELLE_F) values ('NOP_STCK_M',80,'PARAMS','Marge de stock à afficher sur le web (en %)')";
        private static string insertCatStck1IntoOptions = @"INSERT INTO [dbo].[OPTIONS] (TYPE,VALEUR,STEM,LIBELLE_F) values ('NOP_CSTCK1','CAT_STCK_NAME_1','PARAMS','Nom de la catégorie 1 sur nopCommerce')";
        private static string insertCatStck2IntoOptions = @"INSERT INTO [dbo].[OPTIONS] (TYPE,VALEUR,STEM,LIBELLE_F) values ('NOP_CSTCK2','CAT_STCK_NAME_2','PARAMS','Nom de la catégorie 2 sur nopCommerce')";
        private static string insertCatStck3IntoOptions = @"INSERT INTO [dbo].[OPTIONS] (TYPE,VALEUR,STEM,LIBELLE_F) values ('NOP_CSTCK3','CAT_STCK_NAME_3','PARAMS','Nom de la catégorie 3 sur nopCommerce')";
        private static string insertDefaultCountryIntoOptions = @"INSERT INTO [dbo].[OPTIONS] (TYPE,VALEUR,STEM,LIBELLE_F) values ('NOP_PAYS','Belgium','PARAMS','Pays par défaut client nopCommerce')";
        private static string insertIDLivIntoOptions = @"INSERT INTO [dbo].[OPTIONS] (TYPE,VALEUR,STEM,LIBELLE_F) values ('NOP_LIV_ID','0','PARAMS','Article frais de livraison')";
        private static string insertDefaultTarifIntoOptions = @"INSERT INTO [dbo].[OPTIONS] (TYPE,VALEUR,STEM,LIBELLE_F) values ('NOP_TARIF','1','PARAMS','Tarif par défaut des clients nopCommerce')";
        private static string insertUseOfGenericArticleOptions = @"INSERT INTO[dbo].[OPTIONS] (TYPE,VALEUR,STEM,LIBELLE_F) values('NOP_GEN_A','0','PARAMS','Indique si le site utilise les articles génériques')";
        #region script insert Pays
        private static string insertPays = @"
                    insert into PAYS (ISO,NOM_F) values ('AD','Andorre')
                    insert into PAYS (ISO,NOM_F) values ('AE','Émirats arabes unis')
                    insert into PAYS (ISO,NOM_F) values ('AF','Afghanistan')
                    insert into PAYS (ISO,NOM_F) values ('AG','Antigua-et-Barbuda')
                    insert into PAYS (ISO,NOM_F) values ('AI','Anguilla')
                    insert into PAYS (ISO,NOM_F) values ('AL','Albanie')
                    insert into PAYS (ISO,NOM_F) values ('AM','Arménie')
                    insert into PAYS (ISO,NOM_F) values ('AO','Angola')
                    insert into PAYS (ISO,NOM_F) values ('AQ','Antarctique')
                    insert into PAYS (ISO,NOM_F) values ('AR','Argentine')
                    insert into PAYS (ISO,NOM_F) values ('AS','Samoa américaine')
                    insert into PAYS (ISO,NOM_F) values ('AT','Autriche')
                    insert into PAYS (ISO,NOM_F) values ('AU','Australie')
                    insert into PAYS (ISO,NOM_F) values ('AW','Aruba')
                    insert into PAYS (ISO,NOM_F) values ('AX','Iles d''Aland')
                    insert into PAYS (ISO,NOM_F) values ('AZ','Azerbaïdjan')
                    insert into PAYS (ISO,NOM_F) values ('BA','Bosnie-Herzégovine')
                    insert into PAYS (ISO,NOM_F) values ('BB','Barbade')
                    insert into PAYS (ISO,NOM_F) values ('BD','Bangladesh')
                    insert into PAYS (ISO,NOM_F) values ('BE','Belgique')
                    insert into PAYS (ISO,NOM_F) values ('BF','Burkina Faso')
                    insert into PAYS (ISO,NOM_F) values ('BG','Bulgarie')
                    insert into PAYS (ISO,NOM_F) values ('BH','Bahreïn')
                    insert into PAYS (ISO,NOM_F) values ('BI','Burundi')
                    insert into PAYS (ISO,NOM_F) values ('BJ','Bénin')
                    insert into PAYS (ISO,NOM_F) values ('BL','Saint-Barthélemy')
                    insert into PAYS (ISO,NOM_F) values ('BM','Bermudes')
                    insert into PAYS (ISO,NOM_F) values ('BN','Brunei Darussalam')
                    insert into PAYS (ISO,NOM_F) values ('BO','Bolivie')
                    insert into PAYS (ISO,NOM_F) values ('BQ','Pays-Bas caribéens')
                    insert into PAYS (ISO,NOM_F) values ('BR','Brésil')
                    insert into PAYS (ISO,NOM_F) values ('BS','Bahamas')
                    insert into PAYS (ISO,NOM_F) values ('BT','Bhoutan')
                    insert into PAYS (ISO,NOM_F) values ('BV','Ile Bouvet')
                    insert into PAYS (ISO,NOM_F) values ('BW','Botswana')
                    insert into PAYS (ISO,NOM_F) values ('BY','Bélarus')
                    insert into PAYS (ISO,NOM_F) values ('BZ','Belize')
                    insert into PAYS (ISO,NOM_F) values ('CA','Canada')
                    insert into PAYS (ISO,NOM_F) values ('CC','Iles Cocos (Keeling)')
                    insert into PAYS (ISO,NOM_F) values ('CD','Congo, République démocratique du')
                    insert into PAYS (ISO,NOM_F) values ('CF','République centrafricaine')
                    insert into PAYS (ISO,NOM_F) values ('CG','Congo')
                    insert into PAYS (ISO,NOM_F) values ('CH','Suisse')
                    insert into PAYS (ISO,NOM_F) values ('CI','Côte d''Ivoire')
                    insert into PAYS (ISO,NOM_F) values ('CK','Iles Cook')
                    insert into PAYS (ISO,NOM_F) values ('CL','Chili')
                    insert into PAYS (ISO,NOM_F) values ('CM','Cameroun')
                    insert into PAYS (ISO,NOM_F) values ('CN','Chine')
                    insert into PAYS (ISO,NOM_F) values ('CO','Colombie')
                    insert into PAYS (ISO,NOM_F) values ('CR','Costa Rica')
                    insert into PAYS (ISO,NOM_F) values ('CU','Cuba')
                    insert into PAYS (ISO,NOM_F) values ('CV','Cap-Vert')
                    insert into PAYS (ISO,NOM_F) values ('CW','Curaçao')
                    insert into PAYS (ISO,NOM_F) values ('CX','Ile Christmas')
                    insert into PAYS (ISO,NOM_F) values ('CY','Chypre')
                    insert into PAYS (ISO,NOM_F) values ('CZ','République tchèque')
                    insert into PAYS (ISO,NOM_F) values ('DE','Allemagne')
                    insert into PAYS (ISO,NOM_F) values ('DJ','Djibouti')
                    insert into PAYS (ISO,NOM_F) values ('DK','Danemark')
                    insert into PAYS (ISO,NOM_F) values ('DM','Dominique')
                    insert into PAYS (ISO,NOM_F) values ('DO','République dominicaine')
                    insert into PAYS (ISO,NOM_F) values ('DZ','Algérie')
                    insert into PAYS (ISO,NOM_F) values ('EC','Équateur')
                    insert into PAYS (ISO,NOM_F) values ('EE','Estonie')
                    insert into PAYS (ISO,NOM_F) values ('EG','Égypte')
                    insert into PAYS (ISO,NOM_F) values ('EH','Sahara Occidental')
                    insert into PAYS (ISO,NOM_F) values ('ER','Érythrée')
                    insert into PAYS (ISO,NOM_F) values ('ES','Espagne')
                    insert into PAYS (ISO,NOM_F) values ('ET','Éthiopie')
                    insert into PAYS (ISO,NOM_F) values ('FI','Finlande')
                    insert into PAYS (ISO,NOM_F) values ('FJ','Fidji')
                    insert into PAYS (ISO,NOM_F) values ('FK','Iles Malouines')
                    insert into PAYS (ISO,NOM_F) values ('FM','Micronésie, États fédérés de')
                    insert into PAYS (ISO,NOM_F) values ('FO','Iles Féroé')
                    insert into PAYS (ISO,NOM_F) values ('FR','France')
                    insert into PAYS (ISO,NOM_F) values ('GA','Gabon')
                    insert into PAYS (ISO,NOM_F) values ('GB','Royaume-Uni')
                    insert into PAYS (ISO,NOM_F) values ('GD','Grenade')
                    insert into PAYS (ISO,NOM_F) values ('GE','Géorgie')
                    insert into PAYS (ISO,NOM_F) values ('GF','Guyane française')
                    insert into PAYS (ISO,NOM_F) values ('GG','Guernesey')
                    insert into PAYS (ISO,NOM_F) values ('GH','Ghana')
                    insert into PAYS (ISO,NOM_F) values ('GI','Gibraltar')
                    insert into PAYS (ISO,NOM_F) values ('GL','Groenland')
                    insert into PAYS (ISO,NOM_F) values ('GM','Gambie')
                    insert into PAYS (ISO,NOM_F) values ('GN','Guinée')
                    insert into PAYS (ISO,NOM_F) values ('GP','Guadeloupe')
                    insert into PAYS (ISO,NOM_F) values ('GQ','Guinée équatoriale')
                    insert into PAYS (ISO,NOM_F) values ('GR','Grèce')
                    insert into PAYS (ISO,NOM_F) values ('GS','Géorgie du Sud et les Iles Sandwich du Sud')
                    insert into PAYS (ISO,NOM_F) values ('GT','Guatemala')
                    insert into PAYS (ISO,NOM_F) values ('GU','Guam')
                    insert into PAYS (ISO,NOM_F) values ('GW','Guinée-Bissau')
                    insert into PAYS (ISO,NOM_F) values ('GY','Guyana')
                    insert into PAYS (ISO,NOM_F) values ('HK','Hong Kong')
                    insert into PAYS (ISO,NOM_F) values ('HM','Iles Heard et McDonald')
                    insert into PAYS (ISO,NOM_F) values ('HN','Honduras')
                    insert into PAYS (ISO,NOM_F) values ('HR','Croatie')
                    insert into PAYS (ISO,NOM_F) values ('HT','Haïti')
                    insert into PAYS (ISO,NOM_F) values ('HU','Hongrie')
                    insert into PAYS (ISO,NOM_F) values ('ID','Indonésie')
                    insert into PAYS (ISO,NOM_F) values ('IE','Irlande')
                    insert into PAYS (ISO,NOM_F) values ('IL','Israël')
                    insert into PAYS (ISO,NOM_F) values ('IM','Ile de Man')
                    insert into PAYS (ISO,NOM_F) values ('IN','Inde')
                    insert into PAYS (ISO,NOM_F) values ('IO','Territoire britannique de l''océan Indien')
                    insert into PAYS (ISO,NOM_F) values ('IQ','Irak')
                    insert into PAYS (ISO,NOM_F) values ('IR','Iran')
                    insert into PAYS (ISO,NOM_F) values ('IS','Islande')
                    insert into PAYS (ISO,NOM_F) values ('IT','Italie')
                    insert into PAYS (ISO,NOM_F) values ('JE','Jersey')
                    insert into PAYS (ISO,NOM_F) values ('JM','Jamaïque')
                    insert into PAYS (ISO,NOM_F) values ('JO','Jordanie')
                    insert into PAYS (ISO,NOM_F) values ('JP','Japon')
                    insert into PAYS (ISO,NOM_F) values ('KE','Kenya')
                    insert into PAYS (ISO,NOM_F) values ('KG','Kirghizistan')
                    insert into PAYS (ISO,NOM_F) values ('KH','Cambodge')
                    insert into PAYS (ISO,NOM_F) values ('KI','Kiribati')
                    insert into PAYS (ISO,NOM_F) values ('KM','Comores')
                    insert into PAYS (ISO,NOM_F) values ('KN','Saint-Kitts-et-Nevis')
                    insert into PAYS (ISO,NOM_F) values ('KP','Corée du Nord')
                    insert into PAYS (ISO,NOM_F) values ('KR','Corée du Sud')
                    insert into PAYS (ISO,NOM_F) values ('KW','Koweït')
                    insert into PAYS (ISO,NOM_F) values ('KY','Iles Caïmans')
                    insert into PAYS (ISO,NOM_F) values ('KZ','Kazakhstan')
                    insert into PAYS (ISO,NOM_F) values ('LA','Laos')
                    insert into PAYS (ISO,NOM_F) values ('LB','Liban')
                    insert into PAYS (ISO,NOM_F) values ('LC','Sainte-Lucie')
                    insert into PAYS (ISO,NOM_F) values ('LI','Liechtenstein')
                    insert into PAYS (ISO,NOM_F) values ('LK','Sri Lanka')
                    insert into PAYS (ISO,NOM_F) values ('LR','Libéria')
                    insert into PAYS (ISO,NOM_F) values ('LS','Lesotho')
                    insert into PAYS (ISO,NOM_F) values ('LT','Lituanie')
                    insert into PAYS (ISO,NOM_F) values ('LU','Luxembourg')
                    insert into PAYS (ISO,NOM_F) values ('LV','Lettonie')
                    insert into PAYS (ISO,NOM_F) values ('LY','Libye')
                    insert into PAYS (ISO,NOM_F) values ('MA','Maroc')
                    insert into PAYS (ISO,NOM_F) values ('MC','Monaco')
                    insert into PAYS (ISO,NOM_F) values ('MD','Moldavie')
                    insert into PAYS (ISO,NOM_F) values ('ME','Monténégro')
                    insert into PAYS (ISO,NOM_F) values ('MF','Saint-Martin (France)')
                    insert into PAYS (ISO,NOM_F) values ('MG','Madagascar')
                    insert into PAYS (ISO,NOM_F) values ('MH','Iles Marshall')
                    insert into PAYS (ISO,NOM_F) values ('MK','Macédoine')
                    insert into PAYS (ISO,NOM_F) values ('ML','Mali')
                    insert into PAYS (ISO,NOM_F) values ('MM','Myanmar')
                    insert into PAYS (ISO,NOM_F) values ('MN','Mongolie')
                    insert into PAYS (ISO,NOM_F) values ('MO','Macao')
                    insert into PAYS (ISO,NOM_F) values ('MP','Mariannes du Nord')
                    insert into PAYS (ISO,NOM_F) values ('MQ','Martinique')
                    insert into PAYS (ISO,NOM_F) values ('MR','Mauritanie')
                    insert into PAYS (ISO,NOM_F) values ('MS','Montserrat')
                    insert into PAYS (ISO,NOM_F) values ('MT','Malte')
                    insert into PAYS (ISO,NOM_F) values ('MU','Maurice')
                    insert into PAYS (ISO,NOM_F) values ('MV','Maldives')
                    insert into PAYS (ISO,NOM_F) values ('MW','Malawi')
                    insert into PAYS (ISO,NOM_F) values ('MX','Mexique')
                    insert into PAYS (ISO,NOM_F) values ('MY','Malaisie')
                    insert into PAYS (ISO,NOM_F) values ('MZ','Mozambique')
                    insert into PAYS (ISO,NOM_F) values ('NA','Namibie')
                    insert into PAYS (ISO,NOM_F) values ('NC','Nouvelle-Calédonie')
                    insert into PAYS (ISO,NOM_F) values ('NE','Niger')
                    insert into PAYS (ISO,NOM_F) values ('NF','Ile Norfolk')
                    insert into PAYS (ISO,NOM_F) values ('NG','Nigeria')
                    insert into PAYS (ISO,NOM_F) values ('NI','Nicaragua')
                    insert into PAYS (ISO,NOM_F) values ('NL','Pays-Bas')
                    insert into PAYS (ISO,NOM_F) values ('NO','Norvège')
                    insert into PAYS (ISO,NOM_F) values ('NP','Népal')
                    insert into PAYS (ISO,NOM_F) values ('NR','Nauru')
                    insert into PAYS (ISO,NOM_F) values ('NU','Niue')
                    insert into PAYS (ISO,NOM_F) values ('NZ','Nouvelle-Zélande')
                    insert into PAYS (ISO,NOM_F) values ('OM','Oman')
                    insert into PAYS (ISO,NOM_F) values ('PA','Panama')
                    insert into PAYS (ISO,NOM_F) values ('PE','Pérou')
                    insert into PAYS (ISO,NOM_F) values ('PF','Polynésie française')
                    insert into PAYS (ISO,NOM_F) values ('PG','Papouasie-Nouvelle-Guinée')
                    insert into PAYS (ISO,NOM_F) values ('PH','Philippines')
                    insert into PAYS (ISO,NOM_F) values ('PK','Pakistan')
                    insert into PAYS (ISO,NOM_F) values ('PL','Pologne')
                    insert into PAYS (ISO,NOM_F) values ('PM','Saint-Pierre-et-Miquelon')
                    insert into PAYS (ISO,NOM_F) values ('PN','Pitcairn')
                    insert into PAYS (ISO,NOM_F) values ('PR','Puerto Rico')
                    insert into PAYS (ISO,NOM_F) values ('PS','Palestine')
                    insert into PAYS (ISO,NOM_F) values ('PT','Portugal')
                    insert into PAYS (ISO,NOM_F) values ('PW','Palau')
                    insert into PAYS (ISO,NOM_F) values ('PY','Paraguay')
                    insert into PAYS (ISO,NOM_F) values ('QA','Qatar')
                    insert into PAYS (ISO,NOM_F) values ('RE','Réunion')
                    insert into PAYS (ISO,NOM_F) values ('RO','Roumanie')
                    insert into PAYS (ISO,NOM_F) values ('RS','Serbie')
                    insert into PAYS (ISO,NOM_F) values ('RU','Russie')
                    insert into PAYS (ISO,NOM_F) values ('RW','Rwanda')
                    insert into PAYS (ISO,NOM_F) values ('SA','Arabie saoudite')
                    insert into PAYS (ISO,NOM_F) values ('SB','Iles Salomon')
                    insert into PAYS (ISO,NOM_F) values ('SC','Seychelles')
                    insert into PAYS (ISO,NOM_F) values ('SD','Soudan')
                    insert into PAYS (ISO,NOM_F) values ('SE','Suède')
                    insert into PAYS (ISO,NOM_F) values ('SG','Singapour')
                    insert into PAYS (ISO,NOM_F) values ('SH','Sainte-Hélène')
                    insert into PAYS (ISO,NOM_F) values ('SI','Slovénie')
                    insert into PAYS (ISO,NOM_F) values ('SJ','Svalbard et Ile de Jan Mayen')
                    insert into PAYS (ISO,NOM_F) values ('SK','Slovaquie')
                    insert into PAYS (ISO,NOM_F) values ('SL','Sierra Leone')
                    insert into PAYS (ISO,NOM_F) values ('SM','Saint-Marin')
                    insert into PAYS (ISO,NOM_F) values ('SN','Sénégal')
                    insert into PAYS (ISO,NOM_F) values ('SO','Somalie')
                    insert into PAYS (ISO,NOM_F) values ('SR','Suriname')
                    insert into PAYS (ISO,NOM_F) values ('SS','Soudan du Sud')
                    insert into PAYS (ISO,NOM_F) values ('ST','Sao Tomé-et-Principe')
                    insert into PAYS (ISO,NOM_F) values ('SV','El Salvador')
                    insert into PAYS (ISO,NOM_F) values ('SX','Saint-Martin (Pays-Bas)')
                    insert into PAYS (ISO,NOM_F) values ('SY','Syrie')
                    insert into PAYS (ISO,NOM_F) values ('SZ','Swaziland')
                    insert into PAYS (ISO,NOM_F) values ('TC','Iles Turks et Caicos')
                    insert into PAYS (ISO,NOM_F) values ('TD','Tchad')
                    insert into PAYS (ISO,NOM_F) values ('TF','Terres australes françaises')
                    insert into PAYS (ISO,NOM_F) values ('TG','Togo')
                    insert into PAYS (ISO,NOM_F) values ('TH','Thaïlande')
                    insert into PAYS (ISO,NOM_F) values ('TJ','Tadjikistan')
                    insert into PAYS (ISO,NOM_F) values ('TK','Tokelau')
                    insert into PAYS (ISO,NOM_F) values ('TL','Timor-Leste')
                    insert into PAYS (ISO,NOM_F) values ('TM','Turkménistan')
                    insert into PAYS (ISO,NOM_F) values ('TN','Tunisie')
                    insert into PAYS (ISO,NOM_F) values ('TO','Tonga')
                    insert into PAYS (ISO,NOM_F) values ('TR','Turquie')
                    insert into PAYS (ISO,NOM_F) values ('TT','Trinité-et-Tobago')
                    insert into PAYS (ISO,NOM_F) values ('TV','Tuvalu')
                    insert into PAYS (ISO,NOM_F) values ('TW','Taïwan')
                    insert into PAYS (ISO,NOM_F) values ('TZ','Tanzanie')
                    insert into PAYS (ISO,NOM_F) values ('UA','Ukraine')
                    insert into PAYS (ISO,NOM_F) values ('UG','Ouganda')
                    insert into PAYS (ISO,NOM_F) values ('UM','Iles mineures éloignées des États-Unis')
                    insert into PAYS (ISO,NOM_F) values ('US','États-Unis')
                    insert into PAYS (ISO,NOM_F) values ('UY','Uruguay')
                    insert into PAYS (ISO,NOM_F) values ('UZ','Ouzbékistan')
                    insert into PAYS (ISO,NOM_F) values ('VA','Vatican')
                    insert into PAYS (ISO,NOM_F) values ('VC','Saint-Vincent-et-les-Grenadines')
                    insert into PAYS (ISO,NOM_F) values ('VE','Venezuela')
                    insert into PAYS (ISO,NOM_F) values ('VG','Iles Vierges britanniques')
                    insert into PAYS (ISO,NOM_F) values ('VI','Iles Vierges américaines')
                    insert into PAYS (ISO,NOM_F) values ('VN','Vietnam')
                    insert into PAYS (ISO,NOM_F) values ('VU','Vanuatu')
                    insert into PAYS (ISO,NOM_F) values ('WF','Iles Wallis-et-Futuna')
                    insert into PAYS (ISO,NOM_F) values ('WS','Samoa')
                    insert into PAYS (ISO,NOM_F) values ('YE','Yémen')
                    insert into PAYS (ISO,NOM_F) values ('YT','Mayotte')
                    insert into PAYS (ISO,NOM_F) values ('ZA','Afrique du Sud')
                    insert into PAYS (ISO,NOM_F) values ('ZM','Zambie')
                    insert into PAYS (ISO,NOM_F) values ('ZW','Zimbabwe')
                    ";
        #endregion

        public static bool Initiate(ProgressBar progressBar, Label progressText, InstallerDatas install)
        {

            try
            {
                InitiatePB(progressBar);

                //Tables
                if (!DatabaseManager.CheckTableExistence("ADRESSE_WEB"))
                {
                    Program.log("Inserting table \"ADRESSE_WEB\"");
                    ExecuteQuery(addressTableQuery);
                }

                if (!DatabaseManager.CheckTableExistence("PAYS"))
                {
                    Program.log("Inserting table \"PAYS\"");
					UdpateLabelText(progressText, "Inserting table \"PAYS\"");
                    ExecuteQuery(paysTableQuery);                        
                }
				else
                {
                    pb.PerformStep();
                }

                if (!DatabaseManager.CheckTableExistence("WEB_API_CREDENTIALS"))
                {
                    Program.log("Inserting table \"WEB_API_CREDENTIALS\"");
					UdpateLabelText(progressText, "Inserting table \"WEB_API_CREDENTIALS\"");
                    ExecuteQuery(webApiTableQuery);                    
                }
				else
                {
                    pb.PerformStep();
                }

                //Columns
                Program.log("Ajout de colonnes");				
                UdpateLabelText(progressText, "Ajout de colonnes: Table PIEDS_V");
                ExecuteQuery(String.Format(addIntColumnToTable, "PIEDS_V", "ID_WEB"), timeout:99999);
                UdpateLabelText(progressText,"Ajout de colonnes: Table STOCK");
                ExecuteQuery(String.Format(addIntColumnToTable, "STOCK", "S_MODIFTAG"), timeout: 99999);
                ExecuteQuery(String.Format(addBitColumnToTable, "STOCK", "S_WEB"), timeout: 99999);                				
                UdpateLabelText(progressText, "Ajout de colonnes: Table CLI");
                ExecuteQuery(String.Format(addBitColumnToTable, "CLI", "C_FROM_WEB"), timeout: 99999);
                ExecuteQuery(String.Format(addIntColumnToTable, "CLI", "C_ID_WEB"), timeout: 99999);
				UdpateLabelText(progressText, "Ajout de colonnes: Table CAT_STCK");
                ExecuteQuery(String.Format(addIntColumnToTable, "CAT_STCK", "ID_WEB"));

                //Insert
                Program.log("Ajout de données");                
				UdpateLabelText(progressText,"Ajout des options");
                if (!ExecuteQuery(insertIntoOptions, true))
                    Program.log("Inserting mdftag options failed");                
                if (!ExecuteQuery(insertRepMercIntoOptions, true))
                    Program.log("Inserting rep_merc options failed");
                if (!ExecuteQuery(insertJournalIntoOptions, true))
                    Program.log("Inserting journal option failed");
				if (!ExecuteQuery(insertMargeIntoOptions, true))
                    Program.log("Inserting stock marge option failed");
                if (!ExecuteQuery(insertCatStck1IntoOptions, true))
                    Program.log("Inserting cat stck 1 option failed");
                if (!ExecuteQuery(insertCatStck2IntoOptions, true))
                    Program.log("Inserting cat stck 2 option failed");
                if (!ExecuteQuery(insertCatStck3IntoOptions, true))
                    Program.log("Inserting cat stck 3 option failed");
                if (!ExecuteQuery(insertDefaultCountryIntoOptions, true))
                    Program.log("Inserting default country option failed");
                if (!ExecuteQuery(insertDefaultTarifIntoOptions, true))
                    Program.log("Inserting default tarif option failed");
                if (!ExecuteQuery(insertIDLivIntoOptions, true))
                    Program.log("Inserting id liv option failed");
                if (!ExecuteQuery(insertUseOfGenericArticleOptions, install.UseGenericArticles))
                    Program.log("Inserting generic attribute option failed");
                if (!ExecuteQuery(insertPays, true))
                    Program.log("Inserting Pays values failed");                

                SaveAuthModelInDB(install);

                OptionsMercator repMerc = new OptionsMercator();
                OptionsMercator journal = new OptionsMercator();
                OptionsMercator genericArticle = new OptionsMercator();

                string repMercValue = repMerc.GetOptionValue("NOP_REP_M").ToString();
                string journalValue = journal.GetOptionValue("NOP_JOURN").ToString();
                string genericArticleValue = genericArticle.GetOptionValue("NOP_GEN_A")?.ToString()?.TrimEnd();

                if (install.RepMercator != repMercValue)
                    repMerc.SetOptionValue("NOP_REP_M", install.RepMercator);
                if (install.JournalMercator != journalValue)
                    journal.SetOptionValue("NOP_JOURN", install.JournalMercator);
                if ((install.UseGenericArticles == true) != (genericArticleValue == "1"))
                    journal.SetOptionValue("NOP_GEN_A", genericArticleValue == "1");

                progressBar.PerformStep();
                Program.log("Install terminée");
                UdpateLabelText(progressText, "Installation terminée");
                progressBar.Value = progressBar.Maximum;

                return true;
            }
            catch (Exception e)
            {
                Program.log(e.Message);
                Program.log(e.StackTrace);
                return false;
            }
        }

        private static void InitiatePB(ProgressBar progressBar)
        {
            
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Maximum = 23;                
            progressBar.Value = 0;
            progressBar.Step = 1;
            progressBar.Visible = true;                      

            pb = progressBar;
        }     

        private static void UdpateLabelText(Label lbl, string text)
        {
            lbl.Text = text;
            lbl.Refresh();
        }

        private static bool ExecuteQuery(string query, bool isInsert = false, int timeout = 0)
        {
            int result = -1;

            try
            {
                
                DBContextFactory.DBContext.BeginTransaction();
				SqlCommand command = new SqlCommand(query);

                if (timeout > 0)
                    command.CommandTimeout = timeout;

                result = DBContextFactory.DBContext.NonQuery(command);

                if (result < 0 && isInsert)
                    DBContextFactory.DBContext.RollbackTransaction();
                else
                    DBContextFactory.DBContext.CommitTransaction();
            }
            catch(Exception e)
            {
                Program.log(e.Message);
                Program.log(e.StackTrace);
                DBContextFactory.DBContext.RollbackTransaction();
            }
            pb.PerformStep();
            return result >= 0 && isInsert;
            
			
			}
private static void SaveAuthModelInDB(InstallerDatas install)
        {
            //string nonQuery = @"insert into web_api_credentials (client_id,client_secret,access_token,refresh_token,redirect_url,server_url) values
            //    ('{0}','{1}','{2}','{3}','{4}','{5}')";
            //SqlCommand command = new SqlCommand(String.Format(nonQuery, authParams.ClientId, authParams.ClientSecret,
            //    authParams.authModel.AccessToken, authParams.authModel.RefreshToken, authParams.RedirectUrl, authParams.ServerUrl));

            DBContextFactory.SetConnection(install.ConnectionString);

            string nonQuery = @"insert into web_api_credentials (clientName,publicToken,secretToken,storeaddress) values
                ('{0}','{1}','{2}','{3}')";
            SqlCommand command = new SqlCommand(String.Format(nonQuery, install.authParameters.ClientName, install.authParameters.PublicToken, install.authParameters.SecretToken, install.authParameters.StoreAddress));

            DBContextFactory.DBContext.BeginTransaction();

            int result = DBContextFactory.DBContext.NonQuery(command);

            if (result < 0)
                DBContextFactory.DBContext.RollbackTransaction();
            else
                DBContextFactory.DBContext.CommitTransaction();
        }
    }
}
