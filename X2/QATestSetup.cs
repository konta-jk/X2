using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Chrome;
using System.Threading;
using System.Windows.Forms;



namespace X2
{
    public class QATestSetup
    {
        //globalsy
        public IWebDriver driver;
        public Thread seleniumThread;
        public string standardOutput;

        //brane z interfejsu + wartości domyslne ładowane do interfejsu
        public string fileName;
        public int minRow = 2;
        public int maxRow = 5000;
        public bool killDriver = true;

        //dostępne dla interfejsu
        public Structs.TestResult testResult;
        


        public QATestSetup()
        {
            testResult = new Structs.TestResult();
        }


        public void Init()
        {
            try
            {
                Console.WriteLine("Test start: " + DateTime.Now.ToString());
                standardOutput += "Test start: : " + DateTime.Now.ToString() + "\r\n";

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
                string s = "";
                s = "Init failed\r\n" + e.ToString();
                Console.WriteLine(s);
                standardOutput += "START: " + DateTime.Now.ToString() + "\r\n";
                MessageBox.Show(s);
            }
        }


        public void TearDownTest()
        {
            if (seleniumThread != null)
            {
                seleniumThread.Interrupt();
            }

            if (driver != null)
            {
                driver.Close();
                driver.Quit();
            }
        }
    }
}
