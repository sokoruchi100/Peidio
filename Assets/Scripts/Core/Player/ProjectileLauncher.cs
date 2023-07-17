using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class ProjectileLauncher : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject serverProjectilePrefab;
    [SerializeField] private GameObject clientProjectilePrefab;
    [SerializeField] private GameObject muzzleFlash;
    [SerializeField] private Collider2D playerCollider;
    [SerializeField] private CoinWallet coinWallet;

    [Header("Settings")]
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float fireRate;
    [SerializeField] private float muzzleFlashDuration;
    [SerializeField] private int costToFire;

    private bool isPointerOverUI;
    private bool shouldFire;
    private float timer;
    private float muzzleFlashTimer;

    public override void OnNetworkSpawn() {
        if (!IsOwner) { return; }
        
        inputReader.PrimaryFireEvent += InputReader_PrimaryFireEvent;
    }

    public override void OnNetworkDespawn() {
        if (!IsOwner) { return; }

        inputReader.PrimaryFireEvent -= InputReader_PrimaryFireEvent;
    }

    private void Update() {
        if (muzzleFlashTimer > 0f) { //For muzzle flash
            muzzleFlashTimer -= Time.deltaTime;

            if (muzzleFlashTimer <= 0f) { 
                muzzleFlash.SetActive(false);
            }
        }

        if (!IsOwner) { return; }//Only fires if you own the object

        isPointerOverUI = EventSystem.current.IsPointerOverGameObject();

        if (timer > 0) {
            timer -= Time.deltaTime;
        }

        if (!shouldFire) { return; }//If Clicked and held down

        if (timer > 0) { return; }//For fire rate

        if (coinWallet.TotalCoins.Value < costToFire) { return; }

        PrimaryFireServerRpc(projectileSpawnPoint.position, projectileSpawnPoint.up);//Tells the server to create real logic projectile
        SpawnDummyProjectile(projectileSpawnPoint.position, projectileSpawnPoint.up);//Creates visual projectile for player

        timer = 1 / fireRate;
    }

    private void InputReader_PrimaryFireEvent(bool shouldFire) {
        if (shouldFire) {
            if (isPointerOverUI) { return; }
        }
        this.shouldFire = shouldFire;
    }

    [ServerRpc]
    private void PrimaryFireServerRpc(Vector3 spawnPos, Vector3 direction) {
        if (coinWallet.TotalCoins.Value < costToFire) { return; }

        coinWallet.SpendCoins(costToFire);

        GameObject serverProjectileGameObject = Instantiate(serverProjectilePrefab, spawnPos, Quaternion.identity);

        serverProjectileGameObject.transform.up = direction;

        Physics2D.IgnoreCollision(playerCollider, serverProjectileGameObject.GetComponent<Collider2D>());

        if (serverProjectileGameObject.TryGetComponent(out DealDamageOnContact dealDamageOnContact)) {//Handles self shooting collisions
            dealDamageOnContact.SetOwner(OwnerClientId);
        }

        if (serverProjectileGameObject.TryGetComponent(out Rigidbody2D rb)) {//Handles projectile movement
            rb.velocity = rb.transform.up * projectileSpeed;
        }

        SpawnDummyProjectileClientRpc(spawnPos, direction);//Handles visuals for other players
    }

    [ClientRpc]
    private void SpawnDummyProjectileClientRpc(Vector3 spawnPos, Vector3 direction) {
        if (IsOwner) { return; }
        SpawnDummyProjectile(spawnPos, direction);
    }

    private void SpawnDummyProjectile(Vector3 spawnPos, Vector3 direction) {
        muzzleFlash.SetActive(true);
        muzzleFlashTimer = muzzleFlashDuration;
        
        GameObject clientProjectileGameObject = Instantiate(clientProjectilePrefab, spawnPos, Quaternion.identity);

        clientProjectileGameObject.transform.up = direction;

        Physics2D.IgnoreCollision(playerCollider, clientProjectileGameObject.GetComponent<Collider2D>());

        if (clientProjectileGameObject.TryGetComponent(out Rigidbody2D rb)) {
            rb.velocity = rb.transform.up * projectileSpeed;
        }
    }
}
