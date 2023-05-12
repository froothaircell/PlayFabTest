using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoginManager : MonoBehaviour
{
    public static bool IsLoggedIn => PlayFabClientAPI.IsClientLoggedIn();

    private string _email = string.Empty;
    private string _username = string.Empty;
    private string _password = string.Empty;

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

    private void OnRegistrationSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("Registration Successful");
        LoginPlayer(_email, _password);
    }
    
    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Login Successful");
    }

    private void DisplayPlayFabError(PlayFabError error)
    {
        Debug.Log(error.GenerateErrorReport());

        // Run any exception logic here
    }

}
