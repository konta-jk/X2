/*
 * funkcje używane przez Operations
 */ 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Support;
using System.Windows.Forms;
using OpenQA.Selenium.Interactions;
using System.Text.RegularExpressions;
using OpenQA.Selenium.Remote;

namespace X2
{
    class OpActions
    {
        QATestSetup testSetup;

        public OpActions(QATestSetup testSetup1)
        {
            testSetup = testSetup1;
        }

        public void OpActionSendKeys(Structs.TestStep testStep1)
        {
            IWebElement element = testSetup.driver.FindElement(By.XPath(testStep1.xpath));
            //Actions action = new Actions(testSetup.driver); //dla estetyki tylko
            //action.MoveToElement(element).Perform();

            ScrollAndMoveTo(element, testSetup.driver);

            string text = testStep1.operationText;
            element.SendKeys(testStep1.operationText + "\t"); //ważne - z \t chodzi o zejście z pola; użytkownik też dostałby błąd, gdyby nie zszedł z pola z regułą            
        }

        public void OpActionGoToUrl(Structs.TestStep testStep1)
        {
            testSetup.driver.Navigate().GoToUrl(testStep1.operationText);
        }

        public void OpActionRefresh()
        {
            testSetup.driver.Navigate().Refresh();
        }

        public string OpActionClick(Structs.TestStep testStep1)
        {
            //IWebElement element = testSetup.driver.FindElement(By.XPath(testStep1.xpath));

            IWebElement element = ElementFinder(testStep1);

            ScrollAndMoveTo(element, testSetup.driver);
            element.Click();

            //sprawdzenie wystąpienia błędu zdefiniowanego przez uzytkownika (jako fragment html)            
            if ((Settings.customErrors.Count > 0) && (testStep1.operationText == "Err"))
            {
                Sleep(Settings.sleepAfterOperation);
                string customError = CustomErrorDetected();
                if (customError != "no")
                {
                    return "Custom error detected: " + customError;
                }
                else
                {
                    return "ok";
                }
            }
            else
            {
                return "ok";
            }
        }

        public void OpActionMoveToElement(Structs.TestStep testStep1)
        {
            IWebElement element = testSetup.driver.FindElement(By.XPath(testStep1.xpath));
            ScrollAndMoveTo(element, testSetup.driver);
        }

        public void OpActionSetVariable(Structs.TestStep testStep1)
        {
            IWebElement element = testSetup.driver.FindElement(By.XPath(testStep1.xpath));

            Structs.Variable v;

            if ((element.Text != null) && (element.Text != ""))
            {
                v = new Structs.Variable(testStep1.operationText, element.Text);
            }
            else
            {
                v = new Structs.Variable(testStep1.operationText, element.GetAttribute("value"));
            }

            if (testSetup.variables.Where(t => t.name == v.name).Count() == 0)
            {
                testSetup.variables.Add(v);
            }
            else
            {
                testSetup.variables.Remove(testSetup.variables.Where(t => t.name == v.name).First());
                testSetup.variables.Add(v);
            }
        }

        public void OpActionSendVariable(Structs.TestStep testStep1)
        {
            IWebElement element = testSetup.driver.FindElement(By.XPath(testStep1.xpath));
            ScrollAndMoveTo(element, testSetup.driver);
            string value = testSetup.variables.Where(t => t.name == testStep1.operationText).SingleOrDefault().value;
            element.SendKeys(value + "\t"); //ważne - z \t chodzi o zejście z pola; użytkownik też dostałby błąd, gdyby nie zszedł z pola z regułą
        }

        public void OpActionCloseAlert(string operationText)
        {
            try
            {
                switch (operationText)
                {
                    case "Accept":
                        testSetup.driver.SwitchTo().Alert().Accept();
                        break;
                    case "Dismiss":
                        testSetup.driver.SwitchTo().Alert().Dismiss();
                        break;
                }
            }
            catch (NoAlertPresentException)
            {
                testSetup.Log("CloseAlert(): exception caught \"NoAlertPresentException\".");
            }
        }

        public string OpActionSendEnumKeyToAlert(string operationText)
        {
            try
            {
                switch (operationText)
                {
                    case "Escape":
                        testSetup.driver.SwitchTo().Alert().SendKeys(OpenQA.Selenium.Keys.Escape);
                        return "ok";
                    case "Enter":
                        testSetup.driver.SwitchTo().Alert().SendKeys(OpenQA.Selenium.Keys.Enter);
                        return "ok";
                    case "Tab":
                        testSetup.driver.SwitchTo().Alert().SendKeys(OpenQA.Selenium.Keys.Tab);
                        return "ok";
                    default:
                        return "SendKey(): Can't recognize key code " + operationText;
                }
            }
            catch (NoAlertPresentException)
            {
                testSetup.Log("SendEnumKeyToAlert(): exception caught \"NoAlertPresentException\".");
                return "SendEnumKeyToAlert(): NoAlertPresentException";
            }
        }

        public void OpActionSelectOption(Structs.TestStep testStep1)
        {
            IWebElement element = testSetup.driver.FindElement(By.XPath(testStep1.xpath));

            ScrollAndMoveTo(element, testSetup.driver);

            SelectElement option = new SelectElement(element);
            int i;
            Int32.TryParse(testStep1.operationText, out i);
            option.SelectByIndex(i);
            element.Click();
        }

        public string OpActionSendEnumKey(Structs.TestStep testStep1)
        {
            IWebElement element = testSetup.driver.FindElement(By.XPath(testStep1.operationText));
            ScrollAndMoveTo(element, testSetup.driver);

            switch (testStep1.operationText)
            {
                case "Escape":
                    element.SendKeys(OpenQA.Selenium.Keys.Escape);
                    return "ok";
                case "Enter":
                    element.SendKeys(OpenQA.Selenium.Keys.Enter);
                    return "ok";
                case "Tab":
                    element.SendKeys(OpenQA.Selenium.Keys.Tab);
                    return "ok";
                default:
                    return "SendEnumKey(): Can't recognize key code " + testStep1.operationText;
            }
        }

        //desperacka próba obsłużenia zapisz i zatwierdź, używać w ostateczności, nie obsługuje err
        //mam nadziję, że zbędne i do wyeliminowania
        public string OpActionClickJS(Structs.TestStep testStep1)
        {
            Sleep(Settings.ActionsSettings.opActionClickJSInitialSleep);

            IJavaScriptExecutor js = (IJavaScriptExecutor)testSetup.driver;

            IWebElement element = testSetup.driver.FindElement(By.XPath(testStep1.xpath));

            ScrollAndMoveTo(element, testSetup.driver);

            //element.Click();
            js.ExecuteScript("arguments[0].click();", element);

            Sleep(Settings.ActionsSettings.opActionClickJSFinalSleep);

            return "ok";
        }

        //służy do
        //1. weryfikacji, że test dotarł do oczekiwanego elementu (ewentualnie z tekstem)
        //2. oczekiwania sztywno przez x s - np. po zapisz i zatwierdź
        //3. oczekiwania aż pojawi się element (ew. tekst)
        public string OpActionWaitFor(Structs.TestStep testStep1)
        {
            int timeOut = Settings.ActionsSettings.opActionWaitForTimeout;
            TimeSpan whileDuration = new TimeSpan(0, 0, 0);
            DateTime startTime = DateTime.Now;
            string expectedText = "";

            if ((testStep1.operationText != null) && (testStep1.operationText != ""))
            {
                expectedText = testStep1.operationText;
                expectedText = ClearString(expectedText);
            }

            List<IWebElement> elements = new List<IWebElement>();

            while (whileDuration.TotalMilliseconds < timeOut)
            {
                if ((testStep1.xpath != null) && (testStep1.xpath != ""))
                {
                    elements = testSetup.driver.FindElements(By.XPath(testStep1.xpath)).ToList();
                    if (elements.Count > 0)
                    {
                        //dodac warunek, ze visible, interactible itp
                        //uładzic te funkcje
                        

                        if (expectedText != "")
                        {
                            if (elements[0].Text.Contains(expectedText))
                            {
                                if(DisplayedAndEnabled(elements[0]))
                                {
                                    return "ok";
                                }
                                else
                                {
                                    return "OpActionWaitFor(): element found is disabled or not displayed.";
                                }
                                
                            }
                            else
                            {
                                elements.Clear();
                            }
                        }
                        else
                        {
                            if (DisplayedAndEnabled(elements[0]))
                            {
                                return "ok";
                            }
                            else
                            {
                                return "OpActionWaitFor(): element found is disabled or not displayed.";
                            }
                        }
                    }
                }
                Sleep(Settings.ActionsSettings.opActionWaitForSleep);
                whileDuration = DateTime.Now - startTime;
            }

            if ((testStep1.xpath != null) && (testStep1.xpath != ""))
            {
                return "Wait for (expected text: \"" + expectedText + "\") timed out";
            }
            else
            {
                return "ok";
            }
        }

        public string OpActionScroll(Structs.TestStep testStep1)
        {
            Sleep(Settings.ActionsSettings.opActionScrollSleep);
            IJavaScriptExecutor js = (IJavaScriptExecutor)testSetup.driver;
            string destination = testStep1.operationText;

            Tuple<int, int> vector = new Tuple<int, int>(0, 0);
            bool gotVector = false;

            if (destination.Contains("{"))
            {                
                gotVector = TryGetVector2(destination, out vector);
            }            

            switch (destination)
            {
                case "Top":
                    js.ExecuteScript("window.scrollTo(0, 0);");
                    return "ok";
                case "Bottom":
                    js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
                    return "ok";
                default:
                    if (gotVector)
                    {
                        js.ExecuteScript("window.scrollTo(" + vector.Item1.ToString() + ", " + vector.Item2.ToString() + ")");
                        return "ok";
                    }
                    else
                    {
                        return "Error: can't recognize scroll destination \"" + destination + "\"; use \"Top\" or \"Bottom\"";
                    }
            }            

            bool TryGetVector2(string input, out Tuple<int, int> tuple)
            {
                string input1 = input.Replace("{", "").Replace("}", "");
                string[] texts = input1.Split(';');
                tuple = new Tuple<int, int>(0, 0);
                if (texts.Length != 2)
                {
                    return false;
                }
                int x;
                int y;
                for (int i = 0; i < texts.Length; i++)
                {
                    texts[i] = ClearString(texts[i]);
                }
                if (!Int32.TryParse(texts[0], out x) || !Int32.TryParse(texts[1], out y))
                {
                    return false;
                }
                tuple = new Tuple<int, int>(x, y);
                return true;                
            }            
        }

        //odświeża stronę aż znajdzie wiersz, w którym są teksty określone w texts
        //w kroku jako xpath należy podać bezpośredniego rodzica wierszy <tr>, zazwyczaj body
        //w kroku jako text należy podać stałe teksty odzielone podwójnymi przecinkami albo zmienne obramowane podwójnymi nawiasami kwadratowymi
        public string OpActionRefreshUntil(Structs.TestStep testStep1)
        {
            int duration = Settings.ActionsSettings.opActionRefreshUntilSleep;
            int timeout = Settings.ActionsSettings.opActionRefreshUntilTimeout; 
            DateTime start = DateTime.Now;
            TimeSpan whileDuration = new TimeSpan(0, 0, 0);

            while (
                (!GetIsMatchForRefreshUntil(testStep1.xpath, GetTextsForRefreshUntil(testStep1)))
                && (whileDuration.TotalSeconds < timeout))
            {
                Sleep(duration);
                testSetup.driver.Navigate().Refresh();
                whileDuration = DateTime.Now - start;
            }

            if (whileDuration.TotalMilliseconds >= timeout)
            {
                return "timeout";
            }
            else
            {
                return "ok";
            }
        }

        //pomocnicze, nie do wywołania komendą z planu testów --------------------------------------------------

        private bool GetIsMatchForRefreshUntil(string xpath, List<string> texts)
        {
            IWebElement parent;

            try
            {
                parent = testSetup.driver.FindElement(By.XPath(xpath)); //nie zakładać, że istnieje
            }
            catch (NoSuchElementException)
            {
                testSetup.Log("GetIsMatchForRefreshUntil(): exception caught \"NoSuchElementException\". Next actions: none.");
                //throw;
                return false;
            }

            //bezpośrednie dzieci
            List<IWebElement> children = parent.FindElements(By.XPath("./*")).ToList();

            bool existsFit = false;
            foreach (IWebElement elem in children)
            {
                string s = elem.Text;
                bool thisFits = true;

                foreach (string text in texts)
                {
                    if (!s.Contains(text))
                    {
                        thisFits = false;
                    }
                }
                if (thisFits == true)
                {
                    existsFit = true;
                }
            }
            return existsFit;
        }

        private List<string> GetTextsForRefreshUntil(Structs.TestStep testStep1)
        {
            List<string> texts0 = new List<string>();
            List<string> texts = new List<string>();

            texts0 = Regex.Split(testStep1.operationText, @";").ToList();

            foreach (string s in texts0)
            {
                string s1 = s;
                s1 = ClearString(s1);

                if (Regex.IsMatch(s1, @"{(.*?)}"))
                {
                    s1 = ClearString(s1);
                    s1 = s1.Substring(1);
                    s1 = s1.Substring(0, s1.Length - 1);
                    texts.Add(testSetup.variables.Where(t => t.name == s1).First().value); //opatrzyć wyjątkiem w razie braku setvariable przed uzyciem variable albo rozbudować walidację data table
                }
                else
                {
                    texts.Add(s1);
                }
            }
            return texts;
        }

        public void Sleep(int duration)
        {
            System.Threading.Thread.Sleep(duration);
        }

        private string CustomErrorDetected()
        {
            string pageSource = testSetup.driver.PageSource;

            foreach (KeyValuePair<string, string> s in Settings.customErrors)
            {
                if (pageSource.Contains(s.Value))
                {
                    int i = pageSource.IndexOf(s.Value, 0);
                    int leftDelta = 0;
                    int rightDelta = 500;

                    return s.Key + ". Source fragment:\r\n" + pageSource.Substring(Math.Max(i - leftDelta, 0), Math.Min(rightDelta, pageSource.Length - i));
                }
            }
            return "no";
        }


        public void KeepMaximized(Structs.TestStep testStep1)
        {
            if ((testSetup.driver.Manage().Window.Size.Width != testSetup.initialWindowSize.Width)
                || (testSetup.driver.Manage().Window.Size.Height != testSetup.initialWindowSize.Height))
            {
                testSetup.Log("Operation: window size has changed during test step " + testStep1.stepDescription + ". Next action: try to maximize.");
                try
                {
                    testSetup.driver.Manage().Window.Maximize();
                }
                catch (Exception)
                {
                    testSetup.Log("Failed to maximize window.");
                }
            }
        }

        private string ClearString(string text)
        {            
            text = Regex.Replace(text, @"^\s+", "");
            text = Regex.Replace(text, @"\s+$", "");
            return text;
        }

        private void ScrollAndMoveTo(IWebElement element, IWebDriver driver)
        {
            Actions action = new Actions(driver);
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("arguments[0].scrollIntoView({behavior: \"smooth\", block: \"center\"})", element);
            action.MoveToElement(element).Perform();
        }

        private bool DisplayedAndEnabled(IWebElement element)
        {
            if (element.Displayed && element.Enabled)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //tym zastapic szukanie elementow w kazdej op action
        private IWebElement ElementFinder(Structs.TestStep testStep1)
        {
            DateTime start = DateTime.Now;
            int timeout = 60000; //ms, do settings
            TimeSpan whileDuration = TimeSpan.FromMilliseconds(0);

            List<IWebElement> elements = new List<IWebElement>();
            
            while (whileDuration.TotalMilliseconds < timeout)
            {
                elements = testSetup.driver.FindElements(By.XPath(testStep1.xpath)).ToList();
                if (elements.Count > 0)
                {
                    if (DisplayedAndEnabled(elements[0]))
                    {
                        //Console.WriteLine("ElementFinder(): proper element found in step" + testStep1.stepDescription);

                        return elements[0];
                        
                    }
                    else
                    {
                        testSetup.Log("ElementFinder(): element found is disabled or not displayed in step " 
                            + testStep1.stepDescription + ". Next action: sleep, retry.");
                    }
                }
                Sleep(100); //do settings
                whileDuration = DateTime.Now - start;
            }

            //po staremu, na chama, rzeby rzuciło wyjątkami
            return testSetup.driver.FindElement(By.XPath(testStep1.xpath));            
        }


    }
}
