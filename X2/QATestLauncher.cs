/*
 * QATestLauncher to uniwersalna klasa do przeprowadzania testu. Postanowiłem, że nie będzie w niej zmian związanych z pracą nad wyższymi warstwami.
 * Specyfikację jej wymagań zawiera interfejs IQATestLaunchPoint.
 * Trzyma śmieci nie związane bezpośrednio z testem, których nie chcę w QATest (wątek, nasłuch na eventy, reakcje na eventy, utworzenie TestPlan, walidacja planu testów).
 * Nadrzędna względem QATest: przekazuje jej TestPlan (do refaktoryzacji) i QATestStuff, otrzymuje zwrotnie eventy (rezultat QATest ładuje do QATestStuff)
 *   
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Data;
using System.Reflection;

namespace X2
{
    class QATestLauncher
    {
        private IQATestLaunchPoint launchPoint;
        private readonly DataTable testPlanAsDataTable;
        private QATestStuff testStuff;
        public QATest qATest;
        //private Structs.TestPlan testPlan;


        public QATestLauncher(IQATestLaunchPoint launchPoint1)
        {
            launchPoint = launchPoint1; //np. Form1
            testStuff = launchPoint1.GetTestStuff();
            testPlanAsDataTable = launchPoint1.GetTestPlanAsDataTable();
            
            if (TestPlanFromDataTable.IsValid(testPlanAsDataTable))
            {
                testStuff.testPlan = TestPlanFromDataTable.GetTestPlan(testPlanAsDataTable);
                qATest = new QATest(testStuff);
                qATest.RunFinishedEvent += OnRunFinished;
                qATest.StepFinishedEvent += OnStepFinished;
            }            
        }

        public void Run() 
        {
            if (testStuff.testPlan.testSteps != null)
            {
                testStuff.seleniumThread = new Thread(qATest.Run);
                testStuff.seleniumThread.IsBackground = true;
                testStuff.seleniumThread.CurrentUICulture = new System.Globalization.CultureInfo("en-us");
                testStuff.seleniumThread.Start();
            }            
        }

        void OnRunFinished(object sender, EventArgs e)
        {
            launchPoint.OnTestFinish();
        }

        void OnStepFinished(object sender, EventArgs e)
        {
            launchPoint.OnTestProgress();
        }           
    }
}

