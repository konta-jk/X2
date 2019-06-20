/*
 * WNIOSKI
 * pomiędzy kolejnymi krokami konieczne są waity, bo inaczej wyścig i błędy ewaluacji; driver ma wait
 * zmiana na formularzu powoduje zmianę xpath, PK: może lepiej jakiś selector css
 * 
 */

using System;
using System.Collections.Generic;
//using System.ComponentModel;
using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
using OpenQA.Selenium;
//using NUnit.Framework;
using OpenQA.Selenium.Chrome;
//using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;


namespace X2
{
    public partial class Form1 : Form
    {      

        public Form1()
        {
            InitializeComponent();
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        //wybranie wartości z drop-downa
        private void button2_Click(object sender, EventArgs e)
        {
            IWebDriver driver = new ChromeDriver();
            Actions action = new Actions(driver);

            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            string url = "";
            url = "http://taris/";
            driver.Navigate().GoToUrl(url);
            string xpath = "";

            xpath = "/html/body/ng-component/div/div/div/div/div/div/div/div[1]/input";
            IWebElement element = driver.FindElement(By.XPath(xpath));
            element.SendKeys("jkublicki");
            xpath = "/html/body/ng-component/div/div/div/div/div/div/div/div[2]/input";
            element = driver.FindElement(By.XPath(xpath));
            element.SendKeys("2");
            xpath = "/html/body/ng-component/div/div/div/div/div/div/div/div[3]/div[1]/button";
            element = driver.FindElement(By.XPath(xpath));
            element.Click();

            xpath = "/html/body/app-dashboard/div/app-sidebar/div/nav/ul/li[9]/a/span";
            element = driver.FindElement(By.XPath(xpath));
            element.Click();

            //kliknięcie w link do charakterystyki LK | Listy
            xpath = "/html/body/app-dashboard/div/app-sidebar/div/nav/ul/li[9]/ul/li[3]/a/span";
            element = driver.FindElement(By.XPath(xpath));
            element.Click();

            //majechanie nad wiersz listy
            xpath = "/html/body/app-dashboard/div/main/div/ng-component/div/div/div[2]/div/app-table/div/table/tbody/tr[1]/td[3]";
            element = driver.FindElement(By.XPath(xpath));
            action.MoveToElement(element).Perform();

            //kliknięcie w akcję dodaj zadanie
            xpath = "/html/body/app-dashboard/div/main/div/ng-component/div/div/div[2]/div/app-table/div/table/tbody/tr[1]/td[1]/div/div/button[1]/i";
            element = driver.FindElement(By.XPath(xpath));
            element.Click();

            //wybiera z drop downa wartość na literę z
            xpath = "//*[@id=\"_0_6_Status\"]";
            element = driver.FindElement(By.XPath(xpath));
            element.Click();
            element.SendKeys("z");
            element.SendKeys(OpenQA.Selenium.Keys.Enter);
                       
            /*
            xpath = "//*[@id=\"_0_0_Nazwa\"]";
            element = driver.FindElement(By.XPath(xpath));
            element.SendKeys("Selenium zadanie testowe");

            xpath = "//*[@id=\"InfoJet_Control6\"]";
            element = driver.FindElement(By.XPath(xpath));
            element.Click();
            */

            /*
            //zapis formularza
            xpath = "/html/body/app-dashboard/div/main/div/app-main-action/div/app-multiview-action/div/app-multiview/div[3]/div[2]/div/div/div/app-form-process-panel/div/div[1]/div[2]/button[1]";
            element = driver.FindElement(By.XPath(xpath));
            element.Click();
            */
        }


        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Instances.fileName = openFileDialog1.FileName;
            textBox1.Text = openFileDialog1.FileName;
        }

        private void button4_Click(object sender, EventArgs e)
        {            
            textBox2.Text = CompleteTest.Run();
        }
    }
}


