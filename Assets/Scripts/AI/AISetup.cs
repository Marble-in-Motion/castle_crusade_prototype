using System;
using UnityEngine;

public class AISetup : NetworkSetup
{
    [SerializeField]
    private string AIName;

    public void Start()
    {
        RegisterModel(AIName);
    }
}

