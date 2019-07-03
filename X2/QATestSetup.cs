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
        public string log;
        public List<Structs.Variable> variables;

        //brane z interfejsu + wartości domyslne ładowane do interfejsu
        public string fileName;
        public int minRow = 2;
        public int maxRow = 5000;
        public bool killDriver = true;

        public System.Drawing.Size initialWindowSize;        

        //dostępne dla interfejsu graficznego
        public Structs.TestResult testResult;       


        public QATestSetup()
        {
            testResult = new Structs.TestResult();
            variables = new List<Structs.Variable>();
        }

        public void Init()
        {
            //MessageBox.Show("New Chrome window will show up. Please don't touch it. You can interact with other applications, including other Chrome windows.", "Hello!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            MessageBox.Show("Kiedy rozpocznie się test, Selenium otworzy nowe okno przeglądarki Chrome. Nie należy dotykać tego okna. "
                + "\r\nMożna przełączyć się na inne okienko. Logowanie do Windows równiez zaburza przebieg testu.", 
                "Cześć!", MessageBoxButtons.OK, MessageBoxIcon.Information);

            log = "";
            Log("Start: " + DateTime.Now.ToString());

            //opcje które coś dały
            //...

            //opcje których efektu nie widać xd
            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--ignore-certificate-errors"); //współpraca z google
            chromeOptions.AddArgument("--ignore-ssl-errors"); //współpraca z google
            chromeOptions.AddArgument("--proxy-server='direct://'"); //szybkość działania dla zminimalizowanego chrome
            chromeOptions.AddArgument("--proxy-bypass-list=*"); //szybkość działania dla zminimalizowanego chrome
            chromeOptions.AddAdditionalCapability(CapabilityType.AcceptSslCertificates, true, true); //współpraca z google
            chromeOptions.AddArgument("--start-maximized"); //błąd po dodaniu maximize przy każdej akcji i interwencji uzytkownika

            try
            {
                driver = new ChromeDriver(chromeOptions);
                driver.Manage().Window.Maximize();                
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(Settings.implicitWait);
            }
            catch (Exception e)
            {                
                string s = "";
                s = "Init failed\r\n" + e.ToString();
                Log(s);                
                MessageBox.Show(s);
            }

            initialWindowSize = driver.Manage().Window.Size;          
            
            
        }

        public void TearDownTest()
        {
            if (seleniumThread != null)
            {
                seleniumThread.Interrupt();
            }

            if (driver != null)
            {
                try
                {
                    driver.Close();
                    driver.Quit();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Failed to kill Selenium driver. Please close it manually. Exception:\r\n" + e.ToString());
                }                
            }
        }

        public void Log(string text)
        {
            Console.WriteLine(text);
            log += text + "\r\n";
        }
            
    }
}
