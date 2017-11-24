using UnityEngine;
using UnityEngine.Networking;

public class PlayerSetup : NetworkSetup
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
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
            SetLocalCamera();
        } else
        {
            DisableNonLocalCompontents();
            AssignLayer(remoteLayerName);
        }

        RegisterModel(PlayerController.PLAYER_TAG);
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

}
