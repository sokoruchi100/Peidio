using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class TankPlayer : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;
    [SerializeField] private SpriteRenderer minimapIcon;
    [field: SerializeField] public Health Health { get; private set; }
    [field: SerializeField] public CoinWallet CoinWallet { get; private set; }

    [Header("Settings")]
    [SerializeField] private int cameraPriority;
    [SerializeField] private Color myColor;

    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();

    public static event Action<TankPlayer> OnPlayerSpawned;
    public static event Action<TankPlayer> OnPlayerDespawned;

    public override void OnNetworkSpawn() {
        if (IsServer) {
            UserData userData = HostSingleton.Instance.HostGameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);

            PlayerName.Value = userData.userName;

            OnPlayerSpawned?.Invoke(this);
        }

        if (IsOwner) { 
            cinemachineVirtualCamera.Priority = cameraPriority;
            minimapIcon.color = myColor;
        }
    }

    public override void OnNetworkDespawn() {
        if (IsServer) { 
            OnPlayerDespawned?.Invoke(this);
        }
    }
}
