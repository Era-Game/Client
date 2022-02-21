using System;
using System.Collections;
using System.Collections.Generic;
using Model;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

/// <summary>
/// PlayerManager stores individual user data
/// </summary>
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;

    public bool isRejoin = false;

    private User player;

    //Private Member Variables -> Data of individual user data
    private string UID;
    private string Username;
    private string IsOnline;
    private string IsInGame;
    private string Coins;
    private string Steps;
    private string SkinID;
    private string teamCode;
    private string PetID;
    private string Profile_Image_URL;
    private string[] ownedSkins;
    private string[] ownedPets;
    private string[] skillsActivated;
    private string gameStatus_gameName;
    private string gameStatus_status;

    //Private Member Variables -> Manager Utilities
    private bool dataLoaded;
    private bool loggedIn;
    private bool lobbyLoaded;
    private bool gotSomeData;
    private bool gettingData;

    long _statusCode;
    string _responseMessage;

    //Public Variables -> Public Constants
    public int NUM_SKINS = 9;
    public int NUM_PETS = 4;
    public int MAX_SKILL_NUM = 3;

    private void Start()
    {
        Debug.Log("[PlayerManager] Start");
        InvokeRepeating("syncData", 5f, 2f);
        dataLoaded = false;
        loggedIn = false;
        lobbyLoaded = false;
        gotSomeData = false;
        gettingData = false;
        ownedSkins = new string[NUM_SKINS];
    }

    public void SetPlayer(User player)
    {
        this.player = player;
    }
    public User GetPlayer()
    {
        return player; 
    }

    private void Awake()
    {
        isRejoin = false;
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Update_Debug_Block()
    {
        Debug.Log("Bool Status: " + loggedIn.ToString() + " | " + dataLoaded.ToString() + " | " + lobbyLoaded.ToString());

        if (SkinID != null)
        {
            Debug.Log("SkinID: " + SkinID.ToString());
        }
        else
        {
            Debug.Log("SkinID: null");
        }

        if (SkinID == null)
        {
            Debug.Log("Player skinID not ready...");
        }

        if (Username == null)
        {
            Debug.Log("Player username not ready...");
        }

        if (IsOnline == null)
        {
            Debug.Log("Player online status not ready...");
        }

        if (IsInGame == null)
        {
            Debug.Log("Player inGame status not ready...");
        }

        if (Coins == null)
        {
            Debug.Log("Player coins data not ready...");
        }

        if (Steps == null)
        {
            Debug.Log("Player steps data not ready...");
        }

        if (UID == null)
        {
            Debug.Log("Player UID data not ready...");
        }

        if (ownedSkins[0] == null)
        {
            Debug.Log("Player owned skins array not ready...");
        }

        if (PetID == null)
        {
            Debug.Log("Player Pet ID data not ready...");
        }

        if (ownedPets[0] == null)
        {
            Debug.Log("Player owned pets array not ready...");
        }

        if (teamCode == null)
        {
            Debug.Log("Player team Code not ready...");
        }

        /*if (IsInRelayRace == null)
        {
            Debug.Log("Player inRelayRace status not ready...");
        }*/
        if (gameStatus_gameName == null)
        {
            Debug.Log("gameName in Game Status is not ready");
        }

        if (gameStatus_status == null)
        {
            Debug.Log("status in Game Status is not ready");
        }

        if (Profile_Image_URL == null)
        {
            Debug.Log("Profile Image URL is not set");
        }
    }

    private void initMissingData()
    {
        if (SkinID == null)
        {
            PlayerPrefs.SetString("skinID", player.DefaultSkinId.ToString());
            //FirebaseManager.instance.initSpecificAccountData("skinID");
        }

        if (Username == null)
        {
            PlayerPrefs.SetString("username", player.username);

            //FirebaseManager.instance.initSpecificAccountData("username");
        }

        if (IsOnline == null)
        {
            PlayerPrefs.SetString("isOnline", player.IsOnline);

            //FirebaseManager.instance.initSpecificAccountData("isOnline");
        }

        if (IsInGame == null)
        {
            PlayerPrefs.SetString("isInGame", player.IsInGame);

            //FirebaseManager.instance.initSpecificAccountData("isInGame");
        }

        if (Coins == null)
        {
            PlayerPrefs.SetString("coins", player.Coins.ToString());
            //FirebaseManager.instance.initSpecificAccountData("coins");
        }

        if (Steps == null)
        {
            PlayerPrefs.SetString("steps", player.Steps);

            //FirebaseManager.instance.initSpecificAccountData("steps");
        }

        if (UID == null)
        {
            PlayerPrefs.SetString("uid", player.id.ToString());

            //FirebaseManager.instance.initSpecificAccountData("uid");
        }

        if (ownedSkins[0] == null)
        {
            for (int i = 0; i < NUM_SKINS && ownedSkins.Length != 0; i++)
            {
                if (i < ownedSkins.Length && ownedSkins[i] != null)
                    PlayerPrefs.SetString("ownedSkins" + i, player.ownedSkins[i].ToString());
            }

            //FirebaseManager.instance.initSpecificAccountData("ownedSkins");
        }

        if (PetID == null)
        {
            PlayerPrefs.SetString("petID", player.DefaultPetId.ToString());

            //FirebaseManager.instance.initSpecificAccountData("petID");
        }

        if (ownedPets[0] == null)
        {
            for (int i = 0; i < NUM_PETS && ownedPets.Length != 0; i++)
            {
                if (i < ownedPets.Length && ownedPets[i] != null)
                    PlayerPrefs.SetString("ownedPets", player.ownedPets[i].ToString());

            }
            //FirebaseManager.instance.initSpecificAccountData("ownedPets");
        }

        if (teamCode == null)
        {
            PlayerPrefs.SetString("teamCode", player.teamCode);

            //FirebaseManager.instance.initSpecificAccountData("teamCode");
        }

        if (gameStatus_gameName == null)
        {
            PlayerPrefs.SetString("gameName", player.gameStatus_gameName);

            //FirebaseManager.instance.initSpecificAccountData("gameName");
        }

        if (gameStatus_status == null)
        {
            PlayerPrefs.SetString("gameStatus", player.gameStatus_status);
            //FirebaseManager.instance.initSpecificAccountData("gameStatus");
        }
        /*if (IsInRelayRace == null)
        {
            FirebaseManager.instance.initSpecificAccountData("isInRelayRace");
        }*/

        if (Profile_Image_URL == null)
        {
            PlayerPrefs.SetString("Profile_Image_URL", player.ProfileImageURL);

            //FirebaseManager.instance.initSpecificAccountData("Profile_Image_URL");
        }
    }
    void Update()
    {
        //Debug Block:
        //Update_Debug_Block();
        

        if (!dataLoaded && gotSomeData)
        {
            initMissingData();
        }

        if (loggedIn && (SkinID != null || Username != null || IsOnline != null || IsInGame != null
            || Coins != null || Steps != null || UID != null || ownedSkins[0] != null
            || PetID != null || ownedPets[0] != null || teamCode != null || gameStatus_gameName != null
            || gameStatus_status != null || Profile_Image_URL != null))
        {
            gotSomeData = true;
        }

        if (loggedIn && (SkinID != null && Username != null && IsOnline != null && IsInGame != null
            && Coins != null && Steps != null && UID != null && ownedSkins[0] != null
            && PetID != null && ownedPets[0] != null && teamCode != null && gameStatus_gameName != null
            && gameStatus_status != null && Profile_Image_URL != null))
        {
            dataLoaded = true;
        }

        if (!gettingData && !dataLoaded && loggedIn)
        {
            //StartCoroutine(PlayerPrefsHelper.GetDefaultData());
            //StartCoroutine(getDataFromFirebase());
        }

        if (dataLoaded && loggedIn && !lobbyLoaded)
        {
            StartCoroutine(loadLobby());
        }
    }

    private IEnumerator getDataFromFirebase()
    {
        gettingData = true;
        yield return new WaitForEndOfFrame();

        if (!dataLoaded && loggedIn)
        {
            //Debug.Log("Getting Data from Firebase");
            FirebaseManager.instance.getData();

            while (!FirebaseManager.instance.isDataRecieved())
            {
                yield return null;
            }
        }
        gettingData = false;
    }

    private IEnumerator loadLobby()
    {
        if (dataLoaded && loggedIn && !lobbyLoaded)
        {
            lobbyLoaded = true;

            yield return new WaitForEndOfFrame();

            API.instance.Rejoin_Check(UID);

            fix_data();

            while (!API.instance.dataRecieved)
            {
                yield return null;
            }

            _statusCode = API.instance.statusCode;
            _responseMessage = API.instance.responseMessage;


            if (_statusCode == 200)
            {
                if (_responseMessage != "Lobby")
                {
                    isRejoin = true;
                    API.instance.Update_Team_Data(teamCode, UID);

                    while (!API.instance.dataRecieved)
                    {
                        yield return null;
                    }

                    if (_responseMessage == "Relay Game")
                    {
                        API.instance.Update_InGame_Data(teamCode, UID);

                        while (!API.instance.dataRecieved)
                        {
                            yield return null;
                        }
                    }
                }
                else
                {
                    PlayerManager.instance.setTeamCode("placeholder");
                    PlayerManager.instance.setGameStatus("placeholder", "placeholder");
                }

                FirebaseManager.instance.readOrganizationExist(UID);

                while (!FirebaseManager.instance.isDataRecieved())
                {
                    yield return null;
                }

                if (FirebaseManager.instance.organizationExist())
                {
                    LevelLoader.instance.loadScene(_responseMessage);
                }
                else
                {
                    LevelLoader.instance.loadScene("Organization Menu");
                }

            }
        }
    }

    private void fix_data()
    {
        if (ownedSkins.Length < NUM_SKINS)
        {
            Debug.Log("Fix Data in Player Manager");
            bool[] arr = new bool[ownedSkins.Length];

            for (int i = 0; i < ownedSkins.Length; ++i)
            {
                arr[i] = ownedSkins[i] == "True" ? true : false;
            }
            FirebaseManager.instance.fix_ownedSkins_data(arr);
        }
    }

    public void setLoggedIn(bool logic)
    {
        loggedIn = logic;
    }

    public void initPlayManagerUtilities()
    {
        Username = null;
        IsOnline = null;
        IsInGame = null;
        Coins = null;
        Steps = null;
        UID = null;
        SkinID = null;
        ownedSkins = new string[NUM_SKINS];
        PetID = null;
        ownedPets = new string[NUM_PETS];
        skillsActivated = new string[MAX_SKILL_NUM];
        teamCode = null;
        gameStatus_gameName = null;
        gameStatus_status = null;
        //IsInRelayRace = null;
        Profile_Image_URL = null;

        dataLoaded = false;
        loggedIn = false;
        lobbyLoaded = false;
        gotSomeData = false;
    }

    public void setGameStatus(string gameName, string status)
    {
        gameStatus_gameName = gameName;
        gameStatus_status = status;
        //FirebaseManager.instance.setGameStatus(gameStatus_gameName, gameStatus_status);
    }

    public void setUsername(string str)
    {
        Username = str;
    }

    public void setIsOnline(string str)
    {
        IsOnline = str;
        bool online_status = str == "True" ? true : false;
        //FirebaseManager.instance.setIsOnline(online_status);
    }

    public void setIsInGame(string str)
    {
        IsInGame = str;
        bool in_Game_status = str == "True" ? true : false;
        //FirebaseManager.instance.setIsInGame(in_Game_status);
    }

    public void addCoins(string str)
    {
        Coins = (int.Parse(Coins) + int.Parse(str)).ToString();
        int amount = int.Parse(str);
        //FirebaseManager.instance.addCoins(amount);
    }
    public void setCoins(string str)
    {
        Coins = str;
        int amount = int.Parse(str);
        //FirebaseManager.instance.setCoins(amount);
    }

    public void addSteps(string str)
    {
        Steps = str;
        Steps = (int.Parse(Steps) + int.Parse(str)).ToString();
        int amount = int.Parse(str);
        //FirebaseManager.instance.addSteps(amount);
    }
    public void setSteps(string str)
    {
        Steps = str;
        int amount = int.Parse(str);
        //FirebaseManager.instance.setSteps(amount);
    }


    public void setUID(string str)
    {
        UID = str;
    }

    public void setSkinID(string str)
    {
        SkinID = str;
        int ID = int.Parse(str);
        //FirebaseManager.instance.setSkinID(ID);
    }

    private void debug_log_array(string[] str)
    {
        string debugStr = "";
        for (int i = 0; i < str.Length - 1; ++i)
        {
            debugStr += str[i];
            debugStr += " | ";
        }
        debugStr += str[str.Length - 1];
        Debug.Log("Owned Skin Array In database looks like: \n[" + debugStr + "]");
    }

    public void setOwnedSkins(string[] str)
    {
        //debug_log_array(str);
        ownedSkins = str;
    }
    public bool ownSkin(int index)
    {
        if (index > NUM_SKINS)
        {
            Debug.LogError("Skin ID Does not exist.");
        }
        return ownedSkins[index] == "True" ? true : false;
    }

    public void buySkin(int ID)
    {
        ownedSkins[ID] = "True";
        //FirebaseManager.instance.addSkinByID(ID);
    }

    public void setPetID(string str)
    {
        PetID = str;
        int ID = int.Parse(str);
        //FirebaseManager.instance.setPetID(ID);
    }

    public void setOwnedPets(string[] str)
    {
        //debug_log_array(str);
        ownedPets = str;
    }

    public bool ownPet(int index)
    {
        if (index > NUM_PETS)
        {
            Debug.LogError("Pet ID Does not exist.");
        }
        return ownedPets[index] == "True" ? true : false;
    }

    public void buyPet(int ID)
    {
        ownedPets[ID] = "True";
        //FirebaseManager.instance.addPetByID(ID);
    }

    public void setTeamCode(string str)
    {
        teamCode = str;
        //FirebaseManager.instance.setTeamCode(str);
    }

    public void setProfile_Image_URL(string str)
    {
        Profile_Image_URL = str;
    }
    /*public void setIsInRelayRace(string str)
    {
        IsInRelayRace = str;
        bool in_Relay_Race_status = str == "True" ? true : false;
        FirebaseManager.instance.setIsInRelayRace(in_Relay_Race_status);
    }*/

    /// <summary>
    /// Avaliable Data Types (Enter as string "<Data Type>"): 
    /// 1. username
    /// 2. online status
    /// 3. in game status
    /// 4. coins
    /// 5. steps
    /// 6. uid
    /// 7. skinID
    /// 8. petID
    /// 9. teamCode
    /// 10. gameName
    /// 11. gameStatus
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public string getData(string str)
    {
        if (str == "username")
        {
            return player.username;
        }
        else if (str == "online status")
        {
            return player.IsOnline;
        }
        else if (str == "in game status")
        {
            return player.gameStatus_status;
        }
        else if (str == "coins")
        {
            return player.Coins.ToString();
        }
        else if (str == "steps")
        {
            return player.Steps;
        }
        else if (str == "uid")
        {
            return player.id.ToString();
        }
        else if (str == "skinID")
        {
            return player.DefaultSkinId.ToString();
        }
        else if (str == "petID")
        {
            return player.DefaultPetId.ToString();
        }
        else if (str == "teamCode")
        {
            return player.teamCode;
        }
        else if (str == "gameName")
        {
            return player.gameStatus_gameName;
        }
        else if (str == "gameStatus")
        {
            return gameStatus_status;
        }
        else if (str == "Profile_Image_URL")
        {
            return player.ProfileImageURL;
        }
        else
        {
            Debug.Log("Invalid Input.");
            return "";
        }
    }
    public User GetUser()
    {
        return player;
    }

    private void syncData()
    {
        if (loggedIn)
        {
            PlayerPrefsHelper.GetDefaultData();
            //FirebaseManager.instance.getData();
        }

    }
    public string[] getOwnedSkins()
    {
        return ownedSkins;
    }

    public string[] getOwnedPets()
    {
        return ownedPets;
    }

    // PlayerPrefs store data in local 
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            PlayerPrefs.SetString("uid", UID);
            PlayerPrefs.SetString("username", Username);
            PlayerPrefs.SetString("isonline", IsOnline);
            PlayerPrefs.SetString("isingame", IsInGame);
            PlayerPrefs.SetString("coins", Coins);
            PlayerPrefs.SetString("steps", Steps);
            PlayerPrefs.SetString("skinid", SkinID);
            PlayerPrefs.SetString("teamcode", teamCode);
            PlayerPrefs.SetString("petid", PetID);

            for (int i = 0; i < ownedSkins.Length; i++)
            {
                PlayerPrefs.SetString("ownedskin" + i, ownedSkins[i]);
            }
            for (int i = 0; i < ownedPets.Length; i++)
            {
                PlayerPrefs.SetString("ownedpets" + i, ownedPets[i]);
            }
            for (int i = 0; i < skillsActivated.Length; i++)
            {
                PlayerPrefs.SetString("skillsactivated" + i, skillsActivated[i]);
            }
        }
    }

}
