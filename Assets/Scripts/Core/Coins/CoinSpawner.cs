using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class CoinSpawner : NetworkBehaviour
{
    [SerializeField] private RespawningCoin coinPrefab;
    [SerializeField] private int maxCoins;
    [SerializeField] private int coinValue;
    [SerializeField] private Vector2 xSpawnRange;
    [SerializeField] private Vector2 ySpawnRange;
    [SerializeField] private LayerMask layerMask;

    private Collider2D[] coinBuffer = new Collider2D[1];
    private float coinRadius;

    public override void OnNetworkSpawn() {
        if (!IsServer) { return; } //Only for server

        coinRadius = coinPrefab.GetComponent<CircleCollider2D>().radius;

        for (int i = 0; i < maxCoins; i++) {
            SpawnCoin();
        }
    }

    private void SpawnCoin() {
        RespawningCoin coinInstance = Instantiate(coinPrefab, GetSpawnPoint(), Quaternion.identity);//Creates for server only

        coinInstance.SetValue(coinValue);
        coinInstance.GetComponent<NetworkObject>().Spawn();//To span across clients
        coinInstance.OnCollected += CoinInstance_OnCollected;//Events For each specific instance
    }

    private void CoinInstance_OnCollected(RespawningCoin coin) {
        coin.transform.position = GetSpawnPoint();
        coin.Reset();
    }

    private Vector2 GetSpawnPoint() {
        float x = 0;
        float y = 0;

        while (true) {
            x = Random.Range(xSpawnRange.x, ySpawnRange.y);
            y = Random.Range(xSpawnRange.x, ySpawnRange.y);
            Vector2 spawnPoint = new Vector2(x, y); //for random respawn
            
            int numColliders = Physics2D.OverlapCircleNonAlloc(spawnPoint, coinRadius, coinBuffer, layerMask);//for position validation
            if (numColliders == 0) {
                return spawnPoint;
            }
        }
    }
}
