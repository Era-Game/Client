using System;
using UnityEngine;
using Model;

[Flags]
enum StatusOfUser
{
    LOGGED_IN,
    LOGGED_OUT,
    NOT_ACTIVATED,
    BANNED
}

namespace Utils
{
    public enum PlayerData
    {
        AuthStatus,
        Default,
    }

    public static class PlayerPrefsHelper {
        public static string GetAuthStatus()
        {
            Debug.Log("[PlayerPrefsHelper]" + PlayerPrefs.GetString(PlayerData.AuthStatus.ToString("g")));
            return PlayerPrefs.GetString(PlayerData.AuthStatus.ToString("g"));
        }
        public static void SetAuthStatus(string status)
        {
            PlayerPrefs.SetString("AuthStatus", status);
        }

        public static string GetDefaultData()
        {
            Debug.Log("[PlayerPrefsHelper]" + PlayerPrefs.GetString(PlayerData.AuthStatus.ToString("g")));
            return PlayerPrefs.GetString(PlayerData.AuthStatus.ToString("g"));
        }
        public static void SetDefaultData(User user)
        {
            PlayerPrefs.SetString("AuthStatus", JsonUtility.ToJson(user));
        }
    }
}
