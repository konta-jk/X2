using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace X2
{
    static class Program
    {
        /// <summary>
        /// Główny punkt wejścia dla aplikacji.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string path = System.Reflection.Assembly.GetEntryAssembly().Location;
            path = path.Substring(0, path.LastIndexOf('\\'));            

            Prefs.ReadFromFile(path + "\\Preferences.json"); //musi byc full path! inaczej działa dla usera, ale nie dla systemu w startup
            //Prefs.Preferences DELETEME1 = Prefs.Settings; //debug
            //int DELETEME = 0; //debug

            switch (Prefs.Settings.applicationMode)
            {
                case "single":
                    Application.Run(new Form1());
                    break;
                case "batch":
                    Application.Run(new Form3Daemon());
                    break;
                default:
                    Application.Run(new Form1());
                    break;

            }

            

            //zrobić alternatywę dla Form1, klasę nadrzędną dla launchera i z niej spróbować działać
        }
    }
}
