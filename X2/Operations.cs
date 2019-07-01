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
            
        public string Operation(Structs.TestStep testStep1)
        {
            string result = "init";
            int catchLimit = 20;
            int catchCount = 0;

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
                CloseAlert();
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

                case "GoToUrl":
                    GoToUrl(testStep1);
                    result = "ok";
                    break;

                case "Click":
                    result = Click(testStep1);
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
                    CloseAlert();
                    result = "ok";
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

        private void CloseAlert()
        {
            try
            {
                testSetup.driver.SwitchTo().Alert().Accept();                
            }
            catch (NoAlertPresentException)
            {
                testSetup.Log("CloseAlert(): exception caught \"NoAlertPresentException\".");                
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
            int timeOut = 180; //s, default 300
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
            
            foreach(string s in texts0)
            {                
                string s1 = s;
                s1 = Regex.Replace(s1, @"^\s+", "");
                s1 = Regex.Replace(s1, @"\s+$", "");

                if(Regex.IsMatch(s1, @"{(.*?)}"))
                {
                    s1 = Regex.Replace(s1, @"^\s+", "");
                    s1 = Regex.Replace(s1, @"\s+$", "");
                    s1 = s1.Substring(2);
                    s1 = s1.Substring(0, s1.Length - 2);
                    texts.Add(testSetup.variables.Where(t => t.name == s1).First().value); //opatrzyć wyjątkiem w razie braku setvariable przed uzyciem variable albo rozbudować walidację data table
                }
                else
                {
                    texts.Add(s1);
                }                
            }
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
                testSetup.Log("GetIsMatchForRefreshUntil(): exception caught \"NoSuchElementException\".");                
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

    }
}
