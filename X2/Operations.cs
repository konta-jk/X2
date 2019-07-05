/*
 * wykonuje operacje na elemencie, instancja dedykowana dla danego testu
 * 
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
        
        QATestStuff testStuff;
        OpActions opActions;

        public Operations(QATestStuff testStuff1)
        {
            testStuff = testStuff1; //wskaźnik, bez kopiowania blurp hrpfr
            opActions = new OpActions(testStuff1);
        }

        int catchCount = 0;
        int catchLimit = Settings.catchLimit; //do settingsów

        public string Operation(Structs.TestStep testStep1)
        {
            string result = "init";                        

            try
            {
                opActions.KeepMaximized(testStep1); //wielkość okna chrome, ważne dla niezawodności opetarions

                result = PerformOperation(testStep1);
                opActions.Sleep(Settings.sleepAfterOperation);
                catchCount = 0;
            }
            //operacje potrzebne dla wszystkich
            catch (NoAlertPresentException)
            {
                catchCount++;
                testStuff.Log("Exception caught \"NoAlertPresentException\" in step " + testStep1.stepDescription + ". Catch number " + catchCount.ToString() + ". Next: none.");                
            }
            catch (UnhandledAlertException)
            {
                catchCount++;
                testStuff.Log("Exception caught \"UnhandledAlertException\" in step " + testStep1.stepDescription + ". Catch number " + catchCount.ToString() + ". Next: close alert, retry.");
                opActions.OpActionCloseAlert("Accept");
                if(catchCount < catchLimit)
                {
                    result = Operation(testStep1);
                }
                else
                {
                    result = "Catch limit exceeded: \"UnhandledAlertException\" in step " + testStep1.stepDescription + ".";
                }
            }
            catch (StaleElementReferenceException)
            {
                catchCount++;
                testStuff.Log("Exception caught \"StaleElementReferenceException\" in step " + testStep1.stepDescription + ". Catch number " + catchCount.ToString() + ". Next: retry.");
                if (catchCount < catchLimit)
                {
                    result = Operation(testStep1);
                }
                else
                {
                    result = "Catch limit exceeded: \"StaleElementReferenceException\" in step " + testStep1.stepDescription + ".";
                }
            }
            catch (ElementNotInteractableException)
            {
                catchCount++;
                testStuff.Log("Exception caught \"ElementNotInteractableException\" in step " + testStep1.stepDescription + ". Catch number " + catchCount.ToString() + ". Next: sleep, retry.");
                opActions.Sleep(Settings.sleepAfterElementNotInteractible);
                if (catchCount < catchLimit)
                {
                    opActions.TryHelpNonInteractible(testStep1, catchCount); //to jest fajne

                    result = Operation(testStep1);
                }
                else
                {
                    result = "Catch limit exceeded. \"ElementNotInteractableException\" in step " + testStep1.stepDescription + ".";
                }

                
            }
            //niby redundantne z implicit wait, ale w praktyce pomaga
            catch (NoSuchElementException e) 
            {                
                catchCount += 9; //ze względu na implicit wait (jakoś to ogarnąć potem)
                testStuff.Log("Exception caught \"NoSuchElementException\" in test step " + testStep1.stepDescription + ". Catch number " + catchCount.ToString() + ". Next actions: sleep, retry.");
                //bez refresh!
                opActions.Sleep(Settings.sleepAfterNoSuchElement); //do tego dochodzi implicit wait, więc łącznie 40,3 s x 10... wtf
                    
                if (catchCount < catchLimit)
                {
                    result = Operation(testStep1);
                }
                else
                {
                    result = "Catch limit exceeded. \"NoSuchElementException\" in test step " + testStep1.stepDescription + ". Is test scenario up-to-date? Exception: \r\n" + e;
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
                    opActions.OpActionSendKeys(testStep1);
                    result = "ok";
                    break;

                case "SendEnumKey":                    
                    result = opActions.OpActionSendEnumKey(testStep1); ;
                    break;

                case "GoToUrl":
                    opActions.OpActionGoToUrl(testStep1);
                    result = "ok";
                    break;

                case "Click":
                    result = opActions.OpActionClick(testStep1);
                    break;

                case "ClickJS":
                    result = opActions.OpActionClickJS(testStep1);
                    break;

                case "WaitFor":
                    result = opActions.OpActionWaitFor(testStep1);
                    break;

                case "Refresh":
                    opActions.OpActionRefresh();
                    result = "ok";
                    break;

                case "MoveToElement":
                    opActions.OpActionMoveToElement(testStep1);
                    result = "ok";
                    break;

                case "SetVariable":
                    opActions.OpActionSetVariable(testStep1);
                    result = "ok";
                    break;

                case "SendVariable":
                    opActions.OpActionSendVariable(testStep1);
                    result = "ok";
                    break;

                case "CloseAlert":
                    opActions.OpActionCloseAlert(testStep1.operationText);
                    result = "ok";
                    break;

                case "SendEnumKeyToAlert":
                    result = opActions.OpActionSendEnumKeyToAlert(testStep1.operationText);
                    break;

                case "SelectOption":
                    opActions.OpActionSelectOption(testStep1);
                    result = "ok";
                    break;

                case "RefreshUntil":
                    result = opActions.OpActionRefreshUntil(testStep1);
                    break;

                    
                case "Scroll":
                    result = opActions.OpActionScroll(testStep1);
                    break;

                default:
                    result = "Error: can't recognize operation \"" + testStep1.operationName + "\".";
                    break;
            }

            return result;
        } 
    }
}
