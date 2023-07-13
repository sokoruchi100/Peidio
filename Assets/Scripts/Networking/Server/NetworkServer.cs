using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkServer : IDisposable {
    private NetworkManager networkManager;
    private Dictionary<ulong, string> clientIdToAuth;
    private Dictionary<string, UserData> authIdToUserData;

    public NetworkServer(NetworkManager networkManager) { 
        this.networkManager = networkManager;
        clientIdToAuth = new Dictionary<ulong, string>();
        authIdToUserData = new Dictionary<string, UserData>();
        networkManager.ConnectionApprovalCallback += ApprovalCheck;
        networkManager.OnServerStarted += NetworkManager_OnServerStarted;
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) {
        string payload = System.Text.Encoding.UTF8.GetString(request.Payload);
        UserData userData = JsonUtility.FromJson<UserData>(payload);

        clientIdToAuth[request.ClientNetworkId] = userData.userAuthId;
        authIdToUserData[userData.userAuthId] = userData;

        response.Approved = true;
        response.CreatePlayerObject = true;
    }

    private void NetworkManager_OnServerStarted() {
        networkManager.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId) {
        if (clientIdToAuth.TryGetValue(clientId, out string authId)) {
            clientIdToAuth.Remove(clientId);
            authIdToUserData.Remove(authId);
        }
    }

    public void Dispose() {
        if (networkManager != null) {
            networkManager.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
            networkManager.OnServerStarted -= NetworkManager_OnServerStarted;
            networkManager.ConnectionApprovalCallback -= ApprovalCheck;
        }

        if (networkManager.IsListening) { 
            networkManager.Shutdown();
        }
    }
}