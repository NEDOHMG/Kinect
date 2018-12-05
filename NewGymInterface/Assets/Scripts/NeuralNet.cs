using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNet : MonoBehaviour {

    public static int inputnodes;
    public static int outputnodes;
    public static int numhiddenlayers;
    public static int[] hiddenlayernodes;
    public static double[][,] zmatrices;
    public static double[][,] deltas;
    public static double[][,] weights;
    public static double bias;

    // Use this for initialization
    void Start () {
        inputnodes = 2;
        outputnodes = 1;
        numhiddenlayers = 1;
        hiddenlayernodes = new int[numhiddenlayers];
        for (int i = 0; i < numhiddenlayers; i++)
        {
            //hiddenlayernodes[i] = (int)Math.Ceiling(((double)inputnodes + (double)outputnodes) / 2.0);
            hiddenlayernodes[i] = (inputnodes + outputnodes) / 2;
            Debug.Log("Nodes for Hidden Layer: " + hiddenlayernodes[i]);
        }
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

        //experimental
        printWeights();
        //double[,] input = new double[,] { { 2, 2 }, { 3, 2 }, { 9, 11 }, { 11, 4 }, { 6, 1 }, { 18, 4 } };
        // double[,] expected = new double[,] { { 4 }, { 5 }, { 20 }, { 15 }, { 7 }, { 22 } };
        // double[,] predictthis = new double[,] { { 5, 3 } };
        double[,] input = new double[,] {{ 2, 2 }, { 3, 4 }, { 3, 6 }, { 2, 1 }};
        double[,] expected = new double[,] {{ 4 }, { 7 }, { 9 }, { 3 }};
        double[,] predictthis = new double[,] {{ 3, 1 }};
        double themax = MatrixMaximum(input);
        bias = themax;
        input = MatrixNormalize(input, themax);
        expected = MatrixNormalize(expected, 2 * themax);
        themax = MatrixMaximum(predictthis);
        predictthis = MatrixNormalize(predictthis, themax);
        for (int i = 0; i < 10000; i++)
            train(input, expected);
        printWeights();
        Debug.Log(print2darray(MatrixDeNormalize(forward(predictthis), 2*themax)));
        

	}

    private static string print2darray(double [,] inp)
    {
        string outp = "";
        outp = outp + "[";
        for (int x = 0; x < inp.GetLength(0); x++)
        {
            outp = outp + "[";
            for(int y = 0; y < inp.GetLength(1); y++)
            {
                outp = outp + inp[x, y];
                if (y < inp.GetLength(1) - 1)
                    outp = outp + ", ";
            }
            outp = outp + "]";
            if (x < inp.GetLength(0) - 1)
                outp = outp + " ";
        }
        outp = outp + "]";
        return outp;
    }

    private static void printWeights()
    {
        //print weights to console
        string weightsprint = "";

        weightsprint = weightsprint + "||";
        for (int q = 0; q < weights.Length; q++)
        {
            weightsprint = weightsprint + print2darray(weights[q]);
        }
        weightsprint = weightsprint + "||";
        Debug.Log(weightsprint);
    }

    private static void initNodes(int input, int hidden, int output, double newbias)
    {
        inputnodes = input;
        numhiddenlayers = hidden;
        outputnodes = output;
        hiddenlayernodes = new int[numhiddenlayers];
        for (int i = 0; i < numhiddenlayers; i++)
        {
            hiddenlayernodes[i] = (int)Math.Ceiling(((double)inputnodes + (double)outputnodes) / 2.0);
        }
        bias = newbias;
    }

    private static void initMatrices()
    {
        zmatrices = new double[numhiddenlayers + 1][,];
        deltas = new double[numhiddenlayers + 1][,];
    }



    private static void setWeights(double[][,] newweights)
    {
        weights = newweights;
    }

    private static double[,] forward(double [,] x)
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

    private static void backward(double[,] x, double[,] y, double[,] output)
    {
        double[,] outputerror = MatrixSubtract(y, output);
        Debug.Log("Output error: " + print2darray(outputerror));
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

    private static void train(double[,] x, double[,] y)
    {
        initMatrices();
        double[,] forwardoutput = forward(x);
        Debug.Log("input: " + print2darray(MatrixDeNormalize(x, bias)));
        Debug.Log("expected output: " + print2darray(MatrixDeNormalize(y, 2*bias)));
        Debug.Log("actual output: " + print2darray(MatrixDeNormalize(forwardoutput, 2*bias)));
        backward(x, y, forwardoutput);
    }

    //Activation Function
    private static double[,] activation(double[,] x)
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
    private static double[,] activationderivative(double[,] x)
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

    private static double[,] MatrixMultiply(double[,] a, double[,] b)
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

    private static double[,] MatrixAdd(double[,] a, double[,] b)
    {
        double[,] output = new double[a.GetLength(0), a.GetLength(1)];
        for (int m = 0; m < output.GetLength(0); m++)
            for (int n = 0; n < output.GetLength(1); n++)
                output[m, n] = a[m, n] + b[m, n];
        return output;
    }

    private static double[,] MatrixSubtract(double[,] a, double[,] b)
    {
        double[,] output = new double[a.GetLength(0), a.GetLength(1)];
        for (int m = 0; m < output.GetLength(0); m++)
            for (int n = 0; n < output.GetLength(1); n++)
                output[m, n] = a[m, n] - b[m, n];
        return output;
    }

    private static double[,] MatrixTranspose(double[,] a)
    {
        double[,] output = new double[a.GetLength(1), a.GetLength(0)];
        for (int m = 0; m < output.GetLength(0); m++)
            for (int n = 0; n < output.GetLength(1); n++)
                output[m, n] = a[n, m];
        return output;
    }

    private static double [,] MatrixNormalize(double[,] a, double maximum)
    {
        double[,] output = new double[a.GetLength(0), a.GetLength(1)];
        for (int m = 0; m < output.GetLength(0); m++)
            for (int n = 0; n < output.GetLength(1); n++)
                output[m, n] = a[m, n] / maximum;
        return output;
    }

    private static double[,] MatrixDeNormalize(double[,] a, double maximum)
    {
        double[,] output = new double[a.GetLength(0), a.GetLength(1)];
        for (int m = 0; m < output.GetLength(0); m++)
            for (int n = 0; n < output.GetLength(1); n++)
                output[m, n] = a[m, n] * maximum;
        return output;
    }

    private static double MatrixMaximum(double[,] a)
    {
        double output = -9999999.0;
        for (int m = 0; m < a.GetLength(0); m++)
            for (int n = 0; n < a.GetLength(1); n++)
                if (a[m, n] > output)
                    output = a[m, n];
        return output;
    }



    // Update is called once per frame
    void Update()
    {

    }
}
