using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Creatures : Life
{
  public Rigidbody rb;
  public NeuralNetwork Brain;
  public Dictionary<string, float> DNA = new Dictionary<string, float>();
  public float age = 0;
  public float fat = 10;
  public float energy = 10;
  public float food = 10;
  
  private int SpeedIntent;
  private int RotationalIntent;
  private int DuplicateIntent;
  private int[] rays;
  private float checksPerDegree;
  private int totalChecks = 120;
  private int KnownEnergy;
  private int KnownFat;
  public bool immortal = false;
  public bool more = false;
  private bool lastMore = false;

  private static float parentFatProportion = 1.0f/4.0f;
  private static float parentEnergyProportion = 1.0f/3.0f;

  void Awake()
  {
    rb = GetComponent<Rigidbody>();//();//transform.Find("Body").GetComponent<Rigidbody>();
    Brain = new NeuralNetwork();

    DNA.Add("Muscle",Util.RandomFloat(5, 30));
    DNA.Add("MaxFat",Util.RandomFloat(10, 20));
    DNA.Add("Length",Util.RandomFloat(1, 10));
    DNA.Add("Metabolism",Util.RandomFloat(3, 10));
    DNA.Add("MaxFood",Util.RandomFloat(10,100));
    DNA.Add("Fangs",Util.RandomFloat(1, 10));
    DNA.Add("FOV",Util.RandomInt(10,360));
    DNA.Add("Range",Util.RandomInt(5,30));
    DNA.Add("TendencyToLearn",Util.RandomFloat(1, 10));
    DNA.Add("Pheremone", 0);

    DNA.Add("R",Util.RandomFloat(0, 255));
    DNA.Add("G",Util.RandomFloat(0, 255));
    DNA.Add("B",Util.RandomFloat(0, 255));

    fat = (1.0f/4.0f)*DNA["MaxFat"];
    energy = (1.0f/4.0f)*DNA["Muscle"] * DNA["Length"];
    pheremoneVal = DNA["Pheremone"];

    SpeedIntent = Brain.AddNode(NodeType.Output);
    RotationalIntent = Brain.AddNode(NodeType.Output);
    //aka 5 checks per degree
    rays = new int[totalChecks];
    checksPerDegree = (float)rays.Length/(float)DNA["FOV"];
    //This for loop looks at every ray, and given the rays angle from center, applies different rotational and speed weights
    for (int i = 0; i < rays.Length; i++)
    {
      int ray = Brain.AddNode(NodeType.Input);
      rays[i] = ray;
      // float initPower = ((float)i - (float)rays.Length/2)/(rays.Length);
      // float invertInitPower = 1.0f + ((float)i - (float)rays.Length/2)/(float)rays.Length;
    }
    DuplicateIntent = Brain.AddNode(NodeType.Output);
    KnownEnergy = Brain.AddNode(NodeType.Input);
    KnownFat = Brain.AddNode(NodeType.Input);
    for (int i = 0; i < 100; i++)
    {
      Brain.MutateAddConnection();
    }
    for (int i = 0; i < 10; i++)
    {
      Brain.MutateAddNode();
    }
    for (int i = 0; i < 1000; i++)
    {
      Brain.MutateWeights();
    }
    


    gameObject.GetComponent<Renderer>().material.color = new Color(DNA["R"]/255, DNA["G"]/255, DNA["B"]/255);
    
  }

  // Update is called once per frame
  public void Inherit(GameObject creature)
  {
    Creatures cre = creature.GetComponent<Creatures>();
    Dictionary<string, float> newDictionary = new Dictionary<string, float>();
    foreach (string str in cre.DNA.Keys)
    {
      newDictionary[str] = cre.DNA[str] + Util.RandomFloat(-1, 1);
    }
    DNA = newDictionary;
    
    this.Brain.connections = cre.Brain.connections;
    this.Brain.nodes = cre.Brain.nodes;
    if(UnityEngine.Random.Range(1, 100) < 70){
      this.Brain.Mutate();
    }
    fat = cre.fat*parentFatProportion;
    energy = cre.energy*parentEnergyProportion;
    pheremoneVal = DNA["Pheremone"];
    more = false;
    immortal = false;
    food = 0;
    gameObject.GetComponent<Renderer>().material.color = new Color(DNA["R"]/255, DNA["G"]/255, DNA["B"]/255);
  }


  void Update()
  {
    
    float delta = Time.deltaTime;
    age+=delta;
    float added = Math.Min(delta*DNA["Metabolism"],food);
    food-=added;
    if (energy < DNA["Length"] * DNA["MaxFat"]/4)
    {
      energy+=added;
      if (added < delta*DNA["Metabolism"] && fat > delta*DNA["Metabolism"] - added + 1)
      {
        // energy+=delta*DNA["Metabolism"] - added;
        // fat-=delta*DNA["Metabolism"] - added;
      }
    }
    else
    {
      energy += (1-(energy - DNA["Length"] * DNA["MaxFat"]*1/4)
        /(DNA["Length"] * DNA["MaxFat"]*3/4))*added;
      fat += (energy - DNA["Length"] * DNA["MaxFat"]*1/4)
        /(DNA["Length"] * DNA["MaxFat"]*3/4)*added;
    }

    energy-=delta*rb.linearVelocity.magnitude*DNA["Metabolism"]*DNA["Muscle"]/100+DNA["Metabolism"]*delta/4;
    
    if (immortal)
    {
      energy = DNA["Length"] * DNA["MaxFat"];
      fat = DNA["MaxFat"];
    }
    if (more && lastMore != more)
    {
      CreatureSystem.AddCreature(gameObject);
    }
    lastMore = more;
    if (Dead())
    {
      CreatureSystem.RemoveCreature(gameObject);
    }
    Thinking();
  }

  public bool Dead()
  {
    return energy < 1;
  }
  [Obsolete()]
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

  public float[] See()
  {
    float[] rayList = new float[rays.Length];
    Vector3 moveDir = transform.forward;
    for (int i = 0; i < rays.Length; i++)
    {
      RaycastHit hit;
      bool didHit = Physics.Raycast(transform.position, Quaternion.AngleAxis((i-rays.Length/2)/checksPerDegree,Vector3.up)*moveDir.normalized, out hit, DNA["Range"]);
      if (didHit)
      {
        Life l = hit.collider.gameObject.GetComponent<Life>();
        if (l)
        {
          rayList[i] = l.pheremoneVal;
        }
      }
    }
    return rayList;
  }

  public void OnCollisionEnter(Collision collision)
  {
    if (collision.gameObject.tag == "Food")
    {
      FoodSystem.Remove(collision.gameObject);
      food+=5;
      food = Math.Min(food, DNA["MaxFood"]);
    }
  }

  public void Thinking()
  {
    float[] inputs = See().Append(KnownEnergy).Append(KnownFat).ToArray();
    float[] thoughts = Brain.FeedForward(inputs);

    float maxSpeed = Math.Max(DNA["Muscle"] - (fat*DNA["Length"]/10),1);
    float maxRot = Math.Max(DNA["Muscle"]*DNA["Length"]/(fat*2),1);

    if (thoughts[2] > .01 && fat > DNA["MaxFat"]*parentFatProportion*2 && energy > DNA["MaxFat"] * DNA["Length"]*parentEnergyProportion*2 && !immortal)
    {//ISSUE
      CreatureSystem.AddCreature(gameObject);
      energy-=DNA["Muscle"] * DNA["Length"]*parentEnergyProportion;
      fat-=DNA["MaxFat"] * parentFatProportion;
    }

    forward(thoughts[0], maxSpeed, thoughts[1],  maxRot, 1f);
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
