using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNet : MonoBehaviour {

    public int inputnodes;
    public int outputnodes;
    public int numhiddenlayers;
    public int[] hiddenlayernodes;
    public double[][,] zmatrices;
    public double[][,] deltas;
    public double[][,] weights;
    public double bias;

    // Use this for initialization
    void Start () {
        inputnodes = 23;
        outputnodes = 3;
        numhiddenlayers = 1;
        hiddenlayernodes = new int[numhiddenlayers];
        hiddenlayernodes[0] = 13;
        zmatrices = new double[numhiddenlayers + 1][,];
        deltas = new double[numhiddenlayers + 1][,];
        bias = 0.0;

        weights = new double[numhiddenlayers + 1][,];
        System.Random random = new System.Random();
        for (int i = 0; i < weights.Length; i++)
        {
            if(i == 0)
            {
                weights[i] = new double[inputnodes, hiddenlayernodes[0]];
            }
            else if(i == weights.Length - 1)
            {
                weights[i] = new double[hiddenlayernodes[numhiddenlayers - 1], outputnodes];
            }
            else
            {
                weights[i] = new double[hiddenlayernodes[i - 1], hiddenlayernodes[i]];
            }

            for (int m = 0; m < weights[i].GetLength(0); m++)
                for (int n = 0; n < weights[i].GetLength(1); n++)
                    weights[i][m, n] = random.NextDouble();
        }

	}

    void setWeights(double[][,] newweights)
    {
        weights = newweights;
    }

    private double[,] forward(double [,] x)
    {
        double[,] dotmatrix;
        double[,] activematrix = x;
        for(int i = 0; i < numhiddenlayers + 1; i++)
        {
            dotmatrix = MatrixMultiply(activematrix, weights[i]);
            activematrix = activation(dotmatrix);
            zmatrices[i] = activematrix;
        }
        return activematrix;
    }

    private void backward(double[,] x, double[,] y, double[,] output)
    {
        double[,] outputerror = MatrixSubtract(y, output);
        double[,] currenterror;
        double[,] currentdelta = outputerror;

        for(int i = 0; i < numhiddenlayers + 1; i++)
        {
            if(i == 0)
            {
                currenterror = outputerror;
                currentdelta = MatrixMultiply(currenterror, activationderivative(output));
            }
            else
            {
                currenterror = MatrixMultiply(currentdelta, MatrixTranspose(weights[(numhiddenlayers - i) + 1]));
                currentdelta = MatrixMultiply(currenterror, activationderivative(zmatrices[numhiddenlayers - i]));
            }
            deltas[i] = currentdelta;
        }

        for (int i = 0; i < numhiddenlayers + 1; i++)
        {
            if (i == 0)
            {
                weights[i] = MatrixAdd(weights[i], MatrixMultiply(MatrixTranspose(x), deltas[numhiddenlayers - i]));
            }
            else
            {
                weights[i] = MatrixAdd(weights[i], MatrixMultiply(MatrixTranspose(zmatrices[i-1]), deltas[numhiddenlayers - i]));
            }
        }
    }

    private void train(double[,] x, double[,] y)
    {
        double[,] forwardoutput = forward(x);
        backward(x, y, forwardoutput);
    }

    //Activation Function
    private double[,] activation(double[,] x)
    {
        int m = x.GetLength(0);
        int n = x.GetLength(1);
        double[,] output = new double[m, n];

        for (int i = 0; i < m; i++)
        {
            for(int j = 0; j < n; j++)
            {
                //sigmoid
                output[i, j] = 1 / (1 + System.Math.Exp(-x[i, j]));
            }
        }
        return output;
    }

    //Derivative of Activation Function
    private double[,] activationderivative(double[,] x)
    {
        int m = x.GetLength(0);
        int n = x.GetLength(1);
        double[,] output = new double[m, n];

        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                //sigmoid derivative
                output[i, j] = x[i, j] * (1 - x[i, j]);
            }
        }
        return output;
    }


    //UTILITY FUNCTIONS

    //when using 2d arrays as matrices, matrix operations must be manually defined.

    private double[,] MatrixMultiply(double[,] a, double[,] b)
    {
        int m = a.GetLength(0);
        int n = b.GetLength(1);
        int q = a.GetLength(1);
        double[,] c = new double[m, n];

        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                c[i, j] = 0;
                for (int k = 0; k < q; k++)
                {
                    c[i, j] += a[i, k] * b[k, j]; 
                }
            }
        }

        return c;
    }

    private double[,] MatrixAdd(double[,] a, double[,] b)
    {
        double[,] output = new double[a.GetLength(0), a.GetLength(1)];
        for (int m = 0; m < output.GetLength(0); m++)
            for (int n = 0; n < output.GetLength(1); n++)
                output[m, n] = a[m, n] + b[m, n];
        return output;
    }

    private double[,] MatrixSubtract(double[,] a, double[,] b)
    {
        double[,] output = new double[a.GetLength(0), a.GetLength(1)];
        for (int m = 0; m < output.GetLength(0); m++)
            for (int n = 0; n < output.GetLength(1); n++)
                output[m, n] = a[m, n] - b[m, n];
        return output;
    }

    private double[,] MatrixTranspose(double[,] a)
    {
        double[,] output = new double[a.GetLength(1), a.GetLength(0)];
        for (int m = 0; m < output.GetLength(0); m++)
            for (int n = 0; n < output.GetLength(1); n++)
                output[m, n] = a[n, m];
        return output;
    }



    // Update is called once per frame
    void Update()
    {

    }
}
