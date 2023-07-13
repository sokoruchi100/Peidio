using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;
using Unity.Services.Authentication;

public class ClientGameManager : IDisposable {
    private const string MENU_SCENE_NAME = "Menu";

    private JoinAllocation allocation;
    private NetworkClient networkClient;

    public async Task<bool> InitAsync() { //For testing client authentication
        await UnityServices.InitializeAsync();

        networkClient = new NetworkClient(NetworkManager.Singleton);

        AuthState authState = await AuthenticationWrapper.DoAuth(); //Waits until auth finished

        if (authState == AuthState.Authenticated) {
            return true;
        }

        return false;
    }

    public void GoToMenu() {
        SceneManager.LoadScene(MENU_SCENE_NAME);
    }

    public async Task StartClientAsync(string joinCode) {
        try {
            allocation = await Relay.Instance.JoinAllocationAsync(joinCode);
        } catch (Exception e) {
            Debug.Log(e);
            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
        transport.SetRelayServerData(relayServerData);

        UserData userData = new UserData { 
            userName = PlayerPrefs.GetString(NameSelector.PLAYER_NAME_KEY, "Missing Name"),
            userAuthId = AuthenticationService.Instance.PlayerId
        };
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        NetworkManager.Singleton.StartClient();
    }

    public void Dispose() {
        networkClient?.Dispose();
    }
}
