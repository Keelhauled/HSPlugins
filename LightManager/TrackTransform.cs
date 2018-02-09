using System;
using UnityEngine;

namespace LightManager
{
    class TrackTransform : MonoBehaviour
    {
        public string targetName;
        public Transform target;
        public int targetKey;
        public float rotationSpeed = 1f;

        void Update()
        {
            if(target)
            {
                var rotation = Quaternion.LookRotation(target.position - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);
            }
        }
    }
}
