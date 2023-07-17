using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkServer : IDisposable {
    private NetworkManager networkManager;
    private NetworkObject playerPrefab;
    private Dictionary<ulong, string> clientIdToAuth = new Dictionary<ulong, string>();
    private Dictionary<string, UserData> authIdToUserData = new Dictionary<string, UserData>();

    public Action<string> OnClientLeft;
    public Action<UserData> OnUserJoined;
    public Action<UserData> OnUserLeft;

    public NetworkServer(NetworkManager networkManager, NetworkObject playerPrefab) { 
        this.networkManager = networkManager;
        this.playerPrefab = playerPrefab;

        networkManager.ConnectionApprovalCallback += ApprovalCheck;
        networkManager.OnServerStarted += NetworkManager_OnServerStarted;
    }

    public bool OpenConnection(string ip, int port) {
        UnityTransport transport = networkManager.gameObject.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, (ushort) port);
        return networkManager.StartServer();
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) {
        string payload = System.Text.Encoding.UTF8.GetString(request.Payload);
        UserData userData = JsonUtility.FromJson<UserData>(payload);

        clientIdToAuth[request.ClientNetworkId] = userData.userAuthId;
        authIdToUserData[userData.userAuthId] = userData;
        OnUserJoined?.Invoke(userData);

        _ = SpawnPlayerDelayed(request.ClientNetworkId);

        response.Approved = true;
        response.CreatePlayerObject = false;
    }

    private async Task SpawnPlayerDelayed(ulong clientId) {
        await Task.Delay(1000);

        NetworkObject playerInstance = GameObject.Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPos(), Quaternion.identity);

        playerInstance.SpawnAsPlayerObject(clientId);
    }

    private void NetworkManager_OnServerStarted() {
        networkManager.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

    }

    public UserData GetUserDataByClientId(ulong clientNetworkId) {
        if (clientIdToAuth.TryGetValue(clientNetworkId, out string authId)) {
            if (authIdToUserData.TryGetValue(authId, out UserData data)) {
                return data;
            }
        }
        return null;
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId) {
        if (clientIdToAuth.TryGetValue(clientId, out string authId)) {
            clientIdToAuth.Remove(clientId);
            OnUserLeft?.Invoke(authIdToUserData[authId]);
            authIdToUserData.Remove(authId);
            OnClientLeft?.Invoke(authId);
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
