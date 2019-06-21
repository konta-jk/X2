/*
 * wykonanie jednego testu, samo gęste, bez dodatków typu inicjalizacje i wyświetlanie wyników
 */ 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X2
{
    interface ITest
    {
        void Run();
        string GetResult();
    }

    class Test : ITest
    {
        Structs.TestPlan testPlan;
        public List<string> results = new List<string>();
        

        public Test(Structs.TestPlan testPlan1)
        {
            testPlan = new Structs.TestPlan(testPlan1.testSteps);
        }

        public void Run()
        {
            string currentResult = "";
            Operations operations = new Operations();

            foreach (Structs.TestStep testStep in testPlan.testSteps)
            {
                currentResult = operations.Operation(testStep);
                results.Add(currentResult);

                if(currentResult != "ok")
                {
                    break;
                }
            }
        }

        public string GetResult()
        {
            string s = "-1";
            if (testPlan.testSteps.Count != results.Count(t => t == "ok"))
            {
                s = "NIE udało się przejść zaplanowanej ścieżki, wykonano " + results.Count.ToString() + " (" + results.Count(t => t == "ok").ToString() + " poprawnie) z " + testPlan.testSteps.Count.ToString() + " kroków.";
            }
            else
            {
                s = "TAK - udało się przejść zaplanowaną ścieżkę.";
            }

            for(int i = 0; i < testPlan.testSteps.Count; i++)
            {
                s += Environment.NewLine + (i+1).ToString() + ") " + testPlan.testSteps[i].stepDescription + ": ";
                if(i <= results.Count() - 1)
                {
                    s += results[i];
                }
            }

            return s;
        }

    }
}
