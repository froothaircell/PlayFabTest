using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.ClientModels;
using PlayFab;
using System;
using Cysharp.Threading.Tasks;
using Spades.Managers.GeneralUtils;
using System.Threading;
using PlayFabTests.Utils;
using PlayFab.PfEditor.Json;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;

namespace PlayFabTests
{
    public class PlayerAlertManager : MonoBehaviour
    {
        public const string PlayerAlertKey = "PlayerAlerts";

        private void Start()
        {
            // InitializePlayerAlerts();
        }

        public static void InitializePlayerAlerts()
        {
            PlayFabClientAPI.GetUserData(new GetUserDataRequest
            {
                Keys = { PlayerAlertKey }
            }, AlertData =>
            {
                AlertData.Data.TryGetValue(PlayerAlertKey, out var count);
                if (Convert.ToInt32(count.Value) > 0)
                {

                }
            }, DisplayPlayFabError);
        }

        private static void DisplayPlayFabError(PlayFabError error)
        {
            Debug.Log(error.GenerateErrorReport());

            // Run any exception logic here
        }

        public static void SetRecurringNotificationTask()
        {
            TaskUtilitiesManager.RunTask(RecurringNotificationTask);
        }

        private static async UniTask RecurringNotificationTask(CancellationToken token)
        {
            await UniTask.WaitUntil(() => LoginManager.IsLoggedIn);

            // conditions for aborting 
            while (true)
            {
                await UniTask.Delay(5000);
                UnityMainThreadDispatcher.Instance().Enqueue(StartNotificationCheck);
            }
        }

        private static void StartNotificationCheck()
        {
            var userDataReq = new GetUserDataRequest
            {
                PlayFabId = LoginManager.PlayFabId,
                Keys = new List<string>() { PlayerAlertKey }
            };

            var cloudScriptReq = new ExecuteCloudScriptRequest
            {
                FunctionName = "CheckNotifications",
                FunctionParameter = default,
                GeneratePlayStreamEvent = true
            };

            PlayFabClientAPI.GetUserData(userDataReq, AlertData =>
            {
                AlertData.Data.TryGetValue(PlayerAlertKey, out var countJson);
                JsonWrapper.DeserializeObject<JsonObject>(countJson.Value).TryGetValue(PlayerAlertKey, out var count);
                if (Convert.ToInt32(count) > 0)
                {
                    PlayFabClientAPI.ExecuteCloudScript(cloudScriptReq, PopulateNotifications, DisplayPlayFabError);
                }
            }, DisplayPlayFabError);
        }

        private static void PopulateNotifications(ExecuteCloudScriptResult result)
        {
            // Logic to populate the scripts

            var resJson = JsonWrapper.SerializeObject(result.FunctionResult);
            JsonWrapper.DeserializeObject<JsonObject>(resJson).TryGetValue("data", out var dataJson);
            var dataJsonString = JsonWrapper.SerializeObject(dataJson);
            dataJsonString = dataJsonString.Replace("\\\\\\", "");
            dataJsonString = dataJsonString.Replace("\\", "");
            dataJsonString = dataJsonString.Replace("\"{", "{");
            dataJsonString = dataJsonString.Replace("}\"", "}");

            JsonWrapper.DeserializeObject<JsonObject>(dataJsonString).TryGetValue("PlayerAlerts", out var dataJson2);
            dataJsonString = JsonWrapper.SerializeObject(dataJson2);

            // dataJsonString = dataJsonString.Substring(1, dataJsonString.Length - 2);

            PlayerAlert resObj = JsonConvert.DeserializeObject<PlayerAlert>(dataJsonString);

            Debug.Log(resObj);

            
            // Here we fetch each value and cast it to the desired type
            //resObj.TryGetValue("SomeKey", out var someValue);
            //int castValue = (int)someValue;
        }
    }

    public class PlayerAlert
    {
        public string AlertType;
        public bool AlertStatus;
        public string AlertMessage;
        public string AlertTimeStamp;

        public PlayerAlert()
        {
        }

        public PlayerAlert(string alertType, bool alertStatus, string alertMessage, string alertTimeStamp)
        {
            AlertType = alertType ?? throw new ArgumentNullException(nameof(alertType));
            AlertStatus = alertStatus;
            AlertMessage = alertMessage ?? throw new ArgumentNullException(nameof(alertMessage));
            AlertTimeStamp = alertTimeStamp ?? throw new ArgumentNullException(nameof(alertTimeStamp));
        }

        public DateTime ReturnConvertedTimeStamp()
        {
            return new TimeHelperUtils().GetTimeDifference(long.Parse(AlertTimeStamp));
        }
    }

    public class TimeHelperUtils
    {
        public DateTime GetTimeDifference(long c_time)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(c_time).ToLocalTime();
            return dtDateTime;
        }
        public string TimeCalculatorTotalDaysLeft(long gameId)
        {
            DateTime dt2DateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dt2DateTime = dt2DateTime.AddMilliseconds(gameId).ToLocalTime();
            TimeSpan timeRemaining = dt2DateTime - DateTime.UtcNow;
            return timeRemaining.Days.ToString();
        }

        public string TimeCalculatorNumericDays(long c_time)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(c_time).ToLocalTime();
            return dtDateTime.Day.ToString() + ConvertDays(dtDateTime) + " ";
        }

        public string TimeCalculatorAlphaDays(long c_time)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(c_time).ToLocalTime();
            return dtDateTime.DayOfWeek.ToString();
        }

        public string TimeCalculatorNumericMonths(long c_time)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(c_time).ToLocalTime();
            return dtDateTime.Month.ToString();
        }
        public string TimeCalculatorAlphaMonths(long c_time)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(c_time).ToLocalTime();
            return DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(dtDateTime.Month);
        }
        public string TimeCalculatorNumericYear(long c_time)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(c_time).ToLocalTime();
            return dtDateTime.Year.ToString();
        }


        public TimeSpan CalculateTimeEndCountDown(long c_time)
        {
            DateTime endTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc); // start of unix time
            endTime = endTime.AddMilliseconds(c_time); // adding end time in start of unix to get end time in date time
            TimeSpan timeRemaining = endTime - DateTime.UtcNow;
            return timeRemaining;
        }

        public TimeSpan GetTime(long c_time)
        {
            //DateTime endTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc); // start of unix time
            //endTime = endTime.AddSeconds(c_time); // adding end time in start of unix to get end time in date time
            TimeSpan timeRemaining = DateTime.UtcNow - (DateTime.UtcNow.AddSeconds(c_time * -1));
            return timeRemaining;
        }



        string ConvertDays(DateTime dtDateTime)
        {
            if (new[] { 11, 12, 13 }.Contains(dtDateTime.Day))
            {
                return "th";
            }
            else if (dtDateTime.Day % 10 == 1)
            {
                return "st";
            }
            else if (dtDateTime.Day % 10 == 2)
            {
                return "nd";
            }
            else if (dtDateTime.Day % 10 == 3)
            {
                return "rd";
            }
            else
            {
                return "th";
            }
        }
    }
}
