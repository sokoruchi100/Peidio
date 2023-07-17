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
    [SerializeField] private Texture2D crosshair;
    [field: SerializeField] public Health Health { get; private set; }
    [field: SerializeField] public CoinWallet CoinWallet { get; private set; }

    [Header("Settings")]
    [SerializeField] private int cameraPriority;
    [SerializeField] private Color myColor;

    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();
    public NetworkVariable<int> TeamIndex = new NetworkVariable<int>();

    public static event Action<TankPlayer> OnPlayerSpawned;
    public static event Action<TankPlayer> OnPlayerDespawned;

    public override void OnNetworkSpawn() {
        if (IsServer) {
            UserData userData = null;
            if (IsHost) { //For Hosts
                userData = HostSingleton.Instance.HostGameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            } else { //For Dedicated Server
                userData = ServerSingleton.Instance.ServerGameManager.NetworkServer.GetUserDataByClientId(OwnerClientId);
            }
            

            PlayerName.Value = userData.userName;
            TeamIndex.Value = userData.teamIndex;

            OnPlayerSpawned?.Invoke(this);
        }

        if (IsOwner) { 
            cinemachineVirtualCamera.Priority = cameraPriority;

            minimapIcon.color = myColor;

            Cursor.SetCursor(crosshair, new Vector2(crosshair.width / 2, crosshair.height / 2), CursorMode.Auto);
        }
    }

    public override void OnNetworkDespawn() {
        if (IsServer) { 
            OnPlayerDespawned?.Invoke(this);
        }
    }
}
