using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Transporting;
using FishNet.Object;
using TMPro;
using FishNet;
public class PartySystem : MonoBehaviour
{
    // Dictionary to store parties, where key is the party leader's NetworkConnection and value is the list of party members.
    public Dictionary<NetworkConnection, Party> _parties = new Dictionary<NetworkConnection, Party>();


    public partySystemAssistant PSA;

    public Transform partyHUDContainer;

    public NetworkConnection currentPartyLeader;

    public GameManager gameManager;
    public Party lastParty;

    public bool isInParty = false;


    public GameObject arrowPrefab;
    public RectTransform minimapRectTransform;
    private List<MinimapTracker> trackers = new List<MinimapTracker>();


    
    public void fixmyminimap()
    {
        GameObject RLP = gsc.realLocalPlayer;
        NetworkConnection whoyou = RLP.GetComponent<PlayerHealth>().myConn;

        Debug.Log("FIX MINIMAL SIGNAL 1 ");
        PSA.fixmyminimap(whoyou, RLP);

    }
    public void scrubMinimapTracker()
    {


        foreach (MinimapTracker minimaptracker in trackers)
        {
            if (minimaptracker != null)
            {

                Destroy(minimaptracker.gameObject);

            }
        }
        Debug.Log("UPDATE MINIMAP TRACKER");
        trackers.Clear();

    }

    private IEnumerator fixminidelay()
    {

        yield return new WaitForSeconds(0.1f);


        if (minimapRectTransform.gameObject.activeSelf)
        {
            fixmyminimap();

        }

        }

    public void updateMinimapTracker()
    {
        Debug.Log("FIX MINIMAP REQUEST");
        if (!minimapRectTransform.gameObject.activeSelf)
        {
            Debug.Log("BUT NO MINI");
            scrubMinimapTracker();
            return;
        }
        fixmyminimap();
        StartCoroutine(fixminidelay());
        return;
    



        }

    public void TEST_MINIMAP_TRACKER_SYSTEM()
    {

        GameObject TESTOBJECT = GameObject.Find("RODRIGO").gameObject;
        AddPartyMember(TESTOBJECT.transform);



    }

    public void disableAllTrackers()
    {


        foreach (MinimapTracker minimaptracker in trackers)
        {

            Destroy(minimaptracker.gameObject);

        }


        trackers.Clear();
        Debug.Log("trackers disabled");
    }



    public void AddPartyMember(Transform partyMemberTransform)
    {
        if (!minimapRectTransform.gameObject.transform.GetChild(0).gameObject.activeSelf)
        {
            Debug.Log("FAILED TO ADD PARTY MEMBER TO MINIMAP TRACKER");
            return;
        }
        Debug.Log("ADDED PARTY MEMBER TO MINIMAP TRACKER");
        Transform playerTransform = gsc.realLocalPlayer.transform;

        GameObject arrow = Instantiate(arrowPrefab, minimapRectTransform);
        MinimapTracker tracker = arrow.AddComponent<MinimapTracker>();

        tracker.playerTransform = playerTransform;
        tracker.targetTransform = partyMemberTransform;
        Debug.Log("THE PERSON WAS ADDED");
        trackers.Add(tracker);
    }

    public void RemovePartyMember(Transform partyMemberTransform)
    {
        Transform playerTransform = gsc.realLocalPlayer.transform;
        MinimapTracker tracker = trackers.Find(t => t.targetTransform == partyMemberTransform);
        if (tracker != null)
        {
            trackers.Remove(tracker);
            Destroy(tracker.gameObject);
        }
    }



    public void adjustMaxHPInPartySystem(NetworkConnection who, int howmuch)
    {

        if (!InstanceFinder.IsServer)
        {
            Debug.Log("only the server can pass");
            return;
        }

        Debug.Log("adjust maxhp in party system");




    }


    public List<Transform> getPlayersToTrack(NetworkConnection playerConnection, GameObject therBody)
    {
        //   GameObject RLP = gsc.realLocalPlayer;
        List<Transform> playersToTrack = new List<Transform>();

        Party playerParty = GetPlayerParty(playerConnection);
        if (playerParty != null)
        {

           
            foreach (var partyKeyValuePair in _parties)
            {
                Party party = partyKeyValuePair.Value;

                if (party == playerParty) { 

                NetworkConnection leader = party.Leader;

                GameObject leaderiswho = gameManager._players[leader];

                if (leaderiswho != therBody)
                {

                        playersToTrack.Add(leaderiswho.transform);
                   // AddPartyMember(leaderiswho.transform);
                    Debug.Log("ADDING AM leader" + leaderiswho.gameObject.name);
                }

                foreach (NetworkConnection member in party.Members)
                {
                    GameObject thems = gameManager._players[member];
                    Debug.Log("mebmernoted");

                    if (thems != therBody)
                        {
                            playersToTrack.Add(thems.transform);
                            Debug.Log("ADDING AM MEBMER" + thems.gameObject.name);
                     //   AddPartyMember(thems.transform);
                    }
                }


                }
            }



        }

        return playersToTrack;
    }


    // what calls this? nothing?
    public void UpdatePartyMemberHealthBars(NetworkConnection playerConnection, int newHealth, int max)
    {
        Party playerParty = GetPlayerParty(playerConnection);

        if (playerParty != null)
        {
            foreach (NetworkConnection member in playerParty.Members)
            {
                // Send the new health value to each party member.
                UpdatePartyMemberHealthBar(member, playerConnection, newHealth, max);
            }
            // Don't forget to update the health bar for the party leader as well.
            UpdatePartyMemberHealthBar(playerParty.Leader, playerConnection, newHealth, max);
        }
    }




    public void UpdatePartyMemberHealthBarsForPlayer(NetworkConnection playerConnection, int next)
    {
        Debug.Log("NOTICE RECEIVED");
        PSA.updatePartyMemberHealthBarsForPlayer(playerConnection, next);
    }


    public void UpdatePartyMemberHealthBarsForPlayerROUTINE(NetworkConnection playerConnection, int next)
    {
        // Check if the player is in a party.
        if (_parties.TryGetValue(playerConnection, out Party playerParty))
        {
            NetworkConnection leaderConn = playerParty.Leader;
            Debug.Log("PARTIED PLAYER - update players");
            UpdateHealthBar(leaderConn, playerConnection);
            // Update the health bars of all party members.
            foreach (NetworkConnection memberConnection in playerParty.Members)
            {
                UpdateHealthBar(memberConnection, playerConnection);
            }
        }
        else
        {

        }
    }



    public void notifyDisconnect(NetworkConnection conn)
    {

        if (!InstanceFinder.IsServer)
        {
            return;
        }
        // Check if the disconnected player is a party leader.
        if (_parties.TryGetValue(conn, out Party party))
        {
            // Remove the leader's party.
            _parties.Remove(conn);


            string disbandedMessage = "The party has been disbanded.";
            // Notify all party members that the party has been disbanded.
            foreach (var member in party.Members)
            {

                notifyPlayer(member, disbandedMessage);
                targetScrub(member); // recent addition please reconsider
            }



        }
        else
        {
            // The disconnected player is not a party leader.
            // Remove the disconnected player from their party.
            foreach (var partyKeyValuePair in _parties)
            {
                Party currentParty = partyKeyValuePair.Value;

                if (currentParty.Members.Contains(conn))
                {

                    foreach (var member in currentParty.Members)
                    {

                        notifyPlayer(member, " party member disconnected");
                    }

                    currentParty.Members.Remove(conn);
                    targetScrub(conn);  // recent addition please reconsider
                    break;
                }
            }
        }

        // Update the HUD for all party members.
        UpdatePartyHUDForAllMembers();

        // 
    }


    public void UpdateHealthBar(NetworkConnection memberConnection, NetworkConnection playerConnection)
    {
        Debug.Log("UHB 1 ");
        PSA.UpdateHealthBarSERVER(memberConnection, playerConnection);
    }

   


    private void UpdatePartyMemberHealthBar(NetworkConnection memberConnection, NetworkConnection playerConnection, int newHealth, int max)
    {
        GameObject memberGameObject = gameManager.whoisthis(memberConnection);
        if (memberGameObject != null)
        {
            
                UpdateMemberHealthBar(playerConnection, newHealth, max);
          
        }
    }

    public void updateHealthbarsSOmehow(NetworkConnection playerConn, int next, int max)
    {

        if (!InstanceFinder.IsServer)
        {
            Debug.Log("UPDATE HP DENIED");
            return;
        }

        Party theirParty = GetPlayerParty(playerConn);

        if (theirParty == null)
        {
            Debug.Log("NULL PARTY ALERT NULL PARTY ALERT");
            return;
        }
        foreach(NetworkConnection member in theirParty.Members)
        {

            PSA.UPDATEHEALTHBARSSOMEHOW(member, playerConn, next, max);

        }

        NetworkConnection theirLeader = theirParty.Leader;

        PSA.UPDATEHEALTHBARSSOMEHOW(theirLeader, playerConn, next, max);






    }


    public void UpdateMemberHealthBarTARGET(NetworkConnection target, NetworkConnection playerConnection, int newHealth, int max)
    {
        UpdateMemberHealthBar(playerConnection, newHealth, max);
    }
        public void UpdateMemberHealthBar(NetworkConnection playerConnection, int newHealth, int max)
    {
        Debug.Log("UPDATE HEALTH BAR FOR MEMBER TO " + newHealth);
        // Find the corresponding HP bar for the player in the HUD.
        // Assuming you have a dictionary that maps NetworkConnection to the HP bar GameObject.
        if (_hpBars.TryGetValue(playerConnection, out GameObject hpBar))
        {
            // Assuming your HealthBar component is attached to the hpBar GameObject.
            healthbar healthBar = hpBar.GetComponent<healthbar>();
            if (healthBar != null)
            {
                Debug.Log("SET HEALTH SUCCESS");
                healthBar.SetMaxHealth(max);
                healthBar.SetHealth(newHealth);
               // healthBar.gameObject.name = "ADJUSTEDHEALTHBAR";
                Debug.Log(healthBar.transform.root.gameObject.name);
            }
            else
            {
                Debug.Log("NULL HP BARRR");
            }
        }
        else
        {
            Debug.Log("NO NEWS");
        }
    }
    public Dictionary<NetworkConnection, GameObject> _hpBars = new Dictionary<NetworkConnection, GameObject>();


    public int getPlayerPartySize(NetworkConnection somePlayerConnection)
    {
        Party playerParty = GetPlayerParty(somePlayerConnection);
        if (playerParty != null)
        {
            // The player is in a party.
            return playerParty.Members.Count + 1; // Add 1 to include the party leader.
        }
        else
        {
            return 0;
        }



    }


    public Party GetPlayerParty(NetworkConnection playerConnection)
    {
        // Check if the player is a party leader or a member.
        if (_parties.TryGetValue(playerConnection, out Party playerParty))
        {
            return playerParty;
        }

        // If the player is not a party leader, check if they are a member of any party.
        foreach (Party party in _parties.Values)
        {
            if (party.Members.Contains(playerConnection))
            {
                return party;
            }
        }

        // If the player is not a leader or a member of any party, return null.
        return null;
    }

    public List<string> GetPlayerNamesInParty(NetworkConnection playerConnection)
    {
        List<string> playerNames = new List<string>();
        Party currentparty = null;

        // Iterate through the KeyValuePair in the _parties dictionary.
        foreach (var partyKeyValuePair in _parties)
        {
            Party party = partyKeyValuePair.Value;
            if (party.Leader == playerConnection || party.Members.Contains(playerConnection))
            {
                currentparty = party;
                break;
            }
        }

        if (currentparty != null)
        {
            // Add the party leader's name.
            GameObject leaderObject = gameManager._players[currentparty.Leader];
            playerNames.Add(leaderObject.GetComponent<PlayerHealth>().myName);

            // Add the names of the other party members.
            foreach (var member in currentparty.Members)
            {
                GameObject memberObject = gameManager._players[member];
                playerNames.Add(memberObject.GetComponent<PlayerHealth>().myName);
            }
        }

        return playerNames;
    }





















    public void scrubPartyHud()
    {
        foreach (Transform child in partyHUDContainer)
        {
            Destroy(child.gameObject);
        }

    }



    public void UpdatePartyHUD(NetworkConnection leaderConn, List<NetworkConnection> memberConn)
    {
        Debug.Log("ATTEMPTING TO UPDATE HUD");

        if (leaderConn == null)
        {

            Debug.Log("LEADER CONN NULL IN UPDATEPARTYHUD");
        }

        if (memberConn == null)
        {
            Debug.Log("MEMBER CONN NULL IN UPDATEPARTYHUD");
        }

        NetworkConnection partyLeader = leaderConn;
        // Clear existing party HUD items.
        foreach (Transform child in partyHUDContainer)
        {
            Destroy(child.gameObject);
        }


        if (!isInParty)
        {
            Debug.Log("NOT IN PARTY");
            return;
        }
        // Get the party members.
        _hpBars.Clear();

            Debug.Log("UPDATE HUD " + memberConn.Count + " is size");
            // Instantiate the PARTYPLAYER prefab for each party member and set the required information.

         
            GameObject partyPlayerInstanceLeader = Instantiate(partyPlayerPrefab, partyHUDContainer);

            GameObject hpBarLeader = partyPlayerInstanceLeader.transform.Find("ADJUSTEDHEALTHBAR").gameObject; // Replace "Healthbar" with the actual name of the HP bar GameObject in your prefab.
            _hpBars.Add(leaderConn, hpBarLeader);

            GameObject thatLeader = gameManager.whoisthis(leaderConn);
            PlayerHealth playerHealthLeader = thatLeader.GetComponent<PlayerHealth>();

            // Set the player's name.
            TextMeshProUGUI playerNameTextLeader = partyPlayerInstanceLeader.transform.Find("PlayerName").GetComponent<TextMeshProUGUI>();
            playerNameTextLeader.text = playerHealthLeader.myName;

            // Set up the health bar.
            Slider healthBarSliderLeader = partyPlayerInstanceLeader.transform.Find("ADJUSTEDHEALTHBAR").GetComponent<Slider>();
            healthbar healthBarLeader = healthBarSliderLeader.GetComponent<healthbar>();
            healthBarLeader.SetHealth(playerHealthLeader.health);
            healthBarLeader.SetMaxHealth(playerHealthLeader.maxHP);

            // Set up the Leave Party button.
            Button leavePartyButtonLeader = partyPlayerInstanceLeader.transform.Find("LeavePartyButton").GetComponent<Button>();
            leavePartyButtonLeader.gameObject.SetActive(leaderConn == gsc.realLocalPlayer.GetComponent<PlayerHealth>().myConn);
            leavePartyButtonLeader.onClick.AddListener(() => LeaveParty(leaderConn));

            // Set up the Kick Player button.
            Button kickPlayerButtonLeader = partyPlayerInstanceLeader.transform.Find("KickPlayerButton").GetComponent<Button>();
            kickPlayerButtonLeader.gameObject.SetActive(partyLeader == gsc.realLocalPlayer.GetComponent<PlayerHealth>().myConn && leaderConn != partyLeader);
            kickPlayerButtonLeader.onClick.AddListener(() => KickPlayer(partyLeader, leaderConn));


            foreach (NetworkConnection member in memberConn)
            {
                GameObject partyPlayerInstance = Instantiate(partyPlayerPrefab, partyHUDContainer);
             
                GameObject hpBar = partyPlayerInstance.transform.Find("ADJUSTEDHEALTHBAR").gameObject; // Replace "Healthbar" with the actual name of the HP bar GameObject in your prefab.
                _hpBars.Add(member, hpBar);

                GameObject thatMember = gameManager.whoisthis(member);
                PlayerHealth playerHealth = thatMember.GetComponent<PlayerHealth>();

                // Set the player's name.
                TextMeshProUGUI playerNameText = partyPlayerInstance.transform.Find("PlayerName").GetComponent<TextMeshProUGUI>();
                playerNameText.text = playerHealth.myName;

                // Set up the health bar.
                Slider healthBarSlider = partyPlayerInstance.transform.Find("ADJUSTEDHEALTHBAR").GetComponent<Slider>();
                healthbar healthBar = healthBarSlider.GetComponent<healthbar>();
                healthBar.SetHealth(playerHealth.health);
                healthBar.SetMaxHealth(playerHealth.maxHP);

                // Set up the Leave Party button.
                Button leavePartyButton = partyPlayerInstance.transform.Find("LeavePartyButton").GetComponent<Button>();
                leavePartyButton.gameObject.SetActive(member == gsc.realLocalPlayer.GetComponent<PlayerHealth>().myConn);
                leavePartyButton.onClick.AddListener(() => LeaveParty(member));

                // Set up the Kick Player button.
                Button kickPlayerButton = partyPlayerInstance.transform.Find("KickPlayerButton").GetComponent<Button>();
                kickPlayerButton.gameObject.SetActive(partyLeader == gsc.realLocalPlayer.GetComponent<PlayerHealth>().myConn && member != partyLeader);
                kickPlayerButton.onClick.AddListener(() => KickPlayer(partyLeader, member));
            
        }
        Debug.Log("UPDATE HUD AND MINIMAP TRACKER");
        updateMinimapTracker();
    }




    public GameSetupController gsc;


    public GameObject partyPlayerPrefab;
    private Dictionary<NetworkConnection, GameObject> playerPartyObjects = new Dictionary<NetworkConnection, GameObject>();
    public int maxPartySize = 5;

    public GameObject invitePopup;
    public TextMeshProUGUI inviteText;

    public void showInvitePopup(string inviterName)
    {
       invitePopup.SetActive(true);
        inviteText.text = inviterName;


        GameObject.Find("PULLUPCURSOR").GetComponent<PULLUPCURSOR>().pullUpCursor();


    }

    public void acceptInvite()
    {


        invitePopup.SetActive(false);

        NetworkConnection whoInvited = invitePopup.GetComponent<partyPopup>().lastInviterConnection;
        string inviterName = invitePopup.GetComponent<partyPopup>().inviterName;

        NetworkConnection localConnection = gsc.realLocalPlayer.GetComponent<PlayerHealth>().myConn;
        string inviteeName = DBManager.username;

        AcceptInviteServer(PSA, whoInvited, localConnection, inviterName, inviteeName);
        currentPartyLeader = whoInvited;
        GameObject.Find("DISABLECURSOR").GetComponent<TURNOFFCURSOR>().turnOffCursor();
    }
    public void AcceptInviteServer(partySystemAssistant pSA, NetworkConnection inviter, NetworkConnection invitee, string invitername, string inviteename)
    {
        PSA.AcceptInviteServer(pSA, inviter, invitee, invitername, inviteename);


    }

    public void DeclineInviteServer(partySystemAssistant pSA, NetworkConnection inviter, NetworkConnection invitee, string inviterName, string inviteename)
    {
        PSA.DeclineInviteServer(pSA, inviter, invitee, inviterName, inviteename);
    }

    public void UpdatePartyHUDForOnlyMeUsingLeader(NetworkConnection leader)
    {

        NetworkConnection localConnection = gsc.realLocalPlayer.GetComponent<PlayerHealth>().myConn;
        PSA.updateOnePersonHUD(localConnection, leader);



    }




    public void updateOnePersonHUDSERVER(NetworkConnection person, NetworkConnection leader)
    {

        if (leader != null)
        {
            if (_parties.TryGetValue(leader, out Party party))
            {
                // Update HUD for the party leader.
                PSA.updatePartyForOnePersonUsingInfo(leader, party.Leader, party.Members);

            }

        }
        else
        {
            Debug.Log("EMPTY LEADER");
        }



    }


    public void UpdatePartyHUDForAllMembersStrict(NetworkConnection leaderConn)
    {
        Debug.Log("UPDATE PARTY HUD FOR ALL MEMBERS 1");
        // right now only the server comes here
        // but it does not know the correct party leader to update based on, so enforce it knows
        if (leaderConn != null)
        {
            Debug.Log("UPDATE PARTY HUD FOR ALL MEMBERS 2");
            if (_parties.TryGetValue(leaderConn, out Party party))
            {
                Debug.Log("UPDATE PARTY HUD FOR ALL MEMBERS 3");
                // Update HUD for the party leader.
                UpdatePartyHUDForPlayer(leaderConn, party.Leader, party.Members);

                // Iterate through all the party members and call UpdatePartyHUD for each of them.
                foreach (NetworkConnection member in party.Members)
                {
                    Debug.Log("UPDATE PARTY HUD FOR ALL MEMBERS 4");
                    UpdatePartyHUDForPlayer(member, party.Leader, party.Members);
                }
            }

        }
        else
        {
            Debug.Log("EMPTY LEADER");
        }

    }


    public void UpdatePartyHUDForAllMembers()
    {
        Debug.Log("UPDATE PARTY HUD FOR ALL MEMBERS 1");
              // right now only the server comes here
              // but it does not know the correct party leader to update based on, so enforce it knows
            if (currentPartyLeader != null)
        {
            Debug.Log("UPDATE PARTY HUD FOR ALL MEMBERS 2");
            if (_parties.TryGetValue(currentPartyLeader, out Party party))
            {
                Debug.Log("UPDATE PARTY HUD FOR ALL MEMBERS 3");
                // Update HUD for the party leader.
                UpdatePartyHUDForPlayer(currentPartyLeader, party.Leader, party.Members);

                    // Iterate through all the party members and call UpdatePartyHUD for each of them.
                    foreach (NetworkConnection member in party.Members)
                {
                    Debug.Log("UPDATE PARTY HUD FOR ALL MEMBERS 4");
                    UpdatePartyHUDForPlayer(member, party.Leader, party.Members);
                    }
                }

            } else
        {
            Debug.Log("EMPTY LEADER");
        }
        
    }

    public void UpdatePartyHUDForAllMembersUsingLeader(NetworkConnection leader)
    {


        if (leader != null)
        {
            if (_parties.TryGetValue(leader, out Party party))
            {
                // Update HUD for the party leader.
                UpdatePartyHUDForPlayer(leader, party.Leader, party.Members);
                Debug.Log("UPDATE HUD FOR LEADER");
                // Iterate through all the party members and call UpdatePartyHUD for each of them.
                foreach (NetworkConnection member in party.Members)
                {
                    UpdatePartyHUDForPlayer(member, party.Leader, party.Members);
                    Debug.Log("UPDATE HUD FOR MEMBER");
                }
            }

        }
        else
        {
            Debug.Log("EMPTY LEADER");
        }


    }
    private void UpdatePartyHUDForPlayer(NetworkConnection connection, NetworkConnection leaderconn, List<NetworkConnection> memberconn)
    {
        Debug.Log("UPDATE PARTY HUD FOR PLAYER");
        PSA.UpdatePartyHUDForPlayerSERVER(connection, leaderconn, memberconn);


    }







    public void rejectInvite()
    {
        NetworkConnection whoInvited = invitePopup.GetComponent<partyPopup>().lastInviterConnection;
        string inviterName = invitePopup.GetComponent<partyPopup>().inviterName;
     
        NetworkConnection localConnection = gsc.realLocalPlayer.GetComponent<PlayerHealth>().myConn;
        string inviteeName = DBManager.username;

        invitePopup.SetActive(false);
        DeclineInviteServer(PSA, whoInvited, gsc.realLocalPlayer.GetComponent<PlayerHealth>().myConn, inviterName, inviteeName);

        GameObject.Find("DISABLECURSOR").GetComponent<TURNOFFCURSOR>().turnOffCursor();
    }
    public class Party
    {
        public List<NetworkConnection> Members = new List<NetworkConnection>();
        public NetworkConnection Leader;
    }


    public void AddPlayerToParty(NetworkConnection inviter, NetworkConnection invitee, string invitername, string inviteename)
    {

        if (!InstanceFinder.IsServer)
        {
            Debug.Log("NOT ALLOWED");
            return;
        }

        // Check if the invitee is already in a party.
        if (IsPlayerInParty(invitee))
        {
            string errorMsg = "Player " + inviteename + " is already in a party.";
            notifyPlayer(inviter, errorMsg);
            return;
        }


        // Check if the inviter has a party. If not, create one.
        if (!_parties.TryGetValue(inviter, out Party inviterParty))
        {
            inviterParty = new Party();
            _parties.Add(inviter, inviterParty);
            inviterParty.Leader = inviter;
            Debug.Log("ADD ME TO PARTY");
        }

        if (inviterParty.Members.Count >= maxPartySize)
        {
            string errorMsg = "Your party has reached the maximum size of " + maxPartySize + " players.";
            notifyPlayer(inviter, errorMsg);
            return;
        }


        // Add the invitee to the inviter's party.
        inviterParty.Members.Add(invitee);

        Debug.Log("ADD YOU TO PARTY");
        // Add or update the invitee's party in the _parties dictionary.
        /*
        if (_parties.ContainsKey(invitee))
        {
            _parties[invitee] = inviterParty;
        }
        else
        {
            _parties.Add(invitee, inviterParty);
        }*/

        string inviterMsg = "Player " + inviteename + " has joined your party.";
        string inviteeMsg = "You have joined the party of player " + invitername + ".";
        notifyPlayer(inviter, inviterMsg);
        notifyPlayer(invitee, inviteeMsg);

        currentPartyLeader = inviter;

        isInParty = true;
        
    }

    public void learnJoinedParty(NetworkConnection inviter)
    {
        currentPartyLeader = inviter;
        isInParty = true;

        Debug.Log(" I JOINED A PARRTY PARTY SYSTEM FINAL");
        //   UpdatePartyHUDForOnlyMeUsingLeader(inviter);
        PlayerHealth pHealth = gsc.realLocalPlayer.GetComponent<PlayerHealth>();

        requestHealthUpdateAsClient(pHealth.myConn, pHealth.health, pHealth.maxHP, DBManager.username);
        updateMinimapTracker();

     //   pHealth.gameObject.GetComponent<InputNexus>().partySystem.updateMinimapTracker();
    }
    public void notifyPlayerJoinedParty(NetworkConnection invitee, NetworkConnection inviter) 
    {

        PSA.notifyPlayerJoinedParty(invitee, inviter);

    }

    public void notifyPlayer(NetworkConnection who, string whatmsg)
    {
     
            GameObject.Find("CHATMANAGER").GetComponent<ChatBroadcastGuide>().sendTargetedMsg(whatmsg, who);
  

    }

    private void Start()
    {
        partyPlayerPrefab = Resources.Load<GameObject>("PARTYPLAYER");
        gameManager = gsc.gameManager;
    }

    public void InvitePlayer(NetworkConnection inviter, NetworkConnection invitee, string inviterName)
    {
        Debug.Log("RELAY invite player");
        invitePlayerSERVER(PSA, inviter, invitee, inviterName);
    }


    public void SendInviteRequest(NetworkConnection target, NetworkConnection inviter, string inviterName)
    {
        PSA.SendInviteRequest(target, inviter, inviterName);
    }
    public void invitePlayerSERVER(partySystemAssistant ps, NetworkConnection inviter, NetworkConnection invitee, string whoInvited)
    {
        Debug.Log("RELAY to PSA");

        PSA.invitePlayerSERVER(ps, inviter, invitee, whoInvited);
        }


    public void LeaveParty(NetworkConnection player)
    {
        isInParty = false;
        currentPartyLeader = null;
        Debug.Log("PLAYER WANTS T LEAVE");
        PSA.LeavePartySERVER(player, DBManager.username);
        notifyPlayer(player, " you have left the party");
        
     //   _parties = new Dictionary<NetworkConnection, Party>();

        scrubPartyHud();
        scrubMinimapTracker();
     //   updateMinimapTracker();
    }



    public void LeavePartyBYSERVER(NetworkConnection player, string playername)
    {
        Debug.Log("REMOVING SOMEONE FROM PARTY");
        // Find the player's party.
        Party playerParty = GetPlayerParty(player);

        if (playerParty != null)
        {
            // Remove the player from the party.
            playerParty.Members.Remove(player);

            // If the player is the party leader, disband the party and clear the party leader's HP bars.
            if (playerParty.Leader == player)
            {
                // Remove HP bars of all party members.
                foreach (NetworkConnection member in playerParty.Members)
                {
                    if (_hpBars.ContainsKey(member))
                    {
                        _hpBars.Remove(member);
                    }
                }

                // Notify all party members that the party has been disbanded.
                string disbandedMessage = "The party has been disbanded.";
                foreach (NetworkConnection member in playerParty.Members)
                {
                    notifyPlayer(member, disbandedMessage);
                    notifyPlayerDisbanded(member);
                }

                

                // Remove the party from the _parties dictionary.
                _parties.Remove(player);
            }
            else
            {
                // Remove the player's HP bar from the _hpBars dictionary.
                if (_hpBars.ContainsKey(player))
                {
                    _hpBars.Remove(player);
                }

                // Notify the player that they have left the party.
                string leaveMessage = "You have left the party.";
                notifyPlayer(player, leaveMessage);
                string msgforothers = playername + " has left your party.";
                foreach (NetworkConnection member in playerParty.Members)
                {
                  
                    notifyPlayer(member, msgforothers);
                }
            }

            // Update the party HUD for all remaining party members.
          //  UpdatePartyHUDForAllMembers();

            UpdatePartyHUDForAllMembersUsingLeader(playerParty.Leader);

        }

    }


    public void notifyPlayerDisbandedBYSERVER()
    {


        isInParty = false;
        currentPartyLeader = null;
        Debug.Log("DISBANDDED");
     //   _parties = new Dictionary<NetworkConnection, Party>();
        scrubPartyHud();
        scrubMinimapTracker();
       // updateMinimapTracker();
    }
    public void notifyPlayerDisbanded(NetworkConnection member)
    {
        PSA.notifyPlayerDisbandedSERVER(member);
      

    }
    public void KickPlayer(NetworkConnection partyLeader, NetworkConnection playerToKick)
    {
        PSA.KickPlayer(partyLeader, playerToKick);
    }
        public void KickPlayerRoutine(NetworkConnection partyLeader, NetworkConnection playerToKick)
    {

        Debug.Log("KICK PLAYER START");

        if (partyLeader == null)
        {
            Debug.Log("KICK NULL LEADER");
        }
        if (playerToKick == null)
        {
            Debug.Log("KICK NULL playerToKick");
        }
        if (_parties.TryGetValue(partyLeader, out Party party))
        {
            Debug.Log("KICK LEVEL 1");
            if (party.Members.Contains(playerToKick))
            {
                Debug.Log("KICK LEVEL 2");
                // Remove the player from the party.
                party.Members.Remove(playerToKick);

                // Remove the kicked player's HP bar from the _hpBars dictionary.
                if (_hpBars.ContainsKey(playerToKick))
                {
                    Debug.Log("KICK LEVEL 3");
                    _hpBars.Remove(playerToKick);
                }

                // Notify the kicked player and the party leader.
                string kickedPlayerMsg = "You have been kicked from the party.";
                string partyLeaderMsg = "Player " + gameManager.whoisthis(playerToKick).GetComponent<PlayerHealth>().myName + " has been kicked from the party.";
                notifyPlayer(playerToKick, kickedPlayerMsg);
                notifyPlayer(partyLeader, partyLeaderMsg);

                targetScrub(playerToKick);

                // Update the party HUD for all remaining party members.
                UpdatePartyHUDForAllMembersStrict(partyLeader);
            }
        }
    }

    public void targetScrub(NetworkConnection playerToKick)
    {

        PSA.targetScrub(playerToKick);

    }



    public void targetScrubRoutine()
    {


        scrubPartyHud();

        isInParty = false;
        currentPartyLeader = null;
        Debug.Log("SCRUB WHEN KICKED");
        scrubMinimapTracker();
        
    }

    public bool IsPlayerInParty(NetworkConnection playerConnection)
    {
        foreach (Party party in _parties.Values)
        {
            if (party.Leader == playerConnection || party.Members.Contains(playerConnection))
            {
                return true;
            }
        }
        return false;
    }


    public List<NetworkConnection> GetPartyPlayerConnections(NetworkConnection playerConnection)
    {
        List<NetworkConnection> memberConnections = new List<NetworkConnection>();

        // only the server can do this
        Party ourParty = null;
        foreach (Party party in _parties.Values)
        {
            if (party.Leader == playerConnection || party.Members.Contains(playerConnection))
            {
                /*
                memberConnections.Add(playerConnection);
                foreach(NetworkConnection member in party.Members)
                {
                    memberConnections.Add(member);
                }*/
                ourParty = party;
            }
        }

        if (ourParty != null) { 
        memberConnections.Add(ourParty.Leader);
        foreach (NetworkConnection member in ourParty.Members)
        {
            memberConnections.Add(member);
        }
        }
        return memberConnections;
    }



    public void LeavePartyAndFigureOutLeader()
    {
        if (currentPartyLeader != null) { 
        LeaveParty(currentPartyLeader);
        }
    }

 
    public void requestHealthUpdateAsClient(NetworkConnection conn, int next, int maxHP, string playername) // gets base.Owner, next, maxHP
    {
        PSA.requestHealthUpdateAsClient(PSA, conn, next, maxHP, playername);
    }

    public void updatePartyMemberHealthBasedOnValues(NetworkConnection conn, int next, int maxHP, string playername)
    {
        Debug.Log("CALL updatePartyMemberHealthBasedOnValues " + _hpBars.Count);

        

        if (_hpBars.TryGetValue(conn, out GameObject hpBar))
        {
          
            healthbar healthBar = hpBar.GetComponent<healthbar>();
            if (healthBar != null)
            {
                Debug.Log("SET HEALTH SUCCESS for " + playername);
                healthBar.SetMaxHealth(maxHP);
                healthBar.SetHealth(next);
            
            }
            else
            {
                Debug.Log("NULL HP BARRR");
            }
        }
        else
        {
            Debug.Log("NO NEWS");
        }



    }

}
