using UnityEngine;

namespace SaG.SaveSystem.Samples.Platformer2D
{
    public class FollowTarget : MonoBehaviour
    {
        [SerializeField] private Transform _target = default;
        [SerializeField] private Vector3 _offset = default;
        [SerializeField] private float _speed = 5;

        private void Awake()
        {
            transform.position = _target.position + _offset;
        }

        void FixedUpdate()
        {
            var destination = _target.position + _offset;
            transform.position = Vector3.Lerp(transform.position, destination, _speed * Time.fixedDeltaTime);
        }
    }
}