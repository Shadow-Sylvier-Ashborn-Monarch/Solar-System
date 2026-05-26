using System.Collections.Generic;
//using System.Numerics;
using UnityEngine;

public class CreatureSystem : MonoBehaviour
{
    public int minNumCreatures = 109;
    public int maxNumCreatures = 1090;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject FoodSystem;
    public static List<GameObject> Creatures = new List<GameObject>();
    public GameObject CreaturePrefab;
    public static GameObject _CreaturePrefab;
    void Start()
    {
        _CreaturePrefab = CreaturePrefab;
        for (int i = 0; i < minNumCreatures; i++)
        {
            AddCreature();
        }
    }

    public static void AddCreature(GameObject Creature = null)
    {
        GameObject newCreature;
        if (Creature != null){
            newCreature = Instantiate(Creature, 
                new Vector3(Creature.transform.position.x, 5, Creature.transform.position.z), Quaternion.identity);
            newCreature.GetComponent<Creatures>().Inherit(Creature);
        }
        else
        {
            newCreature = Instantiate(_CreaturePrefab, 
                new Vector3(Random.Range(-60, 60), 5, Random.Range(-60, 60)), Quaternion.identity);
            newCreature.GetComponent<Creatures>().Brain.Mutate();
        }

        Creatures.Add(newCreature);
    }

    public static void RemoveCreature(GameObject Life = null)
    {
        if (Life == null){
          int toRemove = Random.Range(0,Creatures.Count);
          GameObject c = Creatures[toRemove];
          Creatures.RemoveAt(toRemove);
          Destroy(c);
          return;
        }
        Creatures.Remove(Life);
        Destroy(Life);
    }


    // Update is called once per frame
    void Update()
    {
        if(Creatures.Count < minNumCreatures)
        {
            AddCreature();
        }
        if (Creatures.Count > maxNumCreatures)
        {
            RemoveCreature();
        }
    }
}
