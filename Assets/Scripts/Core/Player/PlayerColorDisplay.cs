using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;

public class PlayerColorDisplay : MonoBehaviour
{
    [SerializeField] private TankPlayer player;
    [SerializeField] private TeamColorLookup teamColorLookup;
    [SerializeField] private SpriteRenderer[] playerSprites;

    private void Start() {
        HandleTeamChanged(-1, player.TeamIndex.Value);

        player.TeamIndex.OnValueChanged += HandleTeamChanged;
    }

    private void HandleTeamChanged(int oldTeamIndex, int newTeamIndex) {
        Color teamColor = teamColorLookup.GetTeamColor(newTeamIndex);

        foreach (SpriteRenderer sprite in playerSprites) {
            sprite.color = teamColor;
        }
    }

    private void OnDestroy() {
        player.TeamIndex.OnValueChanged -= HandleTeamChanged;
    }
}
