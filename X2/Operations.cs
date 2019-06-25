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
                        RefreshUntil(testStep1);
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

        private static void RefreshUntil(Structs.TestStep testStep1)
        {
            int duration = 5000;
            int timeOut = 60; //s, default 300
            DateTime start = DateTime.Now;
            float whileDurationS = 0;

            //wystąpienie elementu z xpath i określoną wartością
            if(testStep1.operation.text != null)
            {
                //znajdź elementy z pasującym xpath i wartością
                List<IWebElement> elements = Globals.driver.FindElements(By.XPath(testStep1.xpath)).
                        Where(t => t.GetAttribute("value") == testStep1.operation.text).ToList();

                while ((elements.Count() == 0) && (whileDurationS < timeOut))
                {
                    //znajdź elementy z pasującym xpath i wartością
                    elements = Globals.driver.FindElements(By.XPath(testStep1.xpath)).
                        Where(t => t.GetAttribute("value") == testStep1.operation.text).ToList();

                    Sleep(duration);
                    whileDurationS = (DateTime.Now - start).Seconds;
                }                
            }
            else
            //wystąpienie elemetnu z xpath
            {
                //znajdź elementy z pasującym xpath
                List<IWebElement> elements = Globals.driver.FindElements(By.XPath(testStep1.xpath)).ToList();

                while ((elements.Count() == 0) && (whileDurationS < timeOut))
                {
                    //znajdź elementy z pasującym xpath i wartością
                    elements = Globals.driver.FindElements(By.XPath(testStep1.xpath)).ToList();

                    Sleep(duration);
                    whileDurationS = (DateTime.Now - start).Seconds;
                }
            }
        }

        private static void SendKeys(Structs.TestStep testStep1)
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
            SelectElement option = new SelectElement(element);
            int i;
            Int32.TryParse(testStep1.operation.text, out i);
            option.SelectByIndex(i);            
            element.Click();
        }
    }
}
