using Unity.Netcode;
using UnityEngine;

public class NetworkObjectSpawner : NetworkBehaviour
{
    // Prefabs for the client-controlled and server-controlled objects
    [SerializeField] private GameObject clientObjectPrefab;
    [SerializeField] private GameObject serverObjectPrefab;

    // Left (client) and right (server) positions
    private Vector3 leftSpawnPosition = new Vector3(-5, 0, 0);   // Client-controlled object
    private Vector3 rightSpawnPosition = new Vector3(5, 0, 0);   // Server-controlled object

    // This method runs when the network is ready (objects can be spawned)
    public override void OnNetworkSpawn()
    {
        if (IsServer) // Only the server can spawn objects
        {
            SpawnServerControlledObject(rightSpawnPosition);
            SpawnClientControlledObject(leftSpawnPosition);
        }
    }

    // Spawns the client-controlled object and assigns ownership to the client
    private void SpawnClientControlledObject(Vector3 position)
    {
        // Instantiate the client object on the server side
        GameObject clientObject = Instantiate(clientObjectPrefab, position, Quaternion.identity);

        // Spawn it on the network
        NetworkObject networkObject = clientObject.GetComponent<NetworkObject>();
        networkObject.Spawn();

        // Transfer ownership to the client
        if (NetworkManager.ConnectedClientsList.Count > 1) // Ensure a client is connected
        {
            ulong clientId = NetworkManager.ConnectedClientsList[1].ClientId; // Get the first client (ID 1)
            networkObject.ChangeOwnership(clientId); // Assign ownership to the client
        }
    }

    // Spawns the server-controlled object (server retains ownership)
    private void SpawnServerControlledObject(Vector3 position)
    {
        // Instantiate the server object on the server side
        GameObject serverObject = Instantiate(serverObjectPrefab, position, Quaternion.identity);

        // Spawn it on the network, server retains ownership
        NetworkObject networkObject = serverObject.GetComponent<NetworkObject>();
        networkObject.Spawn(); // No ownership change needed, server retains control
    }
}
