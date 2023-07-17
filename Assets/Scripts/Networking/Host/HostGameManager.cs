using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;
using Unity.Services.Authentication;

public class HostGameManager : IDisposable {
    private const int MAX_CONNECTIONS = 20;
    private const string GAME_SCENE_NAME = "Game";

    public NetworkServer NetworkServer { get; private set; }
    public string JoinCode { get; private set; }

    private Allocation allocation;
    private string lobbyId;
    private NetworkObject playerPrefab;

    public HostGameManager(NetworkObject playerPrefab) { 
        this.playerPrefab = playerPrefab;
    }

    public async Task StartHostAsync(bool isPrivate) {//Creates relay data for the host and stores it in the transport to start the host        
        try {
            allocation = await Relay.Instance.CreateAllocationAsync(MAX_CONNECTIONS);
        } catch (Exception e) {
            Debug.Log(e);
            return;
        }

        try {
            JoinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(JoinCode);
        } catch (Exception e) {
            Debug.Log(e);
            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
        transport.SetRelayServerData(relayServerData);

        try {
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
            lobbyOptions.IsPrivate = isPrivate;
            lobbyOptions.Data = new Dictionary<string, DataObject> {
                {
                    "JoinCode",
                    new DataObject(
                        visibility: DataObject.VisibilityOptions.Member,
                        value: JoinCode
                    )
                }
            };

            string playerName = PlayerPrefs.GetString(NameSelector.PLAYER_NAME_KEY, "Unknown");
            Lobby lobby = await Lobbies.Instance.CreateLobbyAsync($"{playerName}'s Lobby", MAX_CONNECTIONS, lobbyOptions);//Lobby name and max players and options set above
            lobbyId = lobby.Id;

            HostSingleton.Instance.StartCoroutine(HeartbeatLobby(15));
        } catch (LobbyServiceException e) {
            Debug.Log(e);
            return;
        }

        NetworkServer = new NetworkServer(NetworkManager.Singleton, playerPrefab);

        UserData userData = new UserData {
            userName = PlayerPrefs.GetString(NameSelector.PLAYER_NAME_KEY, "Missing Name"),
            userAuthId = AuthenticationService.Instance.PlayerId
        };
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        NetworkManager.Singleton.StartHost();

        NetworkServer.OnClientLeft += HandleClientLeft;

        NetworkManager.Singleton.SceneManager.LoadScene(GAME_SCENE_NAME, LoadSceneMode.Single);
    }

    private IEnumerator HeartbeatLobby(float waitTimeSeconds) { 
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(waitTimeSeconds);
        while(true) {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }
    public void Dispose() {
        ShutDown();
    }

    public async void ShutDown() {
        if (string.IsNullOrEmpty(lobbyId)) { return; }

        HostSingleton.Instance.StopCoroutine(nameof(HeartbeatLobby));

        try {
            await Lobbies.Instance.DeleteLobbyAsync(lobbyId);
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }

        lobbyId = string.Empty;

        NetworkServer.OnClientLeft -= HandleClientLeft;

        NetworkServer?.Dispose();
    }

    private void HandleClientLeft(string authId) {
        try {
            LobbyService.Instance.RemovePlayerAsync(lobbyId, authId);
        } catch (LobbyServiceException e) {
            Debug.Log(e);
        }
    }
}
