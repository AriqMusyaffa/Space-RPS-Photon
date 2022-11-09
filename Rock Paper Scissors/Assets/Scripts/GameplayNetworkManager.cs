using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class GameplayNetworkManager : MonoBehaviourPunCallbacks
{
    public void BackToMenu()
    {
        StartCoroutine(BackToMenuCR());
    }

    IEnumerator BackToMenuCR()
    {
        PhotonNetwork.Disconnect();
        while (PhotonNetwork.IsConnected)
        {
            yield return null;
        }
        SceneManager.LoadScene("MainMenu");
    }

    public void BackToLobby()
    {
        StartCoroutine(BackToLobbyCR());
    }

    IEnumerator BackToLobbyCR()
    {
        PhotonNetwork.LeaveRoom();
        while (PhotonNetwork.InRoom || !PhotonNetwork.IsConnectedAndReady)
        {
            yield return null;
        }
        SceneManager.LoadScene("Lobby");
    }

    public void Replay()
    {
        var scene = SceneManager.GetActiveScene();
        PhotonNetwork.LoadLevel(scene.name);
    }

    public void Quit()
    {
        StartCoroutine(QuitCR());
    }

    IEnumerator QuitCR()
    {
        PhotonNetwork.Disconnect();
        while (PhotonNetwork.IsConnected)
        {
            yield return null;
        }
        Application.Quit();
    }

    //public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    //{
    //    if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
    //    {
    //        PhotonNetwork.CurrentRoom.IsVisible = false;
    //        PhotonNetwork.CurrentRoom.IsOpen = false;
    //        BackToLobby();
    //    }
    //}
}
