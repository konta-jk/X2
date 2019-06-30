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
    interface ITest
    {
        void Run();
        string GetResult();
    }

    class QATest : ITest
    {
        Structs.TestPlan testPlan;
        public List<string> results = new List<string>();
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
            EventHandler handler =StepFinishedEvent;
            EventArgs e = new EventArgs();
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void Run()
        {
            string currentResult = "";
            operations = new Operations();

            foreach (Structs.TestStep testStep in testPlan.testSteps)
            {
                currentResult = operations.Operation(testStep);
                results.Add(currentResult);
                OnStepFinished();

                if(currentResult != "ok")
                {
                    OnRunFinished();
                    break;
                }
            }

            OnRunFinished();
        }

        public string GetResult()
        {
            string s = "-1";
            if (testPlan.testSteps.Count != results.Count(t => t == "ok"))
            {
                s = "FAIL\r\nTest failed, " + results.Count(t => t == "ok").ToString() + " test steps passed out of " + testPlan.testSteps.Count.ToString() +".";
            }
            else
            {
                s = "PASS\r\nTest passed (" + testPlan.testSteps.Count.ToString() + " test steps).";
            }

            for(int i = 0; i < testPlan.testSteps.Count; i++)
            {
                s += "\r\n"+ testPlan.testSteps[i].stepDescription + ": ";
                if(i <= results.Count() - 1)
                {
                    s += results[i];
                }
            }

            string v = "";
            foreach(Structs.Variable v1 in operations.GetVariables())
            {
                v += v1.name + " = " + v1.value + "\r\n";
            }            
            if(v.Length > 0)
            {
                s += "\r\n\r\nVariables: \r\n" + v;
            }
                        
            //tu ma się znaleźć output
            string c = "todo: dodać \"process standard output\"";

            if(c.Length > 0)
            {
                s += "\r\n\r\nOutput: \r\n" + c;
            }            

            return s;
        }
    }
}
