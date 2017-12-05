﻿using UnityEngine;
using UnityEngine.UI;

public class PlayerSetup : NetworkSetup
{

    [SerializeField]
    Behaviour[] componentsToDisable;

    [SerializeField]
    string remoteLayerName = "RemotePlayer";

    Camera sceneCamera;

    private Text CurrencyText;

    void Start()
    {
        CurrencyText = this.GetComponentInChildren<Text>();
        sceneCamera = Camera.main;
        CurrencyText.text = "Coin: " + 100.ToString();

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
