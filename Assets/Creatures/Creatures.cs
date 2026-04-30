using System.Collections.Generic;
using UnityEngine;

public class Creatures : MonoBehaviour
{
  public Rigidbody rb;
  public NeuralNetwork Brain;

  public List<float> DNA;// = new List<float>();
  public float Health;
  void Awake()
  {
    rb = GetComponent<Rigidbody>();//();//transform.Find("Body").GetComponent<Rigidbody>();
    Brain = new NeuralNetwork();
    int Input1 = Brain.AddNode(NodeType.Input);
    int Input2 = Brain.AddNode(NodeType.Input);
    int Input3 = Brain.AddNode(NodeType.Input);
    int Input4 = Brain.AddNode(NodeType.Input);

    int Output1 = Brain.AddNode(NodeType.Output);
    int Output2 = Brain.AddNode(NodeType.Output);

    Brain.AddConnection(Input1, Output1, 1f);
    Brain.AddConnection(Input2, Output2, 1f);

    Health = 20;
    //Speed
    DNA.Add(Random.Range(0, 100));
    //Mobility
    DNA.Add(Random.Range(0, 100));
    //RGB colors
    DNA.Add(Random.Range(0, 255));
    DNA.Add(Random.Range(0, 255));
    DNA.Add(Random.Range(0, 255));

    gameObject.GetComponent<Renderer>().material.color = new Color(DNA[2]/255, DNA[3]/255, DNA[4]/255);

    print("Red: " + DNA[2]+ "\n"+ "Green: " + DNA[3]  + "\n" +  "Blue: " + DNA[4]);
  }

  // Update is called once per frame
  public void Inherit(GameObject creature)
  {
    Creatures cre = creature.GetComponent<Creatures>();
    this.Brain = cre.Brain;
    this.Brain.Mutate();
    //Speed
    this.DNA[0] = cre.DNA[0] + Random.Range(-5, 5);
    //Mobility
    this.DNA[1] = cre.DNA[1] + Random.Range(-5, 5);
    //RGB colors
    this.DNA[2] = cre.DNA[2] + Random.Range(-25, 25);
    this.DNA[3] = cre.DNA[3] + Random.Range(-25, 25);
    this.DNA[4] = cre.DNA[4] + Random.Range(-25, 25);
    gameObject.GetComponent<Renderer>().material.color = new Color(DNA[2]/255, DNA[3]/255, DNA[4]/255);
  }


  void Update()
  {
    Health -= 1 * Time.deltaTime;
    float moveX = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
    float moveY = Input.GetAxisRaw("Vertical");   // W/S or Up/Down
    float speed = 10;
    forward(moveY, DNA[0], moveX, DNA[1], 1f);

  }

  public bool Dead()
  {
    return Health <= 0;
  }

  public GameObject Baby()
  {
    return gameObject;
  }

  public Vector3 closestFood(List<GameObject> list)
  {
    Vector3 closest = new Vector3();
    float record = Mathf.Infinity;
    foreach (GameObject s in list)
    {
      float dist = (s.transform.position - transform.position).magnitude;
      if (dist < record)
      {
        closest = s.transform.position;
        record = dist;
      }
    }
    return closest;
  }

  public void OnCollisionEnter(Collision collision)
  {
    if (collision.gameObject.tag == "Food")
    {
      print("Food Collision");
      print(GameObject.Find("Creature System").GetComponent<CreatureSystem>().Food.Remove(collision.gameObject));
      Destroy(collision.gameObject);
      Health += 10;
    }
  }

  public void Thinking(List<GameObject> list)
  {
    Vector3 food = closestFood(list);
    Vector3 distVec = (food - transform.position);
    float angeVec = Vector3.SignedAngle(transform.forward, distVec, Vector3.up);
    float[] inputs = { 1, angeVec / 180f, distVec.magnitude/10f , Health/20};
    float[] thoughts = Brain.FeedForward(inputs);


    forward(thoughts[0], DNA[0], thoughts[1],  DNA[1], 1f);
  }


  public void tracking(Vector3 target)
  {
    Vector3 direction = target - transform.position;
    direction *= 2; //new Vector3(5f, 5f, 5f);
    rb.linearVelocity = direction;
  }

 public void forward(float inputForward, float moveSpeed, float inputTurn, float turnSpeed, float stickForce)
{
    RaycastHit hit;
    bool isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, 1.2f);
    
    // 1. IMPROVED MOVEMENT (Slope Aware)
    Vector3 moveDir = transform.forward * inputForward;
    if (isGrounded)
    {
        // Align movement to the ground slope
        Vector3 slopeMove = Vector3.ProjectOnPlane(moveDir, hit.normal).normalized;
        rb.AddForce(slopeMove * moveSpeed, ForceMode.Acceleration);
        
        // Stick to surface
        rb.AddForce(-hit.normal * stickForce, ForceMode.Force);
    }
    else
    {
        // Optional: less control in the air
        rb.AddForce(moveDir * (moveSpeed * 0.3f), ForceMode.Acceleration);
    }

    // 2. REALISTIC TURNING (Using Torque instead of MoveRotation)
    // MoveRotation is "teleporting" the rotation, which ignores physics.
    // Torque allows the angularDamping to actually do its job.
    float turn = inputTurn * turnSpeed;
    rb.AddTorque(Vector3.up * turn, ForceMode.Acceleration);

    // 3. DRIFT & TRACTION CONTROL (The "Realistic" feel)
    Vector3 velocity = rb.linearVelocity;
    Vector3 rightAxis = transform.right;
    
    // Calculate how much we are sliding sideways
    float sidewaysVelocity = Vector3.Dot(velocity, rightAxis);
    
    // Apply a counter-force to reduce sliding (Traction)
    // Lower the 5.0f value to make it "driftier", raise it for "sharp" turning
    float tractionValue = 5.0f; 
    Vector3 sideFriction = -rightAxis * (sidewaysVelocity * tractionValue);
    
    if (isGrounded)
    {
        rb.AddForce(sideFriction, ForceMode.Acceleration);
    }

    // 4. DAMPING SETUP
    // Set these in Start() usually, but keeping them here per your code
    rb.linearDamping = 1f;      // Allow some coasting
    rb.angularDamping = 2.5f;   // Smooths out the turn
}


}
