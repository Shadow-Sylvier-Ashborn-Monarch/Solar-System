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
        for (int i = 0; i < 100; i++)
        {
            addCreature();

        }
    }

    public void addCreature(GameObject Creature = null)
    {
        GameObject newCreature = Instantiate(CreaturePrefab, 
        new Vector3(Random.Range(-60, 60), 5, Random.Range(-60, 60)), Quaternion.identity);

        if (Creature != null){
          newCreature.GetComponent<Creatures>().Inherit(Creature);
        }
        Creatures.Add(newCreature);

    }

    // Update is called once per frame
    void Update()
    {
        List<GameObject> babyList = new List<GameObject>();
        List<GameObject> deadList = new List<GameObject>();
        foreach (GameObject x in Creatures)
        {
            Creatures creature = x.GetComponent<Creatures>();
            creature.Thinking(Food);
            if (creature.Dead())
            {
                deadList.Add(x);
            }
            if (Random.Range(0, 5000) < 1)
            {
                babyList.Add(creature.Baby());
            }
        }
        foreach (GameObject x in babyList)
        {
            addCreature(x);
        }
        foreach (GameObject x in deadList)
        {
            Creatures.Remove(x);
                Destroy(x);
        }
        if(Creatures.Count < 5)
        {
            addCreature();
        }
    }
}
