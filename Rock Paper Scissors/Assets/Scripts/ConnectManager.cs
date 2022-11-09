using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;

public class ConnectManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField usernameInput;
    [SerializeField] TMP_Text feedbackText;
    [SerializeField] GameManager GM;
    [SerializeField] LobbyManager LM;
    [SerializeField] Image avatar;
    [SerializeField] Image roomYourAvatar;
    [SerializeField] Image roomOtherAvatar;
    [SerializeField] Image winnerAvatar;

    [Header("Spaceship Sprites")]
    [SerializeField] Sprite spaceship1;
    [SerializeField] Sprite spaceship2;
    [SerializeField] Sprite spaceship3;
    [SerializeField] Sprite spaceship4;
    [SerializeField] Sprite spaceship5;

    void Start()
    {
        usernameInput.characterLimit = 13;
        SetRoomYourAvatar();
    }

    public void ClickConnect()
    {
        if (usernameInput.text.Length < 3)
        {
            feedbackText.text = "Min. 3 characters!";
            feedbackText.color = Color.red;
            return;
        }

        // Save username
        PhotonNetwork.NickName = usernameInput.text;
        PhotonNetwork.AutomaticallySyncScene = true;

        // Connect to server
        PhotonNetwork.ConnectUsingSettings();
        feedbackText.text = "Connecting...";
        feedbackText.color = Color.yellow;
    }

    // Run after successfuly connected
    public override void OnConnectedToMaster()
    {
        //usernameInput.text = "";
        feedbackText.text = "Connected to Master";
        feedbackText.color = Color.green;
        GM.versusConnected = true;
        if (DontDestroy.instance.relogSequence)
        {
            Debug.Log("Relog: OnConnectedToMaster");
            LM.roomYourName.text = DontDestroy.instance.yourUsername;
            LM.roomOtherName.text = DontDestroy.instance.otherUsername;
            //GM.OtherLeftPanel();
            if (DontDestroy.instance.relogCauser)
            {
                GM.GoToLobby();
            }
        }
        else
        {
            GM.GoToLobby();
        }

        feedbackText.text = "Connect to a space-time portal :";
        feedbackText.color = Color.white;
        //StartCoroutine(LoadLevelAfterConnectedAndReady());
    }

    //IEnumerator LoadLevelAfterConnectedAndReady()
    //{
    //    while (!PhotonNetwork.IsConnectedAndReady)
    //    {
    //        yield return null;
    //    }
    //    SceneManager.LoadScene("Lobby");
    //}

    public void ClickDisconnect()
    {
        PhotonNetwork.LeaveLobby();
        PhotonNetwork.Disconnect();
        GM.GoToConnect();
    }

    public void NextAvatar()
    {
        switch (DontDestroy.instance.avatarNum)
        {
            case 1:
                DontDestroy.instance.avatarNum = 2;
                avatar.sprite = spaceship2;
                roomYourAvatar.sprite = spaceship2;
                break;
            case 2:
                DontDestroy.instance.avatarNum = 3;
                avatar.sprite = spaceship3;
                roomYourAvatar.sprite = spaceship3;
                break;
            case 3:
                DontDestroy.instance.avatarNum = 4;
                avatar.sprite = spaceship4;
                roomYourAvatar.sprite = spaceship4;
                break;
            case 4:
                DontDestroy.instance.avatarNum = 5;
                avatar.sprite = spaceship5;
                roomYourAvatar.sprite = spaceship5;
                break;
            case 5:
                DontDestroy.instance.avatarNum = 1;
                avatar.sprite = spaceship1;
                roomYourAvatar.sprite = spaceship1;
                break;
        }
    }

    public void PreviousAvatar()
    {
        switch (DontDestroy.instance.avatarNum)
        {
            case 1:
                DontDestroy.instance.avatarNum = 5;
                avatar.sprite = spaceship5;
                roomYourAvatar.sprite = spaceship5;
                break;
            case 2:
                DontDestroy.instance.avatarNum = 1;
                avatar.sprite = spaceship1;
                roomYourAvatar.sprite = spaceship1;
                break;
            case 3:
                DontDestroy.instance.avatarNum = 2;
                avatar.sprite = spaceship2;
                roomYourAvatar.sprite = spaceship2;
                break;
            case 4:
                DontDestroy.instance.avatarNum = 3;
                avatar.sprite = spaceship3;
                roomYourAvatar.sprite = spaceship3;
                break;
            case 5:
                DontDestroy.instance.avatarNum = 4;
                avatar.sprite = spaceship4;
                roomYourAvatar.sprite = spaceship4;
                break;
        }
    }

    public void SetRoomYourAvatar()
    {
        switch (DontDestroy.instance.avatarNum)
        {
            case 1:
                avatar.sprite = spaceship1;
                roomYourAvatar.sprite = spaceship1;
                break;
            case 2:
                avatar.sprite = spaceship2;
                roomYourAvatar.sprite = spaceship2;
                break;
            case 3:
                avatar.sprite = spaceship3;
                roomYourAvatar.sprite = spaceship3;
                break;
            case 4:
                avatar.sprite = spaceship4;
                roomYourAvatar.sprite = spaceship4;
                break;
            case 5:
                avatar.sprite = spaceship5;
                roomYourAvatar.sprite = spaceship5;
                break;
        }
    }

    public void SetRoomOtherAvatar()
    {
        switch (DontDestroy.instance.otherAvatarNum)
        {
            case 1:
                roomOtherAvatar.sprite = spaceship1;
                break;
            case 2:
                roomOtherAvatar.sprite = spaceship2;
                break;
            case 3:
                roomOtherAvatar.sprite = spaceship3;
                break;
            case 4:
                roomOtherAvatar.sprite = spaceship4;
                break;
            case 5:
                roomOtherAvatar.sprite = spaceship5;
                break;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            GM.EnableEnduranceButtons();
        }

        GM.loadingPanel.SetActive(false);
        LM.challengerPanel.SetActive(true);
        GM.roomPanel.SetActive(true);
    }

    public void WinnerAvatar(int x)
    {
        switch (x)
        {
            case 0:
                winnerAvatar.sprite = roomOtherAvatar.sprite;
                break;
            case 1:
                winnerAvatar.sprite = roomYourAvatar.sprite;
                break;
        }
    }
}
