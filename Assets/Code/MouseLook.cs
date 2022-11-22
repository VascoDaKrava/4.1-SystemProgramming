using System;
using UnityEngine;
using UnityEngine.Networking;


namespace SystemProgramming.Lesson4HLAPI_FPS
{
    [Obsolete]
    public sealed class MouseLook : NetworkBehaviour
    {
        [Range(0.1f, 10.0f)][SerializeField] private float sensitivity = 2.0f;
        [Range(-90.0f, 0.0f)][SerializeField] private float minVert = -45.0f;
        [Range(0.0f, 90.0f)][SerializeField] private float maxVert = 45.0f;
        private float rotationX = .0f;
        private float rotationY = .0f;
        private Camera camera;

        public Camera PlayerCamera => camera;

        private void Start()
        {
            camera = GetComponentInChildren<Camera>();
            var rb = GetComponentInChildren<Rigidbody>();
            
            if (rb != null)
            {
                rb.freezeRotation = true;
            }
        }

        public void Rotation()
        {
            rotationX -= Input.GetAxis("Mouse Y") * sensitivity;
            rotationY += Input.GetAxis("Mouse X") * sensitivity;
            rotationX = Mathf.Clamp(rotationX, minVert, maxVert);
            transform.rotation = Quaternion.Euler(0, rotationY, 0);
            camera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        }
    }
}