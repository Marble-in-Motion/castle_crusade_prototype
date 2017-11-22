﻿using System;
using UnityEngine;
using UnityEngine.Networking;


public class NetworkSetup : NetworkBehaviour
{

    protected void RegisterModel(string modelName)
    {
        transform.name = String.Format("{0} {1}", modelName, GetComponent<NetworkIdentity>().netId);
    }

    protected void AssignLayer(string layerName)
    {
        gameObject.layer = LayerMask.NameToLayer(layerName);
    }


}
