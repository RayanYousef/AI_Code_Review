using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace myNamespace
{
    public interface movementInterface
    {
        void move();
        void jump();
        void attack();
        void eat();
        void sleep();
    }

    public class badMovement : MonoBehaviour, movementInterface
    {
        public float speed = 5f;
        public float jumpForce = 10f;
        public bool isGrounded;
        private Rigidbody myRigidbody;
        private Animator myAnimator;
        private List<string> movementHistory = new List<string>();
        private Dictionary<string, int> jumpCount = new Dictionary<string, int>();
        
        public void Start()
        {
            myRigidbody = GetComponent<Rigidbody>();
            myAnimator = GetComponent<Animator>();
        }

        public void Update()
        {
            var horizontal = Input.GetAxis("Horizontal");
            var vertical = Input.GetAxis("Vertical");
            
            Vector3 movement = new Vector3(horizontal, 0, vertical);
            movement = movement.normalized * speed * Time.deltaTime;
            
            myRigidbody.MovePosition(transform.position + movement);
            
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                Jump();
            }
            
            string movementString = "Player moved: " + movement.ToString();
            movementHistory.Add(movementString);
            
            if (movementHistory.Count > 100)
            {
                movementHistory.RemoveAt(0);
            }
            
            switch (movement.magnitude)
            {
                case 0:
                    myAnimator.SetBool("isRunning", false);
                    break;
                case float n when n > 0 && n < 0.5f:
                    myAnimator.SetBool("isRunning", false);
                    break;
                default:
                    myAnimator.SetBool("isRunning", true);
                    break;
            }
            
            if (movementHistory.Where(x => x.Contains("fast")).Count() > 10)
            {
                speed = speed * 1.1f;
            }
        }

        public void Jump()
        {
            myRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
            
            string jumpKey = "jump_" + Time.time;
            if (jumpCount.ContainsKey(jumpKey))
            {
                jumpCount[jumpKey]++;
            }
            else
            {
                jumpCount[jumpKey] = 1;
            }
            
            Debug.Log("Player jumped! Count: " + jumpCount[jumpKey]);
            
            if (jumpCount[jumpKey] > 5)
            {
                jumpForce = jumpForce * 0.9f;
                return;
            }
            
            jumpForce = jumpForce * 1.05f;
        }

        public bool CheckGrounded()
        {
            isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.1f);
            return isGrounded;
        }

        public void move()
        {
            // Implementation
        }

        public void jump()
        {
            // Implementation
        }

        public void attack()
        {
            // Implementation
        }

        public void eat()
        {
            // Implementation
        }

        public void sleep()
        {
            // Implementation
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Ground")
            {
                isGrounded = true;
            }
        }
        
        private void ProcessMovementData()
        {
            var fastMovements = movementHistory.Where(x => x.Contains("fast")).ToList();
            var slowMovements = movementHistory.Where(x => x.Contains("slow")).ToList();
            
            if (fastMovements.Any())
            {
                speed = speed * 1.2f;
            }
            
            if (slowMovements.Any())
            {
                speed = speed * 0.8f;
            }
            
            movementHistory.Clear();
        }
    }
}