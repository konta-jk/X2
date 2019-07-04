/*
 * wykonanie jednego testu, samo gęste, bez dodatków typu inicjalizacje i wyświetlanie wyników
 */ 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.ComponentModel;

namespace X2
{
    interface IQATest
    {
        void Run();
    }

    class QATest : IQATest
    {
        Structs.TestPlan testPlan;
        QATestSetup testSetup;
        Operations operations;

        public QATest(Structs.TestPlan testPlan1, QATestSetup testSetup1)
        {
            testPlan = testPlan1;//new Structs.TestPlan(testPlan1.testSteps);
            testSetup = testSetup1;
        }

        public event EventHandler RunFinishedEvent;
        public event EventHandler StepFinishedEvent;

        protected virtual void OnRunFinished()
        {
            EventHandler handler = RunFinishedEvent;
            EventArgs e = new EventArgs();
            if(handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnStepFinished()
        {
            EventHandler handler = StepFinishedEvent;
            EventArgs e = new EventArgs();
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void Run()
        {
            List<Structs.TestStepResult> testResult = new List<Structs.TestStepResult>();
            
            Structs.TestStepResult currentResult;

            operations = new Operations(testSetup);

            foreach (Structs.TestStep testStep in testPlan.testSteps)
            {
                currentResult = new Structs.TestStepResult(testStep.stepDescription, DateTime.Now, operations.Operation(testStep));
                testResult.Add(currentResult);

                testSetup.testResult = new Structs.TestResult(testResult).DeepClone(); 
                OnStepFinished();

                if(currentResult.result != "ok")
                {
                    testSetup.Log("QATest.Run(): stopping the test. Wrong result: " + currentResult.result + " in test step " + testStep.stepDescription + ".");
                    testSetup.testResult = new Structs.TestResult(testResult).DeepClone();
                    OnRunFinished();
                    return;
                }
            }

            testSetup.testResult = new Structs.TestResult(testResult).DeepClone();

            OnRunFinished();
            return;
        }
    }
}
