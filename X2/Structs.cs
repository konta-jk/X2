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
    public class Structs
    {
        public struct TestPlan
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

            public override string ToString()
            {
                string s = "[stepDescription], [operationName], [operationText], [xpath]\r\n";
                foreach(Structs.TestStep step in testSteps)
                {
                    s += step.stepDescription + ", " + step.operationName + ", " + step.operationText + ", " + step.xpath + "\r\n";
                }
                return s;
            }
        }

        [Serializable]
        public struct TestStep
        {                        
            public string stepDescription;
            public string operationName;
            public string operationText;
            public string xpath;

            public TestStep(string stepDescription1, string operationName1, string operationText1, string xpath1)
            {
                stepDescription = stepDescription1;
                operationName = operationName1;
                operationText = operationText1;
                xpath = xpath1;
            }
        }

        [Serializable]
        public struct TestResult
        {
            public List<TestStepResult> testStepResults;

            public TestResult(List<TestStepResult> testStepResults1)
            {
                testStepResults = new List<TestStepResult>();
                TestStepResult currentTestStepResult;

                foreach (TestStepResult testStepResult in testStepResults1)
                {
                    currentTestStepResult = testStepResult.DeepClone();
                    testStepResults.Add(currentTestStepResult);
                }
            }

            public string ToCsvString()
            {
                string s = "[timeStamp], [stepDescription], [result]\r\n";
                foreach (Structs.TestStepResult stepResult in testStepResults)
                {
                    s += stepResult.timeStamp.ToString("HH:mm:ss") + ", (" + stepResult.stepDescription + ", " + stepResult.result + "\r\n";
                }
                return s;
            }
        }

        [Serializable]
        public struct TestStepResult
        {
            public string stepDescription;
            public DateTime timeStamp;
            public string result;

            public TestStepResult(string stepDescription1, DateTime timeStamp1, string result1)
            {
                stepDescription = stepDescription1;
                timeStamp = timeStamp1;
                result = result1;
            }
        }

        [Serializable]
        public struct Variable
        {
            public string name;
            public string value;

            public Variable(string name1, string value1)
            {
                name = name1;
                value = value1;
            }
        }
    }
}
