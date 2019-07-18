using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Text.RegularExpressions;

namespace X2
{
    class Validator
    {
        private class OpActionValidationInfo
        {
            public string OpAName;
            public string Regex;
            public bool Xpath;

            public OpActionValidationInfo(string OpActionName1, string TextRegex1, bool XpathRequired1)
            {
                OpAName = OpActionName1;
                Regex = TextRegex1;
                Xpath = XpathRequired1;
            }
        }

        List<OpActionValidationInfo> opAVals  = new List<OpActionValidationInfo>()
        {
            { new OpActionValidationInfo("click", "\b(Err)\b|^$", true) },
            { new OpActionValidationInfo("clickJS", "^$", true) },
            { new OpActionValidationInfo("closeAlert", "\b(Accept|Dismiss)\b", true) }/*,
            { new AValInfo("", "", true) },
            { new AValInfo("", "", true) }
            */
        };
            


        public bool ValidateRow(DataRow row)
        {
            bool b1 = false; 
            bool b2 = false;
            bool b3 = false;
            //bool b4 = false; //validacja xpath op ile jest

            b1 = (((string)row[1] != null) || ((string)row[1] != ""));
            if(b1)
            {
                b2 = (opAVals.Where(t => t.OpAName == (string)row[1]).Count() == 1);
                if (b2)
                {
                    b3 = (opAVals.Where(t => t.OpAName == (string)row[1]).First().Xpath == (((string)row[3] != null) && ((string)row[3] != "")));
                }
            }

            if (b1 && b2 && b3)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
