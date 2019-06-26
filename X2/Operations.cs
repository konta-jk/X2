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




namespace X2
{
    interface IOperations
    {
        string Operation(Structs.TestStep testStep1);
        
    }

    class Operations : IOperations
    {
        List<Structs.Variable> variables;

        public Operations()
        {
            variables = new List<Structs.Variable>();
        }
            
        public string Operation(Structs.TestStep testStep1)
        {
            string result = "init";

            try
            {
                switch (testStep1.operation.name)
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
                        Click(testStep1);
                        result = "ok";
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
                        variables.Add(SetVariable(testStep1));
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

                    case "ScrollDown":
                        ScrollDown();
                        result = "ok";
                        break;

                    default:
                        result = "error";
                        break;

                }

                
            }
            catch (Exception e)
            {
                result = "Błąd w kroku o nazwie: \"" + testStep1.stepDescription + "\", operacja: \"" + testStep1.operation.name + "\", wyjątek: " + Environment.NewLine  + e;
                //System.Windows.Forms.MessageBox.Show(result, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            Sleep(Settings.sleepAfterOperation);        

            return result;
        }

        private static void Sleep(int duration)
        {
            try
            {
                Globals.driver.SwitchTo().Alert().Accept();
            }
            catch (NoAlertPresentException)
            {
                System.Threading.Thread.Sleep(duration);
            }
        }

        

        private void SendKeys(Structs.TestStep testStep1)
        {
            IWebElement element = Globals.driver.FindElement(By.XPath(testStep1.xpath));
            Actions action = new Actions(Globals.driver); //dla estetyki tylko
            action.MoveToElement(element).Perform();
            string text = testStep1.operation.text;
            element.SendKeys(testStep1.operation.text + "\t"); //ważne - z \t chodzi o zejście z pola; użytkownik też dostałby błąd, gdyby nie zszedł z pola z regułą            
        }

        private void GoToUrl(Structs.TestStep testStep1)
        {
            Globals.driver.Navigate().GoToUrl(testStep1.operation.text);
        }

        private void Refresh()
        {
            Globals.driver.Navigate().Refresh();
        }

        private void Click(Structs.TestStep testStep1)
        {
            IWebElement element = Globals.driver.FindElement(By.XPath(testStep1.xpath));
            Actions action = new Actions(Globals.driver); //bo inaczej zdarzają się problemy z click
            action.MoveToElement(element).Perform();
            element.Click();            
        }

        private void MoveToElementPerform(Structs.TestStep testStep1)
        {
            IWebElement element = Globals.driver.FindElement(By.XPath(testStep1.xpath));
            Actions action = new Actions(Globals.driver);
            action.MoveToElement(element).Perform();
        }

        private Structs.Variable SetVariable(Structs.TestStep testStep1)
        {
            IWebElement element = Globals.driver.FindElement(By.XPath(testStep1.xpath));            
            return new Structs.Variable(testStep1.operation.text, element.GetAttribute("value"));
        }

        private void SendVariable(Structs.TestStep testStep1)
        {
            IWebElement element = Globals.driver.FindElement(By.XPath(testStep1.xpath));
            Actions action = new Actions(Globals.driver); //dla estetyki tylko
            action.MoveToElement(element).Perform();
            string value = variables.Where(t => t.name == testStep1.operation.text).SingleOrDefault().value;
            element.SendKeys(value + "\t"); //ważne - z \t chodzi o zejście z pola; użytkownik też dostałby błąd, gdyby nie zszedł z pola z regułą
        }

        private void CloseAlert()
        {
            try
            {
                Globals.driver.SwitchTo().Alert().Accept();
            }
            catch (NoAlertPresentException)
            {
                //
            }            
        }

        private void SelectOption(Structs.TestStep testStep1)
        {
            IWebElement element = Globals.driver.FindElement(By.XPath(testStep1.xpath));

            Actions action = new Actions(Globals.driver); //bo inaczej zdarzają się problemy z click??
            action.MoveToElement(element).Perform();

            SelectElement option = new SelectElement(element);
            int i;
            Int32.TryParse(testStep1.operation.text, out i);
            option.SelectByIndex(i);            
            element.Click();
        }

        //odświeża stronę aż znajdzie wiersz, w którym są teksty określone w texts
        //w kroku jako xpath należy podać bezpośredniego rodzica wierszy <tr>, zazwyczaj body
        //w kroku jako text należy podać stałe teksty odzielone podwójnymi przecinkami albo zmienne obramowane podwójnymi nawiasami kwadratowymi
        private string RefreshUntil(Structs.TestStep testStep1)
        {
            //Console.WriteLine("* RefreshUntil: start");

            int duration = 5000;
            int timeOut = 300; //s, default 300
            DateTime start = DateTime.Now;
            TimeSpan whileDuration = new TimeSpan(0, 0, 0);

            while (
                (!GetIsMatchForRefreshUntil(testStep1.xpath, GetTextsForRefreshUntil(testStep1))) 
                && (whileDuration.TotalSeconds < timeOut))
            {
                Sleep(duration);
                Globals.driver.Navigate().Refresh();
                whileDuration = DateTime.Now - start;

                //debug
                //Console.WriteLine("* RefreshUntil whileDurationS: " + whileDuration.TotalSeconds.ToString());
            }

            if(whileDuration.TotalSeconds >= timeOut)
            {
                return "timeout";
            }
            else
            {
                return "ok";
            }

            //Console.WriteLine("* RefreshUntil: koniec");
        }


        private List<string> GetTextsForRefreshUntil(Structs.TestStep testStep1)
        {            
            List<string> texts0 = new List<string>();
            List<string> texts = new List<string>();
            
            texts0 = Regex.Split(testStep1.operation.text, @",,").ToList();
            
            foreach(string s in texts0)
            {                
                string s1 = s;
                s1 = Regex.Replace(s1, @"^\s+", "");
                s1 = Regex.Replace(s1, @"\s+$", "");

                if(Regex.IsMatch(s1, @"\[\[(.*?)\]\]"))
                {
                    s1 = Regex.Replace(s1, @"^\s+", "");
                    s1 = Regex.Replace(s1, @"\s+$", "");
                    s1 = s1.Substring(2);
                    s1 = s1.Substring(0, s1.Length - 2);
                    texts.Add(variables.Where(t => t.name == s1).First().value);
                }
                else
                {
                    texts.Add(s1);
                }                
            }

            //debug
            /*
            foreach(string s in texts)
            {
                Console.WriteLine("* GetTextsForRefreshUntil: " + s);
            }
            */

            return texts;            
        }

        private bool GetIsMatchForRefreshUntil(string xpath, List<string> texts)
        {
            IWebElement parent = Globals.driver.FindElement(By.XPath(xpath));

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

            //debug
            //Console.WriteLine("* GetIsMatchForRefreshUntil: " + existsFit.ToString());

            return existsFit;
        }

        private void ScrollDown() //może potrzebna??
        {
            Sleep(3000);
            IJavaScriptExecutor js = (IJavaScriptExecutor)Globals.driver;
            js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
        }


    }
}
