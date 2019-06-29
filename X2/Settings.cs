/*
 * stałe
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace X2
{
    //docelowo powinny być ładowane z pliku
    class Settings
    {
        public static readonly int sleepAfterOperation = 400; //ms, default: 300
        public static readonly int implicitWait = 40; //seconds, default: 15
        public static readonly bool disableNotifications = false;
        
        //wystapienie tych fragmentów html-a ma przerwać test i wyrzucić komunikat "custom error"; przerwanie w operacji Click z tekstem Err
        public static readonly Dictionary<string, string> customErrors = new Dictionary<string, string>()
        {
            { "CP7 żółty toster", "<div class=\"simple-notification warn has-icon ng-trigger ng-trigger-enterLeave ng-animating\">" },
            { "CP7 błędy walidacji MV", "<div _ngcontent-c8=\"\" class=\"row alert alert-danger ng-tns-c8-3 ng-star-inserted\"><b _ngcontent-c8=\"\" class=\"ng-tns-c8-3\">Błędy walidacji:</b>"},
            { "CP7 czerwony toster", "<div class=\"simple-notification error has-icon ng-trigger ng-trigger-enterLeave\"" }
        };
        
    }
}


