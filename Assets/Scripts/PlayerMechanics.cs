using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerMechanics : NetworkBehaviour {
    
    // Network
    private Transform leftSpawnPointTransform;
    private Transform rightSpawnPointTransform;

    // Input
    private PlayerInputActions _inputActions;
    private InputAction _moveAction;
    private float _moveValue;

    // Movement
    private Rigidbody _rb;
    public float velocityMag;
    public float forceMag;
    public bool canJump = false;
    
    
    private void Awake() {
        _inputActions = new PlayerInputActions();
        
        
        leftSpawnPointTransform = GameObject.Find("LeftSpawnPoint").transform;
                rightSpawnPointTransform = GameObject.Find("RightSpawnPoint").transform;
        
        Debug.Log("Is server : " + IsServer + "\n IsClient : " + IsClient + "\n IsHost : " + IsHost);
        
    }

    private void Start() {
        _rb = GetComponent<Rigidbody>();
        if (IsServer || IsHost) {
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.isKinematic = false;
            _rb.useGravity = true;
        }
        else {
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.isKinematic = true;
            _rb.useGravity = false;
        }
        
        
    }

    

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        Debug.Log("spawned left");
        transform.position = new Vector3(-7.5f, 4.5f, 0);
        // if (IsOwner && IsHost) {
        //     transform.position = leftSpawnPointTransform.position;
        //     
        // }
        // else if (IsOwner && IsClient) {
        //     Debug.Log("Teleported right");
        //     transform.position = rightSpawnPointTransform.position;
        // }
    }

    private void OnEnable() {
        _moveAction = _inputActions.Player.Move;
        _inputActions.Player.Jump.performed += OnJump;
        _inputActions.Player.Jump.Enable();
        _moveAction.Enable();
    }

    private void OnDisable() {
        _moveAction.Disable();
        _inputActions.Player.Jump.performed -= OnJump;
        _inputActions.Player.Jump.Disable();
    }

    private void OnJump(InputAction.CallbackContext context) {
        if (canJump) {
            _rb.AddForce(0f, forceMag, 0f, ForceMode.Impulse);
        }
    }

    private void OnCollisionEnter(Collision other) {
        if (other.collider.CompareTag("Ground")) {
            canJump = true;
            if (_rb.interpolation == RigidbodyInterpolation.None) {
                _rb.interpolation = RigidbodyInterpolation.Interpolate;
            }
        }

        // if (other.collider.CompareTag("Ball")) {
        //     Debug.Log("Ball Collision");
        //
        //     if (IsOwner) {
        //         MoveObjectServerRpc();
        //     }
        //     
        //     
        // }
    }
    
    // [ServerRpc]
    // void MoveObjectServerRpc() {
    //     // Server-side movement logic
    //     Debug.Log("Server update client");
    //     _moveValue = _moveAction.ReadValue<float>();
    //     _rb.velocity.Set(_moveValue * velocityMag, _rb.velocity.y, _rb.velocity.z);
    // }

    private void OnCollisionExit(Collision other) {
        if (other.collider.CompareTag("Ground")) {
            canJump = false;
        }
    }

    [ServerRpc]
    private void ChangeVelocityServerRpc(ulong networkObjectId, float moveValue)
    {
        
        Debug.Log("ChangeVelocity" + networkObjectId + " : " + moveValue);
        
        // Find the GameObject with the specified NetworkObjectId on the server
        NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
        Rigidbody rb = networkObject.GetComponent<Rigidbody>();

        // Set the velocity of the Rigidbody
        rb.velocity = new Vector3(moveValue * velocityMag, rb.velocity.y, rb.velocity.z);

        // Update the local Rigidbody as well (if needed)
        _rb.velocity = new Vector3(_moveValue * velocityMag, _rb.velocity.y, _rb.velocity.z);
    }

    void FixedUpdate()
    {
        if (!IsOwner || !Application.isFocused) return;

        // Read move value from input
        
        _moveValue = _moveAction.ReadValue<float>();
        Debug.Log("Fixed Update" + _moveValue);

        // Call the ServerRpc method, passing the NetworkObjectId instead of the GameObject
        ChangeVelocityServerRpc(NetworkObject.NetworkObjectId, _moveValue);
    }
    
}