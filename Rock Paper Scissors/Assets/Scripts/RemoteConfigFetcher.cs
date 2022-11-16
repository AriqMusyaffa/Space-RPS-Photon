using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.RemoteConfig;
using Unity.Services.Core.Environments;
using Unity.Services.Authentication;

public struct userAttributes
{
    public string modeDifficulty;
}

public struct appAttributes
{

}

public class RemoteConfigFetcher : MonoBehaviour
{
    [SerializeField] string environmentName;
    [SerializeField] string modeDifficulty;
    public static bool fetch;
    [SerializeField] float playerDamage, playerHeal, enemyDamage, enemyHeal;
    [SerializeField] GameManager GM;
    public static bool gameplay;

    async void Awake()
    {
        var options = new InitializationOptions();
        options.SetEnvironmentName(environmentName);
        await UnityServices.InitializeAsync(options);

        Debug.Log("UGS Initialized");

        if (AuthenticationService.Instance.IsSignedIn == false)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        Debug.Log("Player signed in");

        RemoteConfigService.Instance.FetchCompleted += OnFetchConfig;
    }

    private void OnDestroy()
    {
        RemoteConfigService.Instance.FetchCompleted -= OnFetchConfig;
    }

    private void OnFetchConfig(ConfigResponse response)
    {
        Debug.Log(response.requestOrigin);
        Debug.Log(response.body);

        switch (response.requestOrigin)
        {
            case ConfigOrigin.Default:
                Debug.Log("Default");
                break;
            case ConfigOrigin.Cached:
                Debug.Log("Cached");
                break;
            case ConfigOrigin.Remote:
                Debug.Log("Remote");

                playerDamage = RemoteConfigService.Instance.appConfig.GetFloat("PlayerDamage");
                playerHeal = RemoteConfigService.Instance.appConfig.GetFloat("PlayerHeal");
                enemyDamage = RemoteConfigService.Instance.appConfig.GetFloat("EnemyDamage");
                enemyHeal = RemoteConfigService.Instance.appConfig.GetFloat("EnemyHeal");

                GM.SetHealthConfig(playerDamage, playerHeal, enemyDamage, enemyHeal);
                break;
        }
    }

    void Update()
    {
        if (fetch)
        {
            fetch = false;
            Debug.Log("Fetch config");

            switch (GM.Difficulty)
            {
                case GameManager.GameDifficulty.Easy :
                    modeDifficulty = "Easy";
                    break;
                case GameManager.GameDifficulty.Medium:
                    modeDifficulty = "Medium";
                    break;
                case GameManager.GameDifficulty.Hard:
                    modeDifficulty = "Hard";
                    break;
                case GameManager.GameDifficulty.Versus:
                    modeDifficulty = "Versus";
                    break;
            }

            RemoteConfigService.Instance.FetchConfigs
            (
                new userAttributes()
                {
                    modeDifficulty = modeDifficulty
                },

                new appAttributes()
            );
        }
    }
}
