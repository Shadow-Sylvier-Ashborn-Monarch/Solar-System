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
        for(int i = 0; i < 2; i++)
        {
           Create();
        }
    }

    public void Create()
    {
        GameObject newFood =  Instantiate(Body, new Vector3(UnityEngine.Random.Range(-60, 60), 40, UnityEngine.Random.Range(-60, 60)), Quaternion.identity);
            newFood.tag = "Food"; 

            Foods.Add(newFood); 
    }

    // Update is called once per frame
    void Update()
    {
        if(Foods.Count < 100)
        {
         Create();   
        }
    }
}

