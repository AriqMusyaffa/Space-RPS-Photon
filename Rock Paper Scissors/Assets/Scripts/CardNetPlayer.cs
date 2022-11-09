using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class CardNetPlayer : MonoBehaviourPun
{
    public static List<CardNetPlayer> NetPlayers = new List<CardNetPlayer>(2);
    private Player player;
    public Card[] cards;
    GameManager GM;
    LobbyManager LM;
    ConnectManager CM;
    bool avatarSent;

    void Start()
    {
        GM = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
        LM = GameObject.FindWithTag("LobbyManager").GetComponent<LobbyManager>();
        CM = GameObject.FindWithTag("ConnectManager").GetComponent<ConnectManager>();
        avatarSent = false;
    }

    void Update()
    {
        if (GM.sendAvatar && !avatarSent)
        {
            RemoteAssignAvatar(DontDestroy.instance.avatarNum);
            avatarSent = true;
        }
    }

    public void Set(Player p)
    {
        player = p;
        cards = p.GetComponentsInChildren<Card>();

        foreach (var card in cards)
        {
            var button = card.GetComponent<Button>();
            button.onClick.AddListener(() => RemoteClickButton(card.AttackValue));
            //Debug.Log("Add Listener");
        }
    }

    void OnDestroy()
    {
        if (!DontDestroy.instance.relogSequence)
        {
            foreach (var card in cards)
            {
                var button = card.GetComponent<Button>();
                button.onClick.RemoveListener(() => RemoteClickButton(card.AttackValue));
            }
        }
    }

    public void RemoteClickButton(Attack value)
    {
        Debug.Log("Attempt : Send chosen to other");
        if (GM.State == GameManager.GameState.ChooseAttack)
        {
            if (photonView.IsMine)
            {
                photonView.RPC("RemoteClickButtonRPC", RpcTarget.Others, (int)value);
                if (!GM.turnTaken)
                {
                    GM.turnsText.text = "Waiting for " + LM.roomOtherName.text + "'s move...";
                    GM.turnsText.GetComponent<ChallengeText>().Disable();
                    GM.turnsText.gameObject.SetActive(true);
                    GM.turnTaken = true;
                }
                Debug.Log("Success : Send chosen to other");
            }
        }
    }

    [PunRPC]
    private void RemoteClickButtonRPC(int value)
    {
        Debug.Log("Attempt : Other has chosen");
        if (GM.State == GameManager.GameState.ChooseAttack)
        {
            foreach (var card in cards)
            {
                if (card.AttackValue == (Attack)value)
                {
                    var button = card.GetComponent<Button>();
                    button.onClick.Invoke();
                    if (!GM.turnTaken)
                    {
                        GM.turnsText.text = LM.roomOtherName.text + " has chosen a move.";
                        GM.turnsText.GetComponent<ChallengeText>().Enable();
                        GM.turnsText.gameObject.SetActive(true);
                        GM.turnTaken = true;
                    }
                    Debug.Log("Success : Other has chosen");
                    break;
                }
            }
        }
    }

    public void RemoteAssignAvatar(int a)
    {
        Debug.Log("Attempt : Send avatar to other");
        if (photonView.IsMine)
        {
            photonView.RPC("RemoteAssignAvatarRPC", RpcTarget.Others, (int)a);
            Debug.Log("Success : Send avatar to other");
        }
    }

    [PunRPC]
    private void RemoteAssignAvatarRPC(int a)
    {
        Debug.Log("Attempt : Receive avatar from other");
        DontDestroy.instance.otherAvatarNum = a;
        CM.SetRoomOtherAvatar();
        Debug.Log("Success : Receive avatar from other");
    }

    public void RemoteEndurance(int e)
    {
        Debug.Log("Attempt : Send endurance to other");
        if (photonView.IsMine)
        {
            photonView.RPC("RemoteEnduranceRPC", RpcTarget.Others, (int)e);
            Debug.Log("Success : Send endurance to other");
        }
    }

    [PunRPC]
    private void RemoteEnduranceRPC(int e)
    {
        Debug.Log("Attempt : Send endurance to other");
        switch (e)
        {
            case 1:
                GM.EnduranceLow();
                break;
            case 2:
                GM.EnduranceMedium();
                break;
            case 3:
                GM.EnduranceHigh();
                break;
        }
        Debug.Log("Success : Send endurance to other");
    }

    public void ClickStartGame_2()
    {
        //PhotonNetwork.LoadLevel(levelName);
        photonView.RPC("RemoteStartGame", RpcTarget.All);
    }

    [PunRPC]
    private void RemoteStartGame()
    {
        GM.State = GameManager.GameState.ChooseAttack;
        GM.StartGame();
    }

    void OnEnable()
    {
        NetPlayers.Add(this);
    }

    void OnDisable()
    {
        NetPlayers.Remove(this);
    }
}
