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
        for (int i = 0; i < 5; i++)
        {
            GameObject newCreature = Instantiate(CreaturePrefab, new Vector3(Random.Range(-10, 10), 2, Random.Range(-10, 10)), Quaternion.identity);
            Creatures.Add(newCreature);

        }
    }

    public void addCreature(MonoBehaviour Creature)
    {
        if (Creature == null)
        {
            GameObject newCreature = Instantiate(CreaturePrefab, new Vector3(Random.Range(-10, 10), 2, Random.Range(-10, 10)), Quaternion.identity);
            Creatures.Add(newCreature);
        }
        else
        {
            GameObject newCreature = Instantiate(CreaturePrefab, new Vector3(Random.Range(-10, 10), 2, Random.Range(-10, 10)), Quaternion.identity);
            Destroy(newCreature.GetComponent<Creatures>());
          //  newCreature.AddComponent<Creatures>()();
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (GameObject x in Creatures)
        {
            Creatures creature = x.GetComponent<Creatures>();
            creature.Thinking(Food);
            if (creature.Dead())
            {
                Creatures.Remove(x);
                Destroy(x);
            }
            if (Random.Range(0, 100) < 10)
            {
               // Creatures.
            }
        }
    }
}
