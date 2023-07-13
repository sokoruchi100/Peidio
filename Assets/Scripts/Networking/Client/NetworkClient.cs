using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkClient : IDisposable {
    private const string MENU_SCENE_NAME = "Menu";

    private NetworkManager networkManager;

    public NetworkClient(NetworkManager networkManager) {
        this.networkManager = networkManager;

        networkManager.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback1;
    }

    private void NetworkManager_OnClientDisconnectCallback1(ulong clientId) {
        if (clientId != 0 && clientId != networkManager.LocalClientId) { return; }

        if (SceneManager.GetActiveScene().name != MENU_SCENE_NAME) {
            SceneManager.LoadScene(MENU_SCENE_NAME);
        }

        if (networkManager.IsConnectedClient) { 
            networkManager.Shutdown();
        }
    }

    public void Dispose() {
        if (networkManager != null) {
            networkManager.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback1;
        }
    }
}
