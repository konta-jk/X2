using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X2
{
    public class Logger
    {
        private string filePath = "";
        private string logString = "";

        public Logger(string relativeFilePath1, string fileName)
        {
            if (relativeFilePath1[relativeFilePath1.Length - 1] != @"\"[0])
            {
                relativeFilePath1 += @"\";
            }
            filePath = GetPathMakeFolder(relativeFilePath1) + fileName;
            Console.WriteLine("Log file: " + filePath);
        }        

        public void Log(string text1)
        {
            string text = "[" + DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture) + "] " + text1 + "\r\n";
            logString += text;
            Console.WriteLine(text);
            Save(text);
        }

        public string GetLogString()
        {
            return logString;
        }

        public string GetFolderPath()
        {
            return filePath.Substring(0, filePath.LastIndexOf(@"\"[0]));
        }

        private bool Save(string text)
        {


            FileStream fileStream = null;
            bool success = true;

            try
            {
                fileStream = new FileStream(filePath, FileMode.Append);
                using (StreamWriter streamWriter = new StreamWriter(fileStream, Encoding.UTF8, 1024))
                {
                    streamWriter.Write(text);
                }
            }
            catch (Exception e)
            {
                success = false;
                Console.WriteLine("Logger.Save() exception caught: " + e);
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Dispose();                
            }
            return success;
        }



        private string GetPathMakeFolder(string relativePath)
        {
            if (relativePath[0] != @"\"[0])
            {
                relativePath = @"\" + relativePath;
            }
            if (relativePath[relativePath.Length - 1] != @"\"[0])
            {
                relativePath += @"\";
            }


            string exeFolderPath = System.Reflection.Assembly.GetEntryAssembly().Location.ToString();
            exeFolderPath = exeFolderPath.Substring(0, exeFolderPath.LastIndexOf('.'));
            string relativeFolderPath = relativePath;

            

            string fullFolderPath = exeFolderPath + relativeFolderPath;
            
            if (!System.IO.Directory.Exists(fullFolderPath))
            {
                System.IO.Directory.CreateDirectory(fullFolderPath);
            }
            return fullFolderPath;
        }




    }
}
