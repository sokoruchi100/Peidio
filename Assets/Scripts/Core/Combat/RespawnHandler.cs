using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RespawnHandler : NetworkBehaviour
{
    [SerializeField] private TankPlayer playerPrefab;
    [SerializeField] private float keptCoinPercentage;

    public override void OnNetworkSpawn() {//Spawns tanks once server respawn handler exists
        if (!IsServer) { return; }

        TankPlayer[] players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
        foreach (TankPlayer player in players) {
            TankPlayer_OnPlayerSpawned(player);
        }

        TankPlayer.OnPlayerSpawned += TankPlayer_OnPlayerSpawned;
        TankPlayer.OnPlayerDespawned += TankPlayer_OnPlayerDespawned;
    }

    public override void OnNetworkDespawn() {// cleanup
        if (!IsServer) { return; }

        TankPlayer.OnPlayerSpawned -= TankPlayer_OnPlayerSpawned;
        TankPlayer.OnPlayerDespawned -= TankPlayer_OnPlayerDespawned;
    }

    private void TankPlayer_OnPlayerSpawned(TankPlayer player) {//Listening while existing
        player.Health.OnDie += (health) => HandlePlayerDie(player);
    }

    private void TankPlayer_OnPlayerDespawned(TankPlayer player) {//cleanup
        player.Health.OnDie -= (health) => HandlePlayerDie(player);
    }

    private void HandlePlayerDie(TankPlayer player) {//Death logic
        int keptCoins = Mathf.RoundToInt(player.CoinWallet.TotalCoins.Value * (keptCoinPercentage / 100));

        Destroy(player.gameObject);

        StartCoroutine(RespawnPlayer(player.OwnerClientId, keptCoins));
    }

    private IEnumerator RespawnPlayer(ulong ownerClientId, int keptCoins) {//Wait then respawn
        yield return null;

        TankPlayer playerInstance = Instantiate(playerPrefab, SpawnPoint.GetRandomSpawnPos(), Quaternion.identity);
        
        playerInstance.NetworkObject.SpawnAsPlayerObject(ownerClientId);

        playerInstance.CoinWallet.TotalCoins.Value += keptCoins;
    }
}
