using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkSync : NetworkBehaviour {
    
    bool ballSpawned = false;
    
    // Ball and Spawn Objects
    public Transform leftSpawnPoint;
    public Transform rightSpawnPoint;
    public GameObject ball;
    
    public void hostButtonFunc() {
        NetworkManager.Singleton.StartHost();
    }
    
    public void joinButtonFunc() {
        NetworkManager.Singleton.StartClient();
    }
    
    

    void Update() {
        // Only the server should check for connected clients and spawn the ball
        if (!IsServer) return;
        
        // Check if the ball is already spawned to avoid re-spawning
        if (!ballSpawned && NetworkManager.Singleton.ConnectedClients.Count == 2) {
            ballSpawned = true;
            SpawnBallServerRpc();
        }
    }

    [ServerRpc]
    private void SpawnBallServerRpc() {
        
        GameObject b = Instantiate(ball, leftSpawnPoint.position, Quaternion.identity);
        b.GetComponent<NetworkObject>().Spawn();
    }
}
