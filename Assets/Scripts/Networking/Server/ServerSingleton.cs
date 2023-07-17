using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Core;
using UnityEngine;

public class ServerSingleton : MonoBehaviour
{
    public static ServerSingleton instance;
    public static ServerSingleton Instance {
        get {
            if (instance != null) { return instance; }

            instance = FindObjectOfType<ServerSingleton>();

            if (instance == null) {
                Debug.LogError("No ServerSingleton in the scene!");
                return null;
            }

            return instance;
        }
    }

    public ServerGameManager ServerGameManager { get; private set; }


    private void Start() {
        DontDestroyOnLoad(gameObject);
    }

    public async Task CreateServer(NetworkObject playerPrefab) {
        await UnityServices.InitializeAsync();

        ServerGameManager = new ServerGameManager(
                ApplicationData.IP(),
                ApplicationData.Port(),
                ApplicationData.QPort(),
                NetworkManager.Singleton,
                playerPrefab
        );
    }

    private void OnDestroy() {
        ServerGameManager?.Dispose();
    }
}
