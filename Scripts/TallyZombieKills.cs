using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;
public class TallyZombieKills : MonoBehaviour
{


    private int zombiesKilledRecently;


    public void reset()
    {
        zombiesKilledRecently = 0;
        StopAllCoroutines();
        complete = false;
        waitForMoreZombiesToDie1 = null;
        tallybegin = false;
    }

    private IEnumerator waitForMore()
    {

        WaitForSeconds wait = new WaitForSeconds(3f);
        yield return wait;
        afterWait();
    }

    private void afterWait()
    {
        if (complete)
        {
            return;
        }
        if (waitForMoreZombiesToDie1 != null) {
            StopCoroutine(waitForMoreZombiesToDie1);
            waitForMoreZombiesToDie1 = null;
        }

        zombiesKilledRecently = 0;

        string alertMessage = "[var=enemiesKilled]/30 killed";
        float alertDuration = 1f;
        if (!(string.IsNullOrEmpty(alertMessage) || DialogueManager.instance == null))
        {
            if (Mathf.Approximately(0, alertDuration))
            {
                DialogueManager.ShowAlert(alertMessage);
            }
            else
            {
                DialogueManager.ShowAlert(alertMessage, alertDuration);
            }
        }


    }

    public bool complete = false;

    public void completed()
    {
        complete = true;
    }


    private Coroutine waitForMoreZombiesToDie1;

    public bool tallybegin = false;
    public void beginTally()
    {
        tallybegin = true;
    }
    public void receiveKillNotice()
    {

        if (!tallybegin)
        {
            return;
        }

        zombiesKilledRecently += 1;
        if (waitForMoreZombiesToDie1 == null)
        {
            waitForMoreZombiesToDie1 = StartCoroutine(waitForMore());
        }


    }


    // Start is called before the first frame update
    void Start()
    {
        zombiesKilledRecently = 0;
    }

}
