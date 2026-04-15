using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class GridErosion : MonoBehaviour
{
    [SerializeField] private GameObject terrain;
    [SerializeField] private int mapSize = 256;
    [SerializeField] private int iterations = 10;

    [SerializeField] private float capacity;
    [SerializeField] private float deposition;
    [SerializeField] private float softness;

    [Header("Testing")]
    [SerializeField] private bool isTesting;


    private Vector3[] vertices;
    private float[] water;
    private float[] sediment;

    // Start is called before the first frame update
    void Start()
    {
        Mesh mesh = terrain.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        float[] heights = vertices.Select(p => p.y).ToArray();
        float[] newHeights;

        //float minHeight = heights.Min();
        //if (minHeight < 0f)
        //{
        //    for (int i = 0; i < heights.Length; i++)
        //        heights[i] += Mathf.Abs(minHeight);
        //}

        //for (int i = 0; i < 5; i++)
        //{
        //    System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        //    
        //    long end = sw.ElapsedMilliseconds;
        //    Debug.Log(end);
        //}

        if (!isTesting)
        {
            water = new float[heights.Length];
            sediment = new float[heights.Length];
            for (int i = 0; i < water.Length; i++)
            {
                water[i] = 0f;
                sediment[i] = 0f;
            }
            newHeights = Erode(heights, mapSize, iterations);
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].y = newHeights[i];
            }

            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return;
        }


        newHeights = new float[0];

        float[] testValues = { 100, 200, 500 };

        //List<ExperimentResultErosion> results = new List<ExperimentResultErosion>();
        List<(float, float)> results = new List<(float, float)> ();

        foreach (float testValue in testValues)
        {
            //--------PARAMETR-------------

            //erosion

            iterations = (int)testValue;

            //-----------------------------

            for (int j = 0; j < 5; j++)
            {
                heights = vertices.Select(p => p.y).ToArray();
                water = new float[heights.Length];
                sediment = new float[heights.Length];
                for (int i = 0; i < water.Length; i++)
                {
                    water[i] = 0f;
                    sediment[i] = 0f;
                }
                //results.Clear();


                float start = Time.realtimeSinceStartup;
                newHeights = Erode(heights, mapSize, (int)testValue);
                start -= Time.realtimeSinceStartup;
                results.Add((testValue, -start * 1000));
            }
           // newHeights = Erode(heights, mapSize, iterations);

        }

        Metrics.SaveTimeToCSV(results, "/GridErosion/time" + mapSize + ".csv", "Iterations");

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].y = newHeights[i];
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    private float[] Erode(float[] heights, int mapSize, int iterations)
    {
        //----------------BADANIA---------------
        List<ExperimentResultErosion> results = new List<ExperimentResultErosion>();
        //----------------------------------

        int width = (int)Mathf.Sqrt(heights.Length);
        int size = heights.Length;

        if (water.Length != size) water = new float[size];
        if (sediment.Length != size) sediment = new float[size];

        for (int i = 0; i < size; i++)
        {
            water[i] += heights[i] * 0.001f;
        }

        for (int iter = 0; iter < iterations; iter++)
        {
            float[] deltaHeights = new float[size];
            float[] deltaWater = new float[size];
            float[] deltaSediment = new float[size];

            //---PASS 1---
            for (int i = 0; i < size; i++)
            {
                int x = i % width;
                int y = i / width;

                int[] dx = { -1, 1, 0, 0 };
                int[] dy = { 0, 0, -1, 1 };

                int[] neighbors = new int[4];
                float[] diffs = new float[4];
                int count = 0;
                float totalDiff = 0f;
                float topHeight = heights[i] + water[i];

                for (int j = 0; j < 4; j++)
                {
                    int nx = x + dx[j];
                    int ny = y + dy[j];
                    if (nx >= 0 && nx < width && ny >= 0 && ny < width)
                    {
                        int ni = ny * width + nx;
                        float neighborHeight = heights[ni] + water[ni];
                        if (neighborHeight < topHeight)
                        {
                            neighbors[count] = ni;
                            diffs[count] = topHeight - neighborHeight;
                            totalDiff += diffs[count];
                            count++;
                        }
                    }
                }

                if (count == 0)
                {
                    float deposit = deposition * sediment[i];
                    deltaHeights[i] += deposit;
                    deltaSediment[i] -= deposit;
                    continue;
                }

                float totalOutflow = MathF.Min(water[i], totalDiff);

                float cTotal = capacity * totalOutflow;

                float sedimentToMove;
                float depositAmount = 0f;
                float erosionAmount = 0f;

                if (sediment[i] > cTotal)
                {
                    float deposit = deposition * (sediment[i] - cTotal);
                    sedimentToMove = cTotal;
                    deltaHeights[i] += deposit;
                    deltaSediment[i] -= deposit;
                }
                else
                {
                    float erosion = softness * (cTotal - sediment[i]);
                    sedimentToMove = sediment[i] + erosion;
                    deltaHeights[i] -= erosion;
                }

                for (int n = 0; n < count; n++)
                {
                    int ni = neighbors[n];
                    float proportion = diffs[n] / totalDiff;
                    float flow = totalOutflow * proportion;

                    deltaWater[i] -= flow;
                    deltaWater[ni] += flow;

                    deltaSediment[ni] += sedimentToMove * proportion;
                }

                deltaHeights[i] += depositAmount;
                deltaHeights[i] -= erosionAmount;
                deltaSediment[i] -= sedimentToMove;
            }

            //---PASS 2---
            for (int i = 0; i < size; i++)
            {
                water[i] += deltaWater[i];
                sediment[i] += deltaSediment[i];
                heights[i] += deltaHeights[i];

                water[i] *= 0.99f;

                if (water[i] < 0f) water[i] = 0f;
                if (sediment[i] < 0f) sediment[i] = 0f;
            }

            //--------------------BADANIA-----------------------

        //    int[] steps = { 1, 2, 5, 10, 20, 30, 50, 70, 100, 130, 160, 200 };

        //    if (steps.Contains(iter)) {
        //        float stdDev = Metrics.StdDev(heights);
        //        float avgGradient = Metrics.MeanGrad(heights, mapSize, 2);
        //        float varG = Metrics.GradientVariance();
        //        float ruggedness = Metrics.Ruggedness(heights, mapSize);
        //        float meanAbsCurvature = Metrics.MeanAbsCurvature(heights, mapSize);
        //        float skewness = Metrics.HeightSkewness(heights, mapSize);
        //        float erosionScore = Metrics.ErosionScore(heights, mapSize);

        //        results.Add(new ExperimentResultErosion
        //        {
        //            iterations = iter,
        //            stdDev = stdDev,
        //            avgGradient = avgGradient,
        //            varGradient = varG,
        //            ruggedness = ruggedness,
        //            meanAbsCurvature = meanAbsCurvature,
        //            skewness = skewness,
        //            erosionScore = erosionScore,
        //        });
        //    }

        //    //-------------------------------------------

        }
        //string parameterName = "Softness";
        //Metrics.SaveErosionToCSV(results, "GridErosion/New/gridErosion" + parameterName + softness + ".csv");

        return heights;
    }
}

