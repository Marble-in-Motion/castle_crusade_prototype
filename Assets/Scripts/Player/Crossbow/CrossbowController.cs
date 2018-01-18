using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Player.Crossbow
{
    class CrossbowController : NetworkBehaviour
    {
        [SerializeField]
        private float lookSensitivity = 3f;

        private CrossbowMotor motor;
        
        void Start()
        {
            motor = GetComponent<CrossbowMotor>();
        }

        void Update()
        {
            UpdateMovement();
        }


        private void UpdateMovement()
        {
            float yRot = Input.GetAxisRaw("Mouse X");
            Vector3 x = new Vector3(0f, yRot, 0f) * lookSensitivity;


            float xRot = Input.GetAxisRaw("Mouse Y");
            Vector3 y = new Vector3(xRot, 0f, 0f) * lookSensitivity;

            motor.Rotate(x, y);
        }
    }
}
