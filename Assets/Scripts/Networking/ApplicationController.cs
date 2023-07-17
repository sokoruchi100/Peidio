using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ApplicationController : MonoBehaviour
{
    private const string GAME_SCENE_NAME = "Game";

    [SerializeField] private ClientSingleton clientPrefab;
    [SerializeField] private HostSingleton hostPrefab;
    [SerializeField] private ServerSingleton serverPrefab;
    [SerializeField] private NetworkObject playerPrefab;

    private ApplicationData appData;

    private async void Start() {
        DontDestroyOnLoad(gameObject);

        await LaunchInMode(SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null);//Dedicated servers dont have rendering
    }

    private async Task LaunchInMode(bool isDedicatedServer) {
        if (isDedicatedServer) {//Server Logic
            Application.targetFrameRate = 60;

            appData = new ApplicationData();

            ServerSingleton serverSingleton = Instantiate(serverPrefab);

            StartCoroutine(LoadGameSceneAsync(serverSingleton));
        } else {//Client Logic
            HostSingleton hostSingleton = Instantiate(hostPrefab);
            hostSingleton.CreateHost(playerPrefab);

            ClientSingleton clientSingleton = Instantiate(clientPrefab);
            bool authenticated = await clientSingleton.CreateClient();

            //Go to mainmenu
            if (authenticated) {
                clientSingleton.ClientGameManager.GoToMenu();
            }
        }
    }

    private IEnumerator LoadGameSceneAsync(ServerSingleton serverSingleton) {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(GAME_SCENE_NAME);

        while (!asyncOperation.isDone) {
            yield return null;
        }

        Task createServerTask = serverSingleton.CreateServer(playerPrefab);
        yield return new WaitUntil(() => createServerTask.IsCompleted);

        Task startServerTask = serverSingleton.ServerGameManager.StartGameServerAsync();
        yield return new WaitUntil(() => startServerTask.IsCompleted);
    }
}
