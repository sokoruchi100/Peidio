using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class HostSingleton : MonoBehaviour
{
    public static HostSingleton instance;
    public static HostSingleton Instance {
        get {
            if (instance != null) { return instance; }

            instance = FindObjectOfType<HostSingleton>();

            if (instance == null) {
                return null;
            }

            return instance;
        }
    }

    public HostGameManager HostGameManager { get; private set; }


    private void Start() {
        DontDestroyOnLoad(gameObject);
    }

    public void CreateHost(NetworkObject playerPrefab) {
        HostGameManager = new HostGameManager(playerPrefab);
    }

    private void OnDestroy() {
        HostGameManager?.Dispose();
    }
}
