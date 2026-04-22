using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;

public class FoodSystem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public List<Vector3> Foods = new List<Vector3>();
    public GameObject Body;
    void Start()
    {
        for(int i = 0; i < 1; i++)
        {
            Foods.Add(new Vector3(1, 1, 1));
            Instantiate(Body, Foods[i], Quaternion.identity) ;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

