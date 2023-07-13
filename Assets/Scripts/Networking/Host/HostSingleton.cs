using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class HostSingleton : MonoBehaviour
{
    public static HostSingleton instance;
    public static HostSingleton Instance {
        get {
            if (instance != null) { return instance; }

            instance = FindObjectOfType<HostSingleton>();

            if (instance == null) {
                Debug.LogError("No HostSingleton in the scene!");
                return null;
            }

            return instance;
        }
    }

    public HostGameManager HostGameManager { get; private set; }


    private void Start() {
        DontDestroyOnLoad(gameObject);
    }

    public void CreateHost() {
        HostGameManager = new HostGameManager();
    }

    private void OnDestroy() {
        HostGameManager?.Dispose();
    }
}
