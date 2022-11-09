using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerItem : MonoBehaviour
{
    [SerializeField] TMP_Text playerName;

    public void Set(Photon.Realtime.Player player)
    {
        playerName.text = player.NickName;

        if (PhotonNetwork.IsMasterClient)
        {
            playerName.text += " (Master)";
        }
    }
}
