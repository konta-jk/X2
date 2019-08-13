using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace X2
{
    static class Prefs
    {
        public static Preferences Settings;
        

        static string ReadSettingsFile(string fileName)
        {
            using (StreamReader streamReader = new StreamReader(fileName, Encoding.UTF8))
            {
                return streamReader.ReadToEnd();
            }
        }

        static Preferences GetSettingsContent(string fileName)
        {
            string json = ReadSettingsFile(fileName);

            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json));

            using (ms) //skrót od try, finally dispose
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(Preferences));
                return (Preferences)deserializer.ReadObject(ms);
            }
        }

        public static void ReadFromFile(string fileName)
        {
            Settings = GetSettingsContent(fileName);

            //w miarę możliwości pozbyć się tego i korzystać z customErrorDict bez użycia Dictionary
            Settings.customErrors = new Dictionary<string, string>();
            foreach (Preferences.CustomError err in Settings.customErrorDict)
            {
                Settings.customErrors.Add(err.key, err.value);
            }
        }

        [DataContract]
        public class Preferences
        {
            [DataMember] public string applicationMode = "TEST_batch";
            [DataMember] public string connectionString = @"TEST_Data Source=taris\endorcopy;User id=sa;Password=sa;";
            [DataMember] public bool allowTryHelps = true;
            [DataMember] public int sleepAfterOperation = 400;
            [DataMember] public int implicitWait = 15000; //ms, default: 40000
            [DataMember] public bool disableNotifications = false;
            [DataMember] public int catchLimit = 10;
            [DataMember] public int noSuchElementCatchLimit = 6;
            [DataMember] public int sleepAfterNoSuchElement = 300;
            [DataMember] public int sleepAfterElementNotInteractible = 1000;
            [DataMember] public bool logWithScreenshots = true; //SCREENSHOTY
            [DataMember] public float maximumDriveRatioToLogWithSs = 0.8f;
            [DataMember] public int killDriverDelay = 2000;
            [DataMember] public int[] appActiveHours = new int[] { 10004, 10005, 100019, 100020, 100021, 100022 };
            [DataMember] public ActionsSettings actionsSettings;

            [DataContract]
            public class ActionsSettings
            {
                [DataMember] public int opActionClickRetarded = 10000;
                [DataMember] public int opActionClickJSInitialSleep = 1000;
                [DataMember] public int opActionClickJSFinalSleep = 20000;
                [DataMember] public int opActionWaitForTimeout = 40000;
                [DataMember] public int opActionWaitForSleep = 500;
                [DataMember] public int opActionScrollSleep = 1000;
                [DataMember] public int opActionRefreshUntilSleep = 4000;
                [DataMember] public int opActionRefreshUntilTimeout = 180000; //ms
                [DataMember] public int elementFinderTimeout = 3000; //ms default 12000
            }

            [DataMember] public string singleModeInitMessage = "no message found xd";

            //słownik cięzko zdeserializować, obejść to (wymienić słownik na coś innego) 
            //[DataMember] public Dictionary<int, int> customErrors
            [DataMember] public List<CustomError> customErrorDict;

            [DataContract]
            public class CustomError
            {
                [DataMember] public string key;
                [DataMember] public string value;
            }

            public Dictionary<string, string> customErrors;
        }

        


    }
}
