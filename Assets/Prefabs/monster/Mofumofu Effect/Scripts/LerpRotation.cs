using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MofumofuEffect
{
    public class LerpRotation : MonoBehaviour
    {
        Vector3 lookAt;

        private void Start()
        {
            lookAt = transform.position+transform.forward;
        }

        private void LateUpdate()
        {
            lookAt = Vector3.Slerp(lookAt, transform.position + transform.forward, Time.deltaTime);
            transform.LookAt(lookAt, transform.up);
        }
    }
}
