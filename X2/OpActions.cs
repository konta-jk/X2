﻿/*
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
        QATestStuff testStuff;

        private IWebElement lastFoundByRefreshUntil;


        public OpActions(QATestStuff testStuff1)
        {
            testStuff = testStuff1;
        }

        public void OpActionSendKeys(Structs.TestStep testStep1)
        {
            //IWebElement element = testStuff.driver.FindElement(By.XPath(testStep1.xpath));
            IWebElement element = ElementFinder(testStep1);

            ScrollAndMoveTo(element, testStuff.driver);

            string text = testStep1.operationText;

            if ((text[0] == '{') && (text[text.Length - 1] == '}'))
            {
                switch (text)
                {
                    case "{DateTimeNow}":
                        text = DateTime.Now.ToString("yyyy-MM-dd") + "T" + DateTime.Now.ToString("hh-mm-ss");
                        break;
                    case "{DateNow}":
                        text = DateTime.Now.ToString("yyyy-MM-dd");
                        break;
                }
            }

            element.SendKeys(text + "\t"); //ważne - z \t chodzi o zejście z pola; użytkownik też dostałby błąd, gdyby nie zszedł z pola z regułą            
        }

        public void OpActionGoToUrl(Structs.TestStep testStep1)
        {
            testStuff.driver.Navigate().GoToUrl(testStep1.operationText);
        }

        public void OpActionRefresh()
        {
            testStuff.driver.Navigate().Refresh();
        }

        //dziadostwo na potrzeby klikania w zadanie na liście w CP5
        //wolno tego używać wyłącznie po refresh until
        public string OpActionClickLast()
        {
            BrowserFocus();

            if (lastFoundByRefreshUntil != null && lastFoundByRefreshUntil.Displayed && lastFoundByRefreshUntil.Enabled)
            {
                lastFoundByRefreshUntil.Click();
                return "ok";
            }
            else
            {
                return "OpActionClickLast(): Can't find last element. Last element should be set by refreshUntil.";
            }            
        }

        public string OpActionClick(Structs.TestStep testStep1)
        {
            if(testStep1.operationText.Contains("Retarded"))
            {
                Sleep(Settings.ActionsSettings.opActionClickRetarded);
            }


            BrowserFocus();

            //dodane ze względu na problemy - czasem nie klika i nie zgłasza błędu
            testStuff.Log("OpActions.OpActionClick() called in test step " + testStep1.stepDescription);

            //IWebElement element = testStuff.driver.FindElement(By.XPath(testStep1.xpath));
            IWebElement element = ElementFinder(testStep1);

            ScrollAndMoveTo(element, testStuff.driver); 
            element.Click();            

            //sprawdzenie wystąpienia błędu zdefiniowanego przez uzytkownika (jako fragment html)            
            if ((Settings.customErrors.Count > 0) && (testStep1.operationText.Contains("Err")))
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

        //do testów
        public void OpActionDoubleClick(Structs.TestStep testStep1)
        {
            IWebElement element = ElementFinder(testStep1);
            ScrollAndMoveTo(element, testStuff.driver);
            new Actions(testStuff.driver).DoubleClick(element).Build().Perform();
        }

        //do testów
        public string OpActionDoubleClickLast()
        {
            BrowserFocus();

            if (lastFoundByRefreshUntil != null && lastFoundByRefreshUntil.Displayed && lastFoundByRefreshUntil.Enabled)
            {
                IWebElement element = lastFoundByRefreshUntil;
                ScrollAndMoveTo(element, testStuff.driver);
                new Actions(testStuff.driver).DoubleClick(element).Build().Perform();

                return "ok";
            }
            else
            {
                return "OpActionClickLast(): Can't find last element. Last element should be set by refreshUntil.";
            }
        }

        public void OpActionMoveToElement(Structs.TestStep testStep1)
        {            
            IWebElement element = ElementFinder(testStep1);

            HighlightElement(element); //debug, usunąć

            ScrollAndMoveTo(element, testStuff.driver);
        }

        public void OpActionSetVariable(Structs.TestStep testStep1)
        {
            //IWebElement element = testStuff.driver.FindElement(By.XPath(testStep1.xpath));
            IWebElement element = ElementFinder(testStep1);

            Structs.Variable v;

            if ((element.Text != null) && (element.Text != ""))
            {
                v = new Structs.Variable(testStep1.operationText, element.Text);
            }
            else
            {
                v = new Structs.Variable(testStep1.operationText, element.GetAttribute("value"));
            }

            if (testStuff.variables.Where(t => t.name == v.name).Count() == 0)
            {
                testStuff.variables.Add(v);
            }
            else
            {
                testStuff.variables.Remove(testStuff.variables.Where(t => t.name == v.name).FirstOrDefault());
                testStuff.variables.Add(v);
            }
        }

        public void OpActionSendVariable(Structs.TestStep testStep1)
        {
            //IWebElement element = testStuff.driver.FindElement(By.XPath(testStep1.xpath));
            IWebElement element = ElementFinder(testStep1);
            ScrollAndMoveTo(element, testStuff.driver);
            string value = testStuff.variables.Where(t => t.name == testStep1.operationText).SingleOrDefault().value;
            element.SendKeys(value + "\t"); //ważne - z \t chodzi o zejście z pola; użytkownik też dostałby błąd, gdyby nie zszedł z pola z regułą
        }

        //testować
        public string OpActionSetBatchVariable(Structs.TestStep testStep1)
        {
            if (testStuff.testManager != null)
            {
                IWebElement element = ElementFinder(testStep1);

                Structs.Variable v;

                if ((element.Text != null) && (element.Text != ""))
                {
                    v = new Structs.Variable(testStep1.operationText, element.Text);
                }
                else
                {
                    v = new Structs.Variable(testStep1.operationText, element.GetAttribute("value"));
                }

                if (testStuff.testManager.batchVariables.Where(t => t.name == v.name).Count() == 0)
                {
                    testStuff.testManager.batchVariables.Add(v);
                }
                else
                {
                    testStuff.testManager.batchVariables.Remove(testStuff.variables.Where(t => t.name == v.name).FirstOrDefault());
                    testStuff.testManager.batchVariables.Add(v);
                }

                return "ok";
            }        
            else
            {
                return "OpActions.OpActionSetBatchVariable(): test manager is null, are you sure you're running a test batch, not a single test? Using batch variables is allowed only in batch tests.";
            }
        }

        public string OpActionSendBatchVariable(Structs.TestStep testStep1)
        {
            //dodać sprawdzenia
            if (testStuff.testManager != null)
            {
                IWebElement element = ElementFinder(testStep1);
                ScrollAndMoveTo(element, testStuff.driver);

                if (testStuff.testManager.batchVariables.Where(t => t.name == testStep1.operationText).Count() > 0)
                {
                    string value = testStuff.testManager.batchVariables.Where(t => t.name == testStep1.operationText).SingleOrDefault().value;
                    element.SendKeys(value + "\t"); //ważne - z \t chodzi o zejście z pola; użytkownik też dostałby błąd, gdyby nie zszedł z pola z regułą
                    return "ok";
                }
                else
                {
                    return "Can't find variable " + testStep1.operationText + ". Possible causes include: " +
                        "\r\nprevious test in batch failed to set variable" +
                        "\r\napplication was restarted mid-batch - as for 2019-08-01 batch variables are stored in RAM.";
                }
                
            }
            else
            {
                return "OpActions.OpActionSendBatchVariable(): test manager is null, are you sure you're running a test batch, not a single test? Using batch variables is allowed only in batch tests.";
            }

                
        }

        
        public string OpActionCloseAlert(string operationText) 
        {
            IAlert alert;// 
            
            
            //alert = ExpectedConditions.AlertIsPresent().Invoke(testStuff.driver); //przestało działać po aktualizacji do 76

            //ten fragemt robi to samo co powyższa linijka, ale nie używa depreciated ExpectedConditions, za to używa try catch, co jest jeszcze gorsze IMHO //odkomentowane po 76            
            try
            {
                alert = testStuff.driver.SwitchTo().Alert();
            }
            catch (NoAlertPresentException)
            {
                alert = null;
            }
                       


            if (alert != null)
            {
                testStuff.Log("OpActions.OpActionCloseAlert(): alert found. Next: accept or dismiss.");

                switch (operationText)
                {
                    case "Accept":
                        alert.Accept();
                        return "ok"; ;
                    case "Dismiss":
                        alert.Dismiss();
                        return "ok";
                    default:                        
                        return "Incorrect text parameter in action CloseAlert";
                }
            }
            else
            {
                testStuff.Log("OpActions.OpActionCloseAlert(): alert not found. Next: return \"ok\".");
            }

            return "ok"; //zakładając, że chodzi o pozbycie się alertu

        }

        public string OpActionSendEnumKeyToAlert(string operationText)
        {
            IAlert alert;

            alert = ExpectedConditions.AlertIsPresent().Invoke(testStuff.driver); //zamiast może być try catch z no alert present exception

            if (alert != null)
            {
                switch (operationText)
                {
                    case "Escape":
                        testStuff.driver.SwitchTo().Alert().SendKeys(OpenQA.Selenium.Keys.Escape);
                        return "ok";
                    case "Enter":
                        testStuff.driver.SwitchTo().Alert().SendKeys(OpenQA.Selenium.Keys.Enter);
                        return "ok";
                    case "Tab":
                        testStuff.driver.SwitchTo().Alert().SendKeys(OpenQA.Selenium.Keys.Tab);
                        return "ok";
                    default:
                        return "SendKey(): Can't recognize key code " + operationText;
                }
            }
            else
            {
                testStuff.Log("SendEnumKeyToAlert(): exception caught \"NoAlertPresentException\". Next: return \"ok\".");
                return "ok"; //to jest dziwne, ale zakładam, że intencją jest najczęściej pozbycie się alertu
            }

            /*
            try
            {
                switch (operationText)
                {
                    case "Escape":
                        testStuff.driver.SwitchTo().Alert().SendKeys(OpenQA.Selenium.Keys.Escape);
                        return "ok";
                    case "Enter":
                        testStuff.driver.SwitchTo().Alert().SendKeys(OpenQA.Selenium.Keys.Enter);
                        return "ok";
                    case "Tab":
                        testStuff.driver.SwitchTo().Alert().SendKeys(OpenQA.Selenium.Keys.Tab);
                        return "ok";
                    default:
                        return "SendKey(): Can't recognize key code " + operationText;
                }
            }
            catch (NoAlertPresentException)
            {
                testStuff.Log("SendEnumKeyToAlert(): exception caught \"NoAlertPresentException\".");
                return "SendEnumKeyToAlert(): NoAlertPresentException";
            }
            */
        }

        public void OpActionSelectOption(Structs.TestStep testStep1)
        {
            //IWebElement element = testStuff.driver.FindElement(By.XPath(testStep1.xpath));
            IWebElement element = ElementFinder(testStep1);

            ScrollAndMoveTo(element, testStuff.driver);

            SelectElement option = new SelectElement(element);
            //int i = -1;
            

            if (Int32.TryParse(testStep1.operationText, out int i))
            {
                option.SelectByIndex(i);
            }
            else
            {
                option.SelectByText(testStep1.operationText);
            }

            element.Click();
        }

        public string OpActionSendEnumKey(Structs.TestStep testStep1)
        {
            //IWebElement element = testStuff.driver.FindElement(By.XPath(testStep1.operationText));
            IWebElement element = ElementFinder(testStep1);
            ScrollAndMoveTo(element, testStuff.driver);

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
                case "Backspace":
                    element.SendKeys(OpenQA.Selenium.Keys.Backspace);
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

            IJavaScriptExecutor js = (IJavaScriptExecutor)testStuff.driver;

            IWebElement element = testStuff.driver.FindElement(By.XPath(testStep1.xpath));

            ScrollAndMoveTo(element, testStuff.driver);

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
                    elements = testStuff.driver.FindElements(By.XPath(testStep1.xpath)).ToList();
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
            IJavaScriptExecutor js = (IJavaScriptExecutor)testStuff.driver;
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
                        return "Error: can't recognize scroll destination \"" + destination + "\"; use \"Top\" or \"Bottom\" or {dx;dy}";
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

        public string OpActionPresetVariable(Structs.TestStep testStep1)
        {
            string text = testStep1.operationText;
            string name = "";
            string value = "";

            try
            {                
                name = text.Substring(0, text.IndexOf('='));
                value = text.Substring(text.IndexOf('=') + 1);
                name = ClearString(name);
                value = ClearString(value);

                //jeżeli wartość ma być pobrana ze zmiennej batcha, np. text: x={y}
                if (value[0] == '{' && value[value.Length - 1] == '}')
                {
                    value = testStuff.testManager.batchVariables.Where(t => t.name == value.Substring(1, value.Length - 2)).FirstOrDefault().value;                    
                }



                
            }
            catch (Exception e)
            {
                return "OpActionPresetVariable(): can't parse \"" + text + "\". Exception:\r\n" + e;
            }

            Structs.Variable v = new Structs.Variable(name, value);

            if (testStuff.variables.Where(t => t.name == v.name).Count() == 0)
            {
                testStuff.variables.Add(v);
            }
            else
            {
                testStuff.variables.Remove(testStuff.variables.Where(t => t.name == v.name).FirstOrDefault());
                testStuff.variables.Add(v);
            }

            return "ok";
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
                && (whileDuration.TotalMilliseconds < timeout))
            {
                Sleep(duration);
                testStuff.driver.Navigate().Refresh();
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
                parent = testStuff.driver.FindElement(By.XPath(xpath)); //nie zakładać, że istnieje                
            }
            catch (NoSuchElementException)
            {
                testStuff.Log("GetIsMatchForRefreshUntil(): exception caught \"NoSuchElementException\". Next actions: none.");
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

                    //eksperyment
                    //tutaj próbuję zdobyć namiar do dzieciaka z tekstem
                    //zazwyczaj jest to zbędne, ale czasem trzeba to kliknąć
                    List<IWebElement> subElements = elem.FindElements(By.XPath("./*")).ToList();
                    //i wśród nich jest jeden z tekstem, przy czym uwzględniam tylko pierwszy tekst
                    lastFoundByRefreshUntil = subElements.Where(t => t.Text.Contains(texts[0])).FirstOrDefault();

                    
                    
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
                    texts.Add(testStuff.variables.Where(t => t.name == s1).First().value); //opatrzyć wyjątkiem w razie braku setvariable przed uzyciem variable albo rozbudować walidację data table
                }
                else
                {
                    texts.Add(s1);
                }
            }
            return texts;
        }

        public void Sleep(int duration) //ms
        {
            System.Threading.Thread.Sleep(duration);
        }

        private string CustomErrorDetected()
        {
            string pageSource = testStuff.driver.PageSource;

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
            if ((testStuff.driver.Manage().Window.Size.Width != testStuff.initialWindowSize.Width)
                || (testStuff.driver.Manage().Window.Size.Height != testStuff.initialWindowSize.Height))
            {
                testStuff.Log("Operation: window size has changed during test step " + testStep1.stepDescription + ". Next action: try to maximize.");
                try
                {
                    testStuff.driver.Manage().Window.Maximize();
                }
                catch (Exception)
                {
                    testStuff.Log("Failed to maximize window.");
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
            //js.ExecuteScript("arguments[0].scrollIntoView({behavior: \"smooth\", block: \"center\"})", element);
            //smooth okazał się źródłem problemów, np. move to parent czasami znajdowało odkrywało ukryty element i natychmiast go gubiło
            js.ExecuteScript("arguments[0].scrollIntoView({block: \"center\"})", element);
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
            int timeout = Settings.ActionsSettings.elementFinderTimeout; //ms, do settings
            TimeSpan whileDuration = TimeSpan.FromMilliseconds(0);

            List<IWebElement> elements = new List<IWebElement>();
            
            while (whileDuration.TotalMilliseconds < timeout)
            {
                elements = testStuff.driver.FindElements(By.XPath(testStep1.xpath)).ToList();
                if (elements.Count > 0)
                {
                    if (DisplayedAndEnabled(elements[0]))
                    {
                        //Console.WriteLine("ElementFinder(): proper element found in step" + testStep1.stepDescription);

                        return elements[0];
                        
                    }
                    else
                    {
                        testStuff.Log("ElementFinder(): element found is disabled or not displayed in step " 
                            + testStep1.stepDescription + ". Next action: sleep, retry.");
                    }
                }
                Sleep(100); //do settings
                whileDuration = DateTime.Now - start;
            }


            

            //po staremu, na chama, rzeby rzuciło wyjątkami
            return testStuff.driver.FindElement(By.XPath(testStep1.xpath));            
        }

        private void HighlightElement(IWebElement element)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)testStuff.driver;
            js.ExecuteScript("arguments[0].style.border='6px solid orange'", element);
            Sleep(1000);
            IJavaScriptExecutor js1 = (IJavaScriptExecutor)testStuff.driver;
            js1.ExecuteScript("arguments[0].style.border=''", element);
        }

        //działania ostatniej szansy
        public void TryHelpNoSuchElement(int catchCount)
        {
            int deltaH = (int)(testStuff.initialWindowSize.Height * 0.8f);

            switch (catchCount) //!!
            {
                case 2:
                    OpActionScroll(new Structs.TestStep("", "", "{0;" + deltaH.ToString() + "}", ""));
                    Console.WriteLine("Scroll down " + deltaH.ToString() + " px");
                    break;
                case 3:
                    OpActionScroll(new Structs.TestStep("", "", "{0;" + (-2 * deltaH).ToString() + "}", ""));
                    Console.WriteLine("Scroll up " + (-2 * deltaH).ToString() + " px");
                    break;
                case 4:
                    OpActionScroll(new Structs.TestStep("", "", "{0;" + deltaH.ToString() + "}", ""));
                    Console.WriteLine("Scroll down " + deltaH.ToString() + " px");
                    break;
                case 5:
                    //refresh??
                    break;
            }
        }

        //działania ostatniej szansy, kiedy test się zatnie na NonInteractibleException
        public void TryHelpNonInteractible(Structs.TestStep testStep, int catchCount)
        {            
            switch (catchCount) //do zmiany!!! w tym catch mógł się trafić inny błąd //taki switch powinien być w operations?
            {
                case 3:
                    //rekurencyjnie szuka interaktywnego (visible, enabled) rodzica i kiedy znajdzie, jedzie do niego
                    MoveToParent();
                    break;
            }

            void MoveToParent()
            {
                string log = "TryHelpNonInteractible().MoveToParent(): I will try to find an interactible parent.";

                string initialXPath = testStep.xpath;

                string goodXPath = MoveUpAndTry(initialXPath);

                string MoveUpAndTry(string xpath)
                {
                    log += "\r\nMoveUpAndTry() called for xpath " + xpath;

                    string parentXPath = xpath.Substring(0, xpath.LastIndexOf('/'));

                    List<IWebElement> parents;

                    if (parentXPath.Count(t => t == '/') > 1)
                    {
                        parents = testStuff.driver.FindElements(By.XPath(parentXPath)).ToList();
                    }
                    else
                    {                        
                        return "not xpath";
                    }

                    if (parents.Count > 0)
                    {
                        if (parents[0].Displayed && parents[0].Enabled)
                        {
                            return parentXPath;
                        }
                        else
                        {
                            return MoveUpAndTry(parentXPath);
                        }
                    }
                    else
                    {
                        return "parents count 0";
                    }
                }

                if (goodXPath.Count(t => t == '/') > 1)
                {
                    log += "\r\nInteractible parent found, calling ScrollAndMoveTo, xpath found: " + goodXPath;
                    IWebElement parent = testStuff.driver.FindElement(By.XPath(goodXPath));
                    ScrollAndMoveTo(parent, testStuff.driver);

                    //HighlightElement(parent);
                }
                else
                {
                    log += "\r\nSearch for interactible parent failed.";
                }

                testStuff.Log(log);
            }
        }

        private void BrowserFocus()
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)testStuff.driver;
            js.ExecuteScript("window.focus();");
        }
    }
}
