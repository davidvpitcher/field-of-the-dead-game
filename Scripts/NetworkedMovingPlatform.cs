using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Connection;
using FishNet.Object;
public class NetworkedMovingPlatform : NetworkBehaviour
{


    public List<CharacterController> playersOnPlatform = new List<CharacterController>();


    public List<GameObject> jetParticles = new List<GameObject>();
    private bool platformInMotion = false;

    public float platformSpeed = 7f;

    public Vector3 platformUpVector;

    private float waitStartTime;
    public GameObject maxHeightGuide;
    public GameObject minHeightGuide;
    public enum PlatformState { Idle, Ascending, Waiting, Descending, Resting }

    public PlatformState currentState = PlatformState.Idle;
    public bool hasAscendedWithPlayers = false;
    public bool everyoneDisembarkedWhileAscended = false;

    public void TurnOnJetParticles()
    {

        foreach (GameObject jetParticle in jetParticles)
        {
            jetParticle.SetActive(true);
        }
    }
    public void TurnOffJetParticles()
    {

        foreach (GameObject jetParticle in jetParticles)
        {
            jetParticle.SetActive(false);
        }
    }

    public void ParentPlayer(Transform whichPlayer, Transform toWhat)
    {

        whichPlayer.transform.parent = toWhat.transform;
        ParentPlayerServer(this, whichPlayer, toWhat);

    }

    [ServerRpc(RequireOwnership = false)]
    public void ParentPlayerServer(NetworkedMovingPlatform networkedMovingPlatform, Transform whichPlayer, Transform toWhat)
    {

        whichPlayer.transform.parent = toWhat.transform;

        ParentPlayerObservers(networkedMovingPlatform, whichPlayer, toWhat);

    }

    [ObserversRpc(BufferLast = true, ExcludeOwner = false)]
    public void ParentPlayerObservers(NetworkedMovingPlatform networkedMovingPlatform, Transform whichPlayer, Transform toWhat)
    {

        networkedMovingPlatform.ParentPlayerRoutine(whichPlayer, toWhat);
    }

    public void ParentPlayerRoutine(Transform whichPlayer, Transform toWhat)
    {

        whichPlayer.transform.parent = toWhat.transform;

    }


    public void UnparentPlayer(Transform who)
    {

        who.transform.parent = null;
        UnparentPlayerServer(this, who);

    }

    [ServerRpc(RequireOwnership = false)]
    public void UnparentPlayerServer(NetworkedMovingPlatform networkedMovingPlatform, Transform whichPlayer)
    {
        whichPlayer.transform.parent = null;

        UnparentPlayerObservers(networkedMovingPlatform, whichPlayer);

    }

    [ObserversRpc(BufferLast = true, ExcludeOwner = false)]
    public void UnparentPlayerObservers(NetworkedMovingPlatform networkedMovingPlatform, Transform whichPlayer)
    {

        networkedMovingPlatform.unparentPlayerROUTINE(whichPlayer);
    }

    public void unparentPlayerROUTINE(Transform whichPlayer)
    {

        whichPlayer.transform.parent = null;

    }

  
    public void OnTriggerEnter(Collider other)
    {
    
        GameObject otherRoot = other.gameObject.transform.root.gameObject;




        if (otherRoot.CompareTag("Player"))
        {

            ParentPlayer(otherRoot.transform, this.transform);
          
            CharacterController playerController = otherRoot.GetComponent<CharacterController>();
            if (playerController != null)
            {
           
                RegisterPlayerToPlatformSERVER(this, playerController.gameObject);

                TurnOnJetParticles();

            }
        }

    }

    [ServerRpc(RequireOwnership = false)]
    private void RegisterPlayerToPlatformSERVER(NetworkedMovingPlatform networkedMovingPlatform, GameObject whichPlayer)
    {

        RegisterPlayerToPlatformFORALL(networkedMovingPlatform, whichPlayer);

    }

    [ObserversRpc(BufferLast = true, ExcludeOwner = false)]
    public void RegisterPlayerToPlatformFORALL(NetworkedMovingPlatform networkedMovingPlatform, GameObject whichPlayer)
    {

        networkedMovingPlatform.RegisterPlayerToPlatformRoutine(whichPlayer);
    }

    private void RegisterPlayerToPlatformRoutine(GameObject whichPlayer)
    {
        if (!playersOnPlatform.Contains(whichPlayer.GetComponent<CharacterController>()))
        {
            playersOnPlatform.Add(whichPlayer.GetComponent<CharacterController>());
        }

        if (InstanceFinder.IsServer)
        {

            CheckPlayerCount();
        }
    }

    private void Awake()
    {
        platformUpVector = this.transform.up;
    }

    [ServerRpc(RequireOwnership = false)]
    private void UnregisterPlayerToPlatformSERVER(NetworkedMovingPlatform networkedMovingPlatform, GameObject whichPlayer)
    {

        UnregisterPlayerToPlatformFORALL(networkedMovingPlatform, whichPlayer);

    }

    [ObserversRpc(BufferLast = true, ExcludeOwner = false)]
    public void UnregisterPlayerToPlatformFORALL(NetworkedMovingPlatform networkedMovingPlatform, GameObject whichPlayer)
    {

        networkedMovingPlatform.UnregisterPlayerToPlatformRoutine(whichPlayer);
    }

    private void UnregisterPlayerToPlatformRoutine(GameObject whichPlayer)
    {
        if (playersOnPlatform.Contains(whichPlayer.GetComponent<CharacterController>()))
        {
            playersOnPlatform.Remove(whichPlayer.GetComponent<CharacterController>());
        }

        if (InstanceFinder.IsServer)
        {

            CheckPlayerCount();
        }
    }

    public void ScanListAndFixIt(List<CharacterController> playerList)
    {
     
        for (int i = playerList.Count - 1; i >= 0; i--)
        {
          
            if (playerList[i] == null)
            {
                playerList.RemoveAt(i);
            }
        }

    }
    private void StartElevator()
    {
        if ((currentState == PlatformState.Idle || currentState == PlatformState.Resting) && playersOnPlatform.Count > 0)
        {
            currentState = PlatformState.Ascending;
        }
    }
    private void CheckPlayerCount()
    {
        if (platformInMotion) return;
        if (playersOnPlatform.Count == 0)
        {
            if (currentState != PlatformState.Descending && currentState != PlatformState.Waiting)
            {
                currentState = PlatformState.Idle;
                TurnOffJetParticles();
            }
        }
        else if (currentState == PlatformState.Idle || currentState == PlatformState.Resting)
        {
            StartElevator();
        }
        if (hasAscendedWithPlayers && !everyoneDisembarkedWhileAscended && playersOnPlatform.Count == 0 && currentState == PlatformState.Waiting)
        {
            everyoneDisembarkedWhileAscended = true;
        }
    }

    public void FixedUpdate()
    {
        if (!InstanceFinder.IsServer) return;

    

        ScanListAndFixIt(playersOnPlatform);

        switch (currentState)
        {
            case PlatformState.Idle:
               // do nothing
                break;

            case PlatformState.Ascending:
                this.transform.Translate(platformUpVector * Time.deltaTime * platformSpeed);
                platformInMotion = true;
                if (transform.position.y >= maxHeightGuide.transform.position.y)
                {
                    currentState = PlatformState.Waiting;
                    waitStartTime = Time.time;
                    everyoneDisembarkedWhileAscended = false;
                }
                break;
            case PlatformState.Waiting:
                hasAscendedWithPlayers = true;
                platformInMotion = false;
                if ((Time.time - waitStartTime >= 3f) || (hasAscendedWithPlayers && everyoneDisembarkedWhileAscended && playersOnPlatform.Count > 0))
                {
                    currentState = PlatformState.Descending;
                }
                break;
            case PlatformState.Descending:
                platformInMotion = true;
                this.transform.Translate(-platformUpVector * Time.deltaTime * platformSpeed);
                if (transform.position.y <= minHeightGuide.transform.position.y)
                {
                    currentState = PlatformState.Resting;
                    waitStartTime = Time.time; 
                    hasAscendedWithPlayers = false; 
                }
                break;

            case PlatformState.Resting:
                platformInMotion = false;
                if (Time.time - waitStartTime >= 4f) 
                {
                    currentState = PlatformState.Idle;
                }
                break;
        }
    }

    public void OnTriggerExit(Collider other)
    {
       
        PlayerMover playerMover = other.gameObject.GetComponentInParent<PlayerMover>();

        GameObject hitRootPlayer = null;
        if (playerMover != null)
        {

            hitRootPlayer = playerMover.gameObject;
        }
        else
        {
            return;
        }

     
        if (hitRootPlayer.CompareTag("Player"))
        {


            UnparentPlayer(hitRootPlayer.transform);

            CharacterController playerController = hitRootPlayer.GetComponent<CharacterController>();
            if (playerController != null)
            {
            
                UnregisterPlayerToPlatformSERVER(this, hitRootPlayer);
        
                if (playersOnPlatform.Count == 0)
                {

                    TurnOffJetParticles();
                }
            }
        }
    }
}
