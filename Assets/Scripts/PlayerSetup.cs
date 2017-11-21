using UnityEngine;
using UnityEngine.Networking;

public class PlayerSetup : NetworkBehaviour
{

    [SerializeField]
    Behaviour[] componentsToDisable;

    Camera sceneCamera;

    void Start()
    {
        sceneCamera = Camera.main;
        SetLocalCamera();
    }

    void OnDisable()
    {
        sceneCamera.gameObject.SetActive(true);
    }

    private void SetLocalCamera()
    {
        if (isLocalPlayer)
        {
            sceneCamera.gameObject.SetActive(false);
        }
        else
        {
            foreach (Behaviour behaviour in componentsToDisable)
            {
                behaviour.enabled = false;
            }
        }
    }

}
