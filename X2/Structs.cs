/*
 * dodatkowe typy danych
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X2
{    
    class Structs
    {
        public struct TestPlan //zbędne? to mogłaby być lista w Test
        {
            public List<TestStep> testSteps;

            public TestPlan(List<TestStep> testSteps1)
            {
                testSteps = new List<TestStep>();
                TestStep currentTestStep;

                foreach(TestStep testStep in testSteps1)
                {
                    currentTestStep = testStep.DeepClone();
                    testSteps.Add(currentTestStep);                    
                }
            }
        }

        [Serializable]
        public struct TestStep
        {
            public string xpath;
            public Operation operation;            
            public string stepDescription;
            public string operationResult;

            public TestStep(string xpath1, Operation operation1, string stepDescription1)
            {
                xpath = xpath1;                
                operation = new Operation(operation1.name, operation1.text, operation1.wait);
                stepDescription = stepDescription1;
                operationResult = "-1";
            }
        }

        [Serializable]
        public struct Operation
        {
            public string name;
            public string text;
            public int wait; //seconds

            public Operation(string name1, string text1, int wait1)
            {
                name = name1;
                text = text1;
                wait = wait1;
            }

            public Operation(string name1)
            {
                name = name1;
                text = "";
                wait = Settings.sleepAfterOperation;
            }
        }
    }
}
