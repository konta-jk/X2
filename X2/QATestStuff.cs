//komunikacja miedzy warstwami, takie jakby globals

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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;

namespace X2
{
    public class QATestStuff
    {
        //globalsy
        public IWebDriver driver;
        public Thread seleniumThread;
        //public string log;
        public Logger logger;
        public List<Structs.Variable> variables;
        public int minRow;
        public int maxRow;
        public bool killDriver;

        public TestManager testManager; //null albo test manager wywołujący (potrzebne ze względu na zmienne przekazywane między testami)

        public System.Drawing.Size initialWindowSize;
        public string testRunId;
        public Structs.TestPlan testPlan;        
        public Structs.TestResult testResult;
        public bool canSaveScreenshots = true;


        //brane z interfejsu + wartości domyslne ładowane do interfejsu
        //public string fileName; //specyficzne dla launch pointa typu form, a tu powinny być ogólne rzeczy
        public class QATestStuffOptions
        {
            public int minRow;
            public int maxRow;
            public bool killDriver;
        }

        public QATestStuff(QATestStuffOptions stuffOptions)
        {
            testResult = new Structs.TestResult();
            variables = new List<Structs.Variable>();            
            minRow = stuffOptions.minRow;
            maxRow = stuffOptions.maxRow;
            killDriver = stuffOptions.killDriver;
            
        }

        public void CreateDriver() //w przyszłości do argumentów dodać typ przeglądarki
        {
            
            ChromeOptions chromeOptions = new ChromeOptions();

            //chromeOptions.AddArgument("no-sandbox"); //testowo win7
            //chromeOptions.BinaryLocation = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";

            chromeOptions.AddArgument("ignore-certificate-errors"); //współpraca z google
            chromeOptions.AddArgument("ignore-ssl-errors"); //współpraca z google
            chromeOptions.AddArgument("proxy-server='direct://'"); //szybkość działania dla zminimalizowanego chrome
            chromeOptions.AddArgument("proxy-bypass-list=*"); //szybkość działania dla zminimalizowanego chrome
            chromeOptions.AddAdditionalCapability(CapabilityType.AcceptSslCertificates, true, true); //współpraca z google
            chromeOptions.AddArgument("start-maximized"); //błąd po dodaniu maximize przy każdej akcji i interwencji uzytkownika

            try
            {
                //próby zmuszenia do współpracy z win7:
                /*
                ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                service.LogPath = "chromedriver.log";
                service.EnableVerboseLogging = true;
                //driver = new ChromeDriver(service, chromeOptions); 
                */
                

                driver = new ChromeDriver(chromeOptions); 


                driver.Manage().Window.Maximize();
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(Prefs.Settings.implicitWait);


                
            }
            catch (Exception e)
            {
                string s = "";
                s = "Init failed\r\n" + e.ToString();
                //Log(s); //powoduje błąd, bo logger null, bo test manager woła najpierw to, a potem init()
                MessageBox.Show(s);
            }
        }


        public void Init()
        {
            //log = "";           
            Guid guid = Guid.NewGuid();
            testRunId = "Test_run_" + DateTime.Now.Hour.ToString() + "_" + DateTime.Now.Minute.ToString() + "_" + DateTime.Now.Second.ToString() + "_" + guid.ToString().Substring(0, 4).ToUpper();
            logger = new Logger(@"\" + DateTime.Now.Date.ToShortDateString() + @"\" + testRunId, "Log.txt");

            Log("Start " + testRunId);

            canSaveScreenshots = CanSaveScreenshots();
            if (!canSaveScreenshots)
            {
                Log("Can't save screenshots, available disk space < " + Math.Round(100 * (1 - Prefs.Settings.maximumDriveRatioToLogWithSs), 0).ToString() + "%");
            }

            initialWindowSize = driver.Manage().Window.Size;
        }

        public void TearDownTest()
        {
            if (driver != null)
            {
                try
                {
                    driver.Close();
                    driver.Quit();
                }
                catch (Exception e)
                {
                    //MessageBox.Show("Failed to kill Selenium driver. Please close it manually. Exception:\r\n" + e.ToString());
                    //tylko console, bo who cares, skoro driver i tak ginie (to chyba normalne, że czymś rzuca ginac i nie należy tego łapać)
                    Console.WriteLine("TearDownTest(): possible fail closing Selenium driver. Exception:\r\n" + e.ToString());
                }
            }

            if (seleniumThread != null)
            {
                seleniumThread.Interrupt();
            }
        }

        public void Log(string text)
        {
            logger.Log(text);
            if(Prefs.Settings.logWithScreenshots && canSaveScreenshots)
            {
                Screenshot(text);
            }
        }

        private bool CanSaveScreenshots()
        {
            string exeFolderPath = System.Reflection.Assembly.GetEntryAssembly().Location.ToString();
            string drive = exeFolderPath.Substring(0, exeFolderPath.LastIndexOf(':')) + @":\";
            DriveInfo driveInfo = DriveInfo.GetDrives().Where(t => t.ToString() == drive).First();
            float ratio = (float)((float)driveInfo.AvailableFreeSpace / (float)driveInfo.TotalSize); //tara bum, lesson learned          

            if (ratio < Prefs.Settings.maximumDriveRatioToLogWithSs)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Screenshot(string fileName) //nie działa bez drivera
        {
            Screenshot screenshot = null;

            if (driver != null)
            {
                try
                {
                    ITakesScreenshot takesScreenshot = (OpenQA.Selenium.ITakesScreenshot)driver;
                    screenshot = takesScreenshot.GetScreenshot();
                }
                catch
                {
                    Console.WriteLine("QATestStuff.Screenshot(): can't take screenshot.");
                }
                
            }
            else
            {
                MessageBox.Show("QATestStuff.Screenshot(): Can't take screenshot, driver is null.");
                return;
            }
    

            string ssFullFolderPath = logger.GetFolderPath() + @"\Screenshots\";
            MakeFolder(ssFullFolderPath);

            fileName = DateTime.Now.Hour.ToString() + "_" + DateTime.Now.Minute.ToString() + "_" + DateTime.Now.Second.ToString() + "_" + DateTime.Now.Millisecond.ToString("000") + "_" + fileName;
            fileName = Regex.Replace(fileName, "[^a-zA-Z0-9_.]+", "_", RegexOptions.Compiled);

            int max = 240 - ssFullFolderPath.Length; //260 plus zapas na slasha, rozszerzenie i whatnot
            fileName = fileName.Substring(0, Math.Min(max, fileName.Length));

            string ssFullPath = ssFullFolderPath + @"\" + fileName + ".png";

            if(screenshot != null) //może być podczas przerywania testu
            {
                screenshot.SaveAsFile(ssFullPath, ScreenshotImageFormat.Png);
            }
            
        }


        private void MakeFolder(string absolutePath)
        {
            if (!System.IO.Directory.Exists(absolutePath))
            {
                System.IO.Directory.CreateDirectory(absolutePath);
            }
        }

        


    }
}
