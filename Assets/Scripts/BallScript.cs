using System;
using UnityEngine;
using Unity.Netcode;

public class BallScript : NetworkBehaviour {
    
    public float forceMagnitude;

    void Start() {
        // NetworkObject.ChangeOwnership(NetworkManager.Singleton.LocalClient.ClientId);
        // // if (IsServer) {
        // //     NetworkObject.ChangeOwnership(NetworkManager.Singleton.LocalClient.ClientId);
        // // }
    }
    private void Update() {

    }

    [ServerRpc(RequireOwnership = false)]
    public void OnRemoteCollisionEnterServerRpc(Vector3 playerPos, Vector3 ballPos) {
        Debug.Log("OnRemoteCollisionEnterServerRpc called" + IsServer);
        Vector3 forceDirection = (playerPos - ballPos).normalized;
        gameObject.GetComponent<Rigidbody>().AddForce(-forceDirection * forceMagnitude, ForceMode.Impulse);
    }
    private void OnCollisionEnter(Collision other) {
        if (!other.collider.CompareTag("Player")) return;
        Vector3 playerPos = other.collider.transform.position;
        Vector3 ballPos = gameObject.transform.position;
        if (IsServer) {
            Debug.Log("Server collision occured");
            Vector3 forceDirection = (playerPos - ballPos).normalized;
            gameObject.GetComponent<Rigidbody>().AddForce(forceDirection * forceMagnitude, ForceMode.Impulse);
        }
        else {
            OnRemoteCollisionEnterServerRpc(playerPos, ballPos);
        }
    }
}
