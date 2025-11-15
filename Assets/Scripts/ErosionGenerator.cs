using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ErosionGenerator : MonoBehaviour
{
    [SerializeField] private GameObject terrain;

    [SerializeField] private float inertia = 0.05f;
    [SerializeField] private float initialSpeed = 1f;
    [SerializeField] private float initialWaterVolume = 1f;
    [SerializeField] private float minCapacity = .01f;
    [SerializeField] private float capacity = 4;
    [SerializeField] private float deposition = 0.3f;
    [SerializeField] private float erosion = 0.3f;
    [SerializeField] private float gravity = 4;
    [SerializeField] private float evaporation = 0.01f;
    [SerializeField] private int radius = 3;
    [SerializeField] private int maxPath = 30;

    [SerializeField] private int mapSize = 256;
    [SerializeField] private int iterations = 30000;

    private float[][] weights;
    private int[][] indices;

    private Vector3[] vertices;


    void Start()
    {
        CalculateWeights(mapSize ,radius);
        Mesh mesh = terrain.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        float[] heights = vertices.Select(p => p.y).ToArray();

        float minHeight = heights.Min();
        if (minHeight < 0f)
        {
            for (int i = 0; i < heights.Length; i++)
                heights[i] -= minHeight; 
        }

        float[] newHeights = Erode(heights, mapSize, iterations);

        for (int i = 0; i < newHeights.Length; i++)
            newHeights[i] += minHeight;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].y = newHeights[i];
        }
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    /*void Update()
    {
        if(iterations > 0)
        {
            Erode(heights, mapSize, iterations);
            //float[] newHeights = heights; 
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].y = heights[i];
            }
            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            iterations--;
        }
    }*/

    public float[] Erode(float[] heights, int size, int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            float posX = Random.Range(1f, size - 3);
            float posY = Random.Range(1f, size - 3);
            Vector2 dir = new Vector2(0, 0);
            float speed = initialSpeed;
            float water = initialWaterVolume;
            float sediment = 0;

            for (int lifetime = 0; lifetime < maxPath; lifetime++)
            {
                int nodeX = (int)posX;
                int nodeY = (int)posY;

                float cellOffsetX = posX - nodeX;
                float cellOffsetY = posY - nodeY;

                int posIndex = nodeY * size + nodeX;

                Vector3 gradientAndHeight = calculateGradientAndHeight(heights, size, posX, posY);
                dir = dir * inertia - new Vector2(gradientAndHeight.x, gradientAndHeight.y) * (1 - inertia);


                float len = Mathf.Sqrt(dir.x * dir.x + dir.y * dir.y);
                if (len != 0)
                {
                    dir.x /= len;
                    dir.y /= len;
                }
                Vector2 pos = new Vector2(posX, posY);
                pos += dir;
                posX = pos.x;
                posY = pos.y;
                

                if ((dir.x == 0 && dir.y == 0) || posX < 0 || posX >= size - 1 || posY < 0 || posY >= size - 1)
                {
                    break;
                }

                float heightDif = calculateGradientAndHeight(heights, size, posX, posY).z - gradientAndHeight.z;

                //float slope = Mathf.Max(Mathf.Abs(heightDif), minSlope);
                float sedimentCapacity = Mathf.Max(-heightDif * speed * water * capacity, minCapacity);

                if (sediment > sedimentCapacity || heightDif > 0)
                {
                    float amount = (heightDif > 0) ? Mathf.Min(heightDif, sediment) : (sediment - sedimentCapacity) * deposition;
                    sediment -= amount;
                    sediment = Mathf.Max(0f, sediment);

                    if (nodeX < size - 2 && nodeY < size - 2)
                    {
                        heights[posIndex] += amount * (1 - cellOffsetX) * (1 - cellOffsetY);
                        heights[posIndex + 1] += amount * cellOffsetX * (1 - cellOffsetY);
                        heights[posIndex + size] += amount * (1 - cellOffsetX) * cellOffsetY;
                        heights[posIndex + 1 + size] += amount * cellOffsetX * cellOffsetY;
                    }
                    else
                    {
                        break;
                    }

                }
                else
                {
                    float amountToErode = Mathf.Min((sedimentCapacity - sediment) * erosion, -heightDif);

                    for (int k = 0; k < indices[posIndex].Length; k++)
                    {
                        int nodeIndex = indices[posIndex][k];
                        float weighedErodeAmount = amountToErode * weights[posIndex][k];
                        float deltaSediment = (heights[nodeIndex] < weighedErodeAmount) ? heights[nodeIndex] : weighedErodeAmount;
                        /*if(deltaSediment < 0)
                        {
                            Debug.Log(heights[nodeIndex]);
                        }*/
                        heights[nodeIndex] -= deltaSediment;
                        sediment += deltaSediment;
                    }

                }

                speed = Mathf.Sqrt(Mathf.Max(0f, speed * speed + heightDif * gravity));
                water = water * (1 - evaporation);
            }

            
        }
        return heights;

    }

    public Vector3 calculateGradientAndHeight(float[] heights, int size, float posX, float posY)
    {
        int coordX = (int)posX;
        int coordY = (int)posY;

        float u = posX - coordX;
        float v = posY - coordY;

        int indexXY = (coordY * size) + coordX;

        float xy = heights[indexXY];
        float x1y = heights[indexXY + 1];
        float xy1 = heights[indexXY + size]; 
        float x1y1 = heights[indexXY + size + 1];

        float height = xy * (1 - u) * (1 - v) + x1y * u * (1 - v) + xy1 * (1 - u) * v + x1y1 * u * v;

        Vector3 gradientAndHeight = new Vector3(
            (x1y - xy) * (1 - v) + (x1y1 - xy1) * v,
            (xy1 - xy) * (1 - u) + (x1y1 - x1y) * u,
            height
        );


        return gradientAndHeight;

    }


    public void CalculateWeights(int mapSize, int radius)
    {
        indices = new int[mapSize * mapSize][];
        weights = new float[mapSize * mapSize][];

        float total = 0f;

        int[] xOffsets = new int[radius * radius * 4];
        int[] yOffsets = new int[radius * radius * 4];
        float[] weightsA = new float[radius*radius*4];
        int addIndex = 0;

        for (int k = 0; k < indices.GetLength(0); k++)
        {
            int centreX = k % mapSize;
            int centreY = k / mapSize;

            total = 0;
            addIndex = 0;

            //if (centreY <= radius || centreY >= mapSize - radius || centreX <= radius + 1 || centreX >= mapSize - radius)
            {
                

                for (int y = -radius; y <= radius; y++)
                {
                    for (int x = -radius; x <= radius; x++)
                    {
                        float sqrDst = x * x + y * y;
                        if (sqrDst < radius * radius)
                        {
                            int coordX = centreX + x;
                            int coordY = centreY + y;

                            if (coordX >= 0 && coordX < mapSize && coordY >= 0 && coordY < mapSize)
                            {
                                float weight = 1 - Mathf.Sqrt(sqrDst) / radius;
                                total += weight;
                                weightsA[addIndex] = weight;
                                xOffsets[addIndex] = x;
                                yOffsets[addIndex] = y;
                                addIndex++;
                            }
                        }
                    }
                }

            }

            int numEntries = addIndex;
            indices[k] = new int[numEntries];
            weights[k] = new float[numEntries];

            for (int j = 0; j < numEntries; j++)
            {
                indices[k][j] = (yOffsets[j] + centreY) * mapSize + xOffsets[j] + centreX;
                weights[k][j] = weightsA[j] / total;
            }

        }

    }


}
