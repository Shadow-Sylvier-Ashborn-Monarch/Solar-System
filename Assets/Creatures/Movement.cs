using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
  public Rigidbody rb;
  public NeuralNetwork Brain;
  void Awake()
  {
    rb = GetComponent<Rigidbody>();//();//transform.Find("Body").GetComponent<Rigidbody>();
    Brain = new NeuralNetwork();
    int Input1 = Brain.AddNode(NodeType.Input);
    int Input2 = Brain.AddNode(NodeType.Input);
    int Input3 = Brain.AddNode(NodeType.Input);

    int Output1 = Brain.AddNode(NodeType.Output);
    int Output2 = Brain.AddNode(NodeType.Output);
 
    Brain.AddConnection(Input1, Output1, 1f);
    Brain.AddConnection(Input2, Output2, 1f);
  }

  // Update is called once per frame
  void Update()
  {
    //Thinking();
    float moveX = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
    float moveY = Input.GetAxisRaw("Vertical");   // W/S or Up/Down
    float speed = 10;
    forward(moveY, 10f, moveX, 20f, 1f);
  
  }

  public Vector3 closestFood(List<GameObject> list)
  {
    Vector3 closest = new Vector3();
    float record = Mathf.Infinity;
    foreach(GameObject s in list)
    {
      float dist = (s.transform.position - transform.position).magnitude;
      if(dist < record)
        {
            closest = s.transform.position;
            record = dist;   
        } 
    }
    return closest;
  }

    public void OnCollisionEnter(Collision collision)
    {
        
    }

    public void Thinking(List<GameObject> list)
  {
     Vector3 food = closestFood(list);
     Vector3 distVec = (food - transform.position);
     float angeVec = Vector3.SignedAngle(transform.forward, distVec, Vector3.up);
     float[] inputs = {1, angeVec/180f, 0};
     float[] thoughts = Brain.FeedForward(inputs);


    print($"Constant: {inputs[0]}\n Angle: {inputs[1]}\n Speed: {thoughts[0]}\n Turn: {thoughts[1]}");
    forward(thoughts[0], 10f, thoughts[1], 300f, 1f);
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
