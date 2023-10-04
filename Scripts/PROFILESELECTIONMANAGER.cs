using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;

public class PROFILESELECTIONMANAGER : MonoBehaviour
{
    public TextMeshProUGUI primaryGameFeedback;

    public static PROFILESELECTIONMANAGER Instance { get; private set; }


    public Color defaultProfileButtonColor;
    public Color hoveredProfileButtonColor;
    public Color selectedProfileButtonColor;


    public TMP_InputField usernameInputfield;


    public GameObject profileButtonContent;

    public GameObject deleteProfileButton;
    public GameObject loadProfileButton;



    public void showLoadProfileButton()
    {
        loadProfileButton.SetActive(true);
    }
    
    
    public void hideLoadProfileButton()
    {
        loadProfileButton.SetActive(false);
    }
    

    public void showDeleteProfileButton()
    {
        deleteProfileButton.SetActive(true);
    }

    public void deleteSelectedProfile()
    {
        if (lastSelectedProfileButton == null)
        {
            Debug.Log("NOTHING SELECTED1 1");
            return;
        }
        if (!DBManager.useLocalStorage)
        {

            Debug.Log("NOTHING SELECTED1 2");
            feedback.text = "Can't delete user profile while in online mode";
            return;
        }
        Debug.Log("DELETING " + lastSelectedProfile);

        string lowerfeedbacktext = "Successfully deleted profile";


        if (lastSelectedProfile == DBManager.username)
        {
            offlineLogout();
            lowerfeedbacktext = "Successfully deleted active profile and logged out user";
        }
        else
        {

            Debug.Log("you deleted someone who wasnt the active profile");
        }

        LOCALDATABASEMANAGER.Instance.DeletePlayer(lastSelectedProfile);
        Destroy(lastSelectedProfileButton.gameObject);
            lastSelectedProfileButton = null;
        lastSelectedProfile = string.Empty;

        feedback.text = "Successfully deleted profile";
        deleteProfileButton.SetActive(false);
        hideLoadProfileButton();
        closeSanityBag();


        LOCALDATABASEMANAGER.Instance.RegistrationManager.feedback.text = lowerfeedbacktext;


    }

    public void offlineLogout()
    {
        if (string.IsNullOrEmpty(DBManager.username))
        {
            Debug.Log("no need to log out in offline mode as you appear to be already not signed in but we'll do it anyways");
         //   return;
        }
        
        resetDBManager();
        LOCALDATABASEMANAGER.Instance.RegistrationManager.hideregistrationStuffForOfflineMode();
        LOCALDATABASEMANAGER.Instance.resetFundsAndScoreForClient();


    }
    public void offlineLogoutCOMPLETELY()
    {
        if (string.IsNullOrEmpty(DBManager.username))
        {
            Debug.Log("no need to log out in offline mode as you appear to be already not signed in but we'll do it anyways");
         //   return;
        }

       resetDBManager();
        LOCALDATABASEMANAGER.Instance.RegistrationManager.hideregistrationStuffForOfflineMode();
        LOCALDATABASEMANAGER.Instance.RegistrationManager.smalllogout();

    }
    public void ResetAllProfileButtonColors()
    {
        foreach(Transform child in profileButtonContent.transform)
        {
            GameObject profilePrefabButton = child.gameObject;

            HoverTextColor profileButton = profilePrefabButton.GetComponent<HoverTextColor>();

            profileButton.gameObject.GetComponent<Image>().color = defaultProfileButtonColor;
            profileButton.currentlySelected = false;
            profileButton.buttonText.color = profileButton.originalColor;
        }


    }

    public GameObject lastSelectedProfileButton;
    public string lastSelectedProfile;
    public void profileButtonSelected(string whatProfileName, GameObject whatButton)
    {
         ResetAllProfileButtonColors();

        lastSelectedProfile = whatProfileName;
        lastSelectedProfileButton = whatButton;
        Debug.Log("THE LAST SELECTED PROFILE BUTTON: " + whatProfileName);
        whatButton.GetComponent<HoverTextColor>().backgroundImage.color = selectedProfileButtonColor;
        whatButton.GetComponent<HoverTextColor>().currentlySelected = true;

        onSelectedAnything();
    }

    public void onSelectedAnything()
    {
        showDeleteProfileButton();
        showLoadProfileButton();
    }

    public string currentlySelectedProfile;

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;


        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }



    public GameObject sanityBag;

    public void showSanityBag()
    {

        sanityBag.SetActive(true);

    }

    public void closeSanityBag()
    {

        sanityBag.SetActive(false);
    }
     


    public AudioSource clickSound;

    public List<Sprite> backgroundImagesForButtons = new List<Sprite>();

    public Sprite getRandomBackgroundForButton()
    {

        return backgroundImagesForButtons[Random.Range(0, backgroundImagesForButtons.Count)];
    }

    public void clickedSelectProfile()
    {

        clickSound.Play();


        showProfilePanel();

    }

    public GameObject profilePanel;


    public void showProfilePanel()
    {

        profilePanel.SetActive(true);
        populateContent();
    }


    public void hideProfilePanel()
    {



        profilePanel.SetActive(false);



    }

    public void scrubContent()
    {



        foreach(Transform child in profileButtonContent.transform)
        {
            Destroy(child.gameObject);
        }


    }

    public GameObject createProfileButton;
    public void onChangeInputField(string inputFieldData)
    {

        if (string.IsNullOrEmpty(inputFieldData))
        {
            createProfileButton.SetActive(false);
        } else if (inputFieldData.Length < 4)
        {

            createProfileButton.SetActive(false);

        } else
        {

            createProfileButton.SetActive(true);
        }


    }

    public void populateContent()
    {


        scrubContent();


        if (!LOCALDATABASEMANAGER.Instance.checkIfDatabaseExists())
        {
            Debug.Log("NEW DATABASE TIME");
            return;

        }

            List<string> usernames = LOCALDATABASEMANAGER.Instance.GetAllUsernames();
        if (usernames.Count == 0)
        {
            feedback.text = "No users, create a user to play";
            return;
        }
        foreach (string username in usernames)
        {
            GameObject profileButtonInstance = Instantiate(profileButtonPrefab, profileButtonContent.transform);
            TextMeshProUGUI buttonText = profileButtonInstance.GetComponentInChildren<TextMeshProUGUI>();

            if (buttonText != null)
            {
                buttonText.text = username;
            } else
            {
                Debug.Log("Where is the button text?");
            }
            profileButton scriptOnButton = profileButtonInstance.GetComponent<profileButton>();
            if (scriptOnButton != null)
            {
                scriptOnButton.profileName = username;
            }
            else
            {
                Debug.Log("Where is the script on button??");
            }
            profileButtonInstance.GetComponent<HoverTextColor>().hoverSound = hoverSound;
            if (!profileButtonInstance.GetComponent<EventPasser>())
            {
                profileButtonInstance.AddComponent<EventPasser>();
            }
        }


    }

    public TextMeshProUGUI currentProfileLabel;

    public AudioSource hoverSound;

    public TextMeshProUGUI feedback;

    public GameObject profileButtonPrefab;
    public void createNewUserProfile()
    {
        if (!DBManager.useLocalStorage)
        {

            feedback.text = "Can't create new user profile while in online mode";
            return;
        }

        string username = usernameInputfield.text;

        // Regular expression pattern for alphanumeric usernames
        string usernamePattern = "^[a-zA-Z0-9]+$";

        // Regular expression pattern for alphanumeric passwords with a minimum length

        // Validate username for alphanumeric characters
        if (!Regex.IsMatch(username, usernamePattern))
        {
            feedback.gameObject.SetActive(true);
            feedback.text = "Username must be alphanumeric";
            return;
        }
        // this is where we need to make sure the player doesn't already exist first
        // if it does, LOCALDATABASEMANAGER.Instance.initializeDatabaseOnBehalfOf(username)

        if (LOCALDATABASEMANAGER.Instance.checkIfDatabaseExists())
        {


            if (LOCALDATABASEMANAGER.Instance.DoesUserExist(username))
            {
                feedback.gameObject.SetActive(true);
                feedback.text = "Username already exists!";
                return;
            }
        }

        resetDBManager();

        feedback.text = "Creating new user, the game might hang up for a second.";
        Debug.Log("VALIDATED USERNAME: CREATING NEW PROFILE: " + username);
        LOCALDATABASEMANAGER.Instance.initializeDatabaseForNewPlayer(username);
        createNewProfile(username);
        usernameInputfield.text = ""; // clear the input field
    }
    public void createNewProfile(string profileName)
    {


        GameObject newProfileButton = Instantiate(profileButtonPrefab, profileButtonContent.transform);

        newProfileButton.GetComponent<profileButton>().profileName = profileName;
        newProfileButton.GetComponentInChildren<TextMeshProUGUI>().text= profileName;

        Debug.Log("making profile for " + profileName);

        feedback.text = "Successfully created profile";
        currentProfileLabel.text = profileName;

        onSuccessfulOfflineLogin();
    }
    public void loadSelectedProfile()
    {
        if (lastSelectedProfileButton == null)
        {
            Debug.Log("NOTHING SELECTED1 1");
            return;
        }
        if (!DBManager.useLocalStorage)
        {

            Debug.Log("NOTHING SELECTED1 2");
            feedback.text = "Can't load user profile while in online mode";
            return;
        }
        Debug.Log("DELETING " + lastSelectedProfile);
        loadSelectedProfileByName(lastSelectedProfile);
    }
    public void loadSelectedProfileByName(string profileName)
    {
        resetDBManager();
        feedback.text = "Loaded selected profile";
        LOCALDATABASEMANAGER.Instance.initializeDatabaseOnBehalfOf(profileName);

        onSuccessfulOfflineLogin();
    }

    public void resetDBManager()
    {
       // Debug.LogError("RESET DB MANAGER");
        DBManager.username = null;
    //    DBManager.funds = 0;
     //   DBManager.score = 0;
     
        DBManager.requestsMade = 0;
        DBManager.canFly = false;


        PlayerPrefs.SetInt("LASTHEALTH", 20);

    }
    public Login login;
    public void onSuccessfulOfflineLogin() {

        LOCALDATABASEMANAGER.Instance.RegistrationManager.allowPlayButtonsForOfflineMode();

        login.checkForSpecialPermissions(DBManager.username);




      //  Debug.LogError("AT SOME POINT YOU LOGGED IN OFFLINE");


        hideProfilePanel(); // is this good or bad
       login.localdatabasemanager.postLoginRoutine();

        login.fetchSkin();
    }



}
