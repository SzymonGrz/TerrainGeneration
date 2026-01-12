using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.Experimental.GraphView.Port;

public class GridErosion : MonoBehaviour
{
    [SerializeField] private GameObject terrain;
    [SerializeField] private int mapSize = 256;
    [SerializeField] private int iterations = 10;

    [SerializeField] private float capacity;
    [SerializeField] private float deposition;
    [SerializeField] private float softness;

    private Vector3[] vertices;
    private float[] water;
    private float[] sediment;

    // Start is called before the first frame update
    void Start()
    {
        Mesh mesh = terrain.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        float[] heights = vertices.Select(p => p.y).ToArray();

        //float minHeight = heights.Min();
        //if (minHeight < 0f)
        //{
        //    for (int i = 0; i < heights.Length; i++)
        //        heights[i] += Mathf.Abs(minHeight);
        //}

        water = new float[heights.Length];
        sediment = new float[heights.Length];
        for(int i = 0; i < water.Length; i++)
        {
            water[i] = 0f;
            sediment[i] = 0f;
        }

        float[] newHeights = Erode(heights, mapSize, iterations);

        //for (int i = 0; i < newHeights.Length; i++)
        //    newHeights[i] -= minHeight;

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
        int size = heights.Length;
        float[] newHeights = (float[])heights.Clone();
        float[] newWater = (float[])water.Clone();
        float[] newSediment = (float[])sediment.Clone();
        int width = (int)Mathf.Sqrt(heights.Length);

        for (int i = 0; i < water.Length; i++)
        {
            water[i] += heights[i] * 0.001f;
        }

        for (int i = 0; i < iterations; i++)
        {
            float[] deltaHeights = new float[heights.Length];
            float[] deltaWater = new float[heights.Length];
            float[] deltaSediment = new float[heights.Length];

            for (int j = 0; j < heights.Length; j++)
            {
                int x = j % width;
                int y = j / width;


                int[] dx = { -1, 1, 0, 0 };//, -1, 1, -1, 1 };
                int[] dy = { 0, 0, -1, 1 };//, -1, -1, 1, 1 };

                float topOfWaterVertex = heights[j] + water[j];
                float[] topOfWaterNeighbors = new float[dx.Length];

                for (int k = 0; k < dx.Length; k++)
                {
                    int nx = x + dx[k];
                    int ny = y + dy[k];

                    if (nx >= 0 && nx < width && ny >= 0 && ny < width)
                    {
                        int neighbor = ny * width + nx;

                        topOfWaterNeighbors[k] = Mathf.Max(0f, topOfWaterVertex - (heights[neighbor] + water[neighbor]));

                    }
                }

                float sumOfNeighbors = topOfWaterNeighbors.Sum();
                if (sumOfNeighbors > 0f)
                {
                    for (int k = 0; k < dx.Length; k++)
                    {
                        int nx = x + dx[k];
                        int ny = y + dy[k];
                        int z = ny * width + nx;

                        if (nx >= 0 && nx < width && ny >= 0 && ny < width)
                        {
                            int neighbor = ny * width + nx;
                            float waterPassed = Mathf.Min(water[j], (topOfWaterNeighbors[k] / sumOfNeighbors) * water[j]);
                            float sedimentCapacity = 0f;

                            if (k >= 4)
                                waterPassed *= 0.7071f;

                            deltaWater[j] -= waterPassed;
                            deltaWater[neighbor] += waterPassed;

                            float slope = topOfWaterNeighbors[k];
                            sedimentCapacity = capacity * waterPassed * slope;

                            if (sediment[j] > sedimentCapacity)
                            {
                                float deposit = deposition * (sediment[j] - sedimentCapacity);
                                deltaHeights[j] += deposit;
                                deltaSediment[j] -= deposit;
                                deltaSediment[neighbor] += sedimentCapacity;
                            }
                            else
                            {
                                float soft = softness * (sedimentCapacity - sediment[j]);
                                deltaHeights[j] -= soft;
                                deltaSediment[j] -= sediment[j];
                                deltaSediment[neighbor] += sediment[j] + soft;
                            }

                        }
                    }
                }
                else
                {
                    float deposit = deposition * sediment[j];
                    deltaHeights[j] += deposit;
                    deltaSediment[j] -= deposit;
                }

            }
            for (int k = 0; k < heights.Length; k++)
            {
                water[k] += deltaWater[k];
                sediment[k] += deltaSediment[k];
                heights[k] += deltaHeights[k];
            }


        }

        //for(int iter = 0; iter < iterations; iter++)
        //{
        //    for (int i = 0; i < heights.Length; i++)
        //    {
        //        int x = i % width;
        //        int y = i / width;


        //        int[] dx = { -1, 1, 0, 0 };
        //        int[] dy = { 0, 0, -1, 1 };

        //        for(int j = 0; j < dx.Length; j++)
        //        {
        //            int nx = x + dx[j];
        //            int ny = y + dy[j];
        //            int neighbor = ny * width + nx;

        //            if (nx >= 0 && nx < width && ny >= 0 && ny < width)
        //            {
        //                float deltaWater = Mathf.Min(water[i], (water[i] - heights[i]) - (water[neighbor] - heights[neighbor]));

        //                if (deltaWater <= 0)
        //                {
        //                    newHeights[i] = heights[i] + deposition * sediment[i];
        //                    newSediment[i] = (1 - deposition) * sediment[i];
        //                }
        //                else
        //                {
        //                    newWater[i] = water[i] - deltaWater;
        //                    newWater[neighbor] = water[neighbor] + deltaWater;
        //                    float sedimentCapacity = capacity * deltaWater;

        //                    if (sediment[i] >= sedimentCapacity)
        //                    {
        //                        newSediment[neighbor] = sediment[neighbor] + sedimentCapacity;
        //                        newHeights[i] = heights[i] + deposition * (sediment[i] - sedimentCapacity);
        //                        newSediment[i] = (1 - deposition) * (sediment[i] - sedimentCapacity);
        //                    }
        //                    else
        //                    {
        //                        newSediment[neighbor] = sediment[neighbor] + sediment[i] + softness * (sedimentCapacity - sediment[i]);
        //                        newHeights[i] = heights[i] - softness * (sedimentCapacity - sediment[i]);
        //                        newSediment[i] = 0;
        //                    }
        //                }
        //            }
        //        }

        //    }

        //    heights = newHeights;
        //    water = newWater;
        //    sediment = newSediment;
        //}

        return heights;
    }
}

