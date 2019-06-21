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

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(Settings.sleepAfterOperation));

            return result;
        }

        private static void SendKeys(Structs.TestStep testStep1)
        {
            IWebElement element = Instances.driver.FindElement(By.XPath(testStep1.xpath));
            Actions action = new Actions(Instances.driver); //dla estetyki tylko
            action.MoveToElement(element).Perform();
            element.SendKeys(testStep1.operation.text + "\t"); //ważne - z \t chodzi o zejście z pola; użytkownik też dostałby błąd, gdyby nie zszedł z pola z regułą
            
        }

        private void GoToUrl(Structs.TestStep testStep1)
        {
            Instances.driver.Navigate().GoToUrl(testStep1.operation.text);
        }

        private void Refresh()
        {
            Instances.driver.Navigate().Refresh();
        }

        private void Click(Structs.TestStep testStep1)
        {            
            IWebElement element = Instances.driver.FindElement(By.XPath(testStep1.xpath));
            Actions action = new Actions(Instances.driver); //bo inaczej zdarzają się problemy z click
            action.MoveToElement(element).Perform();
            element.Click();            
        }

        private void MoveToElementPerform(Structs.TestStep testStep1)
        {
            IWebElement element = Instances.driver.FindElement(By.XPath(testStep1.xpath)); 
            Actions action = new Actions(Instances.driver);
            action.MoveToElement(element).Perform();
        }

        private Structs.Variable SetVariable(Structs.TestStep testStep1)
        {
            IWebElement element = Instances.driver.FindElement(By.XPath(testStep1.xpath));            
            return new Structs.Variable(testStep1.operation.text, element.GetAttribute("value"));
        }

        private void SendVariable(Structs.TestStep testStep1)
        {
            IWebElement element = Instances.driver.FindElement(By.XPath(testStep1.xpath));
            Actions action = new Actions(Instances.driver); //dla estetyki tylko
            action.MoveToElement(element).Perform();
            string value = variables.Where(t => t.name == testStep1.operation.text).SingleOrDefault().value;
            element.SendKeys(value + "\t"); //ważne - z \t chodzi o zejście z pola; użytkownik też dostałby błąd, gdyby nie zszedł z pola z regułą

        }


    }
}
