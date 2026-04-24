using System.Collections.Generic;
//using System.Numerics;
using UnityEngine;

public class CreatureSystem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject FoodSystem;
    public List<GameObject> Food;
    public List<GameObject> Creatures;
    public GameObject CreaturePrefab;
    void Start()
    {
        Food = FoodSystem.GetComponent<FoodSystem>().Foods;
        for(int i = 0; i < 5; i++)
        {
            GameObject newCreature = Instantiate(CreaturePrefab, new Vector3(Random.Range(-10, 10), 2, Random.Range(-10, 10)), Quaternion.identity);
            Creatures.Add(newCreature);
           
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach(GameObject x in Creatures){
            x.GetComponent<Movement>().Thinking(Food);
        }
    }
}
