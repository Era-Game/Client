using System;

[Serializable]
public class URL
{
    private static readonly string Default_URL = "https://us-central1-team-building-game-6bd67.cloudfunctions.net/";
    public static readonly string Test_URL = "https://us-central1-team-building-game-6bd67.cloudfunctions.net/test_HTTP_Request";
    public static readonly string PlayerData_URL = "";

    public static readonly string Create_Team_URL = "https://us-central1-team-building-game-6bd67.cloudfunctions.net/create_team"; //Request POST: teamname, UID, gameType
    public static readonly string Join_Team_URL = "https://us-central1-team-building-game-6bd67.cloudfunctions.net/join_team"; //Request POST: teamCode, UID, gameType
    public static readonly string Leave_Team_URL = "https://us-central1-team-building-game-6bd67.cloudfunctions.net/leave_team"; //Request POST: teamCode, UID
    public static readonly string Team_Data_Update_URL = "https://us-central1-team-building-game-6bd67.cloudfunctions.net/update_team"; //Request POST: teamCode, UID
    public static readonly string WS_Create_Team_URL = "ws://127.0.0.1:8080"; //Request POST: teamname, UID, gameType
    public static readonly string WS_Join_Team_URL = "ws://127.0.0.1:8080"; //Request POST: teamCode, UID, gameType
    public static readonly string WS_Leave_Team_URL = "ws://127.0.0.1:8080"; //Request POST: teamCode, UID
    public static readonly string WS_Team_Data_Update_URL = "ws://127.0.0.1:8080"; //Request POST: teamCode, UID

    public static readonly string Create_Game_URL = Default_URL + "create_game"; //Request POST: teamCode, teamname, UID
    public static readonly string Leave_Game_URL = Default_URL + "leave_game"; //Request POST: gameID, teamCode, UID
    public static readonly string Join_Game_URL = Default_URL + "join_game"; //Request POST: gameID, teamCode, teamname, UID
    public static readonly string Update_InGame_Data_URL = Default_URL + "update_ingame_data"; //Request POST: teamCode, UID
    public static readonly string Post_Steps_URL = Default_URL + "post_steps"; //Request POST: gameID, teamCode, steps, distance, velocity, UID
    public static readonly string Post_isReady_URL = Default_URL + "post_isReady"; //Request POST: gameID, teamCode, isReady_Status, UID
    public static readonly string Post_teamRecords_URL = Default_URL + "post_teamRecords"; //Request POST: gameID, teamCode, username, personalBestTime, UID

    public static readonly string Rejoin_URL = Default_URL + "rejoin"; //Request POST: gameID, teamCode, username, personalBestTime, UID
    public static readonly string Ability_Multiplier = Default_URL + "velocity_multiplier";
    public static readonly string AFK_Handler_URL = Default_URL + "afk_handler";
    public static readonly string Set_Organization_URL = Default_URL + "set_user_organization";

    public static readonly string Update_Queue_URL = Default_URL + "update_queue";
    public static readonly string Update_Spectator_Data = Default_URL + "update_spectator_data";
    public static readonly string Update_Spectator_Team_Data = Default_URL + "spectator_team_data_update"; 
}
