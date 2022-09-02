using System;
using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using System.Linq;
using Firebase.Extensions;
using Firebase.Storage;
using Unity.VisualScripting;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager instance;
    
    //Firebase Refrences
    private DependencyStatus dependencyStatus;
    private FirebaseAuth auth;    
    private FirebaseUser User;
    private DatabaseReference DBreference;
    private FirebaseStorage storage;
    private StorageReference storageRef;
    
    [Header("Firebase")]
    [SerializeField] private String _storageReferenceUrl;

    [Header("ScreenManager")] 
    [SerializeField] private ScreenManager _screenManager;
    
    [Header("UserData")] 
    [SerializeField] private String baseUserPhotoUrl;
    public Texture userImageTexture;
    
    //Todo: remove this stuff variables
    //User Data variables
    [Header("UserData")]
    public TMP_InputField usernameField;
    public TMP_InputField xpField;
    public TMP_InputField killsField;
    public TMP_InputField deathsField;
    public Transform scoreboardContent;
    public void SaveDataButton()
    {
        // StartCoroutine(TryUpdateUsernameAuth(usernameField.text));
        // StartCoroutine(TryUpdateUsernameDatabase(usernameField.text));
        StartCoroutine(UpdateXp(int.Parse(xpField.text)));
        StartCoroutine(UpdateKills(int.Parse(killsField.text)));
        StartCoroutine(UpdateDeaths(int.Parse(deathsField.text)));
    }
    public void ScoreboardButton()
    {        
        StartCoroutine(LoadScoreboardData());
    }
    //--------------------------
    
    
    //initializer
    void Awake()
    {
        //singleton
        if (instance != null)
        {
            Destroy(gameObject);

        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        
        //Check that all of the necessary dependencies for Firebase are present on the system
        Debug.Log("Initializing Firebase...");
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                //If they are avalible Initialize Firebase
                InitializeFirebase();
                Debug.Log("Firebase Initialization Complete!");
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
        
        storage = FirebaseStorage.DefaultInstance;
        storageRef = storage.GetReferenceFromUrl(_storageReferenceUrl);
    }

    private void Start()
    {
        StartCoroutine(TryAutoLogin());
    }

    private void InitializeFirebase()
    {
        //Set the authentication instance object
        auth = FirebaseAuth.DefaultInstance;
        DBreference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    
    public void SignOut()
    {
        PlayerPrefs.SetString("Username", "null");
        PlayerPrefs.SetString("Password", "null");
        auth.SignOut();
    }
    public void DeleteFile(String _location)
    {
        storageRef = storageRef.Child(_location);
        storageRef.DeleteAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted) {
                Debug.Log("File deleted successfully.");
            }
            else {
                // Uh-oh, an error occurred!
            }
        });
    }
    
    //async client-side
    private IEnumerator TryAutoLogin()
    {
        yield return new WaitForSeconds(0.2f);
        String username = PlayerPrefs.GetString("Username");
        String password = PlayerPrefs.GetString("Password");
        if (username != "null" && password != "null")
        {
            Debug.Log("auto logging in");
            StartCoroutine(FirebaseManager.instance.TryLogin(username, password, (myReturnValue) => {
                if (myReturnValue != null)
                {
                    //confirmLoginText.text = "";
                    //warningLoginText.text = myReturnValue;
                }
                else
                {
                    //warningLoginText.text = "";
                    //confirmLoginText.text = "Confirmed, Your In!";
                    _screenManager.GetComponent<ScreenManager>().Login();
                }
            }));
        }
        else
        {
            Debug.Log("change screen");
            _screenManager.ChangeScreen("LoginScreen");
        }
        //PlayerPrefs.SetString("username","John Doe");
    }
    public IEnumerator TryLogin(string _email, string _password,  System.Action<String> callback)
    {
        Debug.Log(_email + ", " + _password);
        //Call the Firebase auth signin function passing the email and password
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        //Wait until the task completes
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
            // warningLoginText.text = message;
            // Debug.LogWarning(message);
            callback(message);
        }
        else
        {
            User = LoginTask.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            Debug.Log("logged In: user profile photo is: " + User.PhotoUrl);

            // getUserImage
            StartCoroutine(FirebaseManager.instance.TryLoadUserProfileImage((myReturnValue) => {
                if (myReturnValue != null)
                {
                    userImageTexture = myReturnValue;
                }
                else
                {
                
                }
            }));
            
            //stay logged in
            PlayerPrefs.SetString("Username", _email);
            PlayerPrefs.SetString("Password", _password);
            PlayerPrefs.Save();
            
            yield return null;
            callback(null);
        }
    }
    public IEnumerator TryRegister(string _email, string _password, string _username,  System.Action<String> callback)
    {
        if (_username == "")
        {
            callback("Missing Username");
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
                    // case AuthError.WeakPassword:
                    //     message = "Weak Password";
                    //     break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Email Already In Use";
                        break;
                }
                Debug.LogWarning(message);
                callback(message);
            }
            else
            {
                //User has now been created
                //Now get the result
                User = RegisterTask.Result;

                if (User != null)
                {
                    //Create a user profile and set the username
                    UserProfile profile = new UserProfile{DisplayName = _username};
                    
                    //Call the Firebase auth update user profile function passing the profile with the username
                    var ProfileTask = User.UpdateUserProfileAsync(profile);
                    //Wait until the task completes
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        //If there are errors handle them
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        // warningRegisterText.text = "Username Set Failed!";
                        Debug.LogWarning("Username Set Failed!");
                        
                        callback("Something Went Wrong, Sorry");
                    }
                    else
                    {
                        //log user in
                        StartCoroutine(TryLogin(_email, _password, (myReturnValue) => {
                            if (myReturnValue != null)
                            {
                                Debug.LogWarning("failed to login");
                            }
                            else
                            {
                                Debug.Log("Logged In!");
                            }
                        }));
                        
                        //set base user profile photo
                        var user = auth.CurrentUser;
                        if (user != null)
                        {
                            Firebase.Auth.UserProfile userProfile = new Firebase.Auth.UserProfile
                            {
                                DisplayName = user.DisplayName,
                                PhotoUrl = new System.Uri("https://www.htgtrading.co.uk/wp-content/uploads/2016/03/no-user-image-square.jpg")
                            };
                            user.UpdateUserProfileAsync(userProfile).ContinueWith(task =>
                            {
                                if (task.IsCanceled)
                                {
                                    Debug.LogError("UpdateUserProfileAsync was canceled.");
                                    return;
                                }
    
                                if (task.IsFaulted)
                                {
                                    Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
                                    return;
                                }
    
                                User = auth.CurrentUser;
                                Debug.Log(user.DisplayName);
                                Debug.Log(user.PhotoUrl);
    
                                Debug.Log("User profile updated successfully.");
    
                                Debug.Log("current user url:" + user.PhotoUrl);
                                // userProfile.GetProfile();
                            });
    
                            yield return null;
                        }
                    }
                }
                callback(null);
            }
        }
    }
    public IEnumerator TryUpdateUsernameDatabase(string _name, System.Action<String> callback)
    {
        Debug.Log("trying to update username database");
        //Set the currently logged in user username in the database
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("username").SetValueAsync(_name);
        
        Debug.Log("trying to update username database");
    
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        Debug.Log("trying to update username database");
    
        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            Debug.Log("updated username database");
    
            //Database username is now updated
        }
    }
    public IEnumerator TryUpdateProfilePhoto(Image _image, System.Action<String> callback)
    {
        String _profilePhotoUrl = "profileUrl";
        var user = auth.CurrentUser;
    
        if (user != null)
        {
            Firebase.Auth.UserProfile profile = new Firebase.Auth.UserProfile
            {
                DisplayName = user.DisplayName,
                PhotoUrl = new System.Uri("https://randomuser.me/api/portraits/men/95.jpg")
            };
            user.UpdateUserProfileAsync(profile).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("UpdateUserProfileAsync was canceled.");
                    return;
                }
    
                if (task.IsFaulted)
                {
                    Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
                    return;
                }
    
                User = auth.CurrentUser;
                Debug.Log(user.DisplayName);
                Debug.Log(user.PhotoUrl);
    
                Debug.Log("User profile updated successfully.");
    
                Debug.Log("current user url:" + user.PhotoUrl);
                _profilePhotoUrl = user.PhotoUrl.ToString();
                // userProfile.GetProfile();
            });
    
            yield return null;
            callback(_profilePhotoUrl);
        }
    }
    public IEnumerator TryLoadUserProfileImage(System.Action<Texture> callback)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(User.PhotoUrl); //Create a request
        yield return request.SendWebRequest(); //Wait for the request to complete
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
        }
        else
        {
            callback(((DownloadHandlerTexture)request.downloadHandler).texture);
        }
    }
    
    
    
    
    
    
    
    
    // public IEnumerator TryUpdateUsernameAuth(string _username)
    // {
    //     //Create a user profile and set the username
    //     UserProfile profile = new UserProfile { DisplayName = _username };
    //
    //     //Call the Firebase auth update user profile function passing the profile with the username
    //     var ProfileTask = User.UpdateUserProfileAsync(profile);
    //     //Wait until the task completes
    //     yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);
    //
    //     if (ProfileTask.Exception != null)
    //     {
    //         Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
    //     }
    //     else
    //     {
    //         //Auth username is now updated
    //     }        
    // }
   
    // public IEnumerator TryLoadImage(string MediaUrl, System.Action<Texture> callback)
    // {
    //     UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl); //Create a request
    //     yield return request.SendWebRequest(); //Wait for the request to complete
    //     if (request.isNetworkError || request.isHttpError)
    //     {
    //         Debug.Log(request.error);
    //     }
    //     else
    //     {
    //         callback(((DownloadHandlerTexture)request.downloadHandler).texture);
    //     }
    // }
    

    
    // public IEnumerator TryDownloadImage(string MediaUrl, System.Action<Texture> callback)
    // {
    //     
    // }

    private IEnumerator TryUploadImage(string MediaUrl, System.Action<Texture> callback)
    {
        
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl); //Create a request
        yield return request.SendWebRequest(); //Wait for the request to complete
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
        }
        else
        {
            callback(((DownloadHandlerTexture)request.downloadHandler).texture);
        }
    }

    
    
    
    
    //Todo: make these work with present project
    private IEnumerator UpdateXp(int _xp)
    {
        //Set the currently logged in user xp
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("xp").SetValueAsync(_xp);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Xp is now updated
        }
    }
    private IEnumerator UpdateKills(int _kills)
    {
        //Set the currently logged in user kills
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("kills").SetValueAsync(_kills);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Kills are now updated
        }
    }
    private IEnumerator UpdateDeaths(int _deaths)
    {
        //Set the currently logged in user deaths
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("deaths").SetValueAsync(_deaths);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Deaths are now updated
        }
    }

    private IEnumerator LoadUserData()
    {
        //Get the currently logged in user data
        var DBTask = DBreference.Child("users").Child(User.UserId).GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            //No data exists yet
            xpField.text = "0";
            killsField.text = "0";
            deathsField.text = "0";
        }
        else
        {
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;

            // xpField.text = snapshot.Child("xp").Value.ToString();
            // killsField.text = snapshot.Child("kills").Value.ToString();
            // deathsField.text = snapshot.Child("deaths").Value.ToString();
        }
    }

    private IEnumerator LoadScoreboardData()
    {
        //Get all the users data ordered by kills amount
        var DBTask = DBreference.Child("users").OrderByChild("kills").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;

            //Destroy any existing scoreboard elements
            foreach (Transform child in scoreboardContent.transform)
            {
                Destroy(child.gameObject);
            }

            //Loop through every users UID
            foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
            {
                string username = childSnapshot.Child("username").Value.ToString();
                int kills = int.Parse(childSnapshot.Child("kills").Value.ToString());
                int deaths = int.Parse(childSnapshot.Child("deaths").Value.ToString());
                int xp = int.Parse(childSnapshot.Child("xp").Value.ToString());

            }

            //Go to scoareboard screen
            // UIManager.instance.ScoreboardScreen();
        }
    }
}
