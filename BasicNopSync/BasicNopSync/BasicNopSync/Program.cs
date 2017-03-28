using BasicNopSync.Database;
using BasicNopSync.GUI;
using BasicNopSync.Model.Mercator;
using BasicNopSync.Syncers;
using BasicNopSync.Syncers.MercatorToNop;
using BasicNopSync.Syncers.NopToMercator;
using BasicNopSync.Utils;
using MercatorORM;
using Microsoft.Owin.Hosting;
using NopSync.Utils;
using BasicNopSync.Syncers.NopToMercator;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BasicNopSync
{

    static class Program
    {   
        private static string logToAdd;
        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main(String[] args)
        {

            //using (WebApp.Start<Startup>(url: baseAddress))
            //{
            //Console.WriteLine(ws.Patch("Category(1)", "{\"Description\":\"test\"}"));            
            //WebService.Post("UrlRecord", json);
            logToAdd = "";
            SyncDate = DateTime.Now;
            //Turn comma to dot (in doubles) - useful for json format
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;


            //AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            if (args.Length > 0)
                Process(args);
            else
            {
                DataSettings s = DatabaseIsInstalled();

                if (s == null)
                    RunGui(s);
                else
                    RunCli(args);
            }
            //{
            //    if (!DatabaseManager.CheckTableExistence("WEB_API_CREDENTIALS"))
            //        RunInstall();
            //    else if (!DatabaseManager.CheckTableHasData("WEB_API_CREDENTIALS"))
            //        RunConfig();
            //    else
            //    {
            //        if (args.Length == 0)
            //            //Run CLI en mode interactif
            //            RunCli(args);
            //        else
            //            Process(args);
            //    }
            //}
        }

        #region code boris


        private static bool Process(string[] arguments)
        {
            bool result = true;
            //using (TextWriter log = new StreamWriter("./log.txt", true, Encoding.UTF8))
            using (TextWriter w = Console.Out)
            {
                switch (arguments[0].ToLower())
                {
                    case "gui":
                        //dispose les writers ici comme la GUI prend la main
                        //log.Dispose();
                        w.Dispose();
                        RunGui(DatabaseIsInstalled());
                        break;
                    case "sync":
                        if (arguments.Length > 1)
                        {
                            result = Sync(SubArray(arguments, 1, arguments.Length - 1), result, w);
                        }
                        else if (File.Exists("./params"))
                        {
                            string[] file = File.ReadAllLines("./params");
                            foreach (var item in file)
                            {
                                string[] line = item.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                                if (line.Length > 1 && line[0].ToLower() == "options")
                                    Sync(line[1].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToArray(), result, w);
                            }
                        }
                        else
                        {
                            //not enough arguments
                            WriteLine("Not enough arguments");
                            result = false;
                        }
                        break;
                    default:
                        //unknow command
                        WriteLine("Unknown command");
                        result = false;
                        break;
                }
            }

            createLog();

            return result;
        }

        private static void RunGui(DataSettings settings)
        {   
            var handle = GetConsoleWindow();
            // Hide
            ShowWindow(handle, SW_HIDE);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WelcomeWindow(settings));
            //Application.Run(new DataSettingsForm(settings));
        }

        public static void Run(string txt)
        {
            RunCli(txt.Split(' '));
        }

        static private void RunCli(string[] args)
        {
            String cmd = "";
            Write();
            while ((cmd = Console.ReadLine()) != "exit")
            {
                string pattern = "\\s+"; //split on space character(s)
                string[] arguments = Regex.Split(cmd, pattern);
                if (arguments.Length > 0)
                {
                    if (!Process(arguments))
                        PrintUsage();
                    Write();
                }
            }
        }

        static private void PrintUsage()
        {
            WriteLine("======================USAGE======================");
            WriteLine("GUI => launch the user interface.");
            WriteLine("sync [argument] => execute the synchronization of [argument].\n");
            WriteLine("exit => Stop the program.");
            WriteLine("The following options are available for [argument] :\n ");
            WriteLine("RFS      => sync R/F/S from Mercator to NopCommerce");
            WriteLine("scat     => sync S_CAT from Mercator to NopCommerce");
            //WriteLine("tarifs   => sync tarifs from Mercator to NopCommerce");
            WriteLine("discounts   => sync baremes from Mercator to NopCommerce's discounts");
            WriteLine("prods    => sync products from Mercator to NopCommerce");
            WriteLine("*******");
            //WriteLine("mclients => sync B2B clients from Mercator to NopCommerce");
            WriteLine("clients  => sync clients from NopCommerce to Mercator (if they have placed an order)");
            WriteLine("orders   => sync orders from NopCommerce to Mercator");            
            WriteLine("=================================================");
        }

        private static bool Sync(string[] arguments, bool result, TextWriter w)
        {
            string currentDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");

            int index = 0;
            bool stop = false;
            WriteLine("Syncing...");
            while (index < arguments.Length && !stop)
            {
                try
                {
                    switch (arguments[index].ToLower())
                    {
                        case "rfs":
                            RFSSyncer rfs = new RFSSyncer();
                            rfs.Sync();
                            break;

                        case "scat":
                            SCatSyncer scat = new SCatSyncer();
                            if (scat.Sync())
                            {
                                WriteLine("SCat Sync Ok");
                            }
                            else
                            {
                                WriteLine("SCat - Sync Failed");
                            }
                            break;

                        //case "tarifs":
                        //    if (Tarifs.SyncTarifs())
                        //    {
                        //        WriteLine("Tarifs Sync Ok");
                        //    }
                        //    else
                        //    {
                        //        WriteLine("Tarifs - Sync Failed");
                        //    }
                        //    break;

                        case "prods":
                            StockSyncer s = new StockSyncer();                            
                            if (s.Sync())
                            {
                                WriteLine("Product Sync Ok");
                            }
                            else
                            {
                                log("Product - Sync Failed");
                            }
                            break;
							case "dispo":
                            DispoSyncer di = new DispoSyncer();
                            if (di.Sync())
                            {
                                WriteLine("Dispo Sync Ok");
                            }
                            else
                            {
                                log("Dispo - Sync Failed");
                            }
                            break;
                        case "mclients":
                            ClientMToNSyncer cmn = new ClientMToNSyncer();
                            if (cmn.Sync())
                            {
                                WriteLine("Clients Sync Mercator->Nop Ok");
                            }
                            else
                            {
                                log("Clients Sync Mercator -> Nop Sync Failed");
                            }
                            break;
                        //case "discounts":
                        //    RemiseSyncer r  = new RemiseSyncer();
                        //    if (r.Sync())
                        //    {
                        //        WriteLine("Discounts Sync Ok");
                        //    }
                        //    else
                        //    {
                        //        log("Discounts Sync Failed");
                        //    }
                        //    break;
                        case "clients":
                             ClientNToMSyncer cnm = new ClientNToMSyncer();
                            if (cnm.Sync())
                            {
                                WriteLine("Clients Sync Nop->Mercator Ok");
                            }
                            else
                            {
                                log("Clients Sync Nop->Mercator Sync Failed");
                            }
                            break;

                        case "orders":
                            CommandeSyncer co = new CommandeSyncer();
                            if (co.Sync())
                            {
                                WriteLine("Commandes Sync Ok");
                            }
                            else
                            {
                                log("Commandes - Sync Failed");
                            }
                            break;
                        case "urls":
                            UrlsSyncer u = new UrlsSyncer();
                            if (u.Sync())
                            {
                                WriteLine("Urls Sync Ok");
                            }else
                            {
                                log("Urls Sync failed");
                            }
                            break;
                        case "stocks":
                            DispoSyncer d = new DispoSyncer();
                            if (d.Sync())
                            {
                                WriteLine("Stocks udpate Ok");
                            }
                            else
                            {
                                log("Stocks update failed");
                            }
                            break;
                        default:
                            //unknow command
                            WriteLine("Unknown argument");
                            result = false;
                            stop = true;
                            break;
                    }
                }
                catch (Exception e)
                {
                    WriteLine("Error while syncing " + arguments[index].ToLower());
                    WriteLine(e.Message);
                    WriteLine(e.StackTrace);
                    log("Error while syncing " + arguments[index].ToLower());
                    log(e.Message);
                    log(e.StackTrace);
                }
                index++;
            }
			//End with clearing cache			
            WebService.Get(WebApiEntities.clearCache);
            return result;
        }

        static public void WriteLine(string line)
        {
            Console.WriteLine("NopSync> " + line);
        }

        static private void Write()
        {
            Console.Write("NopSync> ");
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int FreeConsole();

        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static void log(Exception e, bool write = true)
        {
            if (write)
            {
                WriteLine(e.Message);
                WriteLine(e.StackTrace);
            }
            logToAdd += "\r\n" + e.Message;
            logToAdd += "\r\n" + e.StackTrace;
        }

        public static void log(string message, bool write = true)
        {   
            if (write)
                WriteLine(message);
            logToAdd += "\r\n" + message;
        }

        private static void Log(string logMessage, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString());
            w.WriteLine("  :");
            w.WriteLine("  :{0}", logMessage);
            w.WriteLine("-------------------------------");
        }

        public static void createLog()
        {
            if (logToAdd != "")
            {
 				if (!Directory.Exists("Log"))
                {
                    Directory.CreateDirectory("Log");
                }
                using (StreamWriter sw = File.AppendText("Log\\" + DateTime.Now.ToString("yyyyMMdd-hhmm") + ".txt"))
                {
                    Log(logToAdd, sw);
                }
            }
        }

        public static DataSettings DatabaseIsInstalled()
        {   
            var settings = DatabaseManager.LoadSettings();
            if(settings != null && !String.IsNullOrEmpty(settings.DataConnectionString))
            {
                DBContextFactory.SetConnection(settings.DataConnectionString);
                
            }
            return settings;
        }


        public static void UpdateConfig(string key, string value)
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configFile.AppSettings.Settings[key].Value = value;

            configFile.Save(ConfigurationSaveMode.Modified);
        }

        public static DateTime SyncDate
        {
            get; set;
        }
		[DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
    }

    #endregion 
}