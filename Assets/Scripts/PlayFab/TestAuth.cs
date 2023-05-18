using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAuth : MonoBehaviour
{
    [SerializeField]
    private string _email;
    [SerializeField]
    private string _username;
    [SerializeField]
    private string _password;

    [SerializeField]
    private LoginManager _loginManager;

    public void Register()
    {
        if (_loginManager != null)
        {
            _loginManager.RegisterPlayer(_email, _username, _password);
        }
    }

    public void Login()
    {
        if (_loginManager != null)
        {
            _loginManager.LoginPlayer(_email, _password);
        }
    }

    public void Logout()
    {
        if (_loginManager != null)
        {
            _loginManager.LogoutPlayer();
        }
    }
}
