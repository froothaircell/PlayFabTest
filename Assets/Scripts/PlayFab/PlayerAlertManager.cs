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
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;

namespace PlayFabTests
{
    public class PlayerAlertManager : MonoBehaviour
    {
        public const string PlayerAlertKey = "PlayerAlerts";
        public const string AlertKey = "PlayerAlert";

        private static int _previousCount = 0;
        private static Dictionary<string, PlayerAlert> PlayerAlerts = new Dictionary<string, PlayerAlert>();

        public static Action<PlayerAlert> OnAlertReceived;

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

        public static void SendNotification(string playFabId, PlayerAlert playerAlert)
        {
            PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
            {
                FunctionName = "SendPlayerAlert",
                FunctionParameter = new
                {
                    AlertingPlayerPlayFabId = playFabId,
                    AlertType = playerAlert.AlertType.ToString(),
                    AlertMessage = playerAlert.AlertMessage.ToString(),
                }
            }, result =>
            {
                Debug.Log("Friend Request Sent");
            }, DisplayPlayFabError);
        }

        public static void SetRecurringNotificationTask()
        {
            TaskUtilitiesManager.RunTask(RecurringNotificationTask);
        }

        public static void StartNotificationCheck()
        {
            PlayFabClientAPI.GetUserData(new GetUserDataRequest
            {
                PlayFabId = LoginManager.PlayFabId,
                Keys = new List<string>() { AlertKey },
            }, PopulateNotifications, DisplayPlayFabError);
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

        private static void PopulateNotifications(GetUserDataResult result)
        {
            // Logic to populate the scripts
            Debug.Log(result.Data);

            Dictionary<string, object> data = new Dictionary<string, object>();
            foreach (var pair in result.Data)
            {
                data.Add(pair.Key, pair.Value.Value as object);
            }

            // Callback script
            if (data.Count > 0 && data.ContainsKey(AlertKey))
            {
                data.TryGetValue(AlertKey, out var value);
                var PlayerAlerts_temp = JsonConvert.DeserializeObject<Dictionary<string, PlayerAlert>>(value.ToString());

                if (PlayerAlerts_temp.Count > 0 && PlayerAlerts_temp.Count > _previousCount)
                {
                    int start = PlayerAlerts.Count;
                    for (int i = start; i < PlayerAlerts_temp.Count; i++)
                    {
                        PlayerAlerts.Add(i.ToString(), PlayerAlerts_temp[i.ToString()]);
                        OnAlertReceived?.Invoke(PlayerAlerts.Last().Value); // Alert raised in the for loop
                    }

                    _previousCount = PlayerAlerts.Count;

                    // raise alert here. there has been and alert added

                }
                else
                {
                    // do nothing here as there was no alert found on server.
                }
            }
            else
            {
                // Playgig.UpdatePlayerData
                // UpdatePlayerData(AlertKey, "{}");
            }
        }

        private static void DisplayPlayFabError(PlayFabError error)
        {
            Debug.Log(error.GenerateErrorReport());

            // Run any exception logic here
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

        public static double GetTime()
        {
            double dateReturn =
                Math.Round((double) DateTimeOffset.Now.ToUnixTimeMilliseconds());
            // note that (..date..).TotalMilliseconds returns a number such as
            // 1606465207140.45 where
            // 1606465207140 is ms and the ".45" is a fraction of a ms
            return dateReturn;
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
