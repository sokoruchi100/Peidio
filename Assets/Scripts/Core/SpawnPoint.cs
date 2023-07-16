using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    private static List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    private void OnEnable() {
        spawnPoints.Add(this);
    }

    private void OnDisable() {
        spawnPoints.Remove(this);
    }

    public static Vector3 GetRandomSpawnPos() {
        if (spawnPoints.Count == 0) {
            return Vector3.zero;
        }

        int randomIndex = Random.Range(0, spawnPoints.Count);
        return spawnPoints[randomIndex].transform.position;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 1);
    }
}
