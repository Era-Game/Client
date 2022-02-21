using UnityEngine;
using System.Collections;
using System;

namespace Model
{
    [Serializable]
    public class User
    {
        /*
         * {
         *  "id":3,
         *  "username":"hardywang@gmail.com",
         *  "email":"hardywang@gmail.com",
         *  "status":"activated",
         *  "org_name":"",
         *  "dept_name":"",
         *  "DefaultSkinId":0,
         *  "Coins":0,
         *  "DefaultPetId":0,
         *  "ProfileImageURL":"",
         *  "ownedSkins":[],
         *  "ownedPets":[],
         *  "IsOnline":"",
         *  "IsInGame":"",
         *  "Steps":"",
         *  "teamCode":"",
         *  "skillsActivated":[],
         *  "gameStatus_gameName":"",
         *  "gameStatus_status":""
         *  }
         */
        // default value
        public int id;
        public string username;
        public string email;
        public string status;

        public string org_name;
        public string dept_name;
        public int DefaultSkinId;
        public int Coins;
        public int DefaultPetId;
        public string ProfileImageURL;

        // TODO: These runtime data should be put it into PlayerManager and save in PlayerPref
        public int[] ownedSkins;
        public int[] ownedPets;

        public string IsOnline;
        public string IsInGame;
        public string Steps;
        public string teamCode;

        public string[] skillsActivated;
        public string gameStatus_gameName;
        public string gameStatus_status;

    }
}
