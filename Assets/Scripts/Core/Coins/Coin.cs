using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class Coin : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    protected int coinValue = 10;
    protected bool alreadyCollected;

    public abstract int Collect();//Handles coin collection logic, each coin is different

    public void SetValue(int value) { //for initialization
        coinValue = value;
    }

    protected void Show(bool isEnabled) {//for display logic
        spriteRenderer.enabled = isEnabled;
    }
}
