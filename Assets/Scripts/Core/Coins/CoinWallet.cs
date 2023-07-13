using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CoinWallet : NetworkBehaviour
{
    public NetworkVariable<int> TotalCoins = new NetworkVariable<int>();

    private void OnTriggerEnter2D(Collider2D collision) {
        if (!collision.TryGetComponent(out Coin coin)) { return; }
        
        int coinValue = coin.Collect();

        if (!IsServer) { return; } //Prevent Clients from always getting money, only occurs once since server logic is disabled after collecting

        TotalCoins.Value += coinValue;
    }

    public void SpendCoins(int costToFire) {
        TotalCoins.Value -= costToFire;
    }
}
