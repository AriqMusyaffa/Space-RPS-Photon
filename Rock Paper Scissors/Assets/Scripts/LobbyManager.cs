using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using ExitGames.Client.Photon;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] TMP_InputField newRoomInputField;
    [SerializeField] TMP_Text feedbackText;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] GameManager GM;
    [SerializeField] ConnectManager CM;
    [SerializeField] GameObject roomListObject;
    [SerializeField] RoomItem roomItemPrefab;
    List<RoomItem> roomItemList = new List<RoomItem>();
    public int anyRoomSelected = 0;
    string roomName;
    [SerializeField] Button joinButton;
    [SerializeField] Button vsStartButton;
    [SerializeField] TMP_Text playerDetectText;
    int otherPlayersCount = 0;
    public TMP_Text roomYourName;
    public TMP_Text roomOtherName;
    public GameObject challengerPanel;
    [SerializeField] GameObject emptyRoomPanel;
    [SerializeField] ChallengeText challengerDetectText;
    public CardNetPlayer netPlayer;
    Dictionary<string, RoomInfo> roomInfoCache = new Dictionary<string, RoomInfo>();
    public bool realLogOut = false;
    bool sendAvatar2 = false;
    bool saveOtherName = false;

    public void JoinLobby()
    {
        PhotonNetwork.JoinLobby();

        //if (DontDestroy.instance.relogCauser)
        //{
            DontDestroy.instance.relogSequence = false;
            DontDestroy.instance.relogCauser = false;
        //}
        Debug.Log("Joined Lobby");
    }

    void Start()
    {
        sendAvatar2 = false;
        saveOtherName = false;
        newRoomInputField.characterLimit = 13;
    }

    void Update()
    {
        if (anyRoomSelected > 0)
        {
            joinButton.gameObject.SetActive(true);
        }
        else
        {
            joinButton.gameObject.SetActive(false);
        }

        if (otherPlayersCount > 0)
        {
            //playerDetectText.transform.localPosition = new Vector2(0, 525);
            if (PhotonNetwork.IsMasterClient)
            {
                playerDetectText.text = "Challenger detected!";
                vsStartButton.gameObject.SetActive(true);
                challengerDetectText.Enable();
            }
            else
            {
                playerDetectText.text = "Waiting for host to start...";
                vsStartButton.gameObject.SetActive(false);
                challengerDetectText.Disable();
            }
            emptyRoomPanel.SetActive(false);
            //challengerPanel.SetActive(true);
            if (!sendAvatar2)
            {
                GM.roomPanel.SetActive(false);
                GM.loadingPanel.SetActive(true);
                sendAvatar2 = true;
            }
        }
        else
        {
            vsStartButton.gameObject.SetActive(false);
            challengerPanel.SetActive(false);
            challengerDetectText.Disable();
            emptyRoomPanel.SetActive(true);
        }

        if (feedbackText.color == Color.red)
        {
            feedbackText.transform.localPosition = new Vector2(0, 530);
        }
        else
        {
            feedbackText.transform.localPosition = new Vector2(0, 510);
        }
    }
    
    public void ClickCreateRoom()
    {
        Debug.Log("Click Create Room");
        if (newRoomInputField.text.Length < 3)
        {
            feedbackText.text = "Min. 3 characters!";
            feedbackText.color = Color.red;
            return;
        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        PhotonNetwork.CreateRoom(newRoomInputField.text, roomOptions);
    }

    public void ClickStartGame_1()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //PhotonNetwork.CurrentRoom.IsOpen = false;
            netPlayer.ClickStartGame_2();
        }
    }

    public void SelectedRoomName(string rn)
    {
        roomName = rn; 
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnCreatedRoom()
    {
        feedbackText.text = "Created room : " + PhotonNetwork.CurrentRoom.Name;
        feedbackText.color = Color.green;
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Relog: OnJoinedRoom");
        newRoomInputField.text = "";
        feedbackText.text = "Joined room : " + PhotonNetwork.CurrentRoom.Name;
        feedbackText.color = Color.green;
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        GM.GoToRoom();

        UpdatePlayerList();

        SetStartGameButton();

        if (netPlayer == null)
        {
            PhotonNetwork.Instantiate(GM.netPlayerPrefab.name, Vector3.zero, Quaternion.identity);
            netPlayer = GameObject.FindWithTag("CardNetPlayer").GetComponent<CardNetPlayer>();
        }

        DontDestroy.instance.yourUsername = roomYourName.text;
        DontDestroy.instance.otherUsername = roomOtherName.text;
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Relog: OnLeftRoom");
        // Relog Fix
        if (DontDestroy.instance.relogCauser)
        {
            PhotonNetwork.LeaveLobby();
        }
    }

    public override void OnLeftLobby()
    {
        Debug.Log("Relog: OnLeftLobby");
        // Relog Fix
        if (DontDestroy.instance.relogSequence)
        {
            PhotonNetwork.Disconnect();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Relog: OnDisconnected");
        // Relog Fix
        if (DontDestroy.instance.relogSequence)
        {
            GM.LoadScene(0);

            /*
            // Save username
            PhotonNetwork.NickName = CM.usernameInput.text;
            PhotonNetwork.AutomaticallySyncScene = true;

            // Connect to server
            PhotonNetwork.ConnectUsingSettings();
            */
        }
        else
        {
            // real log out
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log("Relog: OnPlayerEnteredRoom");
        UpdatePlayerList();

        if (!saveOtherName)
        {
            DontDestroy.instance.otherUsername = roomOtherName.text;
            saveOtherName = true;
        }

        // Send message to everyone to reset CardNetPlayer
        //var actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
        //var raiseEventOptions = new RaiseEventOptions();
        //raiseEventOptions.Receivers = ReceiverGroup.All;
        //PhotonNetwork.RaiseEvent(4, actorNum, raiseEventOptions, SendOptions.SendReliable);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log("Relog: OnPlayerLeftRoom");
        UpdatePlayerList();
        
        if (PhotonNetwork.InRoom)
        {
            // Send message to self that other player left the room
            var actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
            var raiseEventOptions = new RaiseEventOptions();
            raiseEventOptions.Receivers = ReceiverGroup.All;
            PhotonNetwork.RaiseEvent(3, actorNum, raiseEventOptions, SendOptions.SendReliable);

            GM.P1Panel.SetActive(false);
            GM.P2Panel.SetActive(false);
        }
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        Debug.Log("Relog: OnMasterClientSwitched");
        SetStartGameButton();
    }

    private void SetStartGameButton()
    {
        vsStartButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        vsStartButton.interactable = PhotonNetwork.CurrentRoom.PlayerCount > 1;
    }

    private void UpdatePlayerList()
    {
        // PhotonNetwork.PlayerList (alternative)
        // foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)

        otherPlayersCount = 0;
        string[] playerNames = new string[2];
        int i = 0;

        foreach (var (id, player) in PhotonNetwork.CurrentRoom.Players)
        {
            playerNames[i] = player.NickName;
            i++;

            if (PhotonNetwork.IsMasterClient)
            {
                if (player != PhotonNetwork.MasterClient)
                {
                    otherPlayersCount++;
                }
            }
            else
            {
                otherPlayersCount++;
            }
        }

        roomYourName.text = playerNames[0];
        roomOtherName.text = playerNames[1];
        Debug.Log(playerNames[0]);
        Debug.Log(playerNames[1]);

        SetStartGameButton();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Relog: OnCreateRoomFailed");
        //feedbackText.text = returnCode.ToString() + " : " + message;
        feedbackText.text = "Danger : space-time anomaly!\nFailed to create room.";
        feedbackText.color = Color.red;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Relog: OnJoinedRoomFailed");
        //feedbackText.text = returnCode.ToString() + " : " + message;
        feedbackText.text = "Warning : coordinate encrypted!\nFailed to join room.";
        feedbackText.color = Color.red;
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        roomName = null;
        anyRoomSelected = 0;
        joinButton.gameObject.SetActive(false);

        foreach (var roomInfo in roomList)
        {
            roomInfoCache[roomInfo.Name] = roomInfo;
        }

        foreach (var item in this.roomItemList)
        {
            Destroy(item.gameObject);
        }

        this.roomItemList.Clear();

        var roomInfoList = new List<RoomInfo>(roomInfoCache.Count);

        // Sort available rooms first
        foreach (var roomInfo in roomInfoCache.Values)
        {
            if (roomInfo.IsOpen)
            {
                roomInfoList.Add(roomInfo);
            }
        }

        // Sort full rooms then
        foreach (var roomInfo in roomInfoCache.Values)
        {
            if (!roomInfo.IsOpen)
            {
                roomInfoList.Add(roomInfo);
            }
        }

        foreach (var roomInfo in roomInfoList)
        {
            //if (roomInfo.PlayerCount < roomInfo.MaxPlayers)
            if (roomInfo.MaxPlayers == 0)
            {
                continue;
            }

            RoomItem newRoomItem = Instantiate(roomItemPrefab, roomListObject.transform);
            newRoomItem.Set(this, roomInfo);
            this.roomItemList.Add(newRoomItem);
        }
    }

    public void ClickLeaveRoom()
    {
        Debug.Log("Click Leave Room");
        if (GM.Difficulty == GameManager.GameDifficulty.Versus)
        {
            //Card[] cards = netPlayer.cards;
            //foreach (var card in cards)
            //{
            //    var button = card.GetComponent<Button>();
            //    button.onClick.RemoveListener(() => netPlayer.RemoteClickButton(card.AttackValue));
            //}
            //netPlayer = null;

            DontDestroy.instance.relogCauser = true;

            // Send message to other player that we left the room
            var actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
            var raiseEventOptions = new RaiseEventOptions();
            raiseEventOptions.Receivers = ReceiverGroup.All;
            PhotonNetwork.RaiseEvent(3, actorNum, raiseEventOptions, SendOptions.SendReliable);

            //CardNetPlayer.NetPlayers.Clear();

            //GM.ResetMultiplayer();
            //PhotonNetwork.LeaveRoom();
            GM.State = GameManager.GameState.NetPlayersInitialization;
            GM.NextState = GameManager.GameState.NetPlayersInitialization;
            //GM.GoToLobby();
            challengerDetectText.Disable();
        }
        else
        {
            GM.LoadScene(0);
        }
    }
}
