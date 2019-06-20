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
        public List<string> Results = new List<string>();        

        public Test(Structs.TestPlan testPlan1)
        {
            testPlan = new Structs.TestPlan(testPlan1.testSteps);
        }

        public void Run()
        {
            string currentResult = "";

            foreach (Structs.TestStep testStep in testPlan.testSteps)
            {
                currentResult = ElementOperation.Operation(testStep);
                Results.Add(currentResult);

                if(currentResult != "ok")
                {
                    break;
                }
            }
        }

        public string GetResult()
        {
            string s = "-1";
            if (testPlan.testSteps.Count != Results.Count(t => t == "ok"))
            {
                s = "NIE udało się przejść zaplanowanej ścieżki, wykonano " + Results.Count.ToString() + " (" + Results.Count(t => t == "ok").ToString() + " poprawnie) z " + testPlan.testSteps.Count.ToString() + " kroków.";
            }
            else
            {
                s = "TAK - udało się przejść zaplanowaną ścieżkę.";
            }

            for(int i = 0; i < testPlan.testSteps.Count; i++)
            {
                s += Environment.NewLine + (i+1).ToString() + ") " + testPlan.testSteps[i].stepDescription + ": ";
                if(i <= Results.Count() - 1)
                {
                    s += Results[i];
                }
            }

            return s;
        }

    }
}
