using System.Collections.Generic;
//using System.Numerics;
using UnityEngine;

public class Util
{
    public static float RandomFloat(float begin, float end)
    {
        return (float)Random.Range(begin*1000, end*1000)/1000.0f;
    }
    public static float RandomInt(int begin, int end)
    {
        return Random.Range(begin, end);
    }
}
