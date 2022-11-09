using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChallengeText : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    public bool isEnabled;
    float timer;
    bool toggle;
    [SerializeField] bool isRed;
    [SerializeField] bool isYellow;

    void Update()
    {
        if (isEnabled)
        {
            timer += Time.deltaTime;

            if (timer > 0.5f)
            {
                if (toggle)
                {
                    timer = 0;
                    toggle = false;
                    text.color = Color.red;
                }
                else
                {
                    timer = 0;
                    toggle = true;
                    text.color = Color.yellow;
                }
            }
        }
        else
        {
            if (isRed)
            {
                text.color = Color.red;
            }
            else if (isYellow)
            {
                text.color = Color.yellow;
            }
            else
            {
                text.color = Color.white;
            }
        }
    }

    public void Enable()
    {
        if (!isEnabled)
        {
            isEnabled = true;
            if (text.color != Color.red)
            {
                text.color = Color.red;
            }
        }
    }

    public void Disable()
    {
        if (isEnabled)
        {
            isEnabled = false;
        }
    }
}
