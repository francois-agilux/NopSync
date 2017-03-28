IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'WEB_API_CREDENTIALS' AND xtype = 'U') 
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
                                                ) ON [PRIMARY]