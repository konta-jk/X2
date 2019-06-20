/*
 * robi wszystkie instancjonowania i przechowuje instancje w zmiennych
 */

using System;
using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace X2
{
    static class Instances
    {
        public static IWebDriver driver;
        
        public static string fileName;    

        public static void Init()
        {
            driver = new ChromeDriver();                      

            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(Settings.implicitWait);
        }

        public static void TearDownTest()
        {
            driver.Close();
            driver.Quit();
        }
    }
}
