using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class GameManager : MonoBehaviour, IOnEventCallback
{
    public GameObject netPlayerPrefab;
    public Player P1;
    public Player P2;
    public GameState State, NextState = GameState.NetPlayersInitialization;
    public GameObject P1Panel;
    public GameObject P2Panel;
    public GameObject menuPanel;
    public GameObject connectPanel;
    public GameObject lobbyPanel;
    public GameObject roomPanel;
    [SerializeField] TMP_Text lobbyText;
    public GameObject gameOverPanel;
    public TMP_Text winnerText;
    private Player damagedPlayer;
    private Player winner;
    public Vector2 bgSpawnPos;
    public Vector2 bgDeletePos;
    public GameDifficulty Difficulty = GameDifficulty.Medium;
    private Bot bot;
    public Button easyButton, mediumButton, hardButton, versusButton;
    private ColorBlock easyColorYes, mediumColorYes, hardColorYes, versusColorYes, 
                       easyColorNo, mediumColorNo, hardColorNo, versusColorNo;
    private Vector3 defaultButtonLocalScale;
    private DontDestroy DD;
    public GameObject P2Cards, P2HealthBar, P2HealthText;
    [SerializeField] LobbyManager LM;
    [SerializeField] ConnectManager CM;
    public bool versusConnected = false;
    //public List<int> syncReadyPlayers = new List<int>(2);
    HashSet<int> syncReadyPlayers = new HashSet<int>();
    bool Online = true;
    [SerializeField] TMP_Text replayText;
    [SerializeField] TMP_Text rematchOtherText;
    bool firstToAskRematch = true;
    int rematchAgreement = 0;
    bool alreadyClickRematch = false;
    bool alreadyReceiveRematch = false;
    [SerializeField] GameObject pvErrorPrevention;
    [SerializeField] TMP_Text otherLeftRoomText;
    public GameObject loadingPanel;
    public TMP_Text turnsText;
    public bool turnTaken = false;
    public bool sendAvatar = false;
    [SerializeField] GameObject winnerAvatarObject;
    [SerializeField] GameObject winnerAvatarBG;
    public Button enduranceLow, enduranceMedium, enduranceHigh;
    public GameObject chooseEnduranceText;
    public VersusEndurance vsEndurance = VersusEndurance.Medium;
    [SerializeField] GameObject enduranceBlocker;

    [Header("Health Config")]
    public float playerDamage;
    public float playerHeal;
    public float enemyDamage;
    public float enemyHeal;

    public enum GameDifficulty
    {
        Easy,
        Medium,
        Hard,
        Versus,
    }

    public enum GameState
    {
        SyncState,
        NetPlayersInitialization,
        ChooseAttack,
        Attacks,
        Damages,
        Draw,
        GameOver,
    }

    public enum VersusEndurance
    {
        Low,
        Medium,
        High,
    }

    void Start()
    {
        //Screen.SetResolution(540, 960, false);
        RemoteConfigFetcher.gameplay = false;

        DD = GameObject.FindWithTag("DontDestroy").GetComponent<DontDestroy>();

        GoToMainMenu();

        P1Panel.SetActive(false);
        P2Panel.SetActive(false);
        gameOverPanel.SetActive(false);

        bgSpawnPos = GameObject.FindWithTag("BGspawn").GetComponent<Transform>().localPosition;
        bgDeletePos = GameObject.FindWithTag("BGdelete").GetComponent<Transform>().localPosition;
        Destroy(GameObject.FindWithTag("BGspawn").gameObject);

        Difficulty = GameDifficulty.Versus;
        bot = P2.GetComponent<Bot>();

        defaultButtonLocalScale = easyButton.GetComponent<Transform>().localScale;

        easyColorYes = easyColorNo = easyButton.colors;
        mediumColorYes = mediumColorNo = mediumButton.colors;
        hardColorYes = hardColorNo = hardButton.colors;
        versusColorYes = versusColorNo = versusButton.colors;

        easyColorYes.normalColor = easyButton.colors.selectedColor;
        easyColorNo.normalColor = easyButton.colors.normalColor;

        mediumColorYes.normalColor = mediumButton.colors.selectedColor;
        mediumColorNo.normalColor = mediumButton.colors.normalColor;

        hardColorYes.normalColor = hardButton.colors.selectedColor;
        hardColorNo.normalColor = hardButton.colors.normalColor;

        versusColorYes.normalColor = versusButton.colors.selectedColor;
        versusColorNo.normalColor = versusButton.colors.normalColor;

        switch (DD.GameDifficulty)
        {
            case "Easy" :
                SetToEasy();
                break;
            case "Medium" :
                SetToMedium();
                break;
            case "Hard" :
                SetToHard();
                break;
            case "Versus" :
                SetToVersus();
                break;
        }

        DD.audioSource.clip = DD.menuBGM;
        DD.audioSource.Play();

        if (Online)
        {
            //PhotonNetwork.Instantiate(netPlayerPrefab.name, Vector3.zero, Quaternion.identity);
            //StartCoroutine(PingCoroutine());
            State = GameState.NetPlayersInitialization;
            NextState = GameState.NetPlayersInitialization;
        }
        else
        {
            State = GameState.ChooseAttack;
        }

        if (DontDestroy.instance.relogSequence)
        {
            // Save username
            PhotonNetwork.NickName = DontDestroy.instance.yourUsername;
            PhotonNetwork.AutomaticallySyncScene = true;

            if (!DontDestroy.instance.relogCauser)
            {
                DontDestroy.instance.relogSequence = false;
            }

            // Connect to server
            PhotonNetwork.ConnectUsingSettings();

            LoadingPanel();
            otherLeftRoomText.text = DontDestroy.instance.otherUsername + "\nhas left the room.";
            Debug.Log("Start Relog");
        }

        turnsText.gameObject.SetActive(false);
        sendAvatar = false;

        switch (DD.vsEndurance)
        {
            case "Easy":
                EnduranceLow();
                break;
            case "Medium":
                EnduranceMedium();
                break;
            case "Hard":
                EnduranceHigh();
                break;
        }

        DisableEnduranceButtons();
    }

    void Update()
    {
        switch (State)
        {
            case GameState.SyncState :
                if (syncReadyPlayers.Count == 2)
                {
                    if (!sendAvatar)
                    {
                        sendAvatar = true;
                    }
                    if (PhotonNetwork.IsMasterClient)
                    {
                        chooseEnduranceText.GetComponent<TMP_Text>().text = "Choose Endurance :";
                    }
                    syncReadyPlayers.Clear();
                    State = NextState;
                }
                break;

            case GameState.NetPlayersInitialization :
                if (CardNetPlayer.NetPlayers.Count == 2)
                {
                    foreach (var netPlayer in CardNetPlayer.NetPlayers)
                    {
                        if (netPlayer.photonView.IsMine)
                        {
                            netPlayer.Set(P1);
                        }
                        else
                        {
                            netPlayer.Set(P2);
                        }
                    }

                    otherLeftRoomText.text = DontDestroy.instance.otherUsername + "\nhas left the room.";
                    if (Difficulty == GameDifficulty.Versus)
                    {
                        ChangeState(GameState.ChooseAttack);
                    }
                    else
                    {
                        State = GameState.ChooseAttack;
                    }
                }
                break;

            case GameState.ChooseAttack :
                if (P1.AttackValue != null && P2.AttackValue != null)
                {
                    P1.AnimateAttack();
                    P2.AnimateAttack();
                    P1.IsClickable(false);
                    P2.IsClickable(false);
                    if (Difficulty == GameDifficulty.Versus)
                    {
                        ChangeState(GameState.Attacks);
                    }
                    else
                    {
                        State = GameState.Attacks;
                    }
                }
                break;


            case GameState.Attacks :
                turnsText.gameObject.SetActive(false);
                turnTaken = false;
                if (P1.IsAnimating() == false && P2.IsAnimating() == false)
                {
                    damagedPlayer = GetDamagedPlayer();
                    if (damagedPlayer != null)
                    {
                        damagedPlayer.AnimateDamage();
                        if (Difficulty == GameDifficulty.Versus)
                        {
                            ChangeState(GameState.Damages);
                        }
                        else
                        {
                            State = GameState.Damages;
                        }
                    }
                    else
                    {
                        P1.AnimateDraw();
                        P2.AnimateDraw();
                        if (Difficulty == GameDifficulty.Versus)
                        {
                            ChangeState(GameState.Draw);
                        }
                        else
                        {
                            State = GameState.Draw;
                        }
                    }
                }
                break;


            case GameState.Damages :
                if (P1.IsAnimating() == false && P2.IsAnimating() == false)
                {
                    if (damagedPlayer == P1)
                    {
                        P1.ChangeHealth(playerDamage);
                        P2.ChangeHealth(enemyHeal);
                    }
                    else
                    {
                        P1.ChangeHealth(playerHeal);
                        P2.ChangeHealth(enemyDamage);
                    }

                    var winner = GetWinner();

                    if (winner == null)
                    {
                        //ResetPlayers();
                        //P1.IsClickable(true);
                        //P2.IsClickable(true);
                        //State = GameState.ChooseAttack;
                        P1.AnimateAfterDamage();
                        P2.AnimateAfterDamage();
                        if (Difficulty == GameDifficulty.Versus)
                        {
                            ChangeState(GameState.Draw);
                        }
                        else
                        {
                            State = GameState.Draw;
                        }
                    }
                    else
                    {
                        Debug.Log(winner + " wins");
                        gameOverPanel.SetActive(true);
                        //winnerText.text = winner == P1 ? "Player 1 wins" : "Player 2 wins";
                        //if (Difficulty == GameDifficulty.Versus)
                        //{
                            P1Panel.SetActive(false);
                            P2Panel.SetActive(false);
                        //}
                        if (winner == P1)
                        {
                            if (Difficulty != GameDifficulty.Versus)
                            {
                                winnerText.color = new Color(0f / 255f, 225f / 255f, 0f / 255f);
                                winnerText.text = "You win!";
                            }
                            else
                            {
                                winnerText.color = new Color(0f / 255f, 225f / 255f, 0f / 255f);
                                //winnerText.text = "Player 1 wins!";
                                winnerText.text = LM.roomYourName.text + "\nwins!";
                                CM.WinnerAvatar(1);
                            }
                        }
                        else
                        {
                            if (Difficulty != GameDifficulty.Versus)
                            {
                                winnerText.color = new Color(225f / 255f, 0f / 255f, 0f / 255f);
                                winnerText.text = "You lose...";
                            }
                            else
                            {
                                //gameOverPanel.GetComponent<RectTransform>().Rotate(0, 0, 180);
                                winnerText.color = new Color(0f / 255f, 225f / 255f, 0f / 255f);
                                //winnerText.text = "Player 2 wins!";
                                winnerText.text = LM.roomOtherName.text + "\nwins!";
                                CM.WinnerAvatar(0);
                            }
                        }
                        //ResetPlayers();
                        P1.AnimateAfterDamage();
                        P2.AnimateAfterDamage();
                        if (Difficulty == GameDifficulty.Versus)
                        {
                            ChangeState(GameState.GameOver);
                        }
                        else
                        {
                            State = GameState.GameOver;
                        }
                    }
                }
                break;


            case GameState.Draw :
                if (P1.IsAnimating() == false && P2.IsAnimating() == false)
                {
                    ResetPlayers();
                    P1.IsClickable(true);
                    P2.IsClickable(true);
                    if (Difficulty == GameDifficulty.Versus)
                    {
                        ChangeState(GameState.ChooseAttack);
                    }
                    else
                    {
                        State = GameState.ChooseAttack;
                    }
                    //P1.ResetAllCards();
                    //P2.ResetAllCards();
                }
                break;

            case GameState.GameOver :
                if (P1.IsAnimating() == false && P2.IsAnimating() == false)
                {
                    ResetPlayers();
                }

                if (rematchAgreement >= 2)
                {
                    State = GameState.ChooseAttack;
                    NextState = GameState.ChooseAttack;
                    ResetMultiplayer();
                    P1Panel.SetActive(true);
                    P2Panel.SetActive(true);
                    P1.IsClickable(true);
                    P2.IsClickable(true);
                }
                break;
        }
    }

    void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private const byte playerChangeState = 1;

    private void ChangeState(GameState newState)
    {
        if (Difficulty == GameDifficulty.Versus)
        {
            if (!Online)
            {
                State = newState;
                return;
            }

            if (this.NextState == newState)
            {
                return;
            }

            // Send message that we're ready
            var actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
            var raiseEventOptions = new RaiseEventOptions();
            raiseEventOptions.Receivers = ReceiverGroup.All;
            PhotonNetwork.RaiseEvent(playerChangeState, actorNum, raiseEventOptions, SendOptions.SendReliable);
            if (PhotonNetwork.InRoom)
            {
                this.State = GameState.SyncState;
                this.NextState = newState;
            }

            //State = GameState.NetPlayersInitialization;
            //NextState = GameState.NetPlayersInitialization;
        }
    }

    public void ReplayGame()
    {
        Debug.Log("Clicked Replay");
        if (Difficulty == GameDifficulty.Versus)
        {
            if (!alreadyClickRematch)
            {
                // Send message that we want a rematch
                var actorNum = PhotonNetwork.LocalPlayer.ActorNumber;
                var raiseEventOptions = new RaiseEventOptions();
                raiseEventOptions.Receivers = ReceiverGroup.Others;
                PhotonNetwork.RaiseEvent(2, actorNum, raiseEventOptions, SendOptions.SendReliable);
                if (firstToAskRematch)
                {
                    rematchOtherText.text = "Waiting for " + LM.roomOtherName.text + "...";
                    rematchOtherText.gameObject.SetActive(true);
                    firstToAskRematch = false;
                }
                rematchAgreement++;
                alreadyClickRematch = true;
                Debug.Log(rematchAgreement);
            }
        }
        else
        {
            //LoadScene(0);
            ResetSingle();
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case playerChangeState:
                var actorNum = (int)photonEvent.CustomData;

                // HashSet = don't need to check
                //if (!syncReadyPlayers.Contains(actorNum))
                //{
                syncReadyPlayers.Add(actorNum);
                //}
                break;
            case 2:
                Debug.Log("Received");
                if (!alreadyReceiveRematch)
                {
                    firstToAskRematch = false;
                    rematchOtherText.text = LM.roomOtherName.text + " wants a rematch!";
                    rematchOtherText.GetComponent<ChallengeText>().Enable();
                    rematchOtherText.gameObject.SetActive(true);
                    rematchAgreement++;
                    alreadyReceiveRematch = true;
                    Debug.Log(rematchAgreement);
                }
                break;
            case 3:
                Debug.Log("Force quit match");
                Debug.Log("Relog for both players");
                //ResetMultiplayer();
                DontDestroy.instance.relogSequence = true;
                DontDestroy.instance.yourUsername = LM.roomYourName.text;
                //DontDestroy.instance.otherUsername = LM.roomOtherName.text;
                DontDestroy.instance.vsEndurance = "Medium";

                if (DontDestroy.instance.relogCauser)
                {
                    PhotonNetwork.Disconnect();
                }
                else
                {
                    OtherLeftPanel();
                }

                /*
                PhotonNetwork.LeaveRoom();
                LoadingPanel();
                */

                //PhotonNetwork.LeaveRoom();
                //GoToLobby();
                //lobbyPanel.SetActive(false);
                //ResetMultiplayer();
                //pvErrorPrevention.SetActive(true);
                break;
            case 4:
                //Card[] cards = LM.netPlayer.cards;
                //foreach (var card in cards)
                //{
                //    var button = card.GetComponent<Button>();
                //    button.onClick.RemoveListener(() => LM.netPlayer.RemoteClickButton(card.AttackValue));
                //    Debug.Log("Remove Listener");
                //}
                //LM.netPlayer = null;
                Debug.Log("Case 4");
                foreach (var netPlayer in CardNetPlayer.NetPlayers)
                {
                    if (netPlayer.photonView.IsMine)
                    {
                        netPlayer.Set(P1);
                    }
                    else
                    {
                        netPlayer.Set(P2);
                    }
                }
                break;
            default:
                break;
        }
    }

    private void ResetPlayers()
    {
        damagedPlayer = null;
        P1.Reset();
        P2.Reset();
    }

    private Player GetDamagedPlayer()
    {
        Attack? PlayerAtk1 = P1.AttackValue;
        Attack? PlayerAtk2 = P2.AttackValue;

        if (PlayerAtk1 == Attack.Rock && PlayerAtk2 == Attack.Paper)
        {
            return P1;
        }
        else if (PlayerAtk1 == Attack.Rock && PlayerAtk2 == Attack.Scissors)
        {
            return P2;
        }
        else if (PlayerAtk1 == Attack.Paper && PlayerAtk2 == Attack.Rock)
        {
            return P2;
        }
        else if (PlayerAtk1 == Attack.Paper && PlayerAtk2 == Attack.Scissors)
        {
            return P1;
        }
        else if (PlayerAtk1 == Attack.Scissors && PlayerAtk2 == Attack.Rock)
        {
            return P1;
        }
        else if (PlayerAtk1 == Attack.Scissors && PlayerAtk2 == Attack.Paper)
        {
            return P2;
        }

        return null;
    }

    private Player GetWinner()
    {
        if (Difficulty == GameDifficulty.Versus)
        {
            switch (vsEndurance)
            {
                case VersusEndurance.Low :
                    if (P1.health == 50)
                    {
                        return P2;
                    }
                    else if (P2.health == 50)
                    {
                        return P1;
                    }
                    else
                    {
                        return null;
                    }
                case VersusEndurance.Medium :
                    if (P1.health == 100)
                    {
                        return P2;
                    }
                    else if (P2.health == 100)
                    {
                        return P1;
                    }
                    else
                    {
                        return null;
                    }
                case VersusEndurance.High :
                    if (P1.health == 200)
                    {
                        return P2;
                    }
                    else if (P2.health == 200)
                    {
                        return P1;
                    }
                    else
                    {
                        return null;
                    }
                default:
                    return null;
            }
        }
        else
        {
            if (P1.health == 100)
            {
                return P2;
            }
            else if (P2.health == 100)
            {
                return P1;
            }
            else
            {
                return null;
            }
        }
    }

    public void LoadScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public void StartGame()
    {
        if (Difficulty == GameDifficulty.Versus && !versusConnected)
        {
            connectPanel.SetActive(true);
            menuPanel.SetActive(false);
        }
        else
        {
            if (Difficulty != GameDifficulty.Versus)
            {
                State = GameState.ChooseAttack;
                NextState = GameState.ChooseAttack;
            }
            bot.SetInterval();
            menuPanel.SetActive(false);
            connectPanel.SetActive(false);
            lobbyPanel.SetActive(false);
            roomPanel.SetActive(false);
            P1Panel.SetActive(true);
            P2Panel.SetActive(true);

            //if (Difficulty == GameDifficulty.Versus)
            //{
            //    P2Cards.GetComponent<RectTransform>().Rotate(0, 0, 180);
            //    P2HealthBar.GetComponent<RectTransform>().Rotate(0, 0, 180);
            //    P2HealthText.GetComponent<RectTransform>().Rotate(0, 0, 180);
            //}
            //else
            //{
            P2.DisableRaycastTarget();
            //}

            DD.audioSource.clip = DD.levelBGM;
            DD.audioSource.Play();

            RemoteConfigFetcher.fetch = true;
            RemoteConfigFetcher.gameplay = false;
            StartCoroutine(StandbyHealthConfig());
        }
    }

    public void SetToEasy()
    {
        Difficulty = GameDifficulty.Easy;
        DD.GameDifficulty = "Easy";
        easyButton.transform.localScale 
            = mediumButton.transform.localScale 
            = hardButton.transform.localScale 
            = versusButton.transform.localScale 
            = defaultButtonLocalScale;
        easyButton.transform.DOScale(easyButton.transform.localScale * 1.1f, 0.2f);
        easyButton.colors = easyColorYes;
        mediumButton.colors = mediumColorNo;
        hardButton.colors = hardColorNo;
        versusButton.colors = versusColorNo;
        ChangeGameOverAlpha(175);
        winnerAvatarObject.SetActive(false);
        winnerAvatarBG.SetActive(false);
        //replayText.text = "Restart";
    }

    public void SetToMedium()
    {
        Difficulty = GameDifficulty.Medium;
        DD.GameDifficulty = "Medium";
        easyButton.transform.localScale
            = mediumButton.transform.localScale
            = hardButton.transform.localScale
            = versusButton.transform.localScale
            = defaultButtonLocalScale;
        mediumButton.transform.DOScale(easyButton.transform.localScale * 1.1f, 0.2f);
        easyButton.colors = easyColorNo;
        mediumButton.colors = mediumColorYes;
        hardButton.colors = hardColorNo;
        versusButton.colors = versusColorNo;
        ChangeGameOverAlpha(175);
        winnerAvatarObject.SetActive(false);
        winnerAvatarBG.SetActive(false);
        //replayText.text = "Restart";
    }

    public void SetToHard()
    {
        Difficulty = GameDifficulty.Hard;
        DD.GameDifficulty = "Hard";
        easyButton.transform.localScale
            = mediumButton.transform.localScale
            = hardButton.transform.localScale
            = versusButton.transform.localScale
            = defaultButtonLocalScale;
        hardButton.transform.DOScale(easyButton.transform.localScale * 1.1f, 0.2f);
        easyButton.colors = easyColorNo;
        mediumButton.colors = mediumColorNo;
        hardButton.colors = hardColorYes;
        versusButton.colors = versusColorNo;
        ChangeGameOverAlpha(175);
        winnerAvatarObject.SetActive(false);
        winnerAvatarBG.SetActive(false);
        //replayText.text = "Restart";
    }

    public void SetToVersus()
    {
        Difficulty = GameDifficulty.Versus;
        DD.GameDifficulty = "Versus";
        easyButton.transform.localScale
            = mediumButton.transform.localScale
            = hardButton.transform.localScale
            = versusButton.transform.localScale
            = defaultButtonLocalScale;
        versusButton.transform.DOScale(easyButton.transform.localScale * 1.1f, 0.2f);
        easyButton.colors = easyColorNo;
        mediumButton.colors = mediumColorNo;
        hardButton.colors = hardColorNo;
        versusButton.colors = versusColorYes;
        ChangeGameOverAlpha(0);
        winnerAvatarObject.SetActive(true);
        winnerAvatarBG.SetActive(true);
        //replayText.text = "Rematch";
    }

    private void ChangeGameOverAlpha(int alpha)
    {
        Color gameOverAlpha = gameOverPanel.GetComponent<Image>().color;
        gameOverAlpha.a = alpha / 255;
        gameOverPanel.GetComponent<Image>().color = gameOverAlpha;
    }

    public void GoToMainMenu()
    {
        connectPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        roomPanel.SetActive(false);
        pvErrorPrevention.SetActive(false);
        loadingPanel.SetActive(false);
        menuPanel.SetActive(true);
        versusConnected = false;
    }

    public void GoToConnect()
    {
        if (DontDestroy.instance.yourUsername != "")
        {
            CM.usernameInput.text = DontDestroy.instance.yourUsername;
        }
        menuPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        roomPanel.SetActive(false);
        connectPanel.SetActive(true);
    }

    public void GoToLobby()
    {
        lobbyText.text = "Join a room or create a new one :";
        lobbyText.color = Color.white;
        menuPanel.SetActive(false);
        connectPanel.SetActive(false);
        roomPanel.SetActive(false);
        pvErrorPrevention.SetActive(false);
        loadingPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        LM.JoinLobby();
    }

    public void GoToRoom()
    {
        menuPanel.SetActive(false);
        connectPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        P1Panel.SetActive(false);
        P2Panel.SetActive(false);
        gameOverPanel.SetActive(false);
        roomPanel.SetActive(true);
    }

    public void ResetSingle()
    {
        State = GameState.ChooseAttack;
        NextState = GameState.ChooseAttack;
        gameOverPanel.SetActive(false);
        P1.ChangeHealth(-100);
        P2.ChangeHealth(-100);
        P1Panel.SetActive(true);
        P2Panel.SetActive(true);
        P1.IsClickable(true);
        P2.IsClickable(true);
    }

    public void ResetMultiplayer()
    {
        gameOverPanel.SetActive(false);
        P1.ChangeHealth(-100);
        P2.ChangeHealth(-100);
        firstToAskRematch = true;
        rematchAgreement = 0;
        alreadyClickRematch = false;
        alreadyReceiveRematch = false;
        rematchOtherText.GetComponent<ChallengeText>().Disable();
        rematchOtherText.gameObject.SetActive(false);
    }

    public void ClickErrorPrevention()
    {
        //DontDestroy.instance.relogSequence = false;
        //GoToLobby();
        PhotonNetwork.Disconnect();
    }

    public void LoadingPanel()
    {
        menuPanel.SetActive(false);
        connectPanel.SetActive(false);
        lobbyPanel.SetActive(false);
        roomPanel.SetActive(false);
        loadingPanel.SetActive(true);
    }

    public void OtherLeftPanel()
    {
        Debug.Log("Relog: OtherLeftPanel");
        otherLeftRoomText.text = DontDestroy.instance.otherUsername + "\nhas left the room.";
        loadingPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        if (DontDestroy.instance.relogCauser)
        {
            GoToLobby();
        }
        else
        {
            PhotonNetwork.LeaveRoom();
            roomPanel.SetActive(false);
            pvErrorPrevention.SetActive(true);
        }
    }

    //public void SpecialRelog()
    //{
    //    connectPanel.SetActive(false);
    //    lobbyPanel.SetActive(false);
    //    roomPanel.SetActive(false);
    //    pvErrorPrevention.SetActive(false);
    //    menuPanel.SetActive(false);
    //    loadingPanel.SetActive(true);

    //    //PhotonNetwork.LeaveRoom();
    //    //GoToLobby();
    //    //lobbyPanel.SetActive(false);
    //    //ResetMultiplayer();
    //    //pvErrorPrevention.SetActive(true);
    //}

    public void EnduranceLow()
    {
        vsEndurance = VersusEndurance.Low;
        DD.vsEndurance = "Low";
        enduranceLow.transform.localScale
            = enduranceMedium.transform.localScale
            = enduranceHigh.transform.localScale
            = defaultButtonLocalScale;
        enduranceLow.transform.DOScale(enduranceLow.transform.localScale * 1.1f, 0.2f);
        enduranceLow.colors = easyColorYes;
        enduranceMedium.colors = mediumColorNo;
        enduranceHigh.colors = hardColorNo;
        P1.maxHealth = 50;
        P2.maxHealth = 50;

        if (PhotonNetwork.IsMasterClient)
        {
            LM.netPlayer.RemoteEndurance(1);
        }
    }

    public void EnduranceMedium()
    {
        vsEndurance = VersusEndurance.Medium;
        DD.vsEndurance = "Medium";
        enduranceLow.transform.localScale
            = enduranceMedium.transform.localScale
            = enduranceHigh.transform.localScale
            = defaultButtonLocalScale;
        enduranceMedium.transform.DOScale(enduranceMedium.transform.localScale * 1.1f, 0.2f);
        enduranceLow.colors = easyColorNo;
        enduranceMedium.colors = mediumColorYes;
        enduranceHigh.colors = hardColorNo;
        P1.maxHealth = 100;
        P2.maxHealth = 100;

        if (PhotonNetwork.IsMasterClient)
        {
            LM.netPlayer.RemoteEndurance(2);
        }
    }

    public void EnduranceHigh()
    {
        vsEndurance = VersusEndurance.High;
        DD.vsEndurance = "High";
        enduranceLow.transform.localScale
            = enduranceMedium.transform.localScale
            = enduranceHigh.transform.localScale
            = defaultButtonLocalScale;
        enduranceHigh.transform.DOScale(enduranceHigh.transform.localScale * 1.1f, 0.2f);
        enduranceLow.colors = easyColorNo;
        enduranceMedium.colors = mediumColorNo;
        enduranceHigh.colors = hardColorYes;
        P1.maxHealth = 200;
        P2.maxHealth = 200;

        if (PhotonNetwork.IsMasterClient)
        {
            LM.netPlayer.RemoteEndurance(3);
        }
    }

    public void DisableEnduranceButtons()
    {
        enduranceBlocker.SetActive(true);
    }

    public void EnableEnduranceButtons()
    {
        enduranceBlocker.SetActive(false);
    }

    public void SetHealthConfig (float _playerDamage, float _playerHeal, float _enemyDamage, float _enemyHeal)
    {
        playerDamage = _playerDamage;
        playerHeal = _playerHeal;
        enemyDamage = _enemyDamage;
        enemyHeal = _enemyHeal;
    }

    IEnumerator StandbyHealthConfig()
    {
        yield return new WaitForSeconds(1);
        RemoteConfigFetcher.gameplay = true;
    }
}
