using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;

public class FoodSystem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static List<GameObject> Foods = new List<GameObject>();
    public GameObject Body;
    public static GameObject _Body;
    public int AREA = 200;
    public static int _AREA;
    public int minFood = 300;
    void Start()
    {
        _Body = Body;
        _AREA = AREA;
        for(int i = 0; i < 2; i++)
        {
           Create();
        }
    }

    public static void Create()
    {
        GameObject newFood =  Instantiate(_Body, new Vector3(UnityEngine.Random.Range(-_AREA/2, _AREA/2), 40, UnityEngine.Random.Range(-_AREA/2, _AREA/2)), Quaternion.identity);
        newFood.tag = "Food"; 

        Foods.Add(newFood); 
    }

    public static void Remove(GameObject food)
    {
        Foods.Remove(food);
        Destroy(food);
    }

    // Update is called once per frame
    void Update()
    {
        if(Foods.Count < minFood)
        {
            Create();   
        }
    }
}

