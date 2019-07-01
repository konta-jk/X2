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
        Structs.TestResult Run(Structs.TestPlan testPlan, QATestSetup testSetup);
    }

    class QATest : IQATest
    {
        Structs.TestPlan testPlan;
        //public List<string> results = new List<string>();
        Operations operations;

        public QATest(Structs.TestPlan testPlan1)
        {
            testPlan = new Structs.TestPlan(testPlan1.testSteps);
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

        public Structs.TestResult Run(Structs.TestPlan testPlan, QATestSetup testSetup)
        {
            List<Structs.TestStepResult> testResult = new List<Structs.TestStepResult>();
            
            Structs.TestStepResult currentResult;

            operations = new Operations(testSetup);

            foreach (Structs.TestStep testStep in testPlan.testSteps)
            {
                currentResult = new Structs.TestStepResult(testStep.stepDescription, DateTime.Now, operations.Operation(testStep));
                testResult.Add(currentResult);

                //do refaktoryzacji
                testSetup.testResult = new Structs.TestResult(testResult).DeepClone(); //konieczne przed wywałaniem eventu //tymczasowe brzydactwo
                OnStepFinished();

                if(currentResult.result != "ok")
                {
                    testSetup.testResult = new Structs.TestResult(testResult).DeepClone(); //konieczne przed wywałaniem eventu //tymczasowe brzydactwo
                    OnRunFinished();
                    return new Structs.TestResult(testResult);
                }
            }

            testSetup.testResult = new Structs.TestResult(testResult).DeepClone(); //konieczne przed wywałaniem eventu //tymczasowe brzydactwo
            OnRunFinished();
            return new Structs.TestResult(testResult);
        }
    }
}
