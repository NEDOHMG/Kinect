using UnityEngine;
using System;


public class AnglesCalculation : MonoBehaviour {

    public float AngleBetweenTwoVectors(Vector3 vectorA, Vector3 vectorB)
    {
        float dotProduct = 0.0f;
        vectorA.Normalize();
        vectorB.Normalize();
        dotProduct = Vector3.Dot(vectorA, vectorB);

        return (float)(Math.Acos(dotProduct)) / (float)Math.PI * 180.0f;
    }
}
