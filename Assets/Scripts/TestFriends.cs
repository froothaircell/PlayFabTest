using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestFriends : MonoBehaviour
{
    [SerializeField]
    private PF_FriendManager _friendManager;

    [SerializeField]
    private string _email = string.Empty;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void GetFriends()
    {
        if (_friendManager != null)
        {
            _friendManager.GetFriends();
        }
    }

    public void AddPlayer()
    {
        if (_friendManager != null)
        {
            _friendManager.AddFriend(_email, FriendIdType.Email);
        }
    }

    public void RemovePlayer()
    {
        if (_friendManager != null)
        {
            //_friendManager.SelectFriend()
            //_friendManager.RemoveFriend()
        }
    }
}
