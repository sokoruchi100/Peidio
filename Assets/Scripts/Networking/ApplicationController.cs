using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ApplicationController : MonoBehaviour
{
    [SerializeField] private ClientSingleton clientPrefab;
    [SerializeField] private HostSingleton hostPrefab;

    private async void Start() {
        DontDestroyOnLoad(gameObject);

        await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);//Dedicated servers dont have rendering
    }

    private async Task LaunchInMode(bool isDedicatedServer) {
        if (isDedicatedServer) {//Server Logic

        } else {//Client Logic
            HostSingleton hostSingleton = Instantiate(hostPrefab);
            hostSingleton.CreateHost();

            ClientSingleton clientSingleton = Instantiate(clientPrefab);
            bool authenticated = await clientSingleton.CreateClient();

            //Go to mainmenu
            if (authenticated) {
                clientSingleton.ClientGameManager.GoToMenu();
            }
        }
    }
}
