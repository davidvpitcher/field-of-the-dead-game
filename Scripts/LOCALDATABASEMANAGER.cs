using Mono.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;

public class LOCALDATABASEMANAGER : MonoBehaviour
{
    public static LOCALDATABASEMANAGER Instance { get; private set; }


    public Toggle myToggle;
    private const string TOGGLE_STATE_KEY = "UseLocalData";
    private string conn; // Connection string to the SQLite database

    private const string databaseName = "PlayerData";

    private Dictionary<string, string> PlayerDataColumns = new Dictionary<string, string>
{
    { "grenade_count", "INTEGER NOT NULL DEFAULT 0" },
    { "win_count", "INTEGER DEFAULT 0" },
    { "current_armor", "TEXT DEFAULT 'NOTHING/NOTHING/NOTHING/NOTHING/NOTHING/NOTHING/'" },
    { "funds", "INTEGER DEFAULT 0" },
    { "kill_count", "INTEGER NOT NULL DEFAULT 0" },
    { "upgraded_printmod", "INTEGER DEFAULT 0" },
    { "chosen_class", "INTEGER DEFAULT 0" },
    { "current_skin", "INTEGER DEFAULT 0" },
    { "collectiblesData", "TEXT DEFAULT '0000000000000000000000000000000000000000000000000000000000000000'" },
    { "next_daily", "TEXT DEFAULT ''" },
    { "hotkeys", "TEXT DEFAULT 'NULL/NULL/NULL/NULL/NULL/NULL/NULL/NULL/NULL/NULL/NULL'" },
    { "receivedAlphonsoUpgrade", "INTEGER DEFAULT 0" },
    { "reward_crates", "INTEGER DEFAULT 0" },
       { "questdata", "TEXT DEFAULT '0000000000000000000000000000000000000000000000000000000000000000'" },
    { "enemiesKilled", "INTEGER DEFAULT 0" },
    { "thingsDestroyed", "INTEGER DEFAULT 0" },
    { "grindCompleted", "INTEGER DEFAULT 0" },
    { "repeatProgress", "INTEGER DEFAULT 0" },
        { "highest_completed_questdata", "TEXT DEFAULT '0000000000000000000000000000000000000000000000000000000000000000'" },
        { "storage_data", "TEXT DEFAULT ''" },
    { "ACCEL", "INTEGER DEFAULT 1" },
    { "SPEED", "INTEGER DEFAULT 1" },
    { "TURN", "INTEGER DEFAULT 1" },
    { "HEALTH", "INTEGER DEFAULT 1" },
    { "BRAKE", "INTEGER DEFAULT 1" },
            { "unlocked_vehicle_terminal", "INTEGER DEFAULT 0" }

};

    private Dictionary<string, string> ItemsColumns = new Dictionary<string, string>
{
    { "id", "INTEGER PRIMARY KEY AUTOINCREMENT" },
    { "item_name", "TEXT UNIQUE" }
};

    private Dictionary<string, string> PlayerItemsColumns = new Dictionary<string, string>
{
    { "username", "TEXT" },
    { "item_id", "INTEGER" },
    { "quantity", "INTEGER" },
    { "PRIMARY KEY", "(username, item_id)" }
};

    // we'll need to update these dictionary whenever we add a new column in CreateDatabase()
    // if there is no default value, you will run into errors

    private Dictionary<string, string> PlayerAchievementsColumns = new Dictionary<string, string>
{
    { "username", "TEXT NOT NULL" },
    { "achievementID", "INTEGER NOT NULL" },
    { "isCompleted", "INTEGER DEFAULT 0" },
    { "completionDate", "TEXT NOT NULL" }
};

    private Dictionary<string, string> BuiltObjectsColumns = new Dictionary<string, string>
{
    { "id", "INTEGER PRIMARY KEY" },
    { "type", "TEXT" },
    { "posX", "REAL" },
    { "posY", "REAL" },
    { "posZ", "REAL" },
    { "rotX", "REAL" },
    { "rotY", "REAL" },
    { "rotZ", "REAL" },
    { "rotQ", "REAL" },
    { "hp", "INTEGER" }
};

    private Dictionary<string, string> LostAndFoundColumns = new Dictionary<string, string>
{
    { "username", "TEXT NOT NULL" },
    { "itemName", "TEXT NOT NULL" },
    { "amount", "INTEGER NOT NULL DEFAULT 0" }
};



    /*
     *  all files in the project have access to DBManager static class
     *  so they can call DBManager.conn for conn, and DBManager.username to get that data
     * */
    private void Awake()
    {

        checkInitializations();
    }
    public void checkInitializations()
    {

        if (!initializedEver)
        {
            Debug.Log("CHECKED INITIALIZATIONS, WE HAD NOT INITIALIZED THE LOCAL DATABASE");
            initializationRoutine();
            initializedEver = true;
        } else
        {

            Debug.Log("CHECKED INITIALIZATIONS, WE HAD ALREADY INITIALIZED THE LOCAL DATABASE");
        }
    }
    public bool DoesUserExist(string username)
    {
        bool userExists = false;

        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();

            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = $"SELECT COUNT(username) FROM {databaseName} WHERE username = @username";
                cmd.Parameters.Add(new SqliteParameter("@username", username));

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count > 0)
                    userExists = true;
            }

            dbConnection.Close();
        }

        return userExists;
    }

    public void resetFundsAndScoreForClient()
    {

        DBManager.funds = 0;
        DBManager.score = 0;
        Debug.Log("RESETTING FUNDS AND SCORE JUST SO YOU'RE AWARE");
        DBManager.userid = 0;
        DBManager.myclass = 0;
        DBManager.myskin = 0;

        Debug.Log("GONNA RESET USERID CLASS AND SKIN TO JUST SO YOU'RE AWARE");
    }
    public void initializeDatabaseForNewPlayer(string who)
    {
        //   initializedEver = false;
        if (!DBManager.useLocalStorage)
        {
            Debug.Log("dont initialize database if using php server");
            return;
        }
        checkInitializations();
        DBManager.username = who;


        resetFundsAndScoreForClient();

        Debug.Log("INITIALIZING THE GAME OFFLINE STORAGE DATABASE on behalf of: " + who);

        PlayerPrefs.SetString("OFFLINEusername", who);
        PROFILESELECTIONMANAGER.Instance.primaryGameFeedback.text = "Created new offline player AS: " + who;
        PROFILESELECTIONMANAGER.Instance.currentProfileLabel.text = who;
        initializedDatabase = false;
        // postLoginRoutine();
        InsertNewPlayer(who); // Ensure that the player's data exists in the database
       // FetchPlayerData();
        Debug.Log("Database and player data initialized and verified.");
        initializedDatabase = true;
        onInitializeDatabaseForUser();
    }
    public void initializeDatabaseOnBehalfOf(string who)
    {
        //   initializedEver = false;
        if (!DBManager.useLocalStorage)
        {
            Debug.Log("dont initialize database if using php server");
            return;
        }
        checkInitializations();
        DBManager.username = who;
        Debug.Log("INITIALIZING THE GAME OFFLINE STORAGE DATABASE on behalf of: " + who);
        PlayerPrefs.SetString("OFFLINEusername", who);

        if (PROFILESELECTIONMANAGER.Instance == null)
        {
            Debug.Log("NO PROFILE SELECTION MANAGER FOUND");
        }
        if (PROFILESELECTIONMANAGER.Instance.primaryGameFeedback == null)
        {
            Debug.Log("NO PROFILE SELECTION MANAGER primaryGameFeedback FOUND");
        }
        if (PROFILESELECTIONMANAGER.Instance.currentProfileLabel == null)
        {
            Debug.Log("NO PROFILE SELECTION MANAGER currentProfileLabel FOUND");
        }
        PROFILESELECTIONMANAGER.Instance.primaryGameFeedback.text = "LOGGED IN with local data AS: " + who;
        PROFILESELECTIONMANAGER.Instance.currentProfileLabel.text = who;
        initializedDatabase = false;
        // postLoginRoutine();
      //  InsertNewPlayer(who); // Ensure that the player's data exists in the database
                             FetchPlayerData();
        Debug.Log("Database and player data initialized and verified.");
        initializedDatabase = true;

        onInitializeDatabaseForUser();
    }

    public VehicleUpgradeManager vehicleUpgradeManager;
    public void onInitializeDatabaseForUser()
    {
        vehicleUpgradeManager.FetchAndInitializeStatsLocal();
        Debug.Log("initializing local vehicle stats");

        if (!validatedDatabase)
        {
            InitializeDatabase();
        }
    }


    public void initializationRoutine()
    {

        string path1 = Path.Combine(Directory.GetParent(Application.dataPath).FullName, databaseName + ".db");
        Debug.Log("1111 DB Path: " + path1);
      string path2 = Path.Combine(Application.persistentDataPath, databaseName + ".db");
        Debug.Log("2222 DB Path: " + path2);
        if (Instance == null)
        {
            Instance = this;
            // conn = "URI=file:" + Application.dataPath + "/" + databaseName + ".db";
#if UNITY_EDITOR
            conn = "URI=file:" + path1;
          
            Debug.Log("Editor DB Path: " + path1);
            dataBasePath = path1;
#else
    conn = "URI=file:" + path2;
    Debug.Log("Build DB Path: " + path2);
        
    dataBasePath = path2;
#endif


            Debug.Log("Connection String Value: " + conn);


            DBManager.conn = conn;
            DBManager.databaseName = databaseName;
            //   LogColumnCount(databaseName);

            if (string.IsNullOrEmpty(conn))
            {
                Debug.LogError("Connection string is empty or null!");
                PROFILESELECTIONMANAGER.Instance.primaryGameFeedback.text = "database error";

                selectProfileButtonBold.SetActive(true);
                return;
            }




      


            if (myToggle == null)
            {
                Debug.LogError("Toggle reference is not set.");
                return;
            }



            LoadToggleState(); // this allows the player to move from cloud (PHP/mysql) to offline storage (SQLite)

            myToggle.onValueChanged.AddListener(OnToggleValueChanged);

            bool doesDatabaseExist = ensureDatabaseExists();

            if (doesDatabaseExist)
            {
                
                string lastPlayerName = PlayerPrefs.GetString("OFFLINEusername");

                if (!string.IsNullOrEmpty(lastPlayerName)) {

                    if (!DoesPlayerExist(lastPlayerName))
                    {

                        PlayerPrefs.SetString("OFFLINEusername", "");

                        PROFILESELECTIONMANAGER.Instance.primaryGameFeedback.text = "No user logged in..";
                        PROFILESELECTIONMANAGER.Instance.resetDBManager();
                        Debug.Log("FORCED SCRUB USERNAME, did you cahnge database");
                    }

                    }

                divertLoginSystemIfNecessary(DBManager.useLocalStorage);
            } else
            {
                Debug.Log("DATABASE DIDNT EXIST, AVOIDING DIVERSIONS");
                PlayerPrefs.SetString("OFFLINEusername", "");
                if (DBManager.useLocalStorage)
                {
                    selectProfileButtonBold.SetActive(true);

                    PROFILESELECTIONMANAGER.Instance.primaryGameFeedback.text = "No user logged in, create a profile to play";

                } else
                {

                    selectProfileButtonBold.SetActive(true);

                    PROFILESELECTIONMANAGER.Instance.primaryGameFeedback.text = "No user logged in, create a new profile to play";
                    Debug.Log("ODD SCENARIO, is everything working");
                }

            }

        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

    }

    public bool ensureDatabaseExists()
    {
        if (!File.Exists(conn.Replace("URI=file:", "")))
        {
            // Database doesn't exist, create a new one by simply opening a connection.
            using (var connection = new SqliteConnection(conn))
            {
                connection.Open();  // This should create a new database file.
                connection.Close();
            }


            InitializeDatabase();
            return false;
        }

        return true;


    }
    public registrationmanager RegistrationManager;

    public Login login;

    public GameObject selectProfileButtonBold;
    public GameObject logoutButtonBold;
    public void divertLoginSystemIfNecessary(bool usingLocalStorage)
    {
        login.StopAllCoroutines();
        login.autologged = false;

        if (usingLocalStorage)
        {
        //   PROFILESELECTIONMANAGER.Instance.resetDBManager();
            RegistrationManager.smalllogout(); // logs out without booting autosign in
            //  registrationButton.SetActive(false);
            //  signInButton.SetActive(false);

            RegistrationManager.hideregistrationStuff();
            RegistrationManager.registrationScreen = false;
            selectProfileButtonBold.SetActive(true);

            autoSignInUsingOfflineStorage();

            // this is where you would use offline mode, and local storage

            // handle the case where the user is logged online?
            Debug.Log("OFFLINE ROUTE");


        }
        else
        {
            PROFILESELECTIONMANAGER.Instance.offlineLogoutCOMPLETELY();
            // this is where the break happens
            selectProfileButtonBold.SetActive(false);
            Debug.Log("ONLINE ROUTE");

            PROFILESELECTIONMANAGER.Instance.resetDBManager();
            // this is where the game either begins in online or offline mode

            // from here on in, the game calls PHP server in online mode

            login.AwakeRoutine();

        }


    }
    public HOVERMANAGER disconnectButton;

    public GameObject registrationButton;
    public GameObject signInButton;

    public bool initializedEver = false;


    public void autoSignInUsingOfflineStorage()
    {
        int AUTOsigninINT = PlayerPrefs.GetInt("AUTOSIGNIN", 1);
        Debug.Log("CHECKING AUTO SIGNIN: " + AUTOsigninINT);
        if (AUTOsigninINT == 1)
        {



            if (string.IsNullOrEmpty(DBManager.username))
            {
                autoSignIn();
            }
            else
            {

               //RegistrationManager.allowPlayButtonsForOfflineMode();
                Debug.Log("You're already signed in!");
                PROFILESELECTIONMANAGER.Instance.onSuccessfulOfflineLogin();
                autologged = true;
                PROFILESELECTIONMANAGER.Instance.feedback.text = "Signed in as " + DBManager.username;

                if (initializedDatabase)
                {
                    Debug.Log("We even initialized the database!");
                } else
                {
                    initializeDatabaseOnBehalfOf(DBManager.username);
                    autologged = true;
                    Debug.Log("WARNING did NOT initialize the database! Proceeding to attempt connection");
                }
            }






        } else
        {
            Debug.Log("AUTOSIGN IN DENIED");
        }
    }

    public bool checkIfDatabaseExists()
    {

        return File.Exists(conn.Replace("URI=file:", ""));
    }

        public void autoSignIn()
    {
        if (!checkIfDatabaseExists())
        {
            // Database doesn't exist; initialize it for a new player

            login.feedback.text = "error no database exists, relaunch the game";
            Debug.LogError("no database exists");
            return;
        }


        string username = PlayerPrefs.GetString("OFFLINEusername");
        Debug.Log("HISTORY SAYS: " + username);
        if (string.IsNullOrEmpty(username))
        {
            Debug.Log("unable to autosign in, empty user ");


            login.feedback.text = "Unable to auto sign in, select or create a profile to play";
            Debug.LogError("EMPTY USER");
            return;
        }

        EnsurePlayerExistsByName(username);


        login.feedback.text = "Attempting to auto sign in...";
        initializeDatabaseOnBehalfOf(username);
        autologged = true;
        PROFILESELECTIONMANAGER.Instance.onSuccessfulOfflineLogin();
        
        //  RegistrationManager.allowPlayButtonsForOfflineMode();
    }
        public bool autologged = false;

    public void postLoginRoutine() // this guarantees we know the username of the player before initializing any data
    {
        checkInitializations();
        if (initializedDatabase)
        {
            return;
        }
        // username available via DBManager.username
        InitializeDatabase();

        EnsurePlayerExists(); // Ensure that the player's data exists in the database
        FetchPlayerData(); 
        Debug.Log("Database and player data initialized and verified.");
        initializedDatabase = true;

        onInitializeDatabaseForUser();
    }

    private bool DoesPlayerExist(string username)
    {
        Debug.Log("VALIDATING PLAYER:" + username);
        bool playerExists = false;  // Flag to track player's existence

        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                // Check if player exists
                cmd.CommandText = $"SELECT COUNT(1) FROM {databaseName} WHERE username = @username";
                cmd.Parameters.Add(new SqliteParameter("@username", username));

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count == 0)
                {
                    Debug.Log("Player does not exist");
                }
                else
                {
                    // Player already exists
                    playerExists = true;
                }
            }
            dbConnection.Close();
        }

        return playerExists; // Return true if player existed, false otherwise
    }

    private void CreateDatabase()
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();

            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                // Dynamically build the columns for the PlayerData table
                string columnsDefinition = "username TEXT PRIMARY KEY, ";
                foreach (var column in PlayerDataColumns)
                {
                    columnsDefinition += $"{column.Key} {column.Value}, ";
                }
                columnsDefinition = columnsDefinition.TrimEnd(',', ' ');  // remove trailing comma and space

                // Create the PlayerData table
                cmd.CommandText = $@"CREATE TABLE IF NOT EXISTS {databaseName} ({columnsDefinition})";
                cmd.ExecuteNonQuery();

                Debug.Log($"Table '{databaseName}' created successfully.");
            }

            dbConnection.Close();
        }
    }

    public void reset()
    {
        initializedDatabase = false;
    }

    private string dataBasePath;
    public bool initializedDatabase = false;

    public bool validatedDatabase = false;
    private void InitializeDatabase()
    {

        //  if (!System.IO.File.Exists(Application.dataPath + $"/{databaseName}.db"))
        if (!System.IO.File.Exists(dataBasePath))
        {
            Debug.Log("Database file not found. Creating...");
            CreateDatabase();
            CreateAchievementsTable();
            CreateBuiltObjectsTable();
            CreateLostAndFoundTable();
        }



        ValidateAndUpdateTableStructure(databaseName, PlayerDataColumns);
        ValidateAndUpdateTableStructure("PlayerAchievements", PlayerAchievementsColumns);
        ValidateAndUpdateTableStructure("the_builded", BuiltObjectsColumns); // Add this line
        ValidateAndUpdateTableStructure("LostAndFound", LostAndFoundColumns); // Add this line
        ValidateAndUpdateTableStructure("items", ItemsColumns);
        ValidateAndUpdateTableStructure("player_items", PlayerItemsColumns);
        validatedDatabase = true;
        Debug.Log("Database initialized and verified.");
        // Synchronize the items table with the AllItems array
       SyncItemsWithAllItemsArray();

       // Debug.LogError("DATABASE COMPLETELY INITIALIZED");
        /*
         *    using the old SyncItemsWithAllItemsArray() would repair the item database to the current state
         *    this was essential for adding new items to the game
         * 
         *    however, the current version of it will cause a huge lag when run
         *       and another solution would be required
         *       
         *       the current one doesnt seem to have the fault
         *        but consider this a suspect if there is a problem later
         * */


    }





    // other methods omitted for brevity







    public bool DeleteBuiltObject(int id)
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();

            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = $"DELETE FROM the_builded WHERE id = @id";
                cmd.Parameters.Add(new SqliteParameter("@id", id));

                int result = cmd.ExecuteNonQuery();

                dbConnection.Close();
                return result > 0;
            }
        }
    }

    private void EnsurePlayerExistsByName(string username)
    {

        Debug.Log("VALIDATING PLAYER:" + username);
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                // Check if player exists
                cmd.CommandText = $"SELECT COUNT(1) FROM {databaseName} WHERE username = @username";
                cmd.Parameters.Add(new SqliteParameter("@username", username));

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count == 0)
                {
                    // Build the insert query dynamically
                    var columnNames = new List<string> { "username" };
                    var paramNames = new List<string> { "@username" };
                    var parameters = new List<SqliteParameter>
    {
        new SqliteParameter("@username", username)
    };

                    foreach (var pair in PlayerDataColumns)
                    {
                        columnNames.Add(pair.Key);
                        paramNames.Add($"@{pair.Key}");

                        // Extract default value from column definition if available
                        // Extract default value from column definition if available
                        var match = System.Text.RegularExpressions.Regex.Match(pair.Value, @"DEFAULT ([^\s]+)");
                        var defaultValue = match.Success ? match.Groups[1].Value : "NULL";
                        if (defaultValue != "NULL")
                        {
                            defaultValue = defaultValue.Replace("'", "").Replace("\"", "");
                        }
                        parameters.Add(new SqliteParameter($"@{pair.Key}", defaultValue == "NULL" ? DBNull.Value : (object)defaultValue));

                    }

                    cmd.Parameters.Clear();  // Clear previous parameters
                    cmd.CommandText = $"INSERT INTO {databaseName} ({string.Join(", ", columnNames)}) VALUES ({string.Join(", ", paramNames)})";
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.Add(param);
                    }

                    Debug.Log("Executing query: " + cmd.CommandText);
                    foreach (SqliteParameter param in cmd.Parameters)
                    {
                        Debug.Log($"Parameter Name: {param.ParameterName}, Value: {param.Value}");
                    }
                    LogDatabaseSchema();
                    cmd.ExecuteNonQuery();
                    Debug.Log("Added default player data to the database.");
                }
            }
            dbConnection.Close();
        }
    }
    private void EnsurePlayerExists()
    {

        Debug.Log("VALIDATING PLAYER:" + DBManager.username);
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                // Check if player exists
                cmd.CommandText = $"SELECT COUNT(1) FROM {databaseName} WHERE username = @username";
                cmd.Parameters.Add(new SqliteParameter("@username", DBManager.username));

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count == 0)
                {
                    // Build the insert query dynamically
                    var columnNames = new List<string> { "username" };
                    var paramNames = new List<string> { "@username" };
                    var parameters = new List<SqliteParameter>
    {
        new SqliteParameter("@username", DBManager.username)
    };

                    foreach (var pair in PlayerDataColumns)
                    {
                        columnNames.Add(pair.Key);
                        paramNames.Add($"@{pair.Key}");

                        // Extract default value from column definition if available
                        // Extract default value from column definition if available
                        var match = System.Text.RegularExpressions.Regex.Match(pair.Value, @"DEFAULT ([^\s]+)");
                        var defaultValue = match.Success ? match.Groups[1].Value : "NULL";
                        if (defaultValue != "NULL")
                        {
                            defaultValue = defaultValue.Replace("'", "").Replace("\"", "");
                        }
                        parameters.Add(new SqliteParameter($"@{pair.Key}", defaultValue == "NULL" ? DBNull.Value : (object)defaultValue));

                    }

                    cmd.Parameters.Clear();  // Clear previous parameters
                    cmd.CommandText = $"INSERT INTO {databaseName} ({string.Join(", ", columnNames)}) VALUES ({string.Join(", ", paramNames)})";
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.Add(param);
                    }

                    Debug.Log("Executing query: " + cmd.CommandText);
                    foreach (SqliteParameter param in cmd.Parameters)
                    {
                        Debug.Log($"Parameter Name: {param.ParameterName}, Value: {param.Value}");
                    }
                    LogDatabaseSchema();
                    cmd.ExecuteNonQuery();
                    Debug.Log("Added default player data to the database.");
                }
            }
            dbConnection.Close();
        }
    }
    private void InsertNewPlayer(string username)
    {
        Debug.Log("INSERTING PLAYER:" + username);
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                // Build the insert query dynamically
                var columnNames = new List<string> { "username" };
                var paramNames = new List<string> { "@username" };
                var parameters = new List<SqliteParameter>
            {
                new SqliteParameter("@username", username)
            };

                foreach (var pair in PlayerDataColumns)
                {
                    columnNames.Add(pair.Key);
                    paramNames.Add($"@{pair.Key}");

                    // Extract default value from column definition if available
                    var match = System.Text.RegularExpressions.Regex.Match(pair.Value, @"DEFAULT ([^\s]+)");
                    var defaultValue = match.Success ? match.Groups[1].Value : "NULL";
                    if (defaultValue != "NULL")
                    {
                        defaultValue = defaultValue.Replace("'", "").Replace("\"", "");
                    }
                    parameters.Add(new SqliteParameter($"@{pair.Key}", defaultValue == "NULL" ? DBNull.Value : (object)defaultValue));
                }

                cmd.Parameters.Clear();  // Clear previous parameters
                cmd.CommandText = $"INSERT INTO {databaseName} ({string.Join(", ", columnNames)}) VALUES ({string.Join(", ", paramNames)})";
                foreach (var param in parameters)
                {
                    cmd.Parameters.Add(param);
                }

                Debug.Log("Executing query: " + cmd.CommandText);
                foreach (SqliteParameter param in cmd.Parameters)
                {
                    Debug.Log($"Parameter Name: {param.ParameterName}, Value: {param.Value}");
                }
                // Uncomment the next line if you want to see the database schema
                // LogDatabaseSchema();
                cmd.ExecuteNonQuery();
                Debug.Log("Added player data to the database.");
            }
            dbConnection.Close();
        }
    }

    private void CreatePlayerItemsTable()
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();

            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                string columnsDefinition = string.Join(", ", PlayerItemsColumns.Select(x => $"{x.Key} {x.Value}"));

                cmd.CommandText = $"CREATE TABLE IF NOT EXISTS player_items ({columnsDefinition})";
                cmd.ExecuteNonQuery();

                Debug.Log("Table 'player_items' created successfully.");
            }

            dbConnection.Close();
        }
    }
    public string[] AllItems => new string[]
{
     "BUILDMAT", "RUBBISH", "BIN", "BOX", "BLASTER", "BLUELASER", "DOUBLEBALL", "GRAVITYGUN",
    "LASERLANCE", "LIGHTNINGER", "PLASMAGUN", "QUADROCKET", "RAILER", "REDLASER", "ROCKETER",
    "SHOTGUN", "SINGLEBALL", "SMG", "TRIPLESHOT", "PROPANE", "SNIPER", "M416", "TRASHBIN",
    "JAR", "CARNAGEMOD", "REGENCUBE", "BLASTMOD", "SHIELDMOD", "LEECHMOD", "SIPHONMOD",
    "JUMPMOD", "DAMAGEMOD", "MAXHPMOD", "SPEEDMOD", "RADIOACTIVEMOD", "REDKEYCARD",
    "BLUEKEYCARD", "ORANGEKEYCARD", "BUZZSAW", "BLASTMOD2", "BLASTMOD3", "BLASTMOD4",
    "BLASTMOD5", "BLASTMOD1", "DAMAGEMOD1", "DAMAGEMOD2", "DAMAGEMOD3", "DAMAGEMOD4",
    "DAMAGEMOD5", "JUMPMOD1", "JUMPMOD2", "JUMPMOD3", "JUMPMOD4", "JUMPMOD5", "LEECHMOD1",
    "LEECHMOD2", "LEECHMOD3", "LEECHMOD4", "LEECHMOD5", "MAXHPMOD1", "MAXHPMOD2", "MAXHPMOD3",
    "MAXHPMOD4", "MAXHPMOD5", "RADIOACTIVEMOD1", "RADIOACTIVEMOD2", "RADIOACTIVEMOD3",
    "RADIOACTIVEMOD4", "RADIOACTIVEMOD5", "REGENCUBE1", "REGENCUBE2", "REGENCUBE3",
    "REGENCUBE4", "REGENCUBE5", "SHIELDMOD1", "SHIELDMOD2", "SHIELDMOD3", "SHIELDMOD4",
    "SHIELDMOD5", "SIPHONMOD1", "SIPHONMOD2", "SIPHONMOD3", "SIPHONMOD4", "SIPHONMOD5",
    "SPEEDMOD1", "SPEEDMOD2", "SPEEDMOD3", "SPEEDMOD4", "SPEEDMOD5", "POWERCORE",
    "STABILIZERS", "MYSTERIOUSBOX", "SEALEDTECH", "SLEDGER", "PRINTMOD", "PRINTMOD1",
    "PRINTMOD2", "PRINTMOD3", "PRINTMOD4", "PRINTMOD5", "BROKENMOD", "PRINTCHIP", "BATTERY",
    "HALFBOTTLE", "LIGHTER", "PILLS", "TRAVELMUG", "WINEBOTTLE", "BROKENWEAPON",
    "BOXOFSTABILIZERS", "LESSERSEALEDTECH", "ENERGYCELL", "SAFE", "CARBATTERY", "KEY",
    "WATERBOTTLE", "DUCTTAPE", "LAPTOP", "JERRYCAN", "TOILETROLL", "CLEANCLOTHING",
    "FISSIONCATALYST", "SPACEMASS", "BASICSEALEDTECH", "SLEDGEDASH", "DASHMOD", "DASHMOD1",
    "DASHMOD2", "DASHMOD3", "DASHMOD4", "DASHMOD5", "HEALTHSTORAGE", "ROBOFISTS",
    "CLASSIFIEDFILES", "FLASHLIGHT", "GAMECONTROLLER", "LARGEOXYGENTANK", "OLDCAN", "RADIO",
    "SKULL", "SMALLOXYGENTANK", "STOPSIGN", "TISSUEBOX", "TRAFFICLIGHTS", "TROPHY", "VASE",
    "VRHEADSET", "REBAR", "BLASTER1", "BLASTER2", "BLASTER3", "BLASTER4", "BLASTER5", "SNIPER1",
    "SNIPER2", "SNIPER3", "SNIPER4", "SNIPER5", "BLUELASER1", "BLUELASER2", "BLUELASER3",
    "BLUELASER4", "BLUELASER5", "DOUBLEBALL1", "DOUBLEBALL2", "DOUBLEBALL3", "DOUBLEBALL4",
    "DOUBLEBALL5", "LASERLANCE1", "LASERLANCE2", "LASERLANCE3", "LASERLANCE4", "LASERLANCE5",
    "LIGHTNINGER1", "LIGHTNINGER2", "LIGHTNINGER3", "LIGHTNINGER4", "LIGHTNINGER5",
    "PLASMAGUN1", "PLASMAGUN2", "PLASMAGUN3", "PLASMAGUN4", "PLASMAGUN5", "QUADROCKET1",
    "QUADROCKET2", "QUADROCKET3", "QUADROCKET4", "QUADROCKET5", "RAILER1", "RAILER2",
    "RAILER3", "RAILER4", "RAILER5", "REDLASER1", "REDLASER2", "REDLASER3", "REDLASER4",
    "REDLASER5", "ROCKETER1", "ROCKETER2", "ROCKETER3", "ROCKETER4", "ROCKETER5", "SHOTGUN1",
    "SHOTGUN2", "SHOTGUN3", "SHOTGUN4", "SHOTGUN5", "SINGLEBALL1", "SINGLEBALL2",
    "SINGLEBALL3", "SINGLEBALL4", "SINGLEBALL5", "SMG1", "SMG2", "SMG3", "SMG4", "SMG5",
    "TRIPLESHOT1", "TRIPLESHOT2", "TRIPLESHOT3", "TRIPLESHOT4", "TRIPLESHOT5", "M4161",
    "M4162", "M4163", "M4164", "M4165", "FLAMETHROWER", "FLAMETHROWER1", "FLAMETHROWER2",
    "FLAMETHROWER3", "FLAMETHROWER4", "FLAMETHROWER5", "GOGGLES", "RIGHTSHOULDERARMOR",
    "CHESTPLATE", "LEFTSHOULDERPLATE", "POUCH", "FOOTBALLHELMET", "UNICORNHAT", "GASMASK",
        "BUNNYEARS", "ARTIFACT", "CLUTCHMOD", "ELECTRICGRENADEMOD", "STAMINAMOD", "STAMINAMOD1", 
    "STAMINAMOD2", "STAMINAMOD3", "STAMINAMOD4", "STAMINAMOD5", "COOKINGPOT", "MILITARYHELMET",
"SCARF",
"BASEBALLCAP",
"SHADES",
"CAP",
"COWBOYHAT",
"TACTICALHELMET",
"BEANIE",
"WELDINGMASK",
"SLUGTOOTH"
};

    private void CreateItemsTable()
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();

            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                // Create the table
                string columnsDefinition = string.Join(", ", ItemsColumns.Select(x => $"{x.Key} {x.Value}"));
                cmd.CommandText = $"CREATE TABLE IF NOT EXISTS items ({columnsDefinition})";
                cmd.ExecuteNonQuery();

                // For each item in AllItems, check if it exists in the table, if not, insert it.
                foreach (var item in AllItems)
                {
                    cmd.CommandText = $"INSERT OR IGNORE INTO items (item_name) VALUES ('{item}')";
                    cmd.ExecuteNonQuery();
                }

                Debug.Log("Table 'items' checked and updated successfully.");
            }

            dbConnection.Close();
        }
    }

    private void SyncItemsWithAllItemsArray()
    {
        StringBuilder queryBuilder = new StringBuilder();
        queryBuilder.Append("INSERT OR IGNORE INTO items (item_name) VALUES ");

        List<string> valuesList = new List<string>();
        foreach (var item in AllItems)
        {
            string sanitizedItem = item.Replace("'", "''"); // Escape single quotes for SQL safety
            valuesList.Add($"('{sanitizedItem}')");
        }
        queryBuilder.Append(string.Join(", ", valuesList));

        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();

            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = queryBuilder.ToString();
                cmd.ExecuteNonQuery();
            }

            dbConnection.Close();
        }

        Debug.Log("Items table synchronized with AllItems array.");
    }

    /*
    private void SyncItemsWithAllItemsArray()
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();

            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                // Use a transaction to bundle all the inserts into one action
                using (IDbTransaction transaction = dbConnection.BeginTransaction())
                {
                    cmd.Transaction = transaction;

                    cmd.CommandText = "INSERT OR IGNORE INTO items (item_name) VALUES (@ItemName)";
                    IDbDataParameter paramName = cmd.CreateParameter();
                    paramName.ParameterName = "@ItemName";
                    cmd.Parameters.Add(paramName);

                    foreach (var item in AllItems)
                    {
                        paramName.Value = item;
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();  // Commit the transaction
                }
            }

            dbConnection.Close();
        }

        Debug.Log("Items table synchronized with AllItems array.");
    }

    */
    /*
    private void SyncItemsWithAllItemsArray()
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();

            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                foreach (var item in AllItems)
                {
                    // For each item in AllItems, check if it exists in the table, if not, insert it.
                    cmd.CommandText = $"INSERT OR IGNORE INTO items (item_name) VALUES ('{item}')";
                    cmd.ExecuteNonQuery();
                }
            }

            dbConnection.Close();
        }

        Debug.Log("Items table synchronized with AllItems array.");
    }

    */
    private bool TableExists(string tableName)
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
                cmd.Parameters.Add(new SqliteParameter
                {
                    ParameterName = "@tableName",
                    Value = tableName
                });
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return true; // Table exists
                    }
                }
            }
            dbConnection.Close();
        }
        return false; // Table does not exist
    }

    private bool ColumnExists(string tableName, string columnName)
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = $"PRAGMA table_info({tableName})";
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["name"].ToString() == columnName)
                            return true;
                    }
                }
            }
            dbConnection.Close();
        }
        return false;
    }
    private void AddColumn(string tableName, string columnName, string columnDefinition)
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDefinition}";
                cmd.ExecuteNonQuery();
            }
            dbConnection.Close();
        }
        Debug.Log($"Column '{columnName}' added to '{tableName}'.");
    }
    private void ValidateAndUpdateTableStructure(string tableName, Dictionary<string, string> columnsDictionary)
    {
        if (!TableExists(tableName))
        {
            if (tableName == databaseName)
            {
                CreateDatabase();
            }
            else if (tableName == "PlayerAchievements")
            {
                CreateAchievementsTable();
            }
            else if (tableName == "the_builded") // This checks if the "the_builded" table exists
            {
                CreateBuiltObjectsTable(); // This should create the "the_builded" table
            }
            else if (tableName == "LostAndFound") // This checks if the "the_builded" table exists
            {
                CreateLostAndFoundTable(); // This should create the "the_builded" table
            }
            else if (tableName == "items")
            {
                CreateItemsTable();
            }
            else if (tableName == "player_items")
            {
                CreatePlayerItemsTable();
            }

        }
        else
        {
            foreach (var entry in columnsDictionary)
            {
                if ((tableName != "player_items") || (tableName == "items"))
                {
                    if (!ColumnExists(tableName, entry.Key))
                    {
                        AddColumn(tableName, entry.Key, entry.Value);
                    }
                }
            }
        }
    }
    private void CreateTableBasedOnTableName(string tableName)
    {
        switch (tableName)
        {
            case "PlayerAchievements":
                CreateAchievementsTable();
                break;
            case "the_builded":
                CreateBuiltObjectsTable();
                break;
            case "LostAndFound":
                CreateLostAndFoundTable();
                break;
            case "items":
                CreateItemsTable();
                break;
            case "player_items":
                CreatePlayerItemsTable();
                break;
            default:
                Debug.LogWarning($"Unknown table: {tableName}. Unable to create.");
                break;
        }
    }

    private void DropAndRecreateTable(string tableName, Dictionary<string, string> columnsDictionary)
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                // Drop the table
                cmd.CommandText = $"DROP TABLE IF EXISTS {tableName}";
                cmd.ExecuteNonQuery();

                // Recreate the table with all expected columns
                string columnsDefinition = string.Join(", ", columnsDictionary.Select(col => $"{col.Key} {col.Value}"));
                cmd.CommandText = $"CREATE TABLE {tableName} ({columnsDefinition})";
                cmd.ExecuteNonQuery();
            }
            dbConnection.Close();
        }
    }
    public void SaveKillScore(int killScore)
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = $"UPDATE {databaseName} SET kill_count = @killScore WHERE username = @username";
                cmd.Parameters.Add(new SqliteParameter("@killScore", killScore));
                cmd.Parameters.Add(new SqliteParameter("@username", DBManager.username));

                cmd.ExecuteNonQuery();
            }
            dbConnection.Close();
        }
    }

    /*private void ValidateAndUpdateTableStructure(string tableName, Dictionary<string, string> columnsDictionary)
    {
        if (!TableExists(tableName))
        {
            if (tableName == databaseName)
            {
                CreateDatabase();
            }
            else if (tableName == "PlayerAchievements")
            {
                CreateAchievementsTable();
            }
            else if (tableName == "the_builded") // This checks if the "the_builded" table exists
            {
                CreateBuiltObjectsTable(); // This should create the "the_builded" table
            }
            else if (tableName == "LostAndFound") // This checks if the "the_builded" table exists
            {
                CreateLostAndFoundTable(); // This should create the "the_builded" table
            } else if (tableName == "items")
            {
                CreateItemsTable();
            }
            else if (tableName == "player_items")
            {
                CreatePlayerItemsTable();
            }

        }
        else
        {
            foreach (var entry in columnsDictionary)
            {
                if ((tableName != "player_items") || (tableName == "items"))
                {
                    if (!ColumnExists(tableName, entry.Key))
                    {
                        AddColumn(tableName, entry.Key, entry.Value);
                    }
                }
            }
        }
    }
       private void ValidateAndUpdateTableStructure(string tableName, Dictionary<string, string> columnsDictionary)
    {
        // 1. Check if the table exists
        if (!TableExists(tableName))
        {
            CreateTableBasedOnTableName(tableName);  // A refactored method to handle table creation
        }
        else
        {
            List<string> missingColumns = new List<string>();
            foreach (var entry in columnsDictionary)
            {
                // 3. Verify if expected column is present
                if (!ColumnExists(tableName, entry.Key))
                {
                    missingColumns.Add(entry.Key);
                    try
                    {
                        // 4. Attempt to add the missing column
                        AddColumn(tableName, entry.Key, entry.Value);
                    }
                    catch (SqliteException)
                    {
                        Debug.LogWarning($"Failed to add column '{entry.Key}' to '{tableName}'. Table will be recreated.");
                    }
                }
            }

            // 5. If any addition fails, recreate the table
            if (missingColumns.Any(column => !ColumnExists(tableName, column)))
            {
                DropAndRecreateTable(tableName, columnsDictionary);
            }
        }
    }

    private void ValidateAndUpdateTableStructure2(string tableName, Dictionary<string, string> expectedColumns)
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();
            List<string> existingColumns = new List<string>();

            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = $"PRAGMA table_info({tableName})";
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string columnName = reader["name"].ToString();
                        existingColumns.Add(columnName);
                    }
                }
            }

            bool recreateTable = false;
            foreach (var expectedColumn in expectedColumns.Keys)
            {
                if (!existingColumns.Contains(expectedColumn))
                {
                    recreateTable = true;
                    break;
                }
            }

            if (recreateTable)
            {
                using (IDbCommand cmd = dbConnection.CreateCommand())
                {
                    // Drop the old table
                    cmd.CommandText = $"DROP TABLE IF EXISTS {tableName}";
                    cmd.ExecuteNonQuery();

                    // Recreate the table
                    string columnsDefinition = string.Join(", ", expectedColumns.Select(col => $"{col.Key} {col.Value}"));
                    cmd.CommandText = $"CREATE TABLE {tableName} ({columnsDefinition})";
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
*/


    private void ValidateAndUpdateTableStructure2(string tableName, Dictionary<string, string> expectedColumns)
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();
            List<string> existingColumns = new List<string>();

            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = $"PRAGMA table_info({tableName})";
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string columnName = reader["name"].ToString();
                        existingColumns.Add(columnName);
                    }
                }
            }

            bool recreateTable = false;
            foreach (var expectedColumn in expectedColumns.Keys)
            {
                if (!existingColumns.Contains(expectedColumn))
                {
                    recreateTable = true;
                    break;
                }
            }

            if (recreateTable)
            {
                Debug.Log("MUST RECREATE A TABLE" + tableName);
                using (IDbCommand cmd = dbConnection.CreateCommand())
                {
                    // Drop the old table
                    cmd.CommandText = $"DROP TABLE IF EXISTS {tableName}";
                    cmd.ExecuteNonQuery();

                    // Recreate the table
                    string columnsDefinition = string.Join(", ", expectedColumns.Select(col => $"{col.Key} {col.Value}"));
                    cmd.CommandText = $"CREATE TABLE {tableName} ({columnsDefinition})";
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }

    private void CreateAchievementsTable()
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = $@"CREATE TABLE IF NOT EXISTS PlayerAchievements (
                username TEXT NOT NULL,
                achievementID INTEGER NOT NULL,
                isCompleted INTEGER DEFAULT 0,
                completionDate TEXT NOT NULL,
                PRIMARY KEY(username, achievementID))";  // Composite key ensures unique entries for each achievement per player

                cmd.ExecuteNonQuery();
            }
            dbConnection.Close();
        }
    }



    public void SavePlayerFundsToLocal(string username, int funds)
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();

            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = $"UPDATE {databaseName} SET funds = @funds WHERE username = @username";
                cmd.Parameters.Add(new SqliteParameter("@funds", funds));
                cmd.Parameters.Add(new SqliteParameter("@username", username));

                cmd.ExecuteNonQuery();
            }

            dbConnection.Close();
        }
    }










    private void LogColumnCount(string tableName)
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();

            using (IDbCommand dbCommand = dbConnection.CreateCommand())
            {
                string query = "PRAGMA table_info(" + tableName + ");";
                dbCommand.CommandText = query;

                using (IDataReader reader = dbCommand.ExecuteReader())
                {
                    int columnCount = 0;

                    while (reader.Read())
                    {
                        columnCount++;
                    }

                    Debug.Log("Table " + tableName + " has " + columnCount + " columns.");
                }
            }

            dbConnection.Close();
        }
    }


    public void onToggleValueChanged(bool next)
    {

  
            Debug.Log("THE TOGGLE REPORTS: " + next);
        


    }
    private void SaveToggleState(bool isOn)
    {
        int state = isOn ? 1 : 0; // Convert bool to int (ON=1, OFF=0)
        PlayerPrefs.SetInt(TOGGLE_STATE_KEY, state);
        PlayerPrefs.Save(); 


        DBManager.useLocalStorage = isOn;
    }

    private void LoadToggleState()
    {
        if (PlayerPrefs.HasKey(TOGGLE_STATE_KEY))
        {
            int state = PlayerPrefs.GetInt(TOGGLE_STATE_KEY, 1);
            bool isOn = state == 1; // Convert int to bool (1=ON, 0=OFF)
            myToggle.isOn = isOn;
            loadedToggleState = true;
            DBManager.useLocalStorage = isOn;
        } else
        {
            myToggle.isOn = true;
            DBManager.useLocalStorage = true;
        }
    }

    public void checkIfNeedLoadToggleState()
    {
        if (loadedToggleState)
        {
            return;
        }
        LoadToggleState();


    }

    public bool loadedToggleState = false;



    private void OnToggleValueChanged(bool isOn)
    {
        if (isOn)
        {
        

            // this means we are in OFFLINE STORAGE MODE



            Debug.Log("Toggle is ON");
        }
        else
        {
            // this means we are in ONLINE CLOUD MODE
        
            Debug.Log("Toggle is OFF");
        }

     
        SaveToggleState(isOn);


        PROFILESELECTIONMANAGER.Instance.resetDBManager();


        divertLoginSystemIfNecessary(DBManager.useLocalStorage);


        myToggle.interactable = false;

        StartCoroutine(allowToggleAfterDelay());

    }

    private IEnumerator allowToggleAfterDelay()
    {

        yield return new WaitForSeconds(1f);
        myToggle.interactable = true;

    }

    private void FetchPlayerData()
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();

            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = $"SELECT kill_count, funds FROM {databaseName} WHERE username = @username";
                cmd.Parameters.Add(new SqliteParameter("@username", DBManager.username));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        DBManager.score = reader.GetInt32(0); // Assuming score is the first column
                        DBManager.funds = reader.GetInt32(1); // Assuming funds is the second column
                    }
                }
            }

            dbConnection.Close();
        }
    }

    private void CreateBuiltObjectsTable()
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();

            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                // Create the the_builded table
                cmd.CommandText = $@"CREATE TABLE IF NOT EXISTS the_builded (
                id INTEGER PRIMARY KEY,
                type TEXT,
                posX REAL,
                posY REAL,
                posZ REAL,
                rotX REAL,
                rotY REAL,
                rotZ REAL,
                rotQ REAL,
                hp INTEGER)";

                cmd.ExecuteNonQuery();

                Debug.Log($"Table 'the_builded' created successfully.");
            }

            dbConnection.Close();
        }
    }
    private void CreateLostAndFoundTable()
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();

            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                string lostAndFoundColumnsDefinition = "";
                foreach (var column in LostAndFoundColumns)
                {
                    lostAndFoundColumnsDefinition += $"{column.Key} {column.Value}, ";
                }
                lostAndFoundColumnsDefinition = lostAndFoundColumnsDefinition.TrimEnd(',', ' ');

                // Create the LostAndFound table
                cmd.CommandText = $@"CREATE TABLE IF NOT EXISTS LostAndFound ({lostAndFoundColumnsDefinition})";
                cmd.ExecuteNonQuery();

                Debug.Log($"Table 'LastAndFound' created successfully.");
            }

            dbConnection.Close();
        }
    }
    public (int UpgradedPrintmod, int ChosenClass, int ChosenSkin, int UnlockedVehicleTerminal) GetPlayerStatsFromLocalDB(string username)
    {
        using (var connection = new SqliteConnection(conn))
        {
            connection.Open();
            using (var cmd = new SqliteCommand($"SELECT upgraded_printmod, chosen_class, current_skin, unlocked_vehicle_terminal FROM {databaseName} WHERE username = '{username}'", connection))
            {
                using (SqliteDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return (
                            UpgradedPrintmod: reader.GetInt32(0),
                            ChosenClass: reader.GetInt32(1),
                            ChosenSkin: reader.GetInt32(2),
                            UnlockedVehicleTerminal: reader.GetInt32(3)
                        );
                    }
                    else
                    {
                        Debug.LogError("EMPTY TUPLE IN DATABASE");
                        return (UpgradedPrintmod: 0, ChosenClass: 0, ChosenSkin: 0, UnlockedVehicleTerminal: 0);
                    }
                }
            }
        }
    }

    private void LogDatabaseSchema()
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = $"PRAGMA table_info({databaseName})";
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    Debug.Log("Current Database Schema:");
                    while (reader.Read())
                    {
                        Debug.Log($"Name: {reader["name"]}, Type: {reader["type"]}, NotNull: {reader["notnull"]}, DefaultValue: {reader["dflt_value"]}, PrimaryKey: {reader["pk"]}");
                    }
                }
            }
            dbConnection.Close();
        }
    }

    public void SaveCollectiblesToLocalDB(string collectiblesData)
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();

            string cmdText = $"UPDATE {databaseName} SET collectiblesData = @collectiblesData WHERE username = @username";
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = cmdText;
                cmd.Parameters.Add(new SqliteParameter("@username", DBManager.username));
                cmd.Parameters.Add(new SqliteParameter("@collectiblesData", collectiblesData));

                int result = cmd.ExecuteNonQuery();
                if (result == 0)
                {
                    Debug.Log("Error: No rows were updated.");
                }
            }

            dbConnection.Close();
        }
    }
    public string LoadCollectiblesFromLocalDB()
    {
        string collectiblesData = "";
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();

            string cmdText = $"SELECT collectiblesData FROM {databaseName} WHERE username = @username";
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = cmdText;
                cmd.Parameters.Add(new SqliteParameter("@username", DBManager.username));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        collectiblesData = reader.GetString(0);
                    }
                }
            }

            dbConnection.Close();
        }

        return collectiblesData;
    }


    public T ExecuteScalar<T>(string query, params (string, object)[] parameters)
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                dbCmd.CommandText = query;

                foreach (var (name, value) in parameters)
                {
                    IDbDataParameter param = dbCmd.CreateParameter();
                    param.ParameterName = name;
                    param.Value = value;
                    dbCmd.Parameters.Add(param);
                }

                object result = dbCmd.ExecuteScalar();
                return (result != null && result != DBNull.Value) ? (T)Convert.ChangeType(result, typeof(T)) : default(T);
            }
        }
    }

    public int ExecuteNonQuery(string query, params (string, object)[] parameters)
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                dbCmd.CommandText = query;

                foreach (var (name, value) in parameters)
                {
                    IDbDataParameter param = dbCmd.CreateParameter();
                    param.ParameterName = name;
                    param.Value = value;
                    dbCmd.Parameters.Add(param);
                }

                return dbCmd.ExecuteNonQuery();
            }
        }
    }
    public void SaveHotkeysToLocalDatabase(string username, string hotkeysData)
    {

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(hotkeysData))
        {
            Debug.Log("HOTKEYS HOT KEYS CONCERN, ABORT");
            return;
        }
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = $"UPDATE {databaseName} SET hotkeys = @hotkeys WHERE username = @username";
                cmd.Parameters.Add(new SqliteParameter("@hotkeys", hotkeysData));
                cmd.Parameters.Add(new SqliteParameter("@username", username));

                cmd.ExecuteNonQuery();
            }
        }

        //Debug.LogError("HOTKEYS WAS IN FACT RETRIEVED");
        Debug.Log("HOTKEYS ADDED TO LOCAL DATA " + hotkeysData);
    }
    public string LoadHotkeysFromLocalDatabase(string username)
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = $"SELECT hotkeys FROM {databaseName} WHERE username = @username";
                cmd.Parameters.Add(new SqliteParameter("@username", username));

                IDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {

                  //  Debug.LogError("HOTKEYS WAS IN FACT RETRIEVED");
                    return reader.GetString(0);
                }
            }
        }
        return null;
    }
    public void SaveItemToLostAndFound(string username, string itemName, int amount)
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();

            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = "INSERT OR REPLACE INTO LostAndFound (username, itemName, amount) VALUES (@username, @itemName, @amount)";
                Debug.Log("INSERTING NEW LOST AND FOUND");
                cmd.Parameters.Add(new SqliteParameter("@username", username));
                cmd.Parameters.Add(new SqliteParameter("@itemName", itemName));
                cmd.Parameters.Add(new SqliteParameter("@amount", amount));

                cmd.ExecuteNonQuery();
            }

            dbConnection.Close();
        }
    }
    public bool DeleteLostAndFoundItem(string itemName, int amountToRemove, PlayerInventory pInventory)
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();

            // Fetch current amount from the database
            int currentAmount = 0;
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = $"SELECT amount FROM LostAndFound WHERE username = @username AND itemName = @itemName";
                cmd.Parameters.Add(new SqliteParameter("@username", DBManager.username));
                cmd.Parameters.Add(new SqliteParameter("@itemName", itemName));

                IDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    currentAmount = reader.GetInt32(0);
                }
                reader.Close();
            }

            // Calculate the new amount
            int newAmount = currentAmount - amountToRemove;
            if (newAmount < 0)
            {
                Debug.LogError("Error: Not enough items in Lost and Found.");
                dbConnection.Close();
                return false;
            }
            else if (newAmount == 0)
            {
                // Remove the item entry from LostAndFound if new amount is zero
                using (IDbCommand cmd = dbConnection.CreateCommand())
                {
                    cmd.CommandText = $"DELETE FROM LostAndFound WHERE username = @username AND itemName = @itemName";
                    cmd.Parameters.Add(new SqliteParameter("@username", DBManager.username));
                    cmd.Parameters.Add(new SqliteParameter("@itemName", itemName));

                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                // Update the amount in LostAndFound
                using (IDbCommand cmd = dbConnection.CreateCommand())
                {
                    cmd.CommandText = $"UPDATE LostAndFound SET amount = @newAmount WHERE username = @username AND itemName = @itemName";
                    cmd.Parameters.Add(new SqliteParameter("@newAmount", newAmount));
                    cmd.Parameters.Add(new SqliteParameter("@username", DBManager.username));
                    cmd.Parameters.Add(new SqliteParameter("@itemName", itemName));

                    cmd.ExecuteNonQuery();
                }
            }

            // Add the removed items to player's inventory
            pInventory.AddToInventoryByName(itemName, amountToRemove);

            dbConnection.Close();
            return true;
        }
    }


    public string GetItemsFromLostAndFound(string username)
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();

            string query = "SELECT itemName, amount FROM LostAndFound WHERE username = @user";

            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = query;

                IDbDataParameter param = cmd.CreateParameter();
                param.ParameterName = "@user";
                param.Value = username;
                cmd.Parameters.Add(param);

                IDataReader reader = cmd.ExecuteReader();

                // Instead of using a List, we'll use a temporary List to help with creating an array later.
                List<LOSTANDFOUNDDISPLAY.Item> tempList = new List<LOSTANDFOUNDDISPLAY.Item>();

                while (reader.Read())
                {
                    LOSTANDFOUNDDISPLAY.Item item = new LOSTANDFOUNDDISPLAY.Item();
                    item.name = reader["itemName"].ToString();
                    item.amount = reader["amount"].ToString();
                    tempList.Add(item);
                }

                // Convert the List to an array.
                LOSTANDFOUNDDISPLAY.Item[] itemsArray = tempList.ToArray();

                LOSTANDFOUNDDISPLAY.ItemArray wrapper = new LOSTANDFOUNDDISPLAY.ItemArray();
                wrapper.items = itemsArray;

                return JsonUtility.ToJson(wrapper);
            }
        }
    }

    public bool ItemExistsInLostAndFound(string username, string itemName)
    {
        bool exists = false;
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();

            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM LostAndFound WHERE username = @username AND itemName = @itemName";
                cmd.Parameters.Add(new SqliteParameter("@username", username));
                cmd.Parameters.Add(new SqliteParameter("@itemName", itemName));

                exists = (long)cmd.ExecuteScalar() > 0;
            }

            dbConnection.Close();
        }
        return exists;
    }
  

    public void UpdateItemCountInLostAndFound(string username, string itemName)
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();

            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                Debug.Log("UPDATING ITEM COUNT FOR " + itemName);
                cmd.CommandText = "UPDATE LostAndFound SET amount = amount + 1 WHERE username = @username AND itemName = @itemName";
                cmd.Parameters.Add(new SqliteParameter("@username", username));
                cmd.Parameters.Add(new SqliteParameter("@itemName", itemName));

                cmd.ExecuteNonQuery();
            }

            dbConnection.Close();
        }
    }
    public int FetchAlphonsoUpgradeData()
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();

            IDbCommand cmd = dbConnection.CreateCommand();
            cmd.CommandText = $"SELECT receivedAlphonsoUpgrade FROM {databaseName} WHERE username = '{DBManager.username}'";

            int result = Convert.ToInt32(cmd.ExecuteScalar());

            dbConnection.Close();

            return result;
        }
    }
    public void StoreAlphonsoUpgradeData(int value)
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();

            IDbCommand cmd = dbConnection.CreateCommand();
            cmd.CommandText = $"UPDATE {databaseName} SET receivedAlphonsoUpgrade = {value} WHERE username = '{DBManager.username}'";

            cmd.ExecuteNonQuery();

            dbConnection.Close();
        }
    }


    public string GetQuestDataForRestore(string username)
    {
        string connection = "URI=file:" + Application.dataPath + "/" + databaseName + ".db";
        using (IDbConnection dbconn = new SqliteConnection(connection))
        {
            dbconn.Open();

            using (IDbCommand dbcmd = dbconn.CreateCommand())
            {
                string sqlQuery = $"SELECT highest_completed_questdata FROM {databaseName} WHERE username = '{username}'";
                dbcmd.CommandText = sqlQuery;

                using (IDataReader reader = dbcmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader.GetString(0);
                    }
                }
            }
        }

        return "";
    }
    public string GetQuestDataForRestore2(string username)
    {
        string connection = "URI=file:" + Application.dataPath + "/" + databaseName + ".db";
        using (IDbConnection dbconn = new SqliteConnection(connection))
        {
            dbconn.Open();

            string highestQuestData = string.Empty;
            string currentQuestData = string.Empty;

            // Fetch the highest completed quest data
            using (IDbCommand cmd = dbconn.CreateCommand())
            {
                string query = $"SELECT highest_completed_questdata, questdata FROM {DBManager.databaseName} WHERE username = '{username}'";
                cmd.CommandText = query;
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        highestQuestData = reader.GetString(0);
                        currentQuestData = reader.GetString(1);
                    }
                }
            }

            // Calculate totals for comparison
            int highestTotal = highestQuestData.ToCharArray().Where(char.IsDigit).Sum(c => c - '0');
            int currentTotal = currentQuestData.ToCharArray().Where(char.IsDigit).Sum(c => c - '0');

            // Now update the current quest data with the highest completed quest data only if it's higher
            if (highestTotal > currentTotal)
            {
                using (IDbCommand cmd = dbconn.CreateCommand())
                {
                    string updateQuery = $"UPDATE {DBManager.databaseName} SET questdata = '{highestQuestData}' WHERE username = '{username}'";
                    cmd.CommandText = updateQuery;
                    cmd.ExecuteNonQuery();
                }
            }

            dbconn.Close();

            return highestTotal > currentTotal ? highestQuestData : currentQuestData;
        }
    }

    public string GetHighestQuestData(string username)
    {
    //    string connection = "URI=file:" + Application.dataPath + "/" + databaseName + ".db";
        using (IDbConnection dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();

            using (IDbCommand cmd = dbconn.CreateCommand())
            {
                string query = $"SELECT highest_completed_questdata FROM {DBManager.databaseName} WHERE username = '{username}'";
                cmd.CommandText = query;

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader.GetString(0);
                    }
                }
            }

            dbconn.Close();
        }

        return "NOT_FOUND";
    }
    public void StoreItemForPlayer(string username, string itemName, int quantity)
    {
        using (IDbConnection dbconn = new SqliteConnection(conn))
        {
            dbconn.Open();

            // First, get the item_id
            int itemId = -1;
            using (IDbCommand cmd = dbconn.CreateCommand())
            {
                string query = $"SELECT id FROM items WHERE item_name = '{itemName}'";
                cmd.CommandText = query;
                itemId = Convert.ToInt32(cmd.ExecuteScalar());
            }
            Debug.Log("ITEM ID: " + itemId);
            // Now, check if the entry exists
            using (IDbCommand cmd = dbconn.CreateCommand())
            {
                string query = $"SELECT COUNT(*) FROM player_items WHERE username = '{username}' AND item_id = {itemId}";
                cmd.CommandText = query;
                int count = Convert.ToInt32(cmd.ExecuteScalar());

                if (count == 0)
                {
                    // Insert new entry
                    Debug.Log("NEW ENTRY");
                    query = $"INSERT INTO player_items (username, item_id, quantity) VALUES ('{username}', {itemId}, {quantity})";
                }
                else
                {
                    // Update existing entry

                    Debug.Log("UPDATE ENTRY");
                    query = $"UPDATE player_items SET quantity = {quantity} WHERE username = '{username}' AND item_id = {itemId}";
                }

                Debug.Log("EXECUTE ENTRY");
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
            }

            dbconn.Close();
        }
    }


    public List<Tuple<string, int>> FetchPlayerItems(string username)
    {
        List<Tuple<string, int>> playerItems = new List<Tuple<string, int>>();

        try
        {
            using (IDbConnection dbConnection = new SqliteConnection(conn))
            {
                dbConnection.Open();
                using (IDbCommand cmd = dbConnection.CreateCommand())
                {
                    cmd.CommandText = $@"
                SELECT i.item_name, pi.quantity
                FROM player_items pi
                JOIN items i ON pi.item_id = i.id
                WHERE pi.username = @Username";

                    IDbDataParameter param = cmd.CreateParameter();
                    param.ParameterName = "@Username";
                    param.Value = username;
                    cmd.Parameters.Add(param);

                    using (IDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string itemName = reader.GetString(0);
                            int quantity = reader.GetInt32(1);
                            playerItems.Add(new Tuple<string, int>(itemName, quantity));
                        }
                    }
                }

                dbConnection.Close();
            }
        }
        catch (Exception ex)
        {
            // Log the error or handle as appropriate
            Console.WriteLine($"Error fetching player items: {ex.Message}");
        }

        return playerItems;
    }

    public void SaveStorageDataToSQLite(string username, string storageData)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(storageData))
        {
            Debug.Log("storageData STORAGE CONCERN, ABORT");
            return;
        }
     //   if (storageData == "{}" || storageData == "{ }")
    //    {

      //      Debug.LogError("BEWARE: STORAGE WAS STORED EMPTY");
     //   }
        try
        {
            using (var conn = new SqliteConnection(DBManager.conn))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"UPDATE {DBManager.databaseName} SET storage_data = @data WHERE username = @username";
                    cmd.Parameters.AddWithValue("@data", storageData);
                    cmd.Parameters.AddWithValue("@username", username);

                    var result = cmd.ExecuteNonQuery();
                    if (result > 0)
                    {
                        Debug.Log("Storage data saved to SQLite successfully!");
                    }
                    else
                    {
                        Debug.LogError("Failed to save storage data to SQLite. No rows were updated.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"An error occurred while saving to SQLite: {ex.Message}");
        }
    }

    public string GetStorageDataFromSQLite(string username)
    {
        try
        {
            using (var conn = new SqliteConnection(DBManager.conn))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT storage_data FROM {DBManager.databaseName} WHERE username = @username";
                    cmd.Parameters.AddWithValue("@username", username);

                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                   //     Debug.LogError("STORAGE WAS IN FACT RETRIEVED");
                        return result.ToString();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"An error occurred while fetching from SQLite: {ex.Message}");
        }
        Debug.LogError("Failed to fetch storage data from SQLite or no data found.");
        return string.Empty;
    }







    public List<string> GetAllUsernames()
    {
        List<string> usernames = new List<string>();

        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();

            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = $"SELECT DISTINCT username FROM {databaseName}";

                IDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string username = reader.GetString(0);
                    usernames.Add(username);
                }

                reader.Close();
            }

            dbConnection.Close();
        }

        return usernames;
    }


    public void DeletePlayer(string username)
    {
        Debug.Log("DELETING PLAYER:" + username);
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                // List of table names where the player's data may reside
                string[] tables = new string[] { "PlayerAchievements", "player_items", "LostAndFound" };

                foreach (var table in tables)
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = $"DELETE FROM {table} WHERE username = @username";
                    cmd.Parameters.Add(new SqliteParameter("@username", username));

                    // Log the executed query
                    Debug.Log("Executing query: " + cmd.CommandText);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        Debug.Log($"Successfully deleted player data from the {table} table.");
                    }
                    else
                    {
                        Debug.Log($"No player data found in the {table} table with the specified username.");
                    }
                }

                // Finally, remove the player from the main players table
                cmd.Parameters.Clear();
                cmd.CommandText = $"DELETE FROM {databaseName} WHERE username = @username";
                cmd.Parameters.Add(new SqliteParameter("@username", username));

                int mainTableRowsAffected = cmd.ExecuteNonQuery();
                if (mainTableRowsAffected > 0)
                {
                    Debug.Log("Successfully deleted player from the main players table.");
                }
                else
                {
                    Debug.Log("No player found in the main players table with the specified username.");
                }
            }
            dbConnection.Close();
        }
    }



    public void UpdateVehicleStatsInSQLite(string username, string statName, int newTier)
    {
        using (IDbConnection dbConnection = new SqliteConnection(conn))
        {
            dbConnection.Open();

            string cmdText = $"UPDATE {databaseName} SET {statName} = @newTier WHERE username = @username";
            using (IDbCommand cmd = dbConnection.CreateCommand())
            {
                cmd.CommandText = cmdText;
                cmd.Parameters.Add(new SqliteParameter("@newTier", newTier));
                cmd.Parameters.Add(new SqliteParameter("@username", username));
                cmd.ExecuteNonQuery();
            }

            dbConnection.Close();
        }
    }

}
