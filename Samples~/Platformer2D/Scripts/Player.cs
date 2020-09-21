using System.Linq;
using UnityEngine;

namespace SaG.SaveSystem.Samples.Platformer2D
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private float _moveForce = 5f;
        [SerializeField] private float _jumpForce = 50f;

        private Rigidbody2D _rigidbody2D;

        // Start is called before the first frame update
        void Start()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        // Update is called once per frame
        void Update()
        {
            var input = Input.GetAxis("Horizontal");
            _rigidbody2D.AddForce(new Vector2(input * _moveForce * Time.deltaTime, 0), ForceMode2D.Force);
            if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
                _rigidbody2D.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        }

        private bool IsGrounded()
        {
            var hits = Physics2D.BoxCastAll(transform.position - new Vector3(0, 0), new Vector2(1, .1f), 0,
                Vector2.down, 1f);
            return hits.Any(hit => hit.transform != transform && !hit.collider.isTrigger);
        }
    }
}