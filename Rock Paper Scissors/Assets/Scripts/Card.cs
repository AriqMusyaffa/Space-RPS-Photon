using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public Attack AttackValue;
    public Player player;
    public Vector2 originalPosition;
    Vector2 originalScale;
    Color originalColor;
    public bool isClickable = true;
    GameManager GM;

    void Start()
    {
        originalPosition = transform.position;
        originalScale = transform.localScale;
        originalColor = GetComponent<Image>().color;

        if (player.GetComponent<Bot>() != null)
        {
            if (player.GetComponent<Bot>().enabled)
            {
                isClickable = false;
            }
        }

        GM = GameObject.FindWithTag("GameManager").GetComponent<GameManager>();
    }

    public void OnClick()
    {
        if (isClickable && GM.State == GameManager.GameState.ChooseAttack && RemoteConfigFetcher.gameplay)
        {
            player.SetChosenCard(this);
            player.ResetAllCards();
        }
    }

    internal void Reset()
    {
        transform.position = originalPosition;
        transform.localScale = originalScale;
        GetComponent<Image>().color = originalColor;
    }

    public void SetClickable(bool value)
    {
        isClickable = value;
    }
}
