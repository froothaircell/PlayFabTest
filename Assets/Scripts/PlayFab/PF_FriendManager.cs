using PlayFab;
using PlayFab.ClientModels;
using PlayFabTests;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayFabTests
{
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

        public void AddFriend(string friendId, FriendIdType idType)
        {
            SearchUser(idType, friendId, UserInfo =>
            {
                if (UserInfo != null)
                {
                    var friendPlayFabId = UserInfo.PlayFabId;

                    PlayerAlertManager.SendNotification(friendPlayFabId, new PlayerAlert("Friend Request", true, "You have received a new friend request", ""));

                    //PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
                    //{
                    //    FunctionName = "SendFriendRequest",
                    //    FunctionParameter = new { FriendPlayFabId = friendPlayFabId },
                    //    GeneratePlayStreamEvent = true
                    //}, 
                    //result => 
                    //{
                    //    Debug.Log("Friend Added Successfully");
                    //    GetFriends();
                    //}, DisplayPlayFabError);
                }
            });
        }

        public void RemoveFriend(string friendId, FriendIdType idType)
        {
            SearchUser(idType, friendId, UserInfo =>
            {
                if (UserInfo != null)
                {
                    var friendPlayFabId = UserInfo.PlayFabId;
                
                    // Need to validate if the friend exists in our list
                    if (!_friendList.Exists(x => x.FriendPlayFabId == friendPlayFabId))
                    {
                        Debug.LogError("User does not exist in friend list");
                        return;
                    }

                    PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
                    {
                        FunctionName = "RemoveFriends",
                        FunctionParameter = new { FriendPlayFabId = friendPlayFabId },
                        GeneratePlayStreamEvent = true,
                    }, 
                    result =>
                    {
                        Debug.Log("Friend Removed Successfully");
                        GetFriends();
                    }, DisplayPlayFabError);
                }
            });
        }

        public void ApproveFriendRequest(string friendId, FriendIdType idType)
        {
            SearchUser(idType, friendId, UserInfo =>
            {
                if (UserInfo != null)
                {
                    var friendPlayFabId = UserInfo.PlayFabId;

                    // Need to validate if the friend exists in our list
                    if (!_friendList.Exists(x => x.FriendPlayFabId == friendPlayFabId))
                    {
                        Debug.LogError("User does not exist in friend list");
                        return;
                    }

                    PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
                    {
                        FunctionName = "AcceptFriendRequest",
                        FunctionParameter = new { FriendPlayFabId = friendPlayFabId },
                        GeneratePlayStreamEvent = true,
                    },
                    result =>
                    {
                        Debug.Log("Friend Request Accepted Successfully");
                        GetFriends();
                    }, DisplayPlayFabError);
                }
            });
        }

        public void RejectFriendRequest(string friendId, FriendIdType idType)
        {
            SearchUser(idType, friendId, UserInfo =>
            {
                if (UserInfo != null)
                {
                    var friendPlayFabId = UserInfo.PlayFabId;

                    if (!_friendList.Exists(x => x.FriendPlayFabId == friendPlayFabId))
                    {
                        Debug.LogError("User does not exist in friend list");
                        return;
                    }

                    PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
                    {
                        FunctionName = "DenyFriendRequest",
                        FunctionParameter = new { FriendPlayFabId = friendPlayFabId },
                        GeneratePlayStreamEvent = true,
                    },
                    result =>
                    {
                        Debug.Log("Friend Request Denied Successfully");
                        GetFriends();
                    }, DisplayPlayFabError);
                }
            });
        }
        #endregion

        #region Internal Functions
        private void SearchUser(FriendIdType friendIdType, string userId, Action<UserAccountInfo> onAccountFetched = null)
        {
            switch (friendIdType)
            {
                case FriendIdType.PlayFabId:
                    PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest { PlayFabId = userId },
                    result =>
                    {
                        onAccountFetched?.Invoke(result.AccountInfo);
                        Debug.Log($"Playfab Friends System :: Found friend via PlayFabId: {result.AccountInfo.PlayFabId}");
                    },
                    error =>
                    {
                        Debug.LogError($"Playfab Friends System :: Failed to find friend via PlayFabId: {error.Error}");
                    });
                    break;
                case FriendIdType.Username:
                    PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest { Username = userId },
                    result =>
                    {
                        onAccountFetched?.Invoke(result.AccountInfo);
                        Debug.Log($"Playfab Friends System :: Found friend via Username: {result.AccountInfo.PlayFabId}");
                    },
                    error =>
                    {
                        Debug.LogError($"Playfab Friends System :: Failed to find friend via Username: {error.Error}");
                    });
                    break;
                case FriendIdType.Email:
                    PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest { Email = userId },
                    result =>
                    {
                        onAccountFetched?.Invoke(result.AccountInfo);
                        Debug.Log($"Playfab Friends System :: Found friend via Email: {result.AccountInfo.PlayFabId}");
                    },
                    error =>
                    {
                        Debug.LogError($"Playfab Friends System :: Failed to find friend via Email: {error.Error}");
                    });
                    break;
                case FriendIdType.DisplayName:
                    PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest { TitleDisplayName = userId },
                    result =>
                    {
                        onAccountFetched?.Invoke(result.AccountInfo);
                        Debug.Log($"Playfab Friends System :: Found friend via TitleDisplayName: {result.AccountInfo.PlayFabId}");
                    },
                    error =>
                    {
                        Debug.LogError($"Playfab Friends System :: Failed to find friend via TitleDisplayName: {error.Error}");
                    });
                    break;
            }
        }

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
}
