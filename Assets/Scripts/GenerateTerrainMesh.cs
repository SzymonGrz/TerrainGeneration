using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class GenerateTerrainMesh : MonoBehaviour
{
    [Header("Plane Generation")]
    [SerializeField] private int xSize;
    [SerializeField] private int ySize;
    [SerializeField] private int planeResolution;

    private List<Vector3> vertices;
    private List<Vector2> uv;
    private List<int> triangles;


    private GameObject meshObject;
    private Mesh mesh;

    [Header("Noise Generation")]
    [Range(0.1f, 10f)][SerializeField] private float scale;
    [SerializeField] private float heightMultiplier = 0.2f;
    [SerializeField] private Vector2 offset;
    [SerializeField] private float amplitude = 1f;
    [SerializeField] private float frequency = 1f;
    [SerializeField] private int octaves = 4;
    enum NoiseType
    {
        simplex,
        perlin,
        none,
    }

    [SerializeField] private NoiseType noiseType = NoiseType.perlin;

    void Start()
    {


        mesh = new Mesh();
        mesh.name = "CustomMesh";
        //meshObject = new GameObject("Terrain", typeof(MeshRenderer), typeof(MeshFilter));
        meshObject = this.gameObject;
        meshObject.GetComponent<MeshFilter>().mesh = mesh;
        //planeResolution = Mathf.Clamp(planeResolution, 1, 50);
        Vector2 planeSize = new Vector2(xSize, ySize);
        GenerateMeshData(planeSize, planeResolution);
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.uv = uv.ToArray();
        mesh.triangles = triangles.ToArray();
        GenerateTerrain();
        mesh.RecalculateNormals();
    }

    public void GenerateTerrain()
    {

        if (noiseType == NoiseType.perlin)
        {
            GenerateTerrainPerlin();
        }
        else if (noiseType == NoiseType.simplex)
        {
            GenerateTerrainSimplex();
        }
        else
        {

        }
        mesh.vertices = vertices.ToArray();
    }

    private void GenerateMeshData(Vector2 size, int resolution)
    {
        vertices = new List<Vector3>();
        uv = new List<Vector2>();
        float xPerStep = size.x / resolution;
        float yPerStep = size.y / resolution;
        for (int y = 0; y < resolution+1; y++)
        {
            for (int x = 0; x < resolution+1; x++)
            {
                vertices.Add(new Vector3(x*xPerStep, 0, y*yPerStep));
                uv.Add(new Vector2((float)x / resolution, (float)y / resolution));
            }
        }
        

        triangles = new List<int>();
        for (int row = 0; row < resolution; row++)
        {
            for (int column = 0; column < resolution; column++)
            {
                int i = row * (resolution + 1) + column;

                triangles.Add(i);
                triangles.Add(i + resolution + 1);
                triangles.Add(i + resolution + 2);

                triangles.Add(i);
                triangles.Add(i + resolution + 2);
                triangles.Add(i + 1);
            }
        }


    }
    
    void GenerateTerrainPerlin()
    { 

        for(int i= 0; i < vertices.Count;i++)
        { 
                float noiseValue = 0f;
                float amp = amplitude;
                float freq = frequency;
                float maxAmp = 0f;

                for (int o = 0; o < octaves; o++)
                {
                    float xCoord = (float)vertices[i].x / (xSize-1) * scale * freq + offset.x;
                    float zCoord = (float)vertices[i].z / (ySize-1) * scale * freq + offset.y;


                    noiseValue += (Mathf.PerlinNoise(xCoord, zCoord) - 0.5f) * amp;

                    maxAmp += amp;

                    amp *= 0.5f;
                    freq *= 2f;
                }


            vertices[i] = new Vector3(vertices[i].x, 0.5f + noiseValue * heightMultiplier, vertices[i].z);

            
        }
    }

    void GenerateTerrainSimplex()
    {

        for (int i = 0; i < vertices.Count; i++)
        {
            float noiseValue = 0f;
            float amp = amplitude;
            float freq = frequency;
            float maxAmp = 0f;

            for (int o = 0; o < octaves; o++)
            {
                float xCoord = (float)vertices[i].x / (xSize - 1) * scale * freq + offset.x;
                float zCoord = (float)vertices[i].z / (ySize - 1) * scale * freq + offset.y;


                noiseValue += (Noise.SimplexNoise.Noise(xCoord, zCoord) - 0.5f) * amp;

                maxAmp += amp;

                amp *= 0.5f;
                freq *= 2f;
            }


            vertices[i] = new Vector3(vertices[i].x, 0.5f + noiseValue * heightMultiplier, vertices[i].z);


        }
    }

    /*private Texture2D CreateTexture()
    {
        Texture2D texture = new Texture2D(xSize, ySize);
        for(int i= 0; i < vertices.Count; i++)
        {
            float height = vertices[i].y;
            texture.SetPixel((int)vertices[i].x, (int)vertices[i].y, GetColor(height));
            
        }

        return texture;
    }

    private Color GetColor(float height)
    {
        if (height < 290) return new Color(0, 0, 144);
        else if (height < 320) return new Color(144, 121, 5);
        else if (height < 370) return new Color(0, 144, 0);
        else return new Color(111, 111, 111);
    }*/
}
