using UnityEngine;
using UnityEngine.Networking;

public class PlayerSetup : NetworkBehaviour
{

    [SerializeField]
    Behaviour[] componentsToDisable;

    [SerializeField]
    string remoteLayerName = "RemotePlayer";

    Camera sceneCamera;

    void Start()
    {
        sceneCamera = Camera.main;
        
        if (isLocalPlayer)
        {
            SetLocalCamera();
        } else
        {
            DisableNonLocalCompontents();
            AssignRemoteLayer();
        }

        RegisterPlayer();
    }

    void RegisterPlayer()
    {
        string id = "Player" + GetComponent<NetworkIdentity>().netId;
        transform.name = id;
    }

    void OnDisable()
    {
        if (sceneCamera != null)
        {
            sceneCamera.gameObject.SetActive(true);
        }
    }

    private void SetLocalCamera()
    {
        if (isLocalPlayer)
        {
            sceneCamera.gameObject.SetActive(false);
        }
    }

    private void DisableNonLocalCompontents()
    {
        foreach (Behaviour behaviour in componentsToDisable)
        {
            behaviour.enabled = false;
        }
    }

    private void AssignRemoteLayer()
    {
        gameObject.layer = LayerMask.NameToLayer(remoteLayerName);
    }

}
