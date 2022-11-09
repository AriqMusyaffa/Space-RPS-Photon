using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonText : MonoBehaviour
{
    TMP_Text text;
    LobbyManager manager;
    Transform content;

    void Start()
    {
        text = transform.GetChild(0).GetComponent<TMP_Text>();
        manager = GameObject.FindWithTag("LobbyManager").GetComponent<LobbyManager>();
        content = transform.parent;
        Unselected();
    }

    public void Selected()
    {
        foreach (Transform child in content)
        {
            var childText = child.GetComponent<ButtonText>();

            if (childText != this)
            {
                childText.Unselected();
            }
        }
        text.color = Color.yellow;
        manager.anyRoomSelected++;
    }

    public void Unselected()
    {
        if (text.color == Color.yellow)
        {
            text.color = Color.white;
            manager.anyRoomSelected--;
        }
    }
}
