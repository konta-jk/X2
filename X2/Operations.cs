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
            
            catch (NoAlertPresentException)
            {
                catchCount++;
                testStuff.Log("Exception caught \"NoAlertPresentException\" in step " + testStep1.stepDescription + ". Catch number " + catchCount.ToString() + ". Next: sleep, retry.");
                opActions.Sleep(200);
                if (catchCount < catchLimit)
                {
                    result = Operation(testStep1);
                }
                else
                {
                    //result = "Catch limit exceeded: \"UnhandledAlertException\" in step " + testStep1.stepDescription + ".";
                    result = "ok"; //dziwne, ale chodzi o to, że kiedy leci taki wyjątek, to praktycznie zawsze przy próbie pozbycia się jebanego alertu; więc tu zakładany, że jebańca nie ma
                }
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
                    if (Settings.allowTryHelps)
                    {
                        opActions.TryHelpNonInteractible(testStep1, catchCount); //znajduje interaktywnego rodzica i najeżdża na niego kursorem
                    }

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
                catchCount += 1; 
                testStuff.Log("Exception caught \"NoSuchElementException\" in test step " + testStep1.stepDescription + ". Catch number " + catchCount.ToString() + ". Next actions: sleep, retry.");                
                opActions.Sleep(Settings.sleepAfterNoSuchElement); //do tego dochodzi implicit wait, więc łącznie 40,3 s x 10... wtf
                    
                if (catchCount < Settings.noSuchElementCatchLimit)
                {
                    if(Settings.allowTryHelps)
                    {
                        opActions.TryHelpNoSuchElement(catchCount); //scrolluje
                    }

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

            //klucz operationName wzorowany na słowniku komend Selenium (jak w plikach .side)
            switch (testStep1.operationName)
            {
                case "type": //dawne "SendKeys":
                    opActions.OpActionSendKeys(testStep1);
                    result = "ok";
                    break;

                case "sendKeys": //"SendEnumKey":                    
                    result = opActions.OpActionSendEnumKey(testStep1); ;
                    break;

                case "goToUrl":
                    opActions.OpActionGoToUrl(testStep1);
                    result = "ok";
                    break;

                case "click":
                    result = opActions.OpActionClick(testStep1);
                    break;

                case "clickJS":
                    result = opActions.OpActionClickJS(testStep1);
                    break;

                case "clickLast":
                    result = opActions.OpActionClickLast();
                    break;

                case "doubleClick":
                    opActions.OpActionDoubleClick(testStep1);
                    result = "ok";
                    break;

                case "doubleClickLast":
                    result = opActions.OpActionDoubleClickLast();
                    break;

                case "waitFor":
                    result = opActions.OpActionWaitFor(testStep1);
                    break;

                case "refresh":
                    opActions.OpActionRefresh();
                    result = "ok";
                    break;

                case "mouseOver": //"moveToElement":
                    opActions.OpActionMoveToElement(testStep1);
                    result = "ok";
                    break;

                case "setVariable":
                    opActions.OpActionSetVariable(testStep1);
                    result = "ok";
                    break;

                case "sendVariable":
                    opActions.OpActionSendVariable(testStep1);
                    result = "ok";
                    break;

                case "setBatchVariable":
                    result = opActions.OpActionSetBatchVariable(testStep1);
                    break;

                case "sendBatchVariable":
                    result = opActions.OpActionSendBatchVariable(testStep1);
                    break;

                case "closeAlert":
                    opActions.OpActionCloseAlert(testStep1.operationText);
                    result = "ok";
                    break;

                case "sendEnumKeyToAlert":
                    result = opActions.OpActionSendEnumKeyToAlert(testStep1.operationText);
                    break;

                case "select": //"SelectOption":
                    opActions.OpActionSelectOption(testStep1);
                    result = "ok";
                    break;

                case "refreshUntil":
                    result = opActions.OpActionRefreshUntil(testStep1);
                    break;
                    
                case "scroll":
                    result = opActions.OpActionScroll(testStep1);
                    break;

                case "presetVariable":
                    result = opActions.OpActionPresetVariable(testStep1);
                    break;

                default:
                    result = "Error: can't recognize operation \"" + testStep1.operationName + "\".";
                    break;
            }

            return result;
        } 
    }
}
