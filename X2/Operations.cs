/*
 * wykonuje operacje na elemencie, instancja dedykowana dla danego testu
 * można by tez utworzyć bardziej złozone operacje, np. wybierz opcję z dropdowna albo kliknij akcję na liście
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
    interface IOperations
    {
        string Operation(Structs.TestStep testStep1);
    }

    class Operations : IOperations
    {
        
        QATestSetup testSetup;

        public Operations(QATestSetup testSetup1)
        {
            testSetup = testSetup1; //wskaźnik, bez kopiowania blurp hrpfr
        }

        int catchCount = 0;
        int catchLimit = 10; //do settingsów

        public string Operation(Structs.TestStep testStep1)
        {
            string result = "init";                        

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

            try
            {
                result = PerformOperation(testStep1);
                Sleep(Settings.sleepAfterOperation);
                catchCount = 0;
            }
            catch (NoAlertPresentException)
            {
                catchCount++;
                testSetup.Log("Exception caught \"NoAlertPresentException\" in test step " + testStep1.stepDescription + ". Catch number " + catchCount.ToString() + ". Next actions: none.");                
            }
            catch (UnhandledAlertException)
            {
                catchCount++;
                testSetup.Log("Exception caught \"UnhandledAlertException\" in test step " + testStep1.stepDescription + ". Catch number " + catchCount.ToString() + ". Next actions: close alert, retry.");                
                CloseAlert("Accept");
                if(catchCount < catchLimit)
                {
                    result = Operation(testStep1);
                }
                else
                {
                    result = "Catch limit exceeded: \"UnhandledAlertException\" in test step " + testStep1.stepDescription + ".";
                }
            }
            catch (StaleElementReferenceException)
            {
                catchCount++;
                testSetup.Log("Exception caught \"StaleElementReferenceException\" in test step " + testStep1.stepDescription + ". Catch number " + catchCount.ToString() + ". Next action: sleep, then retry.");
                if (catchCount < catchLimit)
                {
                    result = Operation(testStep1);
                }
                else
                {
                    result = "Catch limit exceeded: \"StaleElementReferenceException\" in test step " + testStep1.stepDescription + ".";
                }
            }
            catch (ElementNotInteractableException)
            {
                catchCount++;
                testSetup.Log("Exception caught \"ElementNotInteractableException\" in test step " + testStep1.stepDescription + ". Catch number " + catchCount.ToString() + ". Next actions: sleep, retry.");
                Sleep(1000);
                if (catchCount < catchLimit)
                {
                    result = Operation(testStep1);
                }
                else
                {
                    result = "Catch limit exceeded. \"ElementNotInteractableException\" in test step " + testStep1.stepDescription + ".";
                }
            }
            catch (NoSuchElementException e)
            {
                if (testStep1.operationName == "RefreshUntil") //tylko dla tej operacji reagować odświeżeniem strony na NoSuchElementException
                {
                    catchCount++;
                    testSetup.Log("Exception caught \"NoSuchElementException\" in test step " + testStep1.stepDescription + ". Catch number " + catchCount.ToString() + ". Next actions: sleep, refresh, retry.");
                    Sleep(2000);
                    Refresh();
                    if (catchCount < catchLimit)
                    {
                        result = Operation(testStep1);
                    }
                    else
                    {
                        result = "Catch limit exceeded. \"NoSuchElementException\" in test step " + testStep1.stepDescription + ".";
                    }
                }
                else
                {
                    result = "Error in step named: \"" + testStep1.stepDescription + "\". Operation: \"" + testStep1.operationName + "\". Exception: \r\n" + e;
                }                
            }
            catch (Exception e)
            {
                result = "Error in step named: \"" + testStep1.stepDescription + "\". Operation: \"" + testStep1.operationName + "\". Exception: \r\n"  + e;
            }              

            return result;
        }


        private string PerformOperation(Structs.TestStep testStep1)
        {
            string result = "init";

            switch (testStep1.operationName)
            {
                case "SendKeys":
                    SendKeys(testStep1);
                    result = "ok";
                    break;

                case "SendEnumKey":                    
                    result = SendEnumKey(testStep1); ;
                    break;

                case "GoToUrl":
                    GoToUrl(testStep1);
                    result = "ok";
                    break;

                case "Click":
                    result = Click(testStep1);
                    break;

                case "ClickJS":
                    result = ClickJS(testStep1);
                    break;

                case "WaitFor":
                    result = WaitFor(testStep1);
                    break;

                case "Refresh":
                    Refresh();
                    result = "ok";
                    break;

                case "MoveToElementPerform":
                    MoveToElementPerform(testStep1);
                    result = "ok";
                    break;

                case "SetVariable":
                    SetVariable(testStep1);
                    result = "ok";
                    break;

                case "SendVariable":
                    SendVariable(testStep1);
                    result = "ok";
                    break;

                case "CloseAlert":
                    CloseAlert(testStep1.operationText);
                    result = "ok";
                    break;

                case "SendEnumKeyToAlert":
                    result = SendEnumKeyToAlert(testStep1.operationText);
                    break;

                case "SelectOption":
                    SelectOption(testStep1);
                    result = "ok";
                    break;

                case "RefreshUntil":
                    result = RefreshUntil(testStep1);
                    break;

                case "Scroll":
                    result = Scroll(testStep1);
                    break;

                default:
                    result = "Error: can't recognize operation \"" + testStep1.operationName + "\".";
                    break;
            }

            return result;
        }

        private void SendKeys(Structs.TestStep testStep1)
        {
            IWebElement element = testSetup.driver.FindElement(By.XPath(testStep1.xpath));
            Actions action = new Actions(testSetup.driver); //dla estetyki tylko
            action.MoveToElement(element).Perform();
            string text = testStep1.operationText;
            element.SendKeys(testStep1.operationText + "\t"); //ważne - z \t chodzi o zejście z pola; użytkownik też dostałby błąd, gdyby nie zszedł z pola z regułą            
        }

        private void GoToUrl(Structs.TestStep testStep1)
        {
            testSetup.driver.Navigate().GoToUrl(testStep1.operationText);

        }

        private void Refresh()
        {
            testSetup.driver.Navigate().Refresh();
        }

        private string Click(Structs.TestStep testStep1)
        {
            IWebElement element = testSetup.driver.FindElement(By.XPath(testStep1.xpath));
            Actions action = new Actions(testSetup.driver); //bo inaczej zdarzają się problemy z click
            action.MoveToElement(element).Perform();
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

        private void MoveToElementPerform(Structs.TestStep testStep1)
        {
            IWebElement element = testSetup.driver.FindElement(By.XPath(testStep1.xpath));
            Actions action = new Actions(testSetup.driver);
            action.MoveToElement(element).Perform();
        }

        private void SetVariable(Structs.TestStep testStep1)
        {
            IWebElement element = testSetup.driver.FindElement(By.XPath(testStep1.xpath));

            Structs.Variable v;

            if((element.Text != null) && (element.Text != ""))
            {
                v = new Structs.Variable(testStep1.operationText, element.Text);
            }
            else
            {
                v = new Structs.Variable(testStep1.operationText, element.GetAttribute("value"));
            }            

            if(testSetup.variables.Where(t => t.name == v.name).Count() == 0)
            {
                testSetup.variables.Add(v);
            }
            else
            {
                testSetup.variables.Remove(testSetup.variables.Where(t => t.name == v.name).First());
                testSetup.variables.Add(v);
            }
        }

        private void SendVariable(Structs.TestStep testStep1)
        {
            IWebElement element = testSetup.driver.FindElement(By.XPath(testStep1.xpath));
            Actions action = new Actions(testSetup.driver); //dla estetyki tylko
            action.MoveToElement(element).Perform();
            string value = testSetup.variables.Where(t => t.name == testStep1.operationText).SingleOrDefault().value;
            element.SendKeys(value + "\t"); //ważne - z \t chodzi o zejście z pola; użytkownik też dostałby błąd, gdyby nie zszedł z pola z regułą
        }

        private void CloseAlert(string operationText)
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

        private string SendEnumKeyToAlert(string operationText)
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
            catch (NoAlertPresentException e)
            {                
                testSetup.Log("SendEnumKeyToAlert(): exception caught \"NoAlertPresentException\".");
                return "SendEnumKeyToAlert(): NoAlertPresentException";
            }            
        }

        private void SelectOption(Structs.TestStep testStep1)
        {
            IWebElement element = testSetup.driver.FindElement(By.XPath(testStep1.xpath));

            Actions action = new Actions(testSetup.driver); //bo inaczej zdarzają się problemy z click??
            action.MoveToElement(element).Perform();

            SelectElement option = new SelectElement(element);
            int i;
            Int32.TryParse(testStep1.operationText, out i);
            option.SelectByIndex(i);            
            element.Click();
        }

        private string Scroll(Structs.TestStep testStep1)
        {
            Sleep(3000);
            IJavaScriptExecutor js = (IJavaScriptExecutor)testSetup.driver;
            string destination = testStep1.operationText;

            switch (destination)
            {
                case "Top":
                    js.ExecuteScript("window.scrollTo(0, 0);");
                    return "ok";
                case "Bottom":
                    js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
                    return "ok";
                default:
                    return "Error: can't recognize scroll destination \"" + destination + "\"; use \"Top\" or \"Bottom\"";
            }
        }

        //odświeża stronę aż znajdzie wiersz, w którym są teksty określone w texts
        //w kroku jako xpath należy podać bezpośredniego rodzica wierszy <tr>, zazwyczaj body
        //w kroku jako text należy podać stałe teksty odzielone podwójnymi przecinkami albo zmienne obramowane podwójnymi nawiasami kwadratowymi
        private string RefreshUntil(Structs.TestStep testStep1)
        {
            int duration = 4000;
            int timeOut = 60; //s, default 300
            DateTime start = DateTime.Now;
            TimeSpan whileDuration = new TimeSpan(0, 0, 0);

            while (
                (!GetIsMatchForRefreshUntil(testStep1.xpath, GetTextsForRefreshUntil(testStep1))) 
                && (whileDuration.TotalSeconds < timeOut))
            {
                Sleep(duration);
                testSetup.driver.Navigate().Refresh();
                whileDuration = DateTime.Now - start;
            }

            if(whileDuration.TotalSeconds >= timeOut)
            {
                return "timeout";
            }
            else
            {
                return "ok";
            }
        }


        private List<string> GetTextsForRefreshUntil(Structs.TestStep testStep1)
        {            
            List<string> texts0 = new List<string>();
            List<string> texts = new List<string>();
            
            texts0 = Regex.Split(testStep1.operationText, @";").ToList();
            //debug
            /*
            foreach (string str in Regex.Split(testStep1.operationText, @";").ToList())
            {
                Console.WriteLine(">>>>>>>>>>" + str);
            }
            */
            
            foreach(string s in texts0)
            {                
                string s1 = s;
                s1 = Regex.Replace(s1, @"^\s+", "");
                s1 = Regex.Replace(s1, @"\s+$", "");

                if(Regex.IsMatch(s1, @"{(.*?)}"))
                {
                    s1 = Regex.Replace(s1, @"^\s+", "");
                    s1 = Regex.Replace(s1, @"\s+$", "");
                    s1 = s1.Substring(1);
                    s1 = s1.Substring(0, s1.Length - 1);
                    texts.Add(testSetup.variables.Where(t => t.name == s1).First().value); //opatrzyć wyjątkiem w razie braku setvariable przed uzyciem variable albo rozbudować walidację data table
                }
                else
                {
                    texts.Add(s1);
                }                
            }

            //debug
            /*
            foreach (string str in texts)
            {
                Console.WriteLine("#####" + str);
            }
            int i = 1;
            */

            return texts;            
        }
        
        private bool GetIsMatchForRefreshUntil(string xpath, List<string> texts)
        {
            IWebElement parent;

            try
            {
                parent = testSetup.driver.FindElement(By.XPath(xpath)); //nie zakładać, że istnieje
            }
            catch(NoSuchElementException)
            {
                testSetup.Log("GetIsMatchForRefreshUntil(): exception caught \"NoSuchElementException\". Next actions: throw.");
                //throw;
                return false;
            }

            //bezpośrednie dzieci
            List<IWebElement> children = parent.FindElements(By.XPath("./*")).ToList();


            bool existsFit = false;
            foreach(IWebElement elem in children)
            {
                string s = elem.Text;
                bool thisFits = true;

                foreach (string text in texts)
                {
                    //debug
                    //Console.WriteLine(">> Sprawdzam, czy \"" + s + "\" zawiera " + text);

                    if(!s.Contains(text))
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

        private static void Sleep(int duration)
        {
            System.Threading.Thread.Sleep(duration);
        }

        private string CustomErrorDetected()
        {
            string pageSource = testSetup.driver.PageSource;

            foreach(KeyValuePair<string, string> s in Settings.customErrors)
            {
                if(pageSource.Contains(s.Value))
                {
                    int i = pageSource.IndexOf(s.Value, 0);
                    int leftDelta = 0;
                    int rightDelta = 500;

                    return s.Key + ". Source fragment:\r\n" + pageSource.Substring(Math.Max(i - leftDelta, 0), Math.Min(rightDelta, pageSource.Length - i));
                }
            }
            return "no";
        }

        private string SendEnumKey(Structs.TestStep testStep1)
        {
            IWebElement element = testSetup.driver.FindElement(By.XPath(testStep1.operationText));
            Actions action = new Actions(testSetup.driver); 
            action.MoveToElement(element).Perform();            

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
        private string ClickJS(Structs.TestStep testStep1)
        {
            Sleep(1000);

            IJavaScriptExecutor js = (IJavaScriptExecutor)testSetup.driver;

            IWebElement element = testSetup.driver.FindElement(By.XPath(testStep1.xpath));
            Actions action = new Actions(testSetup.driver); //bo inaczej zdarzają się problemy z click
            action.MoveToElement(element).Perform();
            //element.Click();
            js.ExecuteScript("arguments[0].click();", element);

            Sleep(20000);

            return "ok";
        }

        //służy do
        //1. weryfikacji, że test dotarł do oczekiwanego elementu (ewentualnie z tekstem)
        //2. oczekiwania sztywno przez x s - np. po zapisz i zatwierdź
        //3. oczekiwania aż pojawi się element (ew. tekst)
        private string WaitFor(Structs.TestStep testStep1)
        {
            int timeOut = 40; //do settingsów
            TimeSpan whileDuration = new TimeSpan(0, 0, 0);
            DateTime startTime = DateTime.Now;
            string expectedText = "";

            if ((testStep1.operationText != null) && (testStep1.operationText != ""))
            {
                expectedText = testStep1.operationText;
                expectedText = Regex.Replace(expectedText, @"^\s+", "");
                expectedText = Regex.Replace(expectedText, @"\s+$", "");
            }            
            
            List<IWebElement> elements = new List<IWebElement>();

            while (whileDuration.TotalSeconds < timeOut)
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
                                return "ok";
                            }
                            else
                            {
                                elements.Clear();
                            }
                        }
                        else
                        {
                            return "ok";
                        }
                    }
                }
                Sleep(500);
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
    }
}
