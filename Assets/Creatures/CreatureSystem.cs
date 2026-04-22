using System.Collections.Generic;
//using System.Numerics;
using UnityEngine;

public class CreatureSystem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject FoodSystem;
    public List<Vector3> Food;
    public List<GameObject> Creatures;
    public GameObject CreaturePrefab;
    void Start()
    {
        Food = FoodSystem.GetComponent<FoodSystem>().Foods;
        for(int i = 0; i < 1; i++)
        {
            GameObject newCreature = Instantiate(CreaturePrefab, new Vector3(2, 2, 2), Quaternion.identity);
            Creatures.Add(newCreature);
           
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach(GameObject x in Creatures){
            x.GetComponent<Movement>().tracking(Food[0]);
        }
    }
}
