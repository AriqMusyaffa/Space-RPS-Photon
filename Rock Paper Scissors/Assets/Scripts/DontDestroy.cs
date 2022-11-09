using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    public static DontDestroy instance;

    public string GameDifficulty = "Medium";
    public AudioSource audioSource;
    public AudioClip menuBGM, levelBGM;
    public bool relogSequence = false;
    public bool relogCauser = false;
    public string yourUsername = "";
    public string otherUsername = "";
    public int avatarNum = 1;
    public int otherAvatarNum = 1;
    public string vsEndurance = "Medium";
    public bool audioOn = true;

    public void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }

        DontDestroyOnLoad(gameObject);
    }
}
