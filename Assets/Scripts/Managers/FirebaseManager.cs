using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;

namespace Managers
{

    public class FirebaseManager : MonoBehaviour
    {
        public static FirebaseManager instance;

        public bool gameStart = false;

        //Firebase variables
        [Header("Firebase")]
        public DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
        public FirebaseAuth auth;
        public FirebaseUser User;
        public FirebaseApp app;
        public DatabaseReference DBreference;

        private int[] otherPlayerSteps = new int[2];
        private string otherPlayerUsername;
        private string[] otherPlayerSkinID = new string[2];

        private int normalQueueCount;
        private string[] otherplayer = new string[2]; // in normal queue other player
        private Dictionary<string, int> normalGameData = new Dictionary<string, int>();
        private string[] queueArr = new string[2];
        private bool queueTaskComplete;

        public string winPlayerUsername;
        public int winPlayerSkinID;

        private bool updateLogOut;

        public List<string> gameIDList = new List<string>();
        private ArrayList teamAmountInEachGame = new ArrayList();
        private bool start = false;


        private string guid;

        void Update()
        {

            if (updateLogOut)
            {
                logout();
            }
        }

        public void start_game()
        {
            start = true;
        }

        public void end_game()
        {
            start = false;
        }

        void Start()
        {
            //StartCoroutine(CheckAndFixDependancies());
            createLocalGUID();
        }

        void OnDisable()
        {
            LogOutButton();
        }

        void Awake()
        {
            updateLogOut = false;
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

        private IEnumerator CheckAndFixDependancies()
        {
            var checkAndFixDepenanciesTask = FirebaseApp.CheckAndFixDependenciesAsync();

            yield return new WaitUntil(predicate: () => checkAndFixDepenanciesTask.IsCompleted);

            var dependancyResult = checkAndFixDepenanciesTask.Result;

            if (dependancyResult == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        }

        private IEnumerator CheckAutoLogin()
        {
            yield return new WaitForEndOfFrame();
            if (User != null)
            {
                var reloadUserTask = User.ReloadAsync();
                yield return new WaitUntil(predicate: () => reloadUserTask.IsCompleted);
                AutoLogin();
            }
            else
            {
                StartCoroutine(login_wait_for_start());
                //SceneManager.LoadScene("Login");
                //UIManager.instance.LoginScreen();
            }
        }

        private IEnumerator loading_wait_for_start()
        {
            while (!start)
            {
                yield return null;
            }
            LevelLoader.instance.display_loading_screen();
            PlayerManager.instance.initPlayManagerUtilities();
            PlayerManager.instance.setLoggedIn(true);
            ClearInputFeilds();
            LevelLoader.instance.display_loading_screen();
        }
        private IEnumerator login_wait_for_start()
        {
            while (!start)
            {
                yield return null;
            }
            LevelLoader.instance.display_loading_screen();
            SceneManager.LoadScene("Login");
        }

        private IEnumerator loginProcedure()
        {
            var DBTask = DBreference.Child("users").Child(User.UserId).GetValueAsync();

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else if (DBTask.Result.Value == null)
            {
                //No data exists yet
                Debug.Log("Account does not have existing data, initialzing data...");
                initializeAccount(User.DisplayName);
            }

            //Debug.Log("Loading Lobby...")

            StartCoroutine(loading_wait_for_start());
            //SceneManager.LoadScene("Loading");
        }
        private void AutoLogin()
        {
            if (User != null)
            {
                if (User.IsEmailVerified)
                {
                    StartCoroutine(loginProcedure());
                }
                else
                {
                    StartCoroutine(SendVerificationEmail());
                }
            }
            else
            {
                StartCoroutine(login_wait_for_start());
                //SceneManager.LoadScene("Login");
                //UIManager.instance.LoginScreen();
            }
        }

        private void createLocalGUID()
        {
            // Need to be in server code
            guid = PlayerPrefs.GetString("local_guid");
            if (guid == "")
            {
                guid = System.Guid.NewGuid().ToString();
                PlayerPrefs.SetString("local_guid", guid);
            }
        }

        private void InitializeFirebase()
        {
            Debug.Log("Setting up Firebase Auth");
            //Set the authentication instance object
            app = FirebaseApp.DefaultInstance;
            DBreference = FirebaseDatabase.DefaultInstance.RootReference;
            auth = FirebaseAuth.DefaultInstance;
            StartCoroutine(CheckAutoLogin());

            auth.StateChanged += AuthStateChanged;
            AuthStateChanged(this, null);
        }

        private void AuthStateChanged(object sender, System.EventArgs eventArgs)
        {
            if (auth.CurrentUser != User)
            {
                bool signedIn = User != auth.CurrentUser && auth.CurrentUser != null;

                if (!signedIn && User != null)
                {
                    Debug.Log("Signed Out");
                }

                User = auth.CurrentUser;

                if (signedIn)
                {
                    Debug.Log($"Signed In: {User.DisplayName}");
                }
            }
        }
        public void ClearInputFeilds()
        {
            //LoginManager.instance.clearInputFeilds();
        }

        //Function for the login button
        public void LoginButton()
        {
            //Call the login coroutine passing the email and password
            string _email = LoginManager.instance.getLoginEmail();
            string _password = LoginManager.instance.getLoginPassword();
            Debug.Log("Trying to login, got email: " + _email + " and password: " + _password);
            //StartCoroutine(Login(_email, _password));
            StartCoroutine(newLogin(_email, _password));
        }

        //Function for the register button
        public void RegisterButton()
        {
            //Call the register coroutine passing the email, password, and username
            //StartCoroutine(Register(LoginManager.instance.getRegisterEmail(), LoginManager.instance.getRegisterPassword(), LoginManager.instance.getRegisterUsername()));
            StartCoroutine(newRegister(LoginManager.instance.getRegisterUsername(), LoginManager.instance.getRegisterEmail(), LoginManager.instance.getRegisterPassword(), LoginManager.instance.getRegisterComfirmPassword()));
        }

        //Function for the sign out button
        public void LogOutButton()
        {
            start = false;
            updateLogOut = true;
        }

        private void logout()
        {
            StartCoroutine(logout_coroutine());
        }


        private IEnumerator logout_coroutine()
        {
            StartCoroutine(Update_isOnline(false));
            yield return new WaitForSeconds(0.2f);
            auth.SignOut();

            updateLogOut = false;

        }

        /// <summary>
        /// Set Player Data. "addCoins" and "addSteps" is set to false to overwrite the orginal value.
        /// </summary>
        /// <param name="isInGame"></param>
        /// <param name="addCoins"></param>
        /// <param name="addSteps"></param>
        /// <param name="coins"></param>
        /// <param name="steps"></param>
        public void setData(bool isInGame, bool addCoins, bool addSteps, int coins, int steps)
        {
            StartCoroutine(Update_isInGame(isInGame));
            StartCoroutine(Update_coins(addCoins, coins));
            StartCoroutine(Update_steps(addSteps, steps));
        }

        public void setIsOnline(bool isOnline)
        {
            StartCoroutine(Update_isOnline(isOnline));
        }

        public void setIsInGame(bool isInGame)
        {
            StartCoroutine(Update_isInGame(isInGame));
        }

        public void addCoins(int amount)
        {
            StartCoroutine(Update_coins(true, amount));
        }

        public void addSteps(int amount)
        {
            StartCoroutine(Update_steps(true, amount));
        }

        public void setCoins(int amount)
        {
            StartCoroutine(Update_coins(false, amount));
        }

        public void setSteps(int amount)
        {
            StartCoroutine(Update_steps(false, amount));
        }

        public string[] getQueueArray()
        {
            StartCoroutine(UpdateQueueArray());
            return queueArr;
        }

        public string[] getQueueArrayFromQueue()
        {
            return queueArr;
        }

        public void setWinnerUsername(string UID)
        {
            StartCoroutine(setWinnerName(UID));
        }

        public string getOtherPlayerSkinID(string UID, int index)
        {
            StartCoroutine(getOtherSkinID(UID, index));
            return otherPlayerSkinID[index];
        }

        public void setSkinID(int skinID)
        {
            StartCoroutine(Update_skinID(skinID));
        }

        public void addSkinByID(int skinID)
        {
            StartCoroutine(add_Skin(skinID));
        }

        public void setPetID(int petID)
        {
            StartCoroutine(Update_petID(petID));
        }

        public void addPetByID(int petID)
        {
            StartCoroutine(add_Pet(petID));
        }
        public void setOwnedSkins(bool[] ownedSkins)
        {
            StartCoroutine(Update_ownedSkins(ownedSkins));
        }
        public void setQueueArr(string[] arr)
        {
            queueArr = arr;
        }

        public void setGameStatus(string gameName, string gameStatus)
        {
            StartCoroutine(Update_gameStatus(gameName, gameStatus));
        }

        public void getData()
        {
            StartCoroutine(LoadUserData());
        }

        public int getStepsByUID(string UID)
        {
            return normalGameData[UID];
        }

        public string getUsernameByUID(string UID)
        {
            StartCoroutine(getOtherPlayerUsername(UID));
            return otherPlayerUsername;
        }

        private IEnumerator newLogin(string _email, string _password)
        {
            Credential credential = EmailAuthProvider.GetCredential(_email, _password);

            var LoginTask = auth.SignInWithCredentialAsync(credential);

            yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

            if (LoginTask.Exception != null)
            {
                //If there are errors handle them
                Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
                FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Login Failed!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WrongPassword:
                        message = "Wrong Password";
                        break;
                    case AuthError.InvalidEmail:
                        message = "Invalid Email";
                        break;
                    case AuthError.UserNotFound:
                        message = "Account does not exist";
                        break;
                }
                Debug.Log("Unable to login because: " + message);
                LoginManager.instance.update_warningLoginText(message);
                UIManager.instance.displayWarning("Warning", message);
            }
            else
            {
                if (User.IsEmailVerified)
                {
                    var DBTask = DBreference.Child("users").Child(User.UserId).GetValueAsync();

                    yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

                    if (DBTask.Exception != null)
                    {
                        Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
                    }
                    else if (DBTask.Result.Value == null)
                    {
                        //No data exists yet
                        Debug.Log("Account does not have existing data, initialzing data...");
                        initializeAccount(User.DisplayName);
                    }

                    UIManager.instance.displayWarning("Successs", "Welcome Back " + User.DisplayName + "!");
                    //Debug.Log("Loading Lobby...");
                    PlayerManager.instance.initPlayManagerUtilities();
                    PlayerManager.instance.setLoggedIn(true);
                    ClearInputFeilds();
                    yield return new WaitForSeconds(1.5f);
                    LevelLoader.instance.display_loading_screen();
                }
                else
                {
                    StartCoroutine(SendVerificationEmail());
                }
            }
        }

        private IEnumerator newRegister(string _username, string _email, string _password, string _confirmPassword)
        {
            if (_username == "")
            {
                UIManager.instance.displayWarning("Warning", "Missing Username");
            }
            else if (_username.ToLower() == "bad word")
            {
                UIManager.instance.displayWarning("Warning", "This username is inappropriate");
            }
            else if (_password != _confirmPassword)
            {
                UIManager.instance.displayWarning("Warning", "Password does not match");
            }
            else
            {
                var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);

                yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

                if (RegisterTask.Exception != null)
                {
                    Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                    FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                    AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                    string message = "Register Failed!";
                    switch (errorCode)
                    {
                        case AuthError.MissingEmail:
                            message = "Missing Email";
                            break;
                        case AuthError.MissingPassword:
                            message = "Missing Password";
                            break;
                        case AuthError.WeakPassword:
                            message = "Weak Password";
                            break;
                        case AuthError.EmailAlreadyInUse:
                            message = "Email Already In Use";
                            break;
                    }
                    UIManager.instance.displayWarning("Warning", message);
                }
                else
                {
                    UserProfile profile = new UserProfile { DisplayName = _username };

                    var defaultUserTask = User.UpdateUserProfileAsync(profile);

                    yield return new WaitUntil(predicate: () => defaultUserTask.IsCompleted);

                    if (defaultUserTask.Exception != null)
                    {
                        User.DeleteAsync();

                        FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                        string message = "Unknown Error, Please Try Again";
                        switch (errorCode)
                        {
                            case AuthError.Cancelled:
                                message = "Update User Cancelled";
                                break;
                            case AuthError.SessionExpired:
                                message = "Session Expired";
                                break;
                        }
                        UIManager.instance.displayWarning("Warning", message);
                    }
                    else
                    {
                        StartCoroutine(SendVerificationEmail());
                    }

                }
            }
        }
        private IEnumerator Register(string _email, string _password, string _username)
        {
            if (_username == "")
            {
                //If the username field is blank show a warning
                UIManager.instance.displayWarning("Warning", "Missing Username");

            }
            else if (LoginManager.instance.getRegisterPassword() != LoginManager.instance.getRegisterComfirmPassword())
            {
                //If the password does not match show a warning
                UIManager.instance.displayWarning("Warning", "Password Does Not Match!");
            }
            else
            {
                //Call the Firebase auth signin function passing the email and password
                var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
                //Wait until the task completes
                yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

                if (RegisterTask.Exception != null)
                {
                    //If there are errors handle them
                    Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                    FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                    AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                    string message = "Register Failed!";
                    switch (errorCode)
                    {
                        case AuthError.MissingEmail:
                            message = "Missing Email";
                            break;
                        case AuthError.MissingPassword:
                            message = "Missing Password";
                            break;
                        case AuthError.WeakPassword:
                            message = "Weak Password";
                            break;
                        case AuthError.EmailAlreadyInUse:
                            message = "Email Already In Use";
                            break;
                    }
                    UIManager.instance.displayWarning("Warning", message);
                }
                else
                {
                    //User has now been created
                    //Now get the result
                    User = RegisterTask.Result;

                    if (User != null)
                    {
                        //Create a user profile and set the username
                        UserProfile profile = new UserProfile { DisplayName = _username };

                        //Call the Firebase auth update user profile function passing the profile with the username
                        var ProfileTask = User.UpdateUserProfileAsync(profile);
                        //Wait until the task completes
                        yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                        if (ProfileTask.Exception != null)
                        {
                            //If there are errors handle them
                            Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                            UIManager.instance.displayWarning("Warning", "Username Set Failded!");
                        }
                        else
                        {
                            //Username is now set
                            //Now return to login screen
                            ClearInputFeilds();
                            UIManager.instance.LoginScreen();

                        }
                    }
                }
            }
        }

        public IEnumerator SendVerificationEmail()
        {
            if (User != null)
            {
                var emailTask = User.SendEmailVerificationAsync();

                yield return new WaitUntil(predicate: () => emailTask.IsCompleted);

                if (emailTask.Exception != null)
                {
                    Debug.LogWarning(message: $"Failed to register task with {emailTask.Exception}");
                    FirebaseException firebaseEx = emailTask.Exception.GetBaseException() as FirebaseException;
                    AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                    string message = "Unknown Error, Please Try Again";
                    switch (errorCode)
                    {
                        case AuthError.Cancelled:
                            message = "Verification Task was Cancelled";
                            break;
                        case AuthError.InvalidRecipientEmail:
                            message = "Invalid Email";
                            break;
                        case AuthError.TooManyRequests:
                            message = "Too Many Requests";
                            break;
                    }
                    UIManager.instance.AwaitVerification(false, User.Email, message);
                }
                else
                {
                    UIManager.instance.AwaitVerification(true, User.Email, null);
                    Debug.Log("Email Sent Successfully");
                }
            }
        }
        private IEnumerator UpdateUsernameAuth(string _username)
        {
            //Create a user profile and set the username
            UserProfile profile = new UserProfile { DisplayName = _username };

            //Call the Firebase auth update user profile function passing the profile with the username
            var ProfileTask = User.UpdateUserProfileAsync(profile);
            //Wait until the task completes
            yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

            if (ProfileTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
            }
            else
            {
                //Auth username is now updated
            }
        }

        private IEnumerator UpdateUsernameDatabase(string _username)
        {
            //Set the currently logged in user username in the database
            var DBTask = DBreference.Child("users").Child(User.UserId).Child("username").SetValueAsync(_username);

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else
            {
                //Database username is now updated
            }
        }


        private IEnumerator LoadUserData()
        {
            Debug.Log("[PlayerManager] Load User Data called.");
            dataRecieved = false;

            //Get the currently logged in user data
            var DBTask = DBreference.Child("users").Child(User.UserId).GetValueAsync();

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else
            {
                //Data has been retrieved
                DataSnapshot snapshot = DBTask.Result;
                if (snapshot.Child("username").Value != null)
                {
                    PlayerManager.instance.setUsername(snapshot.Child("username").Value.ToString());
                }

                if (snapshot.Child("isOnline").Value != null)
                {
                    PlayerManager.instance.setIsOnline(snapshot.Child("isOnline").Value.ToString());
                }

                if (snapshot.Child("isInGame").Value != null)
                {
                    PlayerManager.instance.setIsInGame(snapshot.Child("isInGame").Value.ToString());
                }

                if (snapshot.Child("coins").Value != null)
                {
                    PlayerManager.instance.setCoins(snapshot.Child("coins").Value.ToString());
                }

                if (snapshot.Child("steps").Value != null)
                {
                    PlayerManager.instance.setSteps(snapshot.Child("steps").Value.ToString());
                }

                if (snapshot.Child("uid").Value != null)
                {
                    PlayerManager.instance.setUID(snapshot.Child("uid").Value.ToString());
                }

                if (snapshot.Child("skinID").Value != null)
                {
                    PlayerManager.instance.setSkinID(snapshot.Child("skinID").Value.ToString());
                }

                if (snapshot.Child("petID").Value != null)
                {
                    PlayerManager.instance.setPetID(snapshot.Child("petID").Value.ToString());
                }

                if (snapshot.Child("Profile_Image_URL").Value != null)
                {
                    PlayerManager.instance.setProfile_Image_URL(snapshot.Child("Profile_Image_URL").Value.ToString());
                }

                string gameName = "placeholder";
                string gameStatus = "placeholder";

                if (snapshot.Child("gameStatus").Child("gameName").Value != null)
                {
                    gameName = snapshot.Child("gameStatus").Child("gameName").Value.ToString();
                }

                if (snapshot.Child("gameStatus").Child("status").Value != null)
                {
                    gameStatus = snapshot.Child("gameStatus").Child("status").Value.ToString();
                }

                PlayerManager.instance.setGameStatus(gameName, gameStatus);
                int valid_skins = 0;
                string[] ownedSkin = new string[PlayerManager.instance.NUM_SKINS];
                for (int i = 0; i < PlayerManager.instance.NUM_SKINS; ++i)
                {
                    string index = i.ToString();

                    if (snapshot.Child("ownedSkins").Child(index).Value != null)
                    {
                        valid_skins++;
                        ownedSkin[i] = snapshot.Child("ownedSkins").Child(index).Value.ToString();
                    }
                    else
                    {
                        ownedSkin[i] = null;
                    }

                }

                if (valid_skins < PlayerManager.instance.NUM_SKINS)
                {
                    bool[] arr = new bool[valid_skins];

                    for (int i = 0; i < arr.Length; ++i)
                    {
                        arr[i] = ownedSkin[i] == "True" ? true : false;
                    }

                    fix_ownedSkins_data(arr);
                }
                PlayerManager.instance.setOwnedSkins(ownedSkin);

                string[] ownedPet = new string[PlayerManager.instance.NUM_PETS];
                for (int i = 0; i < PlayerManager.instance.NUM_PETS; ++i)
                {
                    string index = i.ToString();
                    if (snapshot.Child("ownedPets").Child(index).Value != null)
                    {
                        ownedPet[i] = snapshot.Child("ownedPets").Child(index).Value.ToString();
                    }
                    else
                    {
                        ownedPet[i] = null;
                    }
                }
                PlayerManager.instance.setOwnedPets(ownedPet);

                if (snapshot.Child("teamCode").Value != null)
                {
                    PlayerManager.instance.setTeamCode(snapshot.Child("teamCode").Value.ToString());
                }

            }

            dataRecieved = true;
        }

        private IEnumerator getOtherPlayerStep(string UID, int index)
        {
            //Get the currently logged in user data
            var DBTask = DBreference.Child("users").Child(UID).GetValueAsync();

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
            //Debug.Log("Got Data (steps) from other Player");
            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else
            {
                //Data has been retrieved
                DataSnapshot snapshot = DBTask.Result;
                otherPlayerSteps[index] = int.Parse(snapshot.Child("steps").Value.ToString());
                string username = snapshot.Child("username").Value.ToString();
                //Debug.Log(username + " with UID(" + UID + ")" + " set to: " + otherPlayerSteps[index]);
            }
        }

        private IEnumerator setWinnerName(string UID)
        {
            //Get the currently logged in user data
            var DBTask = DBreference.Child("users").Child(UID).GetValueAsync();

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
            //Debug.Log("Got Data (steps) from other Player");
            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else
            {
                //Data has been retrieved
                DataSnapshot snapshot = DBTask.Result;
                winPlayerUsername = snapshot.Child("username").Value.ToString();
                //Debug.Log("Winner Username set to: " + winPlayerUsername);
            }
        }

        private IEnumerator getOtherPlayerUsername(string UID)
        {
            //Get the currently logged in user data
            var DBTask = DBreference.Child("users").Child(UID).GetValueAsync();

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
            //Debug.Log("Got Data (steps) from other Player");
            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else
            {
                //Data has been retrieved
                DataSnapshot snapshot = DBTask.Result;
                otherPlayerUsername = snapshot.Child("username").Value.ToString();
                Debug.Log("Winner Username set to: " + otherPlayerUsername);
            }
        }


        private IEnumerator getOtherSkinID(string UID, int index)
        {
            //Get the currently logged in user data
            var DBTask = DBreference.Child("users").Child(UID).GetValueAsync();

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
            //Debug.Log("Got Data (steps) from other Player");
            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else
            {
                //Data has been retrieved
                DataSnapshot snapshot = DBTask.Result;
                otherPlayerSkinID[index] = snapshot.Child("skinID").Value.ToString();
                Debug.Log("Set Skin Array at index " + index + " to: " + otherPlayerSkinID[index]);
            }
        }

        //public void resetQueueArr()
        //{
        //    queueArr = new string[2];
        //}

        //public void resetStepsArr()
        //{
        //    otherPlayerSteps = new int[2];
        //}

        public void resetSkinArr()
        {
            otherPlayerSkinID = new string[2];
        }

        //public void resetQueueTaskComplete()
        //{
        //    queueTaskComplete = true;
        //}

        public bool isQueueTaskComplete()
        {
            return queueTaskComplete;
        }

        private IEnumerator UpdateQueueArray()
        {
            queueArr = new string[2];
            queueTaskComplete = false;
            //Get all the users data ordered by kills amount
            var DBTask = DBreference.Child("users").OrderByChild("isInGame").GetValueAsync();

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
            //Debug.Log("Got Other User's Data");
            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else
            {
                //Data has been retrieved
                DataSnapshot snapshot = DBTask.Result;

                int queuecount = 0;
                //Loop through every users UID
                foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
                {
                    bool exist = false;
                    for (int i = 0; i < queueArr.Length; ++i)
                    {
                        if (childSnapshot.Child("uid").Value.ToString() == queueArr[i])
                        {
                            exist = true;
                        }
                    }

                    //Debug.Log("Got UID: " + childSnapshot.Child("uid").Value.ToString());
                    //Debug.Log(childSnapshot.Child("isInGame").Value.ToString());
                    if (!exist && childSnapshot.Child("uid").Value.ToString() != PlayerManager.instance.getData("uid")
                        && childSnapshot.Child("isInGame").Value.ToString() == "True"
                        && queuecount < 2)
                    {
                        //Debug.Log("User with UID (" + childSnapshot.Child("uid").Value.ToString() + ") join the Queue");
                        queueArr[queuecount++] = childSnapshot.Child("uid").Value.ToString();
                    }

                }

            }


            queueTaskComplete = true;
        }

        private void initializeAccount(string USERNAME)
        {
            StartCoroutine(set_UID());
            StartCoroutine(UpdateUsernameAuth(USERNAME));
            StartCoroutine(UpdateUsernameDatabase(USERNAME));
            StartCoroutine(Update_coins(false, 0));
            StartCoroutine(Update_steps(false, 0));
            StartCoroutine(Update_isInGame(false));
            StartCoroutine(Update_isOnline(true));
            StartCoroutine(Update_skinID(0));
            StartCoroutine(Update_petID(0));
            StartCoroutine(Update_TeamCode("placeholder"));
            StartCoroutine(Update_gameStatus("placeholder", "placeholder"));

            bool[] ownedSkin = new bool[PlayerManager.instance.NUM_SKINS];
            for (int i = 0; i < ownedSkin.Length; ++i)
            {
                ownedSkin[i] = false;
            }
            ownedSkin[0] = true;
            StartCoroutine(Update_ownedSkins(ownedSkin));

            bool[] ownedPet = new bool[PlayerManager.instance.NUM_PETS];
            for (int i = 0; i < ownedPet.Length; ++i)
            {
                ownedPet[i] = false;
            }
            ownedPet[0] = true;
            StartCoroutine(Update_ownedPets(ownedPet));

        }

        public void initSpecificAccountData(string str)
        {
            if (str == "username")
            {
                StartCoroutine(UpdateUsernameAuth("Guest"));
                StartCoroutine(UpdateUsernameDatabase("Guest"));
            }
            else if (str == "isOnline")
            {
                StartCoroutine(Update_isOnline(true));
            }
            else if (str == "isInGame")
            {
                StartCoroutine(Update_isInGame(false));
            }
            else if (str == "coins")
            {
                StartCoroutine(Update_coins(false, 0));
            }
            else if (str == "steps")
            {
                StartCoroutine(Update_steps(false, 0));
            }
            else if (str == "uid")
            {
                StartCoroutine(set_UID());
            }
            else if (str == "skinID")
            {
                StartCoroutine(Update_skinID(0));
            }
            else if (str == "petID")
            {
                StartCoroutine(Update_petID(0));
            }
            else if (str == "ownedSkins")
            {
                bool[] ownedSkin = new bool[PlayerManager.instance.NUM_SKINS];
                for (int i = 0; i < ownedSkin.Length; ++i)
                {
                    ownedSkin[i] = false;
                }
                ownedSkin[0] = true;
                StartCoroutine(Update_ownedSkins(ownedSkin));
            }
            else if (str == "ownedPets")
            {
                bool[] ownedPet = new bool[PlayerManager.instance.NUM_PETS];
                for (int i = 0; i < ownedPet.Length; ++i)
                {
                    ownedPet[i] = false;
                }
                ownedPet[0] = true;
                StartCoroutine(Update_ownedPets(ownedPet));
            }
            else if (str == "teamCode")
            {
                StartCoroutine(Update_TeamCode("placeholder"));
            }
            else if (str == "gameName")
            {
                StartCoroutine(Update_gameStatus("placeholder", "placeholder"));
            }
            else if (str == "gameStatus")
            {
                StartCoroutine(Update_gameStatus("placeholder", "placeholder"));
            }
            else if (str == "Profile_Image_URL")
            {
                StartCoroutine(Update_Profile_Image_URL()); ;
            }
            else
            {
                Debug.Log("Invalid Input.");
            }
        }

        public void fix_ownedSkins_data(bool[] skins)
        {
            Debug.Log("Fix data in fb");
            bool[] newArr = new bool[PlayerManager.instance.NUM_SKINS];

            for (int i = 0; i < skins.Length; ++i)
            {
                newArr[i] = skins[i];
            }

            for (int i = skins.Length; i < PlayerManager.instance.NUM_SKINS; ++i)
            {
                newArr[i] = false;
            }

            StartCoroutine(Update_ownedSkins(newArr));

            LoadUserData();
        }

        private IEnumerator Update_coins(bool addCoins, int _coins)
        {

            int currCoin = 0;

            if (addCoins)
            {


                var DBTaskGet = DBreference.Child("users").Child(User.UserId).GetValueAsync();

                yield return new WaitUntil(predicate: () => DBTaskGet.IsCompleted);

                if (DBTaskGet.Exception != null)
                {
                    Debug.LogWarning(message: $"Failed to register task with {DBTaskGet.Exception}");
                }
                else
                {
                    //Data has been retrieved
                    DataSnapshot snapshot = DBTaskGet.Result;
                    currCoin = int.Parse(snapshot.Child("coins").Value.ToString());
                    //Debug.Log("currCoin (Before Addition) = " + currCoin);
                }


            }

            var DBTask = DBreference.Child("users").Child(User.UserId).Child("coins").SetValueAsync(currCoin + _coins);

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }


        }
        private IEnumerator set_UID()
        {

            var DBTask = DBreference.Child("users").Child(User.UserId).Child("uid").SetValueAsync(User.UserId);

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
        }

        private IEnumerator Update_steps(bool addSteps, int _steps)
        {
            //Set the currently logged in user steps
            int currSteps = 0;
            if (addSteps)
            {


                var DBTaskGet = DBreference.Child("users").Child(User.UserId).GetValueAsync();

                yield return new WaitUntil(predicate: () => DBTaskGet.IsCompleted);

                if (DBTaskGet.Exception != null)
                {
                    Debug.LogWarning(message: $"Failed to register task with {DBTaskGet.Exception}");
                }
                else
                {
                    //Data has been retrieved
                    DataSnapshot snapshot = DBTaskGet.Result;
                    currSteps = int.Parse(snapshot.Child("steps").Value.ToString());
                }


            }

            var DBTask = DBreference.Child("users").Child(User.UserId).Child("steps").SetValueAsync(currSteps + _steps);

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
        }

        private IEnumerator Update_skinID(int _skinID)
        {
            //Set the currently logged in user steps
            var DBTask = DBreference.Child("users").Child(User.UserId).Child("skinID").SetValueAsync(_skinID);

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
        }

        private IEnumerator add_Skin(int skinID)
        {
            //Set the currently logged in user steps
            var DBTask = DBreference.Child("users").Child(User.UserId).Child("ownedSkins").Child(skinID.ToString()).SetValueAsync(true);

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
        }

        private IEnumerator Update_petID(int petID)
        {
            //Set the currently logged in user steps
            var DBTask = DBreference.Child("users").Child(User.UserId).Child("petID").SetValueAsync(petID);

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
        }

        private IEnumerator add_Pet(int petID)
        {
            //Set the currently logged in user steps
            var DBTask = DBreference.Child("users").Child(User.UserId).Child("ownedPets").Child(petID.ToString()).SetValueAsync(true);

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
        }
        private IEnumerator Update_isOnline(bool _isOnline)
        {
            //Set the currently logged in user steps
            var DBTask = DBreference.Child("users").Child(User.UserId).Child("isOnline").SetValueAsync(_isOnline);

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
        }

        private IEnumerator Update_ownedSkins(bool[] ownedSkins)
        {
            //Set the currently logged in user steps
            var DBTask = DBreference.Child("users").Child(User.UserId).Child("ownedSkins").SetValueAsync(ownedSkins);

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
        }

        private IEnumerator Update_ownedPets(bool[] ownedPets)
        {
            //Set the currently logged in user steps
            var DBTask = DBreference.Child("users").Child(User.UserId).Child("ownedPets").SetValueAsync(ownedPets);

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
        }
        private IEnumerator Update_isInGame(bool _isInGame)
        {
            //Set the currently logged in user steps
            var DBTask = DBreference.Child("users").Child(User.UserId).Child("isInGame").SetValueAsync(_isInGame);

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
        }

        private IEnumerator Update_gameStatus(string gameName, string status)
        {
            var DBTask = DBreference.Child("users").Child(User.UserId).Child("gameStatus").Child("gameName").SetValueAsync(gameName);

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }

            DBTask = DBreference.Child("users").Child(User.UserId).Child("gameStatus").Child("status").SetValueAsync(status);

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
        }

        private IEnumerator Update_Profile_Image_URL()
        {
            var DBTask = DBreference.Child("users").Child(User.UserId).Child("Profile_Image_URL").SetValueAsync("placeholder");

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
        }
        public void setGameStatus(string _gameStatus)
        {
            StartCoroutine(setGameStatusCoroutine(_gameStatus));
        }

        private IEnumerator setGameStatusCoroutine(string _gameStatus)
        {
            var DBTask = DBreference.Child("users").Child(User.UserId).Child("gameStatus").Child("status").SetValueAsync(_gameStatus);

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
        }

        //--------------------------------------------------------Relay Game (Team Up) Firebase-------------------------------------------------------------------------

        private int memberCount;
        private string teamName;
        private string[] memberIDs = new string[10];
        private string[] memberIDs_Username = new string[10];
        private bool teamExist;
        private int memberID;
        private bool destroyTeam = false;
        private bool enterQueue = false;
        private bool joinedCustomQueue = false;
        private bool teamNameAvaliable = true;


        private IEnumerator Update_TeamCode(string _teamCode)
        {
            //Set the currently logged in user steps
            var DBTask = DBreference.Child("users").Child(User.UserId).Child("teamCode").SetValueAsync(_teamCode);

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }

        }

        public void setTeamCode(string str)
        {

            StartCoroutine(Update_TeamCode(str));
        }

        private string teamCodeGenerator()
        {
            string avaliableChar = "0123456789";
            string teamCode = "";
            for (int i = 0; i < 6; ++i)
            {
                int randIndex = UnityEngine.Random.Range(0, avaliableChar.Length - 1);
                teamCode += avaliableChar[randIndex];
            }
            return teamCode;
        }


        private IEnumerator leader_leave_team_Coroutine(string _teamCode, string _teamName)
        {
            var DBTask = DBreference.Child("teams").Child(_teamCode).Child("destroyTeam").SetValueAsync(true);

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }

            yield return new WaitForSeconds(3f);

            DBTask = DBreference.Child("teams").Child(_teamCode).RemoveValueAsync();

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }

            yield return new WaitForSeconds(0.5f);

            DBTask = DBreference.Child("users").Child(User.UserId).Child("memberID").SetValueAsync(null);

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }

            DBTask = DBreference.Child("teams").Child("teamNameList").Child(_teamName).SetValueAsync(null);

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }

            resetGameVariables();
        }
        public void leave_Team(string _teamCode, int _memberID)
        {
            StartCoroutine(leave_Team_Coroutine(_teamCode, _memberID));
        }
        private IEnumerator leave_Team_Coroutine(string _teamCode, int _memberID)
        {

            TeamEditorHandler.instance.remove_Player(_memberID);

            var DBTask = DBreference.Child("users").Child(User.UserId).Child("memberID").SetValueAsync(null);

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }

            resetGameVariables();
        }

        public void resetTimeRecords(string _teamCode, int _memberCount)
        {
            StartCoroutine(resetTimeRecordsCoroutine(_teamCode, _memberCount));
        }

        private IEnumerator resetTimeRecordsCoroutine(string _teamCode, int _memberCount)
        {
            for (int i = 0; i < 10; ++i)
            {
                //Debug.Log("Reset Time Records #" + i.ToString() + " in team " + _teamCode);
                var DBTask = DBreference.Child("teams").Child(_teamCode).Child("timeRecords").Child(i.ToString()).Child("username").SetValueAsync("placeholder");

                yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

                if (DBTask.Exception != null)
                {
                    Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
                }

                DBTask = DBreference.Child("teams").Child(_teamCode).Child("timeRecords").Child(i.ToString()).Child("personalBest").SetValueAsync(-1.00);

                yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

                if (DBTask.Exception != null)
                {
                    Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
                }
            }
        }

        //--------------------------------------------------------Relay Game (Matchmaking) Firebase-------------------------------------------------------------------------
        private bool joinedGame = false;
        private string gameID = "placeholder";

        public void initRelayGamePlay(string gameID, string _teamCode, string teamName)
        {
            StartCoroutine(initRelayGamePlayCoroutine(gameID, _teamCode, teamName));
        }

        private IEnumerator initRelayGamePlayCoroutine(string gameID, string _teamCode, string teamName)
        {
            //Initialize Runner At /games/{gameID}/gameInfo/teamInfo/{teamCode}/totalSteps as 0
            var DBTask1 = DBreference.Child("games").Child(gameID).Child("gameInfo").Child("teamInfo").Child(_teamCode).Child("runner").SetValueAsync(0);
            yield return new WaitUntil(predicate: () => DBTask1.IsCompleted);
            if (DBTask1.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask1.Exception}");
            }

            //Initalize total steps At /games/{gameID}/gameInfo/teamInfo/{teamCode}/totalSteps as 0
            DBTask1 = DBreference.Child("games").Child(gameID).Child("gameInfo").Child("teamInfo").Child(_teamCode).Child("totalSteps").SetValueAsync(0);
            yield return new WaitUntil(predicate: () => DBTask1.IsCompleted);
            if (DBTask1.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask1.Exception}");
            }

            //Initalize total steps At /games/{gameID}/gameInfo/teamInfo/{teamCode}/teamName as 0
            DBTask1 = DBreference.Child("games").Child(gameID).Child("gameInfo").Child("teamInfo").Child(_teamCode).Child("teamname").SetValueAsync(teamName);
            yield return new WaitUntil(predicate: () => DBTask1.IsCompleted);
            if (DBTask1.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask1.Exception}");
            }

            DBTask1 = DBreference.Child("games").Child(gameID).Child("gameInfo").Child("teamInfo").Child(_teamCode).Child("teamDistance").SetValueAsync(0);
            yield return new WaitUntil(predicate: () => DBTask1.IsCompleted);
            if (DBTask1.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask1.Exception}");
            }

            DBTask1 = DBreference.Child("games").Child(gameID).Child("gameInfo").Child("teamInfo").Child(_teamCode).Child("currVelocity").SetValueAsync(0);
            yield return new WaitUntil(predicate: () => DBTask1.IsCompleted);
            if (DBTask1.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask1.Exception}");
            }

            DBTask1 = DBreference.Child("games").Child(gameID).Child("gameInfo").Child("teamInfo").Child(_teamCode).Child("status").SetValueAsync("none");
            yield return new WaitUntil(predicate: () => DBTask1.IsCompleted);
            if (DBTask1.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask1.Exception}");
            }
        }
        public void queue_to_matchmaking(string _teamCode)
        {
            StartCoroutine(queue_to_matchmaking_Coroutine(_teamCode));
        }
        private IEnumerator queue_to_matchmaking_Coroutine(string _teamCode)
        {
            var DBTask2 = DBreference.Child("relay_matchmaking").Child(_teamCode).SetValueAsync("placeholder");
            yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
            if (DBTask2.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask2.Exception}");
            }

            DBTask2 = DBreference.Child("teams").Child(_teamCode).Child("enterQueue").SetValueAsync(true);

            yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);

            if (DBTask2.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask2.Exception}");
            }
        }

        public void getMatchmakingData(string _teamCode)
        {
            StartCoroutine(getMatchmakingDataFromFirebase(_teamCode));
        }
        private IEnumerator getMatchmakingDataFromFirebase(string _teamCode)
        {
            var DBTask = DBreference.Child("relay_matchmaking").Child(_teamCode).GetValueAsync();

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else if (DBTask.Result.Value == null)
            {
                joinedGame = false;
                //Debug.Log("Setting Joined Game to: " + joinedGame.ToString());
            }
            else
            {
                //Data has been retrieved
                DataSnapshot snapshot = DBTask.Result;
                gameID = snapshot.Value.ToString();
                Debug.Log("Game ID: " + gameID);
                joinedGame = gameID == "placeholder" ? false : true;
                Debug.Log("Setting Joined Game to: " + joinedGame.ToString());
            }
        }

        public bool getJoinedGame()
        {
            return joinedGame;
        }

        public string getGameID()
        {
            return gameID;
        }
        public void cancel_Queue(string _teamCode)
        {
            StartCoroutine(cancel_Queue_Coroutine(_teamCode));
        }

        private IEnumerator cancel_Queue_Coroutine(string _teamCode)
        {
            var DBTask1 = DBreference.Child("relay_matchmaking").Child(_teamCode).RemoveValueAsync();
            yield return new WaitUntil(predicate: () => DBTask1.IsCompleted);
            if (DBTask1.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask1.Exception}");
            }

            DBTask1 = DBreference.Child("teams").Child(_teamCode).Child("enterQueue").RemoveValueAsync();
            yield return new WaitUntil(predicate: () => DBTask1.IsCompleted);
            if (DBTask1.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask1.Exception}");
            }
        }

        //--------------------------------------------------------Relay Game (Game Play) Firebase-------------------------------------------------------------------------
        private string winner = "placeholder";

        private string[] leaderboardTeamNameOrderedByRank = new string[5];
        private double[] leaderboardTotalStepsOrderedByRank = new double[5];
        private string[] otherTeamCodes = new string[5];
        private string[] otherTeamNames = new string[5];
        private float[] otherTeamSteps = new float[5];
        private double[] otherTeamDistance = new double[5];
        private double[] otherTeamVelocity = new double[5];
        public int[] otherTeamSkinIDs = new int[5];
        public string[] otherTeamRunners = new string[5];
        public string[] otherTeamImageURL = new string[5];
        private int ranking = 5;

        public void updateGameResult(string _gameID)
        {
            StartCoroutine(updateGameResultCoroutine(_gameID));
        }

        private IEnumerator updateGameResultCoroutine(string _gameID)
        {
            Debug.Log("Calling firebase to update winner");
            var DBTask = DBreference.Child("games").Child(_gameID).Child("gameInfo").Child("anyWin").Child("winner").GetValueAsync();

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else if (DBTask.Result.Value != null)
            {
                //Data has been retrieved

                DataSnapshot snapshot = DBTask.Result;
                winner = snapshot.Value.ToString();
                Debug.Log("Setting winner to: " + winner);
            }
        }
        public string getWinner()
        {
            return winner;
        }

        public void getLeaderBoardData(string _gameID, string _teamCode)
        {
            StartCoroutine(getLeaderBoardDataCoroutine(_gameID, _teamCode));
        }

        IEnumerator getLeaderBoardDataCoroutine(string _gameID, string _teamCode)
        {
            var DBTask = DBreference.Child("games").Child(_gameID).Child("gameInfo").Child("teamInfo").OrderByChild("distance").GetValueAsync();
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else if (DBTask.Result.Value != null)
            {
                //Data has been retrieved
                DataSnapshot snapshot = DBTask.Result;

                for (int i = 0; i < leaderboardTeamNameOrderedByRank.Length; ++i)
                {
                    leaderboardTeamNameOrderedByRank[i] = "placeholder";
                    leaderboardTotalStepsOrderedByRank[i] = 0;
                }

                int index = 0;
                foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
                {
                    if (_teamCode == childSnapshot.Value.ToString())
                    {
                        ranking = index;
                    }

                    leaderboardTeamNameOrderedByRank[index] = childSnapshot.Child("teamname").Value.ToString();
                    leaderboardTotalStepsOrderedByRank[index] = double.Parse(childSnapshot.Child("distance").Value.ToString());
                    ++index;
                }
            }

            DBTask = DBreference.Child("games").Child(_gameID).Child("gameInfo").Child("teamInfo").GetValueAsync();
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else if (DBTask.Result.Value != null)
            {
                //Data has been retrieved
                DataSnapshot snapshot = DBTask.Result;

                int index = 1;
                foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
                {
                    //Debug.Log("Looking at team with team code: " + childSnapshot.Key);
                    if (childSnapshot.Key != _teamCode)
                    {
                        if (childSnapshot.Child("teamname").Value != null)
                        {
                            otherTeamNames[index] = childSnapshot.Child("teamname").Value.ToString();
                            otherTeamCodes[index] = childSnapshot.Key;
                        }
                        else
                        {
                            otherTeamNames[index] = "placeholder";
                            otherTeamCodes[index] = "placeholder";
                        }

                        if (childSnapshot.Child("totalSteps").Value != null)
                        {
                            otherTeamSteps[index] = float.Parse(childSnapshot.Child("totalSteps").Value.ToString());
                        }
                        else
                        {
                            otherTeamSteps[index] = 0;
                        }

                        if (childSnapshot.Child("distance").Value != null)
                        {
                            otherTeamDistance[index] = double.Parse(childSnapshot.Child("distance").Value.ToString());
                        }
                        else
                        {
                            otherTeamDistance[index] = 0.0f;
                        }

                        if (childSnapshot.Child("velocity").Value != null)
                        {
                            otherTeamVelocity[index] = double.Parse(childSnapshot.Child("velocity").Value.ToString());
                        }
                        else
                        {
                            otherTeamVelocity[index] = 0.0f;
                        }

                        if (childSnapshot.Child("runnerSkinID").Value != null)
                        {
                            Debug.Log("Runner Skin ID: " + int.Parse(childSnapshot.Child("runnerSkinID").Value.ToString()));
                            otherTeamSkinIDs[index] = int.Parse(childSnapshot.Child("runnerSkinID").Value.ToString());
                        }
                        else
                        {
                            otherTeamSkinIDs[index] = 0;
                        }

                        if (childSnapshot.Child("runnerImageURL").Value != null)
                        {
                            otherTeamImageURL[index] = childSnapshot.Child("runnerImageURL").Value.ToString();
                        }
                        else
                        {
                            otherTeamImageURL[index] = "placeholder";
                        }

                        if (childSnapshot.Child("runner").Value != null && childSnapshot.Child("runner").Value.ToString() != "" && childSnapshot.Child("runner").Value.ToString() != null)
                        {
                            DBTask = DBreference.Child("users").Child(childSnapshot.Child("runner").Value.ToString()).GetValueAsync();
                            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

                            if (DBTask.Exception != null)
                            {
                                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
                            }
                            else if (DBTask.Result.Value != null)
                            {
                                //Data has been retrieved
                                DataSnapshot snapshot2 = DBTask.Result;
                                otherTeamRunners[index] = snapshot2.Child("username").Value.ToString();
                            }

                        }
                        else
                        {
                            otherTeamRunners[index] = "placeholder";
                        }

                        ++index;
                    }
                    else
                    {
                        if (childSnapshot.Child("runnerSkinID").Value != null)
                        {
                            otherTeamSkinIDs[0] = int.Parse(childSnapshot.Child("runnerSkinID").Value.ToString());
                            otherTeamImageURL[0] = childSnapshot.Child("runnerImageURL").Value.ToString();
                        }
                        else
                        {
                            otherTeamSkinIDs[0] = 0;
                            otherTeamImageURL[0] = "placeholder";
                        }
                    }
                }
            }
        }

        public double[] getLeaderboardTotalSteps()
        {
            return leaderboardTotalStepsOrderedByRank;
        }

        public string[] getLeaderboardTeamName()
        {
            return leaderboardTeamNameOrderedByRank;
        }

        public string[] getOtherTeamNames()
        {
            return otherTeamNames;
        }

        public string[] getOtherTeamCodes()
        {
            return otherTeamCodes;
        }

        public float[] getOtherTeamSteps()
        {
            return otherTeamSteps;
        }

        public double[] getOtherTeamDist()
        {
            return otherTeamDistance;
        }

        public double[] getOtherTeamVelocity()
        {
            return otherTeamVelocity;
        }

        public int[] getOtherTeamSkinIDs()
        {
            return otherTeamSkinIDs;
        }

        public string[] getOtherTeamRunners()
        {
            return otherTeamRunners;
        }

        public string[] getOtherTeamImageURL()
        {
            return otherTeamImageURL;
        }

        //------------------------------------------------------------Relay Game (Win Scene) Firebase-------------------------------------------------------------------------
        string[] winnerMVPNames = new string[10];
        string[] winnerMVPImageURL = new string[10];
        double[] winnerMVPTimes = new double[10];
        int[] winnerMVPSkins = new int[10];
        string winTeamName = "placeholder";

        string[] teamNames = new string[10];
        double[] teamTimes = new double[10];

        public void updateWinnerMVPNames(string _gameID, string _teamCode)
        {
            StartCoroutine(updateWinnerMVPNamesCoroutine(_gameID, _teamCode));
        }
        private IEnumerator updateWinnerMVPNamesCoroutine(string _gameID, string _teamCode)
        {
            //Debug.Log("Getting Winner Team Code: " + _teamCode);
            var DBTask = DBreference.Child("games").Child(_gameID).Child("gameInfo").Child("teamInfo").Child(_teamCode).Child("teamRecords").OrderByValue().GetValueAsync();
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else if (DBTask.Result.Value != null)
            {
                //Data has been retrieved
                DataSnapshot snapshot = DBTask.Result;

                int index = 0;
                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    //Debug.Log("winnderMVPNames[" + index + "]: " + childSnapshot.Child("username").Value.ToString());
                    if (childSnapshot.Key != null && childSnapshot.Key != "placeholder")
                    {
                        //Get Username and SkinID by the uid
                        var DBTaskGet = DBreference.Child("users").Child(childSnapshot.Key).GetValueAsync();
                        yield return new WaitUntil(predicate: () => DBTaskGet.IsCompleted);

                        if (DBTaskGet.Exception != null)
                        {
                            Debug.LogWarning(message: $"Failed to register task with {DBTaskGet.Exception}");
                        }
                        else if (DBTaskGet.Result.Value != null)
                        {
                            //Data has been retrieved
                            DataSnapshot snapshot1 = DBTaskGet.Result;

                            winnerMVPNames[index] = snapshot1.Child("username").Value.ToString();
                            winnerMVPImageURL[index] = snapshot1.Child("Profile_Image_URL").Value.ToString();
                            winnerMVPSkins[index] = int.Parse(snapshot1.Child("skinID").Value.ToString());
                        }
                    }
                    else
                    {
                        winnerMVPNames[index] = "plcaeholder";
                        winnerMVPImageURL[index] = "placeholder";
                        winnerMVPSkins[index] = 0;
                    }

                    if (childSnapshot.Value.ToString() != null)
                    {
                        winnerMVPTimes[index] = double.Parse(childSnapshot.Value.ToString());
                    }
                    else
                    {
                        winnerMVPTimes[index] = Mathf.Infinity;
                    }

                    ++index;
                }
            }

            var DBTask2 = DBreference.Child("teams").Child(_teamCode).Child("teamname").GetValueAsync();

            yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);

            if (DBTask2.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask2.Exception}");
            }
            else if (DBTask2.Result.Value != null)
            {
                DataSnapshot snapshot1 = DBTask2.Result;
                winTeamName = snapshot1.Value.ToString();
                //Debug.Log("FirebaseManager got Team Name: " + winTeamName);
            }

        }

        public string[] getWinnerMVPName()
        {
            return winnerMVPNames;
        }

        public string[] getWinnerMVPImageURL()
        {
            return winnerMVPImageURL;
        }
        public double[] getWinnerMVPTimes()
        {
            return winnerMVPTimes;
        }

        public int[] getWinnerMVPSkinIDs()
        {
            return winnerMVPSkins;
        }
        public string getWinTeamName()
        {
            return winTeamName;
        }

        public void updateTeamNames(string _gameID, string _teamCode)
        {
            StartCoroutine(updateTeamNamesCoroutine(_gameID, _teamCode));
        }
        private IEnumerator updateTeamNamesCoroutine(string _gameID, string _teamCode)
        {
            var DBTask = DBreference.Child("games").Child(_gameID).Child("gameInfo").Child("teamInfo").Child(_teamCode).Child("teamRecords").OrderByValue().GetValueAsync();
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else if (DBTask.Result.Value != null)
            {
                //Data has been retrieved
                DataSnapshot snapshot = DBTask.Result;

                int index = 0;
                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    if (childSnapshot.Key != null && childSnapshot.Key != "")
                    {
                        var DBTaskGet = DBreference.Child("users").Child(childSnapshot.Key).GetValueAsync();
                        yield return new WaitUntil(predicate: () => DBTaskGet.IsCompleted);

                        if (DBTaskGet.Exception != null)
                        {
                            Debug.LogWarning(message: $"Failed to register task with {DBTaskGet.Exception}");
                        }
                        else if (DBTaskGet.Result.Value != null)
                        {
                            //Data has been retrieved
                            DataSnapshot snapshot1 = DBTaskGet.Result;

                            teamNames[index] = snapshot1.Child("username").Value.ToString();
                        }
                    }
                    else
                    {
                        teamNames[index] = "placeholder";
                    }

                    if (childSnapshot.Value != null)
                    {
                        teamTimes[index] = double.Parse(childSnapshot.Value.ToString());
                    }
                    else
                    {
                        teamTimes[index] = 0;
                    }

                    ++index;
                }
            }

        }

        public string[] getTeamNames()
        {
            return teamNames;
        }

        public double[] getTeamTimes()
        {
            return teamTimes;
        }

        public void clearGameData(string _teamCode, string _gameID)
        {
            StartCoroutine(clearGameDataCoroutine(_teamCode, _gameID));
        }

        private IEnumerator clearGameDataCoroutine(string _teamCode, string _gameID)
        {
            /*
            var DBTask = DBreference.Child("relay_matchmaking").Child(_teamCode).RemoveValueAsync();

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            */

            var DBTask2 = DBreference.Child("teams").Child(_teamCode).Child("gameId").SetValueAsync("placeholder");

            yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);

            if (DBTask2.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask2.Exception}");
            }

            resetGameVariables();

            DBTask2 = DBreference.Child("games").Child(_gameID).RemoveValueAsync();

            yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);

            if (DBTask2.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask2.Exception}");
            }

            DBTask2 = DBreference.Child("game_version_Control").Child(_gameID).RemoveValueAsync();

            yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);

            if (DBTask2.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask2.Exception}");
            }
        }

        //------------------------------------------------------------------------Custom Game Code---------------------------------------------------------------------------------------------
        private string[] queueTeamNames = new string[5];
        private string[] queueTeamStatus = new string[5];
        private string[] queueTeamCodes = new string[5];

        private Dictionary<string, string>[] teamPairsValue = new Dictionary<string, string>[5];


        public void updateCustomGameID(string _teamCode)
        {
            StartCoroutine(updateCustomGameIDCoroutine(_teamCode));
        }

        private IEnumerator updateCustomGameIDCoroutine(string _teamCode)
        {
            var DBTask = DBreference.Child("teams").Child(_teamCode).Child("customGameID").GetValueAsync();

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else if (DBTask.Result.Value != null)
            {
                DataSnapshot snapshot = DBTask.Result;
                gameID = snapshot.Value.ToString();
            }
        }


        public void updateCustomQueueData(string _gameID)
        {
            StartCoroutine(updateCustomQueueDataCoroutine(_gameID));
        }

        private IEnumerator updateCustomQueueDataCoroutine(string _gameID)
        {

            dataRecieved = false;

            var DBTask = DBreference.Child("games").Child(_gameID).Child("gameInfo").Child("teamInfo").GetValueAsync();
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else if (DBTask.Result.Value != null)
            {
                //Data has been retrieved
                DataSnapshot snapshot = DBTask.Result;

                for (int i = 0; i < queueTeamNames.Length; ++i)
                {
                    queueTeamNames[i] = "placeholder";
                    queueTeamCodes[i] = "placeholder";
                }

                int index = 0;

                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    if (childSnapshot.Child("teamname").Value != null && childSnapshot.Child("isReady").Value != null)
                    {
                        queueTeamNames[index] = childSnapshot.Child("teamname").Value.ToString();
                        queueTeamCodes[index] = childSnapshot.Key;
                        ++index;
                    }
                }
            }

            dataRecieved = true;

        }

        public void gameLeader_start_game(string _gameID)
        {
            StartCoroutine(gameLeader_start_game_Coroutine(_gameID));
        }

        private IEnumerator gameLeader_start_game_Coroutine(string _gameID)
        {
            var DBTask1 = DBreference.Child("games").Child(_gameID).Child("gameInfo").Child("startGame").SetValueAsync(true);
            yield return new WaitUntil(predicate: () => DBTask1.IsCompleted);
            if (DBTask1.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask1.Exception}");
            }
        }

        public string[] getQueueTeamNames()
        {
            return queueTeamNames;
        }

        public string[] getQueueTeamStatus()
        {
            return queueTeamStatus;
        }

        public string[] getQueueTeamCodes()
        {
            return queueTeamCodes;
        }

        public Dictionary<string, string>[] getTeamPairsValue()
        {
            return teamPairsValue;
        }
        //----------------------------------------------------------------------------------Admin Utilities---------------------------------------------------------------------------------------------

        int TOTALSTEPS = 0;
        int STEPS_THRESHOLD = 0;

        public void generateFakeTeams(int num)
        {
            StartCoroutine(generateFakeTeamsCoroutine(num));
        }

        private IEnumerator generateFakeTeamsCoroutine(int num)
        {

            for (int i = 0; i < num; ++i)
            {
                string teamCodeTemp = teamCodeGenerator();

                var DBTask1 = DBreference.Child("relay_matchmaking").Child(teamCodeTemp).SetValueAsync("placeholder");
                yield return new WaitUntil(predicate: () => DBTask1.IsCompleted);
                if (DBTask1.Exception != null)
                {
                    Debug.LogWarning(message: $"Failed to register task with {DBTask1.Exception}");
                }
            }
        }

        public void resetGameVariables()
        {
            //Debug.Log("Reseting all the game variables");
            teamExist = false;
            destroyTeam = false;
            enterQueue = false;
            joinedCustomQueue = false;
            teamNameAvaliable = true;

            joinedGame = false;
            memberCount = 0;
            memberID = 10;
            ranking = 5;
            gameID = "placeholder";
            winTeamName = "placeholder";
            winner = "placeholder";
            teamName = "placeholder";
            enterNormalGame = false;
            gameID_for_Normal_Race = "placeholder";
            normalGameExist = false;
            normalQueueExist = false;

            memberIDs = new string[10];
            memberIDs_Username = new string[10];
            leaderboardTeamNameOrderedByRank = new string[5];
            leaderboardTotalStepsOrderedByRank = new double[5];
            otherTeamNames = new string[5];
            otherTeamCodes = new string[5];
            otherTeamSteps = new float[5];
            otherTeamDistance = new double[5];
            otherTeamVelocity = new double[5];
            otherTeamSkinIDs = new int[5];
            otherTeamRunners = new string[5];
            otherTeamImageURL = new string[5];
            winnerMVPNames = new string[10];
            winnerMVPTimes = new double[10];
            winnerMVPImageURL = new string[10];
            winnerMVPSkins = new int[10];
            winTeamName = "placeholder";
            teamNames = new string[10];
            teamTimes = new double[10];
            queueTeamNames = new string[5];
            queueTeamStatus = new string[5];
        }

        public void setConfig()
        {
            StartCoroutine(setConfigCoroutine());
        }

        private IEnumerator setConfigCoroutine()
        {
            /*
            var DBTask1 = DBreference.Child("Config_Constants").Child("RELAY_TOTALSTEPS").SetValueAsync(2000);
            yield return new WaitUntil(predicate: () => DBTask1.IsCompleted);
            if (DBTask1.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask1.Exception}");
            }

            DBTask1 = DBreference.Child("Config_Constants").Child("RELAY_STEPS_THRESHOLD").SetValueAsync(200);
            yield return new WaitUntil(predicate: () => DBTask1.IsCompleted);
            if (DBTask1.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask1.Exception}");
            }

            DBTask1 = DBreference.Child("Config_Constants").Child("RELAY_MAP").SetValueAsync(0);
            yield return new WaitUntil(predicate: () => DBTask1.IsCompleted);
            if (DBTask1.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask1.Exception}");
            }*/
            var DBTask1 = DBreference.Child("Organization").Child("Helios Studio").Child("1_1").SetValueAsync("Department Rock");
            yield return new WaitUntil(predicate: () => DBTask1.IsCompleted);
            if (DBTask1.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask1.Exception}");
            }

            DBTask1 = DBreference.Child("Organization").Child("Helios Studio").Child("1_2").SetValueAsync("Department Sam");
            yield return new WaitUntil(predicate: () => DBTask1.IsCompleted);
            if (DBTask1.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask1.Exception}");
            }
            DBTask1 = DBreference.Child("Organization").Child("Helios Studio").Child("1_3").SetValueAsync("Department Howard");
            yield return new WaitUntil(predicate: () => DBTask1.IsCompleted);
            if (DBTask1.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask1.Exception}");
            }
            DBTask1 = DBreference.Child("Organization").Child("Helios Studio").Child("1_4").SetValueAsync("Department Ting");
            yield return new WaitUntil(predicate: () => DBTask1.IsCompleted);
            if (DBTask1.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask1.Exception}");
            }
            DBTask1 = DBreference.Child("Organization").Child("Helios Studio").Child("1_5").SetValueAsync("Department Ashley");
            yield return new WaitUntil(predicate: () => DBTask1.IsCompleted);
            if (DBTask1.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask1.Exception}");
            }

            DBTask1 = DBreference.Child("Organization").Child("Helios Studio").Child("1_2_1").SetValueAsync("Department EternalSam");
            yield return new WaitUntil(predicate: () => DBTask1.IsCompleted);
            if (DBTask1.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask1.Exception}");
            }

            DBTask1 = DBreference.Child("Organization").Child("Helios Studio").Child("1_3_1").SetValueAsync("Department Austin");
            yield return new WaitUntil(predicate: () => DBTask1.IsCompleted);
            if (DBTask1.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask1.Exception}");
            }

            DBTask1 = DBreference.Child("Organization").Child("Helios Studio").Child("1_3_1").SetValueAsync("Department Austin");
            yield return new WaitUntil(predicate: () => DBTask1.IsCompleted);
            if (DBTask1.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask1.Exception}");
            }

        }

        public void getConfigConstant()
        {
            StartCoroutine(getConfigConstantCoroutine());
        }

        private IEnumerator getConfigConstantCoroutine()
        {
            var DBTask1 = DBreference.Child("Config_Constants").GetValueAsync();
            yield return new WaitUntil(predicate: () => DBTask1.IsCompleted);

            if (DBTask1.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask1.Exception}");
            }
            else if (DBTask1.Result.Value != null)
            {
                DataSnapshot snapshot1 = DBTask1.Result;
                TOTALSTEPS = int.Parse(snapshot1.Child("RELAY_TOTALSTEPS").Value.ToString());
                STEPS_THRESHOLD = int.Parse(snapshot1.Child("RELAY_STEPS_THRESHOLD").Value.ToString());

                //Debug.Log("From Firebase => TOTALSTEPS: " + TOTALSTEPS + ", STEPS_THRESHOLD: " + STEPS_THRESHOLD);
            }
        }

        public int getTotalStepConstant()
        {
            //Debug.Log("From Firebase => TOTALSTEPS: " + TOTALSTEPS );
            return TOTALSTEPS;
        }

        public int getStepThresholdConstant()
        {
            //Debug.Log("From Firebase => STEPS_THRESHOLD: " + STEPS_THRESHOLD);
            return STEPS_THRESHOLD;
        }
        //------------------------Lobby game stack----------------------
        public List<string> getGameIDList()
        {
            return gameIDList;
        }

        public ArrayList getTeamAmountInEachGame()
        {
            return teamAmountInEachGame;
        }

        public void updateAvaliableGames(string _teamCode)
        {
            StartCoroutine(updateGamesCoroutine(_teamCode));
        }

        private IEnumerator updateGamesCoroutine(string _teamCode)
        {
            dataRecieved = false;

            List<string> tempGameIDs = new List<string>();
            teamAmountInEachGame.Clear();

            var DBTask = DBreference.Child("games").GetValueAsync();
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else if (DBTask.Result.Value != null)
            {
                //Data has been retrieved
                DataSnapshot snapshot = DBTask.Result;
                //Debug.Log("Getting Available Games");
                int index = 0;
                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    if (childSnapshot.Child("gameInfo").Child("teamCount").Value != null &&
                        childSnapshot != null)
                    {
                        String _teamNumber = childSnapshot.Child("gameInfo").Child("teamCount").Value.ToString();
                        String _gameID = childSnapshot.Key;

                        //Check if the name has already started
                        DBTask = DBreference.Child("games").Child(_gameID).Child("gameInfo").Child("startGame").GetValueAsync();
                        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
                        if (DBTask.Exception != null)
                        {
                            //Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
                        }
                        else if (DBTask.Result.Value == null)
                        {
                            bool foundSelf = false;
                            int teamAmount = 0;

                            Debug.Log("Checking Game (" + _gameID + ")'s Teams");

                            DBTask = DBreference.Child("games").Child(_gameID).Child("gameInfo").Child("teamInfo").GetValueAsync();
                            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
                            if (DBTask.Exception != null)
                            {
                                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
                            }
                            else if (DBTask.Result.Value != null)
                            {

                                foreach (DataSnapshot teamsSnapshot in DBTask.Result.Children)
                                {

                                    Debug.Log("Found Team " + teamsSnapshot.Value.ToString());
                                    if (teamsSnapshot.Value.ToString() == _teamCode)
                                    {
                                        foundSelf = true;
                                    }

                                    teamAmount++;
                                }

                                Debug.Log("There are total of " + teamAmount + " teams; # Childern (DBTask Children Count): " + DBTask.Result.ChildrenCount);

                                if (teamAmount != int.Parse(_teamNumber))
                                {
                                    Debug.Log("Found local teamAmount(" + teamAmount + ") is different from database's _teamNumber(" + _teamNumber + "). Updating...");
                                    var DBTaskSet = DBreference.Child("games").Child(_gameID).Child("gameInfo").Child("teamCount").SetValueAsync(teamAmount);
                                    yield return new WaitUntil(predicate: () => DBTaskSet.IsCompleted);
                                    if (DBTaskSet.Exception != null)
                                    {
                                        Debug.LogWarning(message: $"Failed to register task with {DBTaskSet.Exception}");
                                    }
                                }

                                //Check if the team is full or not
                                if (foundSelf || teamAmount < 5)
                                {
                                    Debug.Log("Found Avaliable Game: " + _gameID);
                                    teamAmountInEachGame.Add(teamAmount);
                                    tempGameIDs.Add(_gameID);
                                    Debug.Log(tempGameIDs.Count + " items in temp Game List");
                                    index++;
                                }
                            }


                        }

                    }

                }
            }

            gameIDList = tempGameIDs;
            //Debug.Log(gameIDList.Count + " items in gameIDList");

            dataRecieved = true;
        }

        //-----------------------------------------------New Normal Queue--------------------------------------------------------

        private bool enterNormalGame = false;
        private string gameID_for_Normal_Race = "placeholder";
        private bool normalGameExist = false;
        private bool normalQueueExist = false;

        public void setInNormalQueue(string uid)
        {
            StartCoroutine(setInNormalQueueCoroutine(uid));
        }

        IEnumerator setInNormalQueueCoroutine(string _uid)
        {
            var DBTask2 = DBreference.Child("normal_queue").Child(_uid).SetValueAsync("placeholder");
            yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
            if (DBTask2.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask2.Exception}");
            }
        }

        public int getNormalQueueCount()
        {
            return normalQueueCount;
        }

        public void UpdateNormalQueue()
        {
            StartCoroutine(UpdateNormalQueueCoroutine());
        }

        IEnumerator UpdateNormalQueueCoroutine()
        {
            var DBTask = DBreference.Child("normal_queue").Child("playerCount").GetValueAsync();

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else if (DBTask.Result.Value != null)
            {
                DataSnapshot snapshot = DBTask.Result;
                normalQueueCount = int.Parse(snapshot.Value.ToString());
            }
        }

        public void getNormalQueueData(string _uid)
        {
            StartCoroutine(getNormalQueueDataFromFirebase(_uid));
        }
        private IEnumerator getNormalQueueDataFromFirebase(string _uid)
        {
            var DBTask = DBreference.Child("normal_queue").Child(_uid).GetValueAsync();

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else if (DBTask.Result.Value == null)
            {
                enterNormalGame = false;
                //Debug.Log("Setting Joined Game to: " + joinedGame.ToString());
            }
            else
            {
                //Data has been retrieved
                DataSnapshot snapshot = DBTask.Result;
                gameID_for_Normal_Race = snapshot.Value.ToString();
                Debug.Log("Game ID: " + gameID);
                enterNormalGame = gameID_for_Normal_Race == "placeholder" ? false : true;
                Debug.Log("Setting Enter Normal Game to: " + enterNormalGame.ToString());
                if (enterNormalGame)
                {
                    StartCoroutine(UpdateNormalQueueOtherPlayerCoroutine(gameID_for_Normal_Race, _uid));
                }
            }
        }

        public bool getEnterNormalGame()
        {
            return enterNormalGame;
        }

        IEnumerator UpdateNormalQueueOtherPlayerCoroutine(string _gameID, string _uid)
        {
            var DBTask = DBreference.Child("games").Child(_gameID).Child("gameInfo").Child("playerInfo").GetValueAsync();
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else if (DBTask.Result.Value != null)
            {
                //Data has been retrieved
                DataSnapshot snapshot = DBTask.Result;
                int index = 0;
                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    if (childSnapshot.Value != null && childSnapshot.Key != _uid)
                    {
                        String _otherID = childSnapshot.Key;
                        otherplayer[index] = _otherID;
                        index++;
                    }
                    normalGameData.Add(childSnapshot.Key, int.Parse(childSnapshot.Value.ToString()));
                    Debug.Log(childSnapshot.Key);
                    Debug.Log(childSnapshot.Value.ToString());
                }
            }
        }

        public string[] getOtherPlayerArray()
        {
            return otherplayer;
        }

        public void setNormalSteps(string _gameID, string _uid, int steps)
        {
            StartCoroutine(setNormalStepsCoroutine(_gameID, _uid, steps));
        }

        IEnumerator setNormalStepsCoroutine(string _gameID, string _uid, int steps)
        {
            var DBTask2 = DBreference.Child("games").Child(_gameID).Child("gameInfo").Child("playerInfo").Child(_uid).SetValueAsync(steps);
            yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
            if (DBTask2.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask2.Exception}");
            }
        }

        public void getNormalSteps(string _gameID)
        {
            StartCoroutine(getNormalStepsCoroutine(_gameID));
        }

        IEnumerator getNormalStepsCoroutine(string _gameID)
        {
            var DBTask = DBreference.Child("games").Child(_gameID).Child("gameInfo").Child("playerInfo").GetValueAsync();

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else if (DBTask.Result.Value != null)
            {
                DataSnapshot snapshot = DBTask.Result;
                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    if (childSnapshot.Value != null)
                    {
                        normalGameData[childSnapshot.Key] = int.Parse(childSnapshot.Value.ToString());
                    }
                }
            }
        }

        public string getGameIDForNormalRace()
        {
            return gameID_for_Normal_Race;
        }

        public void ResetNormalRaceData(string _uid)
        {
            StartCoroutine(ResetNormalRaceDataCoroutine(_uid));
        }

        IEnumerator ResetNormalRaceDataCoroutine(string _uid)
        {
            var DBTask = DBreference.Child("games").Child(gameID_for_Normal_Race).RemoveValueAsync();

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }

            DBTask = DBreference.Child("normal_queue").Child(_uid).RemoveValueAsync();

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }

            enterNormalGame = false;
            gameID_for_Normal_Race = "placeholder";
            normalGameData.Clear();
        }

        public void cancelNormalRaceQueue(string _uid)
        {
            StartCoroutine(cancelNormalRaceQueueCoroutine(_uid));
        }

        IEnumerator cancelNormalRaceQueueCoroutine(string _uid)
        {
            var DBTask = DBreference.Child("normal_queue").Child(_uid).RemoveValueAsync();

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
        }

        public void checkNormalRaceStatus(string _UID)
        {
            StartCoroutine(checkNormalRaceStatusCoroutine(_UID));
        }

        private IEnumerator checkNormalRaceStatusCoroutine(string _UID)
        {
            normalGameExist = true;
            normalQueueExist = true;

            var DBTask = DBreference.Child("normal_queue").Child(_UID).GetValueAsync();

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else if (DBTask.Result.Value == null)
            {

                normalGameExist = false;
                normalQueueExist = false;
            }
            else
            {
                DataSnapshot snapshot = DBTask.Result;
                string _gameID = snapshot.Value.ToString();

                if (_gameID == "placeholder")
                {
                    normalQueueExist = true;
                    normalGameExist = false;
                }

                DBTask = DBreference.Child("games").Child(_gameID).GetValueAsync();

                yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

                if (DBTask.Exception != null)
                {
                    Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
                }
                else if (DBTask.Result.Value == null)
                {
                    normalGameExist = false;
                }
            }
        }

        public bool getNormalGameExist()
        {
            return normalGameExist;
        }

        public bool getNormalQueueExist()
        {
            return normalQueueExist;
        }

        public void updateTeamVersion(string _teamCode)
        {
            StartCoroutine(updateTeamVersion_Coroutine(_teamCode));
        }

        int FB_Team_Version = 0;

        private IEnumerator updateTeamVersion_Coroutine(string _teamCode)
        {
            dataRecieved = false;
            var DBTask = DBreference.Child("team_version_control").Child(_teamCode).GetValueAsync();

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else if (DBTask.Result.Value != null)
            {
                DataSnapshot snapshot = DBTask.Result;
                FB_Team_Version = int.Parse(snapshot.Value.ToString());
            }
            dataRecieved = true;
        }

        public int getTeamVersion()
        {
            return FB_Team_Version;
        }

        public void updateGameVersion(string _gameID)
        {
            StartCoroutine(updateGameVersion_Coroutine(_gameID));
        }

        int FB_Game_Version = 0;

        private IEnumerator updateGameVersion_Coroutine(string _gameID)
        {
            var DBTask = DBreference.Child("game_version_control").Child(_gameID).GetValueAsync();

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else if (DBTask.Result.Value != null)
            {
                DataSnapshot snapshot = DBTask.Result;
                FB_Game_Version = int.Parse(snapshot.Value.ToString());
            }
        }

        public int getGameVersion()
        {
            return FB_Game_Version;
        }

        public void write_teamRecords(string _gameID, string _teamCode, string _uid, float _bestTime)
        {
            StartCoroutine(write_teamRecords_Coroutine(_gameID, _teamCode, _uid, _bestTime));
        }

        private IEnumerator write_teamRecords_Coroutine(string _gameID, string _teamCode, string _uid, float _bestTime)
        {
            var DBTask2 = DBreference.Child("games").Child(_gameID).Child("gameInfo").Child("teamInfo").Child(_teamCode).Child("teamRecords").Child(_uid).SetValueAsync(_bestTime);
            yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
            if (DBTask2.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask2.Exception}");
            }
        }

        //-----------------------------------------------Organization--------------------------------------------------------
        private List<string> CompanyNames = new List<string>();
        private List<string> DepartmentNames = new List<string>();
        private List<string> DepartmentIDs = new List<string>();
        private bool organizationInitialized = false;

        public void readCompanyNames()
        {
            StartCoroutine(readCompanyNamesCoroutine());
        }

        private IEnumerator readCompanyNamesCoroutine()
        {
            CompanyNames.Clear();

            var DBTask = DBreference.Child("Organization").GetValueAsync();
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else if (DBTask.Result.Value != null)
            {
                //Data has been retrieved
                DataSnapshot snapshot = DBTask.Result;
                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    CompanyNames.Add(childSnapshot.Key);
                }
            }
        }

        public List<string> getCompanyNameList()
        {
            return CompanyNames;
        }

        public void readDepartmentNames(string _companyName)
        {
            StartCoroutine(readDepartmentNamesCoroutine(_companyName));
        }

        private IEnumerator readDepartmentNamesCoroutine(string _companyName)
        {
            DepartmentNames.Clear();
            DepartmentIDs.Clear();

            var DBTask = DBreference.Child("Organization").Child(_companyName).GetValueAsync();
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else if (DBTask.Result.Value != null)
            {
                //Data has been retrieved
                DataSnapshot snapshot = DBTask.Result;
                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    DepartmentNames.Add(childSnapshot.Value.ToString());
                    DepartmentIDs.Add(childSnapshot.Key);
                }
            }
        }

        public List<string> getDepartmentList()
        {
            return DepartmentNames;
        }

        public List<string> getDepartmentIDList()
        {
            return DepartmentIDs;
        }

        public void readOrganizationExist(string _uid)
        {
            StartCoroutine(OrganizaitonExistCoroutine(_uid));
        }

        private IEnumerator OrganizaitonExistCoroutine(string _uid)
        {
            dataRecieved = false;
            organizationInitialized = false;
            var DBTask = DBreference.Child("users").Child(_uid).Child("organization").GetValueAsync();
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else if (DBTask.Result.Value != null)
            {
                organizationInitialized = true;
            }
            dataRecieved = true;
        }

        public bool organizationExist()
        {
            return organizationInitialized;
        }

        //---------------------------------------------------PET SYSTEM-------------------------------------------------------------------------

        private string petName;
        private float hunger_stats;
        private float exp_stats;
        private double last_fed_time;
        private int food;
        private bool petDataExist;
        private bool dataRecieved;


        //Check if Data Exists
        public void check_pet_data_exist()
        {
            StartCoroutine(check_pet_data_exist_coroutine());
        }

        private IEnumerator check_pet_data_exist_coroutine()
        {
            dataRecieved = false;
            petDataExist = false;

            var DBTask = DBreference.Child("users").Child(User.UserId).Child("petInfo").GetValueAsync();
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else if (DBTask.Result.Value != null)
            {
                petDataExist = true;
            }

            dataRecieved = true;
        }

        public bool get_pet_data_exist()
        {
            return petDataExist;
        }

        //Initialize Data (Pet Name, Food, Hunger Stats, Exp Stats, Lest Fed Time)

        public void initializePetData()
        {
            StartCoroutine(initializePetDataCoroutine());
        }

        private IEnumerator initializePetDataCoroutine()
        {
            dataRecieved = false;

            var DBTask = DBreference.Child("users").Child(User.UserId).Child("petInfo").Child("petName").SetValueAsync("placeholder");
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }

            DBTask = DBreference.Child("users").Child(User.UserId).Child("petInfo").Child("Food").SetValueAsync(0);
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }

            DBTask = DBreference.Child("users").Child(User.UserId).Child("petInfo").Child("HungerStats").SetValueAsync(0);
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }

            DBTask = DBreference.Child("users").Child(User.UserId).Child("petInfo").Child("ExpStats").SetValueAsync(0);
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }

            DBTask = DBreference.Child("users").Child(User.UserId).Child("petInfo").Child("LastFedTime").SetValueAsync(0);
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }

            dataRecieved = true;
        }


        //Get Data (Pet Name, Food, Hunger Stats, Exp Stats, Last Fed Time)
        public void readPetData()
        {
            StartCoroutine(readPetDataCoroutine());
        }

        private IEnumerator readPetDataCoroutine()
        {
            dataRecieved = false;

            var DBTask = DBreference.Child("users").Child(User.UserId).Child("petInfo").GetValueAsync();

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else if (DBTask.Result.Value != null)
            {
                DataSnapshot snapshot = DBTask.Result;

                if (snapshot.Child("petName").Value != null)
                {
                    petName = snapshot.Child("petName").Value.ToString();
                }

                if (snapshot.Child("Food").Value != null)
                {
                    food = int.Parse(snapshot.Child("Food").Value.ToString());
                }

                if (snapshot.Child("HungerStats").Value != null)
                {
                    hunger_stats = float.Parse(snapshot.Child("HungerStats").Value.ToString());
                }

                if (snapshot.Child("ExpStats").Value != null)
                {
                    hunger_stats = float.Parse(snapshot.Child("ExpStats").Value.ToString());
                }

                if (snapshot.Child("LastFedTime").Value != null)
                {
                    last_fed_time = double.Parse(snapshot.Child("LastFedTime").Value.ToString());
                }
            }

            Debug.Log("Read Pet Data From Firebase: \n " +
                "\nPet Name: " + petName +
                "\nFood: " + food +
                "\nHunger Stats: " + hunger_stats +
                "\nExp Stats: " + exp_stats +
                "\nLast Fed Time: " + last_fed_time + "\n\n");

            dataRecieved = true;
        }

        public string getPetName()
        {
            return petName;
        }

        public int getFood()
        {
            return food;
        }

        public float getHungerStats()
        {
            return hunger_stats;
        }

        public float getExpStats()
        {
            return exp_stats;
        }

        public double getLastFedTime()
        {
            return last_fed_time;
        }

        public bool isDataRecieved()
        {
            return dataRecieved;
        }

        //Set Pet Name

        public void writePetName(string _petName)
        {
            StartCoroutine(writePetNameCoroutine(_petName));
        }

        private IEnumerator writePetNameCoroutine(string _petName)
        {
            var DBTask = DBreference.Child("users").Child(User.UserId).Child("petInfo").Child("petName").SetValueAsync(_petName);
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
        }

        //Set Food Amount

        public void writeFood(int _food)
        {
            StartCoroutine(writeFoodCoroutine(_food));
        }

        private IEnumerator writeFoodCoroutine(int _food)
        {
            var DBTask = DBreference.Child("users").Child(User.UserId).Child("petInfo").Child("Food").SetValueAsync(_food);
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
        }

        //Set Hunger Stats

        public void writeHungerStats(float _hungerStats)
        {
            StartCoroutine(writeHungerStatsCoroutine(_hungerStats));
        }

        private IEnumerator writeHungerStatsCoroutine(float _hungerStats)
        {
            var DBTask = DBreference.Child("users").Child(User.UserId).Child("petInfo").Child("HungerStats").SetValueAsync(_hungerStats);
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
        }

        //Set Exp Stats

        public void writeExpStats(float _expStats)
        {
            StartCoroutine(writeExpStatsCoroutine(_expStats));
        }

        private IEnumerator writeExpStatsCoroutine(float _expStats)
        {
            var DBTask = DBreference.Child("users").Child(User.UserId).Child("petInfo").Child("ExpStats").SetValueAsync(_expStats);
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
        }

        // Set Last Fed Time
        public void writeLastFedTime(float _lft)
        {
            StartCoroutine(writeLastFedTimeCoroutine(_lft));
        }

        private IEnumerator writeLastFedTimeCoroutine(float _lastFedTime)
        {
            var DBTask = DBreference.Child("users").Child(User.UserId).Child("petInfo").Child("LastFedTIme").SetValueAsync(_lastFedTime);
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
        }

        //--------------------------------------------------Spectator Mode------------------------------------------------------------------------
        private string[] memberNames = new string[10];

        public void getSpectatorData(string _gameID)
        {
            StartCoroutine(getSpectatorDataCoroutine(_gameID));
        }

        IEnumerator getSpectatorDataCoroutine(string _gameID)
        {

            var DBTask = DBreference.Child("games").Child(_gameID).Child("gameInfo").Child("teamInfo").GetValueAsync();
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else if (DBTask.Result.Value != null)
            {
                //Data has been retrieved
                DataSnapshot snapshot = DBTask.Result;

                int index = 0;
                foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
                {
                    //Debug.Log("Looking at team with team code: " + childSnapshot.Key);
                    if (childSnapshot.Child("teamname").Value != null)
                    {
                        otherTeamNames[index] = childSnapshot.Child("teamname").Value.ToString();
                        otherTeamCodes[index] = childSnapshot.Key;
                    }
                    else
                    {
                        otherTeamNames[index] = "placeholder";
                        otherTeamCodes[index] = "placeholder";
                    }

                    if (childSnapshot.Child("totalSteps").Value != null)
                    {
                        otherTeamSteps[index] = float.Parse(childSnapshot.Child("totalSteps").Value.ToString());
                    }
                    else
                    {
                        otherTeamSteps[index] = 0;
                    }

                    if (childSnapshot.Child("distance").Value != null)
                    {
                        otherTeamDistance[index] = double.Parse(childSnapshot.Child("distance").Value.ToString());
                    }
                    else
                    {
                        otherTeamDistance[index] = 0.0f;
                    }

                    if (childSnapshot.Child("velocity").Value != null)
                    {
                        otherTeamVelocity[index] = double.Parse(childSnapshot.Child("velocity").Value.ToString());
                    }
                    else
                    {
                        otherTeamVelocity[index] = 0.0f;
                    }

                    if (childSnapshot.Child("runnerSkinID").Value != null)
                    {
                        Debug.Log("Runner Skin ID: " + int.Parse(childSnapshot.Child("runnerSkinID").Value.ToString()));
                        otherTeamSkinIDs[index] = int.Parse(childSnapshot.Child("runnerSkinID").Value.ToString());
                    }
                    else
                    {
                        otherTeamSkinIDs[index] = 0;
                    }

                    if (childSnapshot.Child("runnerImageURL").Value != null)
                    {
                        otherTeamImageURL[index] = childSnapshot.Child("runnerImageURL").Value.ToString();
                    }
                    else
                    {
                        otherTeamImageURL[index] = "placeholder";
                    }

                    if (childSnapshot.Child("runner").Value != null && childSnapshot.Child("runner").Value.ToString() != "" && childSnapshot.Child("runner").Value.ToString() != null)
                    {
                        DBTask = DBreference.Child("users").Child(childSnapshot.Child("runner").Value.ToString()).GetValueAsync();
                        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

                        if (DBTask.Exception != null)
                        {
                            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
                        }
                        else if (DBTask.Result.Value != null)
                        {
                            //Data has been retrieved
                            DataSnapshot snapshot2 = DBTask.Result;
                            otherTeamRunners[index] = snapshot2.Child("username").Value.ToString();
                        }

                    }
                    else
                    {
                        otherTeamRunners[index] = "placeholder";
                    }

                    ++index;

                }
            }
        }


        public void updateSpectatableGames()
        {
            StartCoroutine(updateSpectatableGamesCoroutine());
        }

        private IEnumerator updateSpectatableGamesCoroutine()
        {
            dataRecieved = false;

            List<string> tempGameIDs = new List<string>();
            teamAmountInEachGame.Clear();

            var DBTask = DBreference.Child("games").GetValueAsync();
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else if (DBTask.Result.Value != null)
            {
                //Data has been retrieved
                DataSnapshot snapshot = DBTask.Result;
                //Debug.Log("Getting Available Games");
                int index = 0;
                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    if (childSnapshot.Child("gameInfo").Child("teamCount").Value != null &&
                        childSnapshot != null)
                    {
                        String _teamNumber = childSnapshot.Child("gameInfo").Child("teamCount").Value.ToString();
                        String _gameID = childSnapshot.Key;

                        //Check if the name has already started
                        DBTask = DBreference.Child("games").Child(_gameID).Child("gameInfo").Child("startGame").GetValueAsync();
                        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
                        if (DBTask.Exception != null)
                        {
                            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
                        }
                        else if (DBTask.Result.Value == null)
                        {
                            Debug.Log("Found Avaliable Game: " + _gameID);
                            teamAmountInEachGame.Add(_teamNumber);
                            tempGameIDs.Add(_gameID);
                            Debug.Log(tempGameIDs.Count + " items in temp Game List");
                            index++;
                        }

                    }

                }
            }

            gameIDList = tempGameIDs;
            Debug.Log(gameIDList.Count + " items in gameIDList");

            dataRecieved = true;
        }

        public void clear_spectator_data()
        {
            resetGameVariables();
        }

        public void updateSpectatorLeaderboard(string _gameID, string _winTeamCode, int _TOTAL_STEPS)
        {
            StartCoroutine(updateSpectatorLeaderboardCoroutine(_gameID, _winTeamCode, _TOTAL_STEPS));
        }

        IEnumerator updateSpectatorLeaderboardCoroutine(string _gameID, string _winTeamCode, int _TOTAL_STEPS)
        {
            var DBTask = DBreference.Child("games").Child(_gameID).Child("gameInfo").Child("teamInfo").OrderByChild("distance").GetValueAsync();
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else if (DBTask.Result.Value != null)
            {
                //Data has been retrieved
                DataSnapshot snapshot = DBTask.Result;

                int index = 1;

                foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
                {
                    if (childSnapshot.Key == _winTeamCode)
                    {
                        leaderboardTeamNameOrderedByRank[0] = childSnapshot.Child("teamname").Value.ToString();
                        leaderboardTotalStepsOrderedByRank[0] = _TOTAL_STEPS;
                    }
                    else
                    {
                        if (childSnapshot.Child("teamname").Value != null)
                        {
                            leaderboardTeamNameOrderedByRank[index] = childSnapshot.Child("teamname").Value.ToString();
                        }
                        else
                        {
                            leaderboardTeamNameOrderedByRank[index] = "placeholder";
                        }

                        if (childSnapshot.Child("distance").Value != null)
                        {
                            leaderboardTotalStepsOrderedByRank[index] = double.Parse(childSnapshot.Child("distance").Value.ToString());
                        }
                        else
                        {
                            leaderboardTotalStepsOrderedByRank[index] = 0;
                        }

                        index++;
                    }
                }

                for (int i = index; i < 5; ++i)
                {
                    leaderboardTeamNameOrderedByRank[i] = "placeholder";
                    leaderboardTotalStepsOrderedByRank[i] = 0;
                }
            }
        }

        public void readTeamMemberNames(string _teamCode)
        {
            StartCoroutine(readTeamMemberNamesCoroutine(_teamCode));
        }

        private IEnumerator readTeamMemberNamesCoroutine(string _teamCode)
        {
            dataRecieved = false;

            var DBTask = DBreference.Child("teams").Child(_teamCode).Child("nameList").GetValueAsync();
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else if (DBTask.Result.Value != null)
            {
                //Data has been retrieved
                DataSnapshot snapshot = DBTask.Result;

                int index = 0;

                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    if (childSnapshot.Value != null)
                    {
                        memberNames[index] = childSnapshot.Value.ToString();
                    }
                    else
                    {
                        Debug.Log("memberNames[" + index + "]: " + "placeholder");
                        memberNames[index] = "placeholder";
                    }

                    ++index;
                }


            }

            dataRecieved = true;
        }

        public string[] getMemberNames()
        {
            return memberNames;
        }
    }
}

