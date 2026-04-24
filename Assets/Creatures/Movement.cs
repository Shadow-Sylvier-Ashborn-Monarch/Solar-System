using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
  public Rigidbody rb;
  void Awake()
  {
    rb = GetComponent<Rigidbody>();//();//transform.Find("Body").GetComponent<Rigidbody>();

  }

  // Update is called once per frame
  void Update()
  {
    float moveX = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
    float moveY = Input.GetAxisRaw("Vertical");   // W/S or Up/Down
    float speed = 10;
    forward(moveY, 10f, moveX, 20f, 1f);
  
  }

  public Vector3 closestFood(List<Vector3> list)
  {
      foreach(Vector3 s in list)
  }  


  public void tracking(Vector3 target)
  {
    Vector3 direction = target-transform.position;
        direction *= 2; //new Vector3(5f, 5f, 5f);
        rb.linearVelocity = direction;
  }

  public void forward(float inputForward, float moveSpeed, float inputTurn, float turnSpeed, float stickForce)
  {
    RaycastHit hit;
    Vector3 moveDir = transform.forward * inputForward;

    if (Physics.Raycast(transform.position, Vector3.down, out hit, 1.2f))
    {
      Vector3 slopeMove = Vector3.ProjectOnPlane(moveDir, hit.normal).normalized;

      rb.AddForce(slopeMove * moveSpeed, ForceMode.Acceleration);
    }

    float turn = inputTurn * turnSpeed * Time.deltaTime;
    rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turn, 0f));

    rb.AddForce(Vector3.down * stickForce);

    rb.linearDamping = 2f;          // slows linear movement
    rb.angularDamping = 5f;   // prevents unwanted spinning

    Vector3 velocity = rb.linearVelocity;
    Vector3 forwardVel = transform.forward * Vector3.Dot(velocity, transform.forward);
    Vector3 sideVel = transform.right * Vector3.Dot(velocity, transform.right);

    rb.linearVelocity = forwardVel + sideVel * 0.2f + Vector3.up * velocity.y;
  }


}
