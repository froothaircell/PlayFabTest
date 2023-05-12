using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FriendIdType { PlayFabId, Username, Email, DisplayName };

public class PF_FriendManager : MonoBehaviour
{
    private List<FriendInfo> _friendList;

    #region Public Calls
    public void GetFriends()
    {
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest
        {
            ExternalPlatformFriends = ExternalFriendSources.None,
            XboxToken = null
        },
        result =>
        {
            _friendList = new List<FriendInfo>(result.Friends);
            OnFriendListFetched(_friendList);
        },
        DisplayPlayFabError);
    }

    public FriendInfo SelectFriend(string playFabId)
    {
        if (_friendList == null || _friendList.Count <= 0)
        {
            return null;
        }

        return _friendList.Find(x => x.FriendPlayFabId == playFabId);
    }

    public void AddFriend(string friendId, FriendIdType idType = FriendIdType.PlayFabId)
    {
        var request = new AddFriendRequest();
        switch (idType)
        {
            case FriendIdType.PlayFabId:
                request.FriendPlayFabId = friendId;
                break;
            case FriendIdType.Username:
                request.FriendUsername = friendId;
                break;
            case FriendIdType.Email:
                request.FriendEmail = friendId;
                break;
            case FriendIdType.DisplayName:
                request.FriendTitleDisplayName = friendId;
                break;
        }

        PlayFabClientAPI.AddFriend(request, result =>
        {
            Debug.Log("Friend Added Successfully");
            GetFriends();
        }, DisplayPlayFabError);
    }

    public void RemoveFriend(FriendInfo friendInfo)
    {
        if (!_friendList.Contains(friendInfo))
        {
            DisplayError($"{friendInfo.Username} is not present in the current friend list");
        }

        PlayFabClientAPI.RemoveFriend(new RemoveFriendRequest
        {
            FriendPlayFabId = friendInfo.FriendPlayFabId,
        }, result =>
        {
            _friendList.Remove(friendInfo);
        }, DisplayPlayFabError);
    }
    #endregion

    #region Internal Functions
    private void OnFriendListFetched(List<FriendInfo> friendsList)
    {
        Debug.Log("Displaying Friend List");
        friendsList.ForEach(f => Debug.Log(f.FriendPlayFabId));

        // Run any logic related to fetching friends here
    }

    private void DisplayPlayFabError(PlayFabError error) 
    { 
        Debug.Log(error.GenerateErrorReport());

        // Run any exception logic here
    }

    private void DisplayError(string error) 
    {
        Debug.LogError(error); 
    }
    #endregion
}
