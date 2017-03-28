//using System;
//using System.Collections.Generic;
//using System.Text;
//using Microsoft.Deployment.WindowsInstaller;

//namespace DatabaseSettingsCustomAction
//{
//    public class CustomActions
//    {
//        #region Public Methods and Operators

//        [CustomAction]
//        public static ActionResult EnumerateSqlServers(Session session)
//        {
//            if (null == session)
//            {
//                throw new ArgumentNullException("session");
//            }

//            session.Log("EnumerateSQLServers: Begin");

//            // Check if running with admin rights and if not, log a message to
//            // let them know why it's failing.
//            if (false == HasAdminRights())
//            {
//                session.Log("EnumerateSQLServers: " + "ATTEMPTING TO RUN WITHOUT ADMIN RIGHTS");
//                return ActionResult.Failure;
//            }

//            ActionResult result;

//            DataTable dt = SmoApplication.EnumAvailableSqlServers(false);
//            DataRow[] rows = dt.Select(string.Empty, "IsLocal desc, Name asc");
//            result = EnumSqlServersIntoComboBox(session, rows);

//            session.Log("EnumerateSQLServers: End");
//            return result;
//        }

//        //[CustomAction]
//        //public static ActionResult VerifySqlConnection(Session session)
//        //{
//        //    try
//        //    {
//        //        //Debugger.Break();

//        //        session.Log("VerifySqlConnection: Begin");

//        //        var builder = new SqlConnectionStringBuilder
//        //        {
//        //            DataSource = session["DATABASE_SERVER"],
//        //            InitialCatalog = "master",
//        //            ConnectTimeout = 5
//        //        };

//        //        if (session["DATABASE_LOGON_TYPE"] != "DatabaseIntegratedAuth")
//        //        {
//        //            builder.UserID = session["DATABASE_USERNAME"];
//        //            builder.Password = session["DATABASE_PASSWORD"];
//        //        }
//        //        else
//        //        {
//        //            builder.IntegratedSecurity = true;
//        //        }

//        //        using (var connection = new SqlConnection(builder.ConnectionString))
//        //        {
//        //            if (connection.CheckConnection(session))
//        //            {
//        //                session["ODBC_CONNECTION_ESTABLISHED"] = "1";
//        //            }
//        //            else
//        //            {
//        //                session["ODBC_CONNECTION_ESTABLISHED"] = string.Empty;
//        //            }
//        //        }

//        //        session.Log("VerifySqlConnection: End");
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        session.Log("VerifySqlConnection: exception: {0}", ex.Message);
//        //        throw;
//        //    }

//        //    return ActionResult.Success;
//        //}

//        #endregion

//        #region Methods

//        private static ActionResult EnumSqlServersIntoComboBox(Session session, IEnumerable<DataRow> rows)
//        {
//            try
//            {
//                //Debugger.Break();

//                session.Log("EnumSQLServers: Begin");

//                View view = session.Database.OpenView("DELETE FROM ComboBox WHERE ComboBox.Property='DATABASE_SERVER'");
//                view.Execute();

//                view = session.Database.OpenView("SELECT * FROM ComboBox");
//                view.Execute();

//                Int32 index = 1;
//                session.Log("EnumSQLServers: Enumerating SQL servers");
//                foreach (DataRow row in rows)
//                {
//                    String serverName = row["Name"].ToString();

//                    // Create a record for this web site. All I care about is
//                    // the name so use it for fields three and four.
//                    session.Log("EnumSQLServers: Processing SQL server: {0}", serverName);

//                    Record record = session.Database.CreateRecord(4);
//                    record.SetString(1, "DATABASE_SERVER");
//                    record.SetInteger(2, index);
//                    record.SetString(3, serverName);
//                    record.SetString(4, serverName);

//                    session.Log("EnumSQLServers: Adding record");
//                    view.Modify(ViewModifyMode.InsertTemporary, record);
//                    index++;
//                }

//                view.Close();

//                session.Log("EnumSQLServers: End");
//            }
//            catch (Exception ex)
//            {
//                session.Log("EnumSQLServers: exception: {0}", ex.Message);
//                throw;
//            }

//            return ActionResult.Success;
//        }

//        private static bool HasAdminRights()
//        {
//            WindowsIdentity identity = WindowsIdentity.GetCurrent();
//            var principal = new WindowsPrincipal(identity);
//            return principal.IsInRole(WindowsBuiltInRole.Administrator);
//        }

//        private static bool CheckConnection(this SqlConnection connection, Session session)
//        {
//            try
//            {
//                if (connection == null)
//                {
//                    return false;
//                }

//                connection.Open();
//                var canOpen = connection.State == ConnectionState.Open;
//                connection.Close();

//                return canOpen;
//            }
//            catch (SqlException ex)
//            {
//                session["ODBC_ERROR"] = ex.Message;
//                return false;
//            }
//        }

//        #endregion
//    }
//}
