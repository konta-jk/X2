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
        public string testRunId;
        

        //dostępne dla interfejsu graficznego
        public Structs.TestResult testResult;

        public bool canSaveScreenshots = true;

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

            ChromeOptions chromeOptions = new ChromeOptions();
            //chromeOptions.AddArgument("no-sandbox"); //http timeout; to podobno śmierdzi
            chromeOptions.AddArgument("ignore-certificate-errors"); //współpraca z google
            chromeOptions.AddArgument("ignore-ssl-errors"); //współpraca z google
            chromeOptions.AddArgument("proxy-server='direct://'"); //szybkość działania dla zminimalizowanego chrome
            chromeOptions.AddArgument("proxy-bypass-list=*"); //szybkość działania dla zminimalizowanego chrome
            chromeOptions.AddAdditionalCapability(CapabilityType.AcceptSslCertificates, true, true); //współpraca z google
            chromeOptions.AddArgument("start-maximized"); //błąd po dodaniu maximize przy każdej akcji i interwencji uzytkownika
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

            log = "";           
            Guid guid = Guid.NewGuid();
            testRunId = DateTime.Now.Hour.ToString() + "_" + DateTime.Now.Minute.ToString() + "_" + DateTime.Now.Second.ToString() + "_" + guid.ToString().Substring(0, 4).ToUpper();
            Log("Start run " + testRunId);
            canSaveScreenshots = CanSaveScreenshots();
            if (!canSaveScreenshots)
            {
                Log("Can't save screenshots, available disk space < " + Math.Round(100 * (1 - Settings.maximumDriveRatioToLogWithSs), 0).ToString() + "%");
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
            if(Settings.logWithScreenshots && canSaveScreenshots)
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

            if (ratio < Settings.maximumDriveRatioToLogWithSs)
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
            /*
            int scale = 2; //powinna być potęga 2, inaczej brzydkie screeny
            Rectangle bounds = Screen.GetBounds(Point.Empty);
            Bitmap bitmap0 = new Bitmap(bounds.Width, bounds.Height);
            Graphics graphics = Graphics.FromImage(bitmap0);
            graphics.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
            Bitmap bitmap = new Bitmap(bitmap0, new Size((int)(bitmap0.Width / scale), (int)(bitmap0.Height / scale)));
            */

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
                    Console.WriteLine("QATestSetup.Screenshot(): can't take screenshot.");
                }
                
            }
            else
            {
                MessageBox.Show("QATestSetup.Screenshot(): Can't take screenshot, driver is null.");
                return;
            }

            string ssFullFolderPath = GetPathMakeFolder(@"\Screenshots\");            

            fileName = DateTime.Now.Hour.ToString() + "_" + DateTime.Now.Minute.ToString() + "_" + DateTime.Now.Second.ToString() + "_" + DateTime.Now.Millisecond.ToString("000") + "_" + fileName;
            fileName = Regex.Replace(fileName, "[^a-zA-Z0-9_.]+", "_", RegexOptions.Compiled);
            fileName = fileName.Substring(0, Math.Min(260, fileName.Length));

            string ssFullPath = ssFullFolderPath + @"\" + fileName + ".png";

            screenshot.SaveAsFile(ssFullPath, ScreenshotImageFormat.Png);
        }

        public string GetPathMakeFolder(string relativePath)
        {
            string exeFolderPath = System.Reflection.Assembly.GetEntryAssembly().Location.ToString();
            exeFolderPath = exeFolderPath.Substring(0, exeFolderPath.LastIndexOf('.'));
            string relativeFolderPath = relativePath + DateTime.Now.Date.ToShortDateString();
            string fullFolderPath;
            if ((testRunId != null) && (testRunId != ""))
            {
                fullFolderPath = exeFolderPath + relativeFolderPath + @"\" + testRunId; //Path.Combine(exePath, ssRelativePath);
            }
            else
            {
                fullFolderPath = exeFolderPath + relativeFolderPath + @"\no testRunId"; //Path.Combine(exePath, ssRelativePath);
            }
            if (!System.IO.Directory.Exists(fullFolderPath))
            {
                System.IO.Directory.CreateDirectory(fullFolderPath);
            }
            return fullFolderPath;
        }
    }
}
