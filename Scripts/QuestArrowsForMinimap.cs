using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PixelCrushers.DialogueSystem;
public class QuestArrowsForMinimap : MonoBehaviour
{
    public GameObject alphonso;
    public GameObject healthStation1;
    public GameObject genericDumpster;

    public GameObject arrowPrefab;
    public RectTransform minimapRectTransform;

    private List<MinimapTracker> trackers = new List<MinimapTracker>();

    public GameObject minimap;

    public List<GameObject> currentObjectsTrackedInQuest = new List<GameObject>();

    public void scrubTracker()
    {
        disableAllTrackers();
    }

    public void completelyScrubTracker()
    {
        scrubTracker();
        currentObjectsTrackedInQuest.Clear();
    }

    public void updateMinimapQuestTracker()
    {
        if (!minimap.activeSelf)
        {
            return;
        }
        if (currentObjectsTrackedInQuest.Count > 0)
        {
            foreach (GameObject currentObjectTrackedInQuest in currentObjectsTrackedInQuest)
            {
                AddQuestObjectiveToMiniMap(currentObjectTrackedInQuest.transform);
            }
        }
    }

    public void UpdateTracker()
    {
        if (!minimap.gameObject.activeSelf)
        {
            return;
        }
        if (currentObjectsTrackedInQuest.Count > 0)
        {
            foreach (GameObject currentObjectTrackedInQuest in currentsObjectTrackedInQuest)
            {
                AddQuestObjectiveToMiniMap(currentObjectTrackedInQuest.transform);
            }
        }
    }
    public void disableAllTrackers()
    {


        foreach (MinimapTracker minimapTracker in trackers)
        {

            Destroy(minimapTracker.gameObject);

        }


        trackers.Clear();
    }

    public void AddQuestObjectiveToMiniMap(Transform whichObjective)
    {
        if (!minimap.gameObject.activeSelf)
        {
            return;
        }
        AddPartyMember(whichObjective);
    }


    public void AddPartyMember(Transform partyMemberTransform)
    {
        if (!minimap.gameObject.activeSelf)
        {
            return;
        }
        GameSetupController gsc = GameObject.Find("playermaker").GetComponent<GameSetupController>();
        Transform playerTransform = gsc.realLocalPlayer.transform;

        GameObject arrow = Instantiate(arrowPrefab, minimapRectTransform);
        MinimapTracker tracker = arrow.AddComponent<MinimapTracker>();

        tracker.playerTransform = playerTransform;
        tracker.targetTransform = partyMemberTransform;

        trackers.Add(tracker);
    }

    public void RemovePartyMember(Transform partyMemberTransform)
    {



        MinimapTracker tracker = trackers.Find(t => t.targetTransform == partyMemberTransform);
        if (tracker != null)
        {
            trackers.Remove(tracker);
            Destroy(tracker.gameObject);
        }
    }
    public PathHighlighter2 pathFinder;

    public void drawAWaypointPathToTarget(Transform who)
    {

        pathFinder.gameObject.SetActive(true);

        pathFinder.objective = who.gameObject;


        pathFinder.ShowPath();

    }



    public void removeWaypointPath()
    {
        pathFinder.HidePath();
        pathFinder.gameObject.SetActive(false);


    }



    public void loadForMinimapTracker()
    {
        if (!minimap.gameObject.activeSelf)
        {
            return;
        }

        currentObjectsTrackedInQuest.Clear();

        string currentQuestState = PixelCrushers.DialogueSystem.QuestLog.CurrentQuestState("Meet Alphonso");

        switch (currentQuestState)
        {
            case "Meet Alphonso":
            case "Talk to Alphonso":
            case "Return to Alphonso Again":
            case "Return to Alphonso":
                currentObjectsTrackedInQuest.Add(alphonso);
                break;

            case "Use health station":
                currentObjectsTrackedInQuest.Add(healthStation1);
                break;

            case "Dumpster dive":
                currentObjectsTrackedInQuest.Add(genericDumpster);
                break;

            default:
               
                break;
        }

        if (currentObjectsTrackedInQuest.Count > 0)
        {
            UpdateTracker();
        }
    }


    public void cQuestTrackerToAlphonso()
    {
        currentObjectsTrackedInQuest.Add(alphonso);
        UpdateTracker();

    }

}
