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
    static class Settings
    {
        public const string applicationMode = "batch"; //słownik wartości: { "batch", "single" }

        public static readonly string connectionString = @"Data Source=taris\endorcopy;User id=sa;Password=sa;";

        public static readonly bool allowTryHelps = true;
        public static readonly int sleepAfterOperation = 400; //ms, default: 400
        public static readonly int implicitWait = 15000; //ms, default: 40000
        public static readonly bool disableNotifications = false;
        public static readonly int catchLimit = 10;
        public static readonly int noSuchElementCatchLimit = 6;
        public static readonly int sleepAfterNoSuchElement = 300;
        public static readonly int sleepAfterElementNotInteractible = 1000;
        public static readonly bool logWithScreenshots = false; //SCREENSHOTY
        public static readonly float maximumDriveRatioToLogWithSs = 0.8f;
        public static readonly int killDriverDelay = 2000;

        public struct ActionsSettings
        {
            public static readonly int opActionClickJSInitialSleep = 1000;
            public static readonly int opActionClickJSFinalSleep = 20000;
            public static readonly int opActionWaitForTimeout = 40000;
            public static readonly int opActionWaitForSleep = 500;
            public static readonly int opActionScrollSleep = 1000;
            public static readonly int opActionRefreshUntilSleep = 4000;
            public static readonly int opActionRefreshUntilTimeout = 180000; //ms
            public static readonly int elementFinderTimeout = 3000; //ms default 12000


        }

        public static readonly string message1 = "Kiedy rozpocznie się test, Selenium otworzy nowe okno przeglądarki Chrome. Podnieś wtedy ręce do góry."
                + "\r\nPraca na komputerze, na którym Selenium przeprowadza test, nie jest zalecana. Nie należy dotykać automatycznie sterowanego okna przeglądarki."
                + "\r\nMożliwe przyczyny błędów w przebiegu testu:"
                + "\r\n1. Zaburzenia wprowadzone przez użytkownika komputera"
                + "\r\n2. Błędy w scenariuszu"
                + "\r\n3. Zmiany w testowanym procesie, np. xpath"
                + "\r\n4. Błędy w procesie";


        //wystapienie tych fragmentów html-a ma przerwać test i wyrzucić komunikat "custom error"; przerwanie w operacji Click z tekstem Err
        public static readonly Dictionary<string, string> customErrors = new Dictionary<string, string>()
        {
            { "CP7 żółty toster", "<div class=\"simple-notification warn has-icon ng-trigger ng-trigger-enterLeave ng-animating\">" },
            { "CP7 błędy walidacji MV", "<div _ngcontent-c8=\"\" class=\"row alert alert-danger ng-tns-c8-3 ng-star-inserted\"><b _ngcontent-c8=\"\" class=\"ng-tns-c8-3\">Błędy walidacji:</b>"},
            { "CP7 czerwony toster", "<div class=\"simple-notification error has-icon ng-trigger ng-trigger-enterLeave\"" }
        };
        
    }
}


