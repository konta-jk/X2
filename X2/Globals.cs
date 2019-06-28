/*
 * robi wszystkie instancjonowania i przechowuje instancje w zmiennych
 */

using System;
using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;

namespace X2
{
    static class Globals
    {
        public static IWebDriver driver;        
        public static string fileName;
        public static int minRow = 2;
        public static int maxRow = 1000;
        public static bool killDriver = true;
        public static string testResult; //brzydkie tymczasowe
        public static Thread seleniumThread;
        
        public static void Init()
        {
            try
            {
                
                Console.WriteLine("START: " + DateTime.Now.ToString());

                ChromeOptions chromeOptions = new ChromeOptions();
                chromeOptions.AddArgument("--ignore-certificate-errors");
                chromeOptions.AddArgument("--ignore-ssl-errors");
                chromeOptions.AddAdditionalCapability(CapabilityType.AcceptSslCertificates, true, true);
                


                driver = new ChromeDriver(chromeOptions);
                driver.Manage().Window.Maximize();
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(Settings.implicitWait);
            }
            catch (Exception e)
            {
                MessageBox.Show("Init failed\r\n" + e.ToString());
            }
        }

        public static void TearDownTest()
        {
            if(seleniumThread != null)
            {
                seleniumThread.Interrupt();
                /*
                if (!seleniumThread.Join(100))
                { // or an agreed resonable time
                    seleniumThread.Abort();
                }
                */
            }

            if(driver != null)
            {
                driver.Close();
                driver.Quit();
            }            
        }
    }
}
