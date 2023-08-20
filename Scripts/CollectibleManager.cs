using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
public class CollectibleManager : MonoBehaviour
{

    public GameObject collectibleDisplayBag;
    public Image collectibleDisplayImage;
    public TextMeshProUGUI collectibleDisplayLabel;
    public int totalNumberOfCollectibles; 

    public List<CollectibleGuide> collectibleGuides = new List<CollectibleGuide>();

    public List<bool> collectibles;

    private void Start()
    {
        totalNumberOfCollectibles = worldCollectibleObjects.Count;
      
        collectibles = new List<bool>(new bool[totalNumberOfCollectibles]);

        foreach(GameObject whatever in worldCollectibleObjects)
        {
     
            collectibleGuides.Add(whatever.gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.GetComponent<GroundCollectibleGuide>().collectibleScriptable);
        }
    }



    public void playerTouchedCollectible(CollectibleGuide guide)
    {

        Sprite ourImage = guide.collectibleIcon;

        collectibleDisplayLabel.text = guide.collectibleName;
        collectibleDisplayImage.sprite = ourImage;
        collectibleDisplayBag.SetActive(true);


        collectibles[guide.id] = true;
        saveCollectiblesData();

        if (allComplete())
        {
            GameObject.Find("ACHIEVEMENTMANAGER").GetComponent<AchievementManager>().onFoundAllCollectibles();
        }
    }

    public bool allComplete()
    {
        bool ourbool = true;

        foreach(bool collectible in collectibles)
        {
            if (!collectible)
            {
                ourbool = false;
            }

        }


        return ourbool; 


    }
    public string GetCollectiblesData()
    {
        string collectiblesData = "";

        // Assuming you have a bool array or BitArray representing the collectibles
        foreach (bool found in collectibles)
        {
            collectiblesData += found ? "1" : "0";
        }

        return collectiblesData;
    }

    public int GetTotalCollectiblesFound()
    {
        int totalFound = 0;

        foreach (bool found in collectibles)
        {
            if (found)
            {
                totalFound++;
            }
        }

        return totalFound;
    }


    public void load()
    {

        StartCoroutine(LOADCOLLECTIBLES());


    }

    public List<GameObject> worldCollectibleObjects = new List<GameObject>();
    IEnumerator LOADCOLLECTIBLES()
    {
        WWWForm form = new WWWForm();
        form.AddField("username", DBManager.username);

        UnityWebRequest www = UnityWebRequest.Post("https://www.mortalitycore.com/sqlconnect/retrieve-collectibles.inc.php", form);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
           

            if (string.IsNullOrEmpty(www.downloadHandler.text))
            {
                Debug.Log("Failed to load collectibles data");
            }
            else
            {
                string collectiblesData = www.downloadHandler.text;
           
              
                List<bool> collectiblesFound = new List<bool>();
                foreach (char c in collectiblesData)
                {
                    collectiblesFound.Add(c == '1');
                }

             
                for (int i = 0; i < collectiblesFound.Count; i++)
                {
                    if (collectiblesFound[i])
                    {
                    
                        collectibles[i] = true;
                        worldCollectibleObjects[i].SetActive(false);
                    }
                }


           
                AchievementManager achievementManager = GameObject.Find("ACHIEVEMENTMANAGER").GetComponent<AchievementManager>();

                achievementManager.onLoad();

            }



        }

        www.Dispose();
    }

    public GameObject collectiblesGrid;
    public Transform collectiblesGridContent;

    public GameObject completedTag;

    public TextMeshProUGUI totalCollected;
    public TextMeshProUGUI totalToCollect;

    public GameObject collectibleMenuPrefab;

    public void resetALL()
    {

        for (int i = 0; i < collectibles.Count; i++)
        {
            collectibles[i] = false;
        }

        saveCollectiblesData();
        resetAllCollectibles();


     
        scrubGrid();
      
        totalCollected.text = "0";
        totalToCollect.text = totalNumberOfCollectibles.ToString();

      
            completedTag.SetActive(false);
      
        populateGrid();
 
    }

    public void resetAllCollectibles()
    {
        foreach(GameObject what in worldCollectibleObjects)
        {
            what.gameObject.SetActive(true);
            what.gameObject.transform.GetChild(1).gameObject.SetActive(true);
            what.gameObject.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.SetActive(true);
            what.gameObject.transform.GetChild(1).gameObject.transform.GetChild(1).gameObject.SetActive(false);


        }
    }


    public void showGridFromInventory()
    {
        collectiblesGrid.gameObject.SetActive(true);
        scrubGrid();
        int howmanyfound = GetTotalCollectiblesFound();
        totalCollected.text = howmanyfound.ToString();
        totalToCollect.text = totalNumberOfCollectibles.ToString();

        if (howmanyfound == totalNumberOfCollectibles)
        {
            completedTag.SetActive(true);
        }
        else
        {
            completedTag.SetActive(false);
        }
        populateGrid();
        GameObject inv = GameObject.Find("INVENTORY");
        if (inv != null)
        {
            if (inv.transform.GetChild(1).gameObject.activeInHierarchy)
            {
                inv.transform.GetChild(1).gameObject.GetComponent<OTHERINVENTORY>().hideInv3();
            }
        }
    }

    public void showGrid()
    {
        collectiblesGrid.gameObject.SetActive(true);
        scrubGrid();
        int howmanyfound = GetTotalCollectiblesFound();
        totalCollected.text = howmanyfound.ToString();
        totalToCollect.text = totalNumberOfCollectibles.ToString();

        if (howmanyfound == totalNumberOfCollectibles)
        {
            completedTag.SetActive(true);
        } else
        {
            completedTag.SetActive(false);
        }
        populateGrid();
        GameObject inv = GameObject.Find("INVENTORY");
        if (inv != null) {
            if (inv.transform.GetChild(1).gameObject.activeInHierarchy) {
                inv.transform.GetChild(1).gameObject.GetComponent<OTHERINVENTORY>().hideInv2();
            }
                }
    }

    public void populateGrid()
    {
      
        for (int i = 0; i < collectibles.Count; i++)
        {
            
            GameObject newItem = Instantiate(collectibleMenuPrefab, collectiblesGridContent);

       
            Transform sparkles = newItem.transform.GetChild(0);
            Transform collectedImage = newItem.transform.GetChild(1);
            Transform collectedLabel = newItem.transform.GetChild(2);
            Transform notCollectedImage = newItem.transform.GetChild(3);
            Transform notCollectedLabel = newItem.transform.GetChild(4);

        
            collectedImage.GetComponent<Image>().sprite = collectibleGuides[i].collectibleIcon;
            notCollectedImage.GetComponent<Image>().sprite = collectibleGuides[i].collectibleIcon;
            collectedLabel.GetComponent<TextMeshProUGUI>().text = collectibleGuides[i].collectibleName;
        
            bool hasCollectible = collectibles[i];
            collectedImage.gameObject.SetActive(hasCollectible);
            collectedLabel.gameObject.SetActive(hasCollectible);
            notCollectedImage.gameObject.SetActive(!hasCollectible);
            notCollectedLabel.gameObject.SetActive(!hasCollectible);
            sparkles.gameObject.SetActive(hasCollectible);
        }
    }
   






    public void hideGrid()
    {

        collectiblesGrid.gameObject.SetActive(false);
        GameSetupController gsc = GameObject.Find("fakeplayermaker").GetComponent<fakeplayermaker>().realplayermaker.gameObject.GetComponent<GameSetupController>();
        gsc.realLocalPlayer.GetComponent<PlayerMover>().DISABLECURSOR();

    }

    public void scrubGrid()
    {

        foreach (Transform child in collectiblesGridContent)
        {
            Destroy(child.gameObject);
        }
    }
    public void saveCollectiblesData()
    {


        StartCoroutine(SaveCollectibles(GetCollectiblesData()));


    }

    IEnumerator SaveCollectibles(string collectiblesData)
    {
        
        List<IMultipartFormSection> form = new List<IMultipartFormSection>();

        form.Add(new MultipartFormDataSection("username", DBManager.username));
        form.Add(new MultipartFormDataSection("collectiblesData", collectiblesData));

        string url = "https://www.mortalitycore.com/sqlconnect/storecollectibles.inc.php";

        UnityWebRequest www = UnityWebRequest.Post(url, form);

        yield return www.SendWebRequest();


  
    
        www.Dispose();
    }









}
