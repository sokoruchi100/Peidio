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
    private MatchplayMatchmaker matchmaker;
    private UserData userData;

    public async Task<bool> InitAsync() { //For testing client authentication
        await UnityServices.InitializeAsync();

        networkClient = new NetworkClient(NetworkManager.Singleton);
        matchmaker = new MatchplayMatchmaker();

        AuthState authState = await AuthenticationWrapper.DoAuth(); //Waits until auth finished

        if (authState == AuthState.Authenticated) {
            userData = new UserData {
                userName = PlayerPrefs.GetString(NameSelector.PLAYER_NAME_KEY, "Missing Name"),
                userAuthId = AuthenticationService.Instance.PlayerId
            };

            return true;
        }

        return false;
    }

    public void GoToMenu() {
        SceneManager.LoadScene(MENU_SCENE_NAME);
    }

    public async Task StartClientAsync(string joinCode) {//For connecting to host lobby
        try {
            allocation = await Relay.Instance.JoinAllocationAsync(joinCode);
        } catch (Exception e) {
            Debug.Log(e);
            return;
        }

        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
        transport.SetRelayServerData(relayServerData);

        ConnectClient();
    }

    public void StartClient(string ip, int port) {// For connecting to dedicated server
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ip, (ushort) port);

        ConnectClient();
    }

    private void ConnectClient() {//For connecting clients to the network
        string payload = JsonUtility.ToJson(userData);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        NetworkManager.Singleton.StartClient();
    }

    public async void MatchmakeAsync(bool isTeamQueue, Action<MatchmakerPollingResult> onMatchmakeResponse) {
        if (matchmaker.IsMatchmaking) {
            return;
        }

        userData.userGamePreferences.gameQueue = isTeamQueue ? GameQueue.Team : GameQueue.Solo;
        MatchmakerPollingResult matchResult = await GetMatchAsync();
        onMatchmakeResponse?.Invoke(matchResult);
    }

    private async Task<MatchmakerPollingResult> GetMatchAsync() {
        MatchmakingResult matchmakingResult = await matchmaker.Matchmake(userData);

        if (matchmakingResult.result == MatchmakerPollingResult.Success) {
            //Connect to Server
            StartClient(matchmakingResult.ip, matchmakingResult.port);
        }

        return matchmakingResult.result;
    }

    public async Task CancelMatchmaking() {
        await matchmaker.CancelMatchmaking();
    }

    public void Disconnect() {
        networkClient.Disconnect();
    }

    public void Dispose() {
        networkClient?.Dispose();
    }
}
