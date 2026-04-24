using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;

public class FoodSystem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public List<GameObject> Foods = new List<GameObject>();
    public GameObject Body;
    void Start()
    {
        for(int i = 0; i < 1; i++)
        {
            GameObject newFood =  Instantiate(Body, new Vector3(UnityEngine.Random.Range(-10, 10), 1, UnityEngine.Random.Range(-10, 10)), Quaternion.identity);
            Foods.Add(newFood);
           ;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

