using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Noise;
using Unity.VisualScripting;
using UnityEngine.Tilemaps;
using System.Linq;

public class TerrainGeneration : MonoBehaviour
{
    [Range(0.1f, 10f)] [SerializeField] private float scale = 2f; 
    [SerializeField] private float heightMultiplier = 0.2f;       
    [SerializeField] private Vector2 offset;
    private bool terrainGenerated = false;
    [SerializeField] private float amplitude = 1f;
    [SerializeField] private float frequency = 1f;
    [SerializeField] private int octaves = 4;

    [Header("")]
    [SerializeField] private Terrain[] terrains;

    enum NoiseType
    {
        simplex,
        perlin,
        none,
    }

    [SerializeField] private NoiseType noiseType = NoiseType.perlin;

    void Awake()
    {
        terrains = GetComponentsInChildren<Terrain>();
        //terrain = GetComponent<Terrain>();
        GenerateTerrain();
        
    }

    void GenerateTerrain()
    {
        foreach (var terrain in terrains)
        {
            int width = terrain.terrainData.heightmapResolution;
            int length = terrain.terrainData.heightmapResolution;

            float[,] originalHeights = new float[length, width];

            terrain.terrainData.SetHeights(0, 0, originalHeights);

            float[,] heights = terrain.terrainData.GetHeights(0, 0, width, length);

            if (noiseType == NoiseType.perlin)
            {
                GenerateTerrainPerlin(width, length, heights, terrain);
            }
            else if (noiseType == NoiseType.simplex)
            {
                GenerateTerrainSimplex(width, length, heights, terrain);
            }
            else
            {

            }
            CreateTexture(terrain);
            
        }
        int width1 = terrains[0].terrainData.heightmapResolution;
        int length1 = terrains[0].terrainData.heightmapResolution;
        float[] h = terrains[0].terrainData.GetHeights(0, 0, width1, length1).Cast<float>().Select(c => c).ToArray();
        float minheight = h.Min();
        FlattenTerrain(terrains[0], minheight, 10, 40, 60);

        terrainGenerated = true;
    }

    void GenerateTerrainPerlin(int width, int length, float[,] heights, Terrain terrain, int blockSize = 10)
    {
        float worldOffsetX = (terrain.transform.position.x / terrain.terrainData.size.x) * (width-1);
        float worldOffsetZ = (terrain.transform.position.z / terrain.terrainData.size.z) * (length-1);

        for (int y = 0; y < length; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float noiseValue = 0f;
                float amp = amplitude;
                float freq = frequency;
                float maxAmp = 0f;

                for (int o = 0; o < octaves; o++)
                {
                    float xCoord = (float)(x+worldOffsetX) / (width-1) * scale * freq + offset.x;
                    float zCoord = (float)(y+worldOffsetZ) / (length-1) * scale * freq + offset.y;


                    noiseValue += (Mathf.PerlinNoise(xCoord, zCoord) - 0.5f) * amp;

                    maxAmp += amp;

                    amp *= 0.5f;
                    freq *= 2f;
                }

    
                heights[y, x] = 0.5f + noiseValue * heightMultiplier;
            }

            
        }
        terrain.terrainData.SetHeights(0, 0, heights);
    }

    void GenerateTerrainSimplex(int width, int length, float[,] heights, Terrain terrain, int blockSize = 10)
    {
        float worldOffsetX = (terrain.transform.position.x / terrain.terrainData.size.x) * (width - 1);
        float worldOffsetZ = (terrain.transform.position.z / terrain.terrainData.size.z) * (length - 1);

        for (int y = 0; y < length; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float noiseValue = 0f;
                float amp = amplitude;
                float freq = frequency;
                float maxAmp = 0f;

                for (int o = 0; o < octaves; o++)
                {
                    float xCoord = (float)(x + worldOffsetX) / (width - 1) * scale * freq + offset.x;
                    float zCoord = (float)(y + worldOffsetZ) / (length - 1) * scale * freq + offset.y;

                    //noiseValue += (Unity.Mathematics.noise.snoise( new Unity.Mathematics.float2(xCoord, zCoord)) - 0.5f) * amp;
                    noiseValue += (SimplexNoise.Noise(xCoord, zCoord) - 0.5f) * amp;


                    maxAmp += amp;

                    amp *= 0.5f;
                    freq *= 2f;
                }


                heights[y, x] = 0.5f + noiseValue * heightMultiplier;
            }


        }
        terrain.terrainData.SetHeights(0, 0, heights);
    }

    enum TerrainLayers
    {
        SAND = 0,
        GRASS = 1,
        STONE = 2,
        WATER = 3,
        SNOW = 4,
    }

    private void CreateTexture(Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;
        int width = terrainData.alphamapWidth;
        int length = terrainData.alphamapHeight;
        int layers = terrainData.alphamapLayers;

        float[,,] splatmapData = new float[width, length, layers];

        for (int z = 0; z < length; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float normX = x * 1.0f / (width - 1);
                float normZ = z * 1.0f / (length - 1);
                float terrainHeight = terrainData.GetHeight(
                    Mathf.RoundToInt(normZ * terrainData.heightmapResolution),
                    Mathf.RoundToInt(normX * terrainData.heightmapResolution)
                ) / terrainData.size.y;

                float[] textureWeights = new float[layers];
                /*if (terrainHeight < 0.18f)
                {
                    textureWeights[(int)TerrainLayers.WATER] = 1; //water
                }*/
                
                if (terrainHeight < 0.2f)
                {
                    textureWeights[(int)TerrainLayers.SAND] = 1; // sand
                }
                else if(terrainHeight < 0.24f)
                {
                    float t = Mathf.InverseLerp(0.2f, 0.24f, terrainHeight);
                    textureWeights[(int)TerrainLayers.SAND] = 1 - t;
                    textureWeights[(int)TerrainLayers.GRASS] = t;
                }
                else if(terrainHeight < 0.4f && terrainHeight > 0.24f)
                {

                    textureWeights[(int)TerrainLayers.GRASS] = 1; //grass
                }
                else if(terrainHeight < 0.70f)
                {
                    float t = Mathf.InverseLerp(0.4f, 0.6f, terrainHeight);
                    textureWeights[(int)TerrainLayers.GRASS] = 1-t;
                    textureWeights[(int)TerrainLayers.STONE] = t; // stone
                }
                else
                {
                    float t = Mathf.InverseLerp(0.85f, 1, terrainHeight);
                    textureWeights[(int)TerrainLayers.STONE] = 1-t; // stone
                    textureWeights[(int)TerrainLayers.SNOW] = t; // snow
                }

                for (int i = 0; i < layers; i++)
                    splatmapData[x, z, i] = textureWeights[i];
            }
        }

        terrainData.SetAlphamaps(0, 0, splatmapData);
    }

    private void FlattenTerrain(Terrain terrain, float height, int x, int y, int size)
    {
        TerrainData terrainData = terrain.terrainData;
        float[,] heights = new float[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                heights[i, j] = height + Random.Range(0.02f, 0.022f);
            }
        }
        terrainData.SetHeights(x, y, heights);
    }

    /*private Texture2D CreateTexture(Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;
        int res = terrainData.heightmapResolution;
        Texture2D texture = new Texture2D(res, res);

        float maxheight = 0;
        float minheight = float.MaxValue;

        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float height = terrainData.GetHeight(x, y);
                if(height > maxheight)
                    maxheight = height;
                if(height < minheight)
                    minheight= height;
                texture.SetPixel(x, y, GetColor(height));
            }
        }
*//*        Debug.Log(maxheight);
        Debug.Log(minheight);*//*
        return texture;
    }*/

    /*private Color GetColor(float height)
    {
        if (height < 290) return new Color(0, 0, 144);
        else if (height < 320) return new Color(144, 121, 5);
        else if (height < 370) return new Color(0, 144, 0);
        else return new Color(111, 111, 111);
    }*/

    void Update()
    {
        if (Input.GetKey(KeyCode.Space) && !terrainGenerated)
        {
            terrains = GetComponentsInChildren<Terrain>();
            GenerateTerrain();
        }

        if (!Input.GetKey(KeyCode.Space))
        {
            terrainGenerated = false;
        }
    }


}
