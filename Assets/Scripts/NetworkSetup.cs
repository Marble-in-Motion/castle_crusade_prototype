using System;
using UnityEngine;
using UnityEngine.Networking;


public class NetworkSetup : NetworkBehaviour
{
    [SyncVar(hook = "SetTransformName")]
    private string networkName;
    public string NetworkName
    {
        get
        {
            return networkName;
        }
    }

    protected void RegisterModel(string modelName)
    {
        networkName = String.Format("{0} {1}", modelName, GetComponent<NetworkIdentity>().netId);
    }

    protected void RegisterModel(string modelName, int id)
    {
        networkName = String.Format("{0} {1}", modelName, id);
    }

    private void SetTransformName(string name)
    {
        networkName = name;
        gameObject.name = name;
        transform.name = name;
    }

    protected void AssignLayer(string layerName)
    {
        gameObject.layer = LayerMask.NameToLayer(layerName);
    }

}
