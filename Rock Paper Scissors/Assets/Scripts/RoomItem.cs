using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;
using UnityEngine.UI;

public class RoomItem : MonoBehaviour
{
    [SerializeField] TMP_Text roomNameText;
    LobbyManager manager;
    RoomInfo roomInfo;
    Image img;
    [SerializeField] Sprite gradientBlue, gradientRed;

    void Start()
    {
        img = GetComponent<Image>();
    }

    void Update()
    {
        if (roomInfo != null)
        {
            if (roomInfo.PlayerCount < 2)
            {
                img.sprite = gradientBlue;
            }
            else
            {
                img.sprite = gradientRed;
            }
        }
    }

    public void Set(LobbyManager manager, RoomInfo roomInfo)
    {
        this.manager = manager;
        this.roomInfo = roomInfo;
        roomNameText.text = roomInfo.Name;
        //roomNameText.text = roomInfo.Name + $"({roomInfo.PlayerCount}/{roomInfo.MaxPlayers})";
        
        //if (!roomInfo.IsOpen)
        //{
        //    GetComponent<Button>().interactable = false;
        //}
    }

    public void ClickRoomName()
    {
        //manager.SelectedRoomName(roomNameText.text);
        manager.SelectedRoomName(this.roomInfo.Name);
    }
}
