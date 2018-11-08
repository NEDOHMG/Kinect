using UnityEngine;
using System;


public class AnglesCalculation : MonoBehaviour {

    public double AngleBetweenTwoVectors(Vector3 vectorA, Vector3 vectorB)
    {
        double dotProduct = 0.0;
        vectorA.Normalize();
        vectorB.Normalize();
        dotProduct = Vector3.Dot(vectorA, vectorB);

        return (double)Math.Acos(dotProduct) / Math.PI * 180;
    }
}
