using PlayFab;
using PlayFab.ClientModels;
using PlayFabTests;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayFabTests
{
    public class LoginManager : MonoBehaviour
    {
        public static bool IsLoggedIn => PlayFabClientAPI.IsClientLoggedIn();

        private string _email = string.Empty;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private static string _playfabID = string.Empty;

        public static string PlayFabId => _playfabID;
        public static int OnlineThreshold { get; private set; }

        public static bool OnlineStatus { get; private set; }

        public void RegisterPlayer(string email, string username, string password)
        {
            if (IsLoggedIn)
            {
                LogoutPlayer();
            }

            _email = email;
            _username = username;
            _password = password;

            PlayFabClientAPI.RegisterPlayFabUser(new RegisterPlayFabUserRequest
            {
                Email = email,
                Username = username,
                DisplayName = username,
                Password = password,
                RequireBothUsernameAndEmail = true,
            }, OnRegistrationSuccess, DisplayPlayFabError);
        }

        public void LoginPlayer(string email, string password)
        {
            if (IsLoggedIn)
            {
                LogoutPlayer();
            }

            _email = email;
            _password = password;

            PlayFabClientAPI.LoginWithEmailAddress(new LoginWithEmailAddressRequest
            {
                Email = email,
                Password = password,
            }, OnLoginSuccess, DisplayPlayFabError);
        }

        public void LogoutPlayer()
        {
            _email = string.Empty;
            _password = string.Empty;

            PlayFabClientAPI.ForgetAllCredentials();
        }

        public static void Ping()
        {
            var currentTime = TimeHelperUtils.GetTime().ToString();

            PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string>()
                {
                    { "LastMessageTime" , currentTime.ToString() },
                }
            }, result =>
            {
                Debug.Log("Player Ping updated");
            }, DisplayPlayFabError);

        }

        public static void CheckOnlineStatus()
        {
            PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
            {
                FunctionName = "SetPlayerOnlineStatus",
                FunctionParameter = default,
                GeneratePlayStreamEvent = true,
            }, result =>
            {
                Debug.Log("Ping Sent");
            }, DisplayPlayFabError);
        }

        private void OnRegistrationSuccess(RegisterPlayFabUserResult result)
        {
            Debug.Log("Registration Successful");
            LoginPlayer(_email, _password);
        }
    
        private void OnLoginSuccess(LoginResult result)
        {
            Debug.Log("Login Successful");
            _playfabID = result.PlayFabId;

            UpdateMetaData();
            //PlayerAlertManager.SetRecurringNotificationTask();
        }

        private void UpdateMetaData()
        {
            PlayFabClientAPI.GetTitleData(new GetTitleDataRequest
            {
                Keys = new List<string>() { "OnlineThreshold" },
            }, result =>
            {
                Debug.Log("Online threshold fetched");
                result.Data.TryGetValue("OnlineThreshold", out var res);
                OnlineThreshold = Convert.ToInt32(res);
                Debug.Log(OnlineThreshold);
            }, DisplayPlayFabError);
        }

        private static void DisplayPlayFabError(PlayFabError error)
        {
            Debug.Log(error.GenerateErrorReport());

            // Run any exception logic here
        }
    }
}
