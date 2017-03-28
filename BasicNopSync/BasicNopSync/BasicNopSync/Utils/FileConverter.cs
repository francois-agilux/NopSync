using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicNopSync.Utils
{
    class FileConverter
    {
        public static readonly string REP_FILES_LOCATION = ConfigurationManager.AppSettings["REP_FILES_STOCK"];

        /// <summary>
        /// Convert picture in base64
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string convertPicture(string path)
        {
            using (Image image = Image.FromFile(path))
            {
                using (MemoryStream m = new MemoryStream())
                {
                    image.Save(m, image.RawFormat);
                    byte[] imageBytes = m.ToArray();

                    // Convert byte[] to Base64 String
                    string base64String = Convert.ToBase64String(imageBytes);
                    return base64String;
                }
            }
        }

        /// <summary>
        /// Convert file in base64
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ConvertFileToB64(string path)
        {
            try
            {
                byte[] fileBytes = File.ReadAllBytes(path);
                string base64String = Convert.ToBase64String(fileBytes);
                return base64String;
            }
            catch (Exception e)
            {
                Program.log(e.Message);
                Program.log(e.InnerException.Message);
                Program.log(e.StackTrace);
                return "";
            }
        }

        /// <summary>
        /// Compress a directory (code from MSDN)
        /// </summary>
        /// <param name="directorySelected"></param>
        /// <returns>List of compressed files paths</returns>
        public static Dictionary<string,string> Compress(string directoryPath, string id)
        {
            List<string> paths = new List<string>();
            DirectoryInfo directorySelected = new DirectoryInfo(directoryPath);
            Dictionary<string, string> files = new Dictionary<string, string>();
            ////string zipPath = String.Format("{0}.zip",directoryPath);            
            //ZipFile.CreateFromDirectory(directoryPath, zipPath);

            //return zipPath;

            

            foreach (FileInfo fileToCompress in directorySelected.GetFiles())
            {
                if (!files.ContainsKey(fileToCompress.Name))
                {
                    using (FileStream originalFileStream = fileToCompress.OpenRead())
                    {
                        if ((File.GetAttributes(fileToCompress.FullName) &
                           FileAttributes.Hidden) != FileAttributes.Hidden & fileToCompress.Extension != ".gz")
                        {
                            using (FileStream compressedFileStream = File.Create(fileToCompress.FullName + ".gz"))
                            {
                                using (GZipStream compressionStream = new GZipStream(compressedFileStream,
                                   CompressionMode.Compress))
                                {
                                    originalFileStream.CopyTo(compressionStream);

                                }
                            }
                            string path = directoryPath + "\\" + fileToCompress.Name + ".gz";
                            FileInfo info = new FileInfo(path);
                            Console.WriteLine("Compressed {0} from {1} to {2} bytes.",
                            fileToCompress.Name, fileToCompress.Length.ToString(), info.Length.ToString());
                            files.Add(fileToCompress.Name, path);
                        }
                    }
                }
            }
            return files;
            //return directoryPath + "\\" + fileToCompress.Name + ".gz";
        }
    }
}
