using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public struct PlayerData
{
    private string COINS;
    private string GAME_NAME;
    private string GAME_STATUS;
    private string MEMBER_ID;
    private string SKIN_ID;
    private string USERNAME;
}

[Serializable]
public struct TeamData
{
    public string teamCode;
    public string gameID;
    public string gameType;
    public int memberCount;
    public string[] memberIDs;
    public string[] nameList;
    public string[] skinIDList;
    public string[] isOnlineList;
    public string[] profileImageUrlList;
    public string teamname;
    public bool isLeader;
}

[Serializable]
public struct InGameData {
    public string gameID;
    public float totalSteps;
    public float distance;
    public float velocity;
    public float bonus;
    public bool gameStart;
    public bool gameEnd;
    public string winnerTeamID;
    public bool myTurn;
    public string runner;
    public int relay_totalsteps;
    public int relay_steps_threshold;
    public int relay_map;
}

[Serializable]
public struct QueueData
{
    public string[] teamNameList;
    public bool[] teamReadyStatus;
}

[Serializable]
public struct SpectatorData
{
    public bool gameEnd;
    public bool gameStart;
    public string winnerTeamID;
    public int relay_totalsteps;
    public int relay_steps_threshold;
    public int relay_map;
}

[Serializable]
public struct SpectatorTeamData 
{
    public string isReady;
    public string[] nameList; 
}


