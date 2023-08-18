using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Palette
{
    public class CameraController : MonoBehaviour
    {
        [Header("Framing")]
        [SerializeField] private Camera camera = null;
        [SerializeField] private Transform follow = null;  //characterFollowObject
        [SerializeField] Vector2 framing=new Vector2 (0,0);

        [Header("Rotation")]
        [SerializeField] private bool invertX=false;
        [SerializeField] private bool invertY=false;
        [SerializeField][Range(-90, 90)] private float defaultVerticalAngle = 20;
        [SerializeField][Range(-90, 90)] private float minVerticalAngle=-90;
        [SerializeField][Range(-90, 90)] private float maxVerticalAngle=90;
        [SerializeField] private float rotationSharpness = 25f;


        [Header("Distance")]
        [SerializeField] private float zoomSpeed = 10f;
        [SerializeField] private float defaultDistance = 10f;
        [SerializeField] private float minDistance = 6f;
        [SerializeField] private float maxDistance = 18f;

        [Header("Obstructions")]
        [SerializeField] private float checkRadius = 0.2f;
        [SerializeField] private LayerMask obstructionsLayer=-1;

        //private
        private Vector3 planarDirection;
        private Quaternion targetRotation;
        private Quaternion newRotation;
        private float targetVerticalAngle;
        private float targetDistance;
        private Vector3 targetPosition;
        private Vector3 newPosition;
        private List<Collider> ignoreColliders=new List<Collider>();

        public Vector3 CameraPlanarDirection { get => planarDirection; }

        void Start()
        {
            Collider[] childColliders = GetComponentsInChildren<Collider>();
            ignoreColliders.AddRange(childColliders);
            planarDirection = follow.forward;
            targetVerticalAngle = defaultVerticalAngle;
            targetDistance=defaultDistance;
            targetRotation = Quaternion.LookRotation(planarDirection) * Quaternion.Euler(targetVerticalAngle, 0, 0);
            targetPosition = follow.position - (targetRotation * Vector3.forward) * targetDistance;

            Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnValidate()
        {
            defaultDistance=Mathf.Clamp(defaultDistance, minDistance, maxDistance);
            defaultVerticalAngle=Mathf.Clamp(defaultVerticalAngle,minVerticalAngle,maxVerticalAngle);   
        }

        // Update is called once per frame
        void Update()
        {
            if (Cursor.lockState != CursorLockMode.Locked)
                return;

            //Handle Inputs
            float MouseX = PlayerInputs.MouseXInput;
            float MouseY = PlayerInputs.MouseYInput;
            if (invertX) MouseX *= -1;
            float zoom = PlayerInputs.MouseScrollInput * zoomSpeed;
            Vector3 focusPosition = follow.position + new Vector3(framing.x, framing.y, 0);
            planarDirection=Quaternion.Euler(0,MouseX,0)*planarDirection;
            targetDistance=Mathf.Clamp(targetDistance+zoom,minDistance, maxDistance);   
            if(!invertY)
                targetVerticalAngle = Mathf.Clamp(targetVerticalAngle + MouseY, minVerticalAngle, maxVerticalAngle);
            else
                targetVerticalAngle = Mathf.Clamp(targetVerticalAngle - MouseY, minVerticalAngle, maxVerticalAngle);

            Debug.DrawLine(camera.transform.position, camera.transform.position + planarDirection, Color.red);

            //Handle Obstructions
            float smallestDistance = targetDistance;
            RaycastHit[] hits = Physics.SphereCastAll(focusPosition, checkRadius, targetRotation * Vector3.forward, targetDistance, obstructionsLayer);

            if (hits.Length != 0)
            {
                foreach (RaycastHit hit in hits)
                {
                    if (!ignoreColliders.Contains(hit.collider))
                    {
                        if (hit.distance < smallestDistance)
                            smallestDistance = hit.distance;
                    }
                }
                targetDistance = smallestDistance;
            }

            //Handle Smoothing
            targetRotation = Quaternion.LookRotation(planarDirection) * Quaternion.Euler(targetVerticalAngle, 0, 0);
            newRotation = Quaternion.Slerp(camera.transform.rotation, targetRotation, Time.deltaTime * rotationSharpness);
            targetPosition = focusPosition - (targetRotation * Vector3.forward) * targetDistance;
            newPosition=Vector3.Lerp(camera.transform.position,targetPosition, Time.deltaTime * rotationSharpness);

            
            //Final Taeget
            camera.transform.position= newPosition;
            camera.transform.rotation = newRotation; 
        }
    }

}