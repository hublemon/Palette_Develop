using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace MofumofuEffect
{
    public class DirectionalWindZone : MonoBehaviour
    {
        public static DirectionalWindZone instance;
        private void Awake()
        {
            instance = this;
        }

        [Tooltip("The moving speed of the wind zone.")]
        public float speed = 1f;

        [Tooltip("The bigger multiplier makes the wind zone denser.")]
        public float multiplier = 1;

        [Tooltip("The max strength of the wind zone.")]
        public float max = 1;

        [Tooltip("The  min strength of the wind zone.")]
        public float min = .1f;

        public Vector3 SampleWindZone(Vector3 worldPos)
        {
            Vector3 vel = new();

            if (!gameObject.activeInHierarchy) return vel;

            //主要风力计算
            Vector3 project = Vector3.Project(worldPos, transform.forward);
            float length = project.magnitude;
            if (Vector3.Dot(project, transform.forward) < 0) length *= -1f;
            vel += math.remap(-1f, 1f, min, max, Mathf.Sin(-speed * Time.time + multiplier * length)) * transform.forward;

            return vel;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward);
            Gizmos.DrawLine(transform.position + transform.forward,
                transform.position + transform.forward + Quaternion.LookRotation(transform.forward) * Quaternion.Euler(0f, 160f, 0f) * Vector3.forward * 0.2f);
            Gizmos.DrawLine(transform.position + transform.forward,
                transform.position + transform.forward + Quaternion.LookRotation(transform.forward) * Quaternion.Euler(0f, 200f, 0f) * Vector3.forward * 0.2f);
        }
    }
}
