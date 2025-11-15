using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Mathematics;
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
    [SerializeField] private float gain = 0.5f; //change in amplitude
    [SerializeField] private float frequency = 1f;
    [SerializeField] private float lacunarity = 2f; //change in frequency
    [SerializeField] private int octaves = 4;
    enum NoiseType
    {
        simplex,
        perlin,
        voronoi,
        none,
    }
    enum NoiseModificator
    {
        domain_warping,
        none,
    }
    enum DerivativeModificator
    {
        derivative_simplex,
        none,
    }

    [SerializeField] private NoiseType noiseType = NoiseType.perlin;
    [SerializeField] private NoiseModificator noiseModificator = NoiseModificator.none;
    [SerializeField] private DerivativeModificator derivativeModificator = DerivativeModificator.none;

    [SerializeField][Range(-1, 1)] private float sharpness;

    [Header("Domain Warping")]
    [SerializeField] private float[] warpingImpact;
    [SerializeField] private float[] warpValues = new float[] { 0, 0, 5.2f, 1.3f, 1.7f, 9.2f, 8.3f, 2.8f };

    bool terrainGenerated = false;

    void Awake()
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
        ClearTerrainHeights();
        if (noiseType == NoiseType.perlin)
        {
            GenerateTerrainPerlin();
        }
        else if (noiseType == NoiseType.simplex)
        {
            GenerateTerrainSimplex();
        }
        else if (noiseType == NoiseType.voronoi)
        {
            GenerateTerrainVoronoi();
        }
        else
        {

        }
        mesh.vertices = vertices.ToArray();
    }

    private void ClearTerrainHeights()
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = new Vector3(vertices[i].x, transform.position.y, vertices[i].z);
        }
    }

    private void GenerateMeshData(Vector2 size, int resolution)
    {
        vertices = new List<Vector3>();
        uv = new List<Vector2>();
        float xPerStep = size.x / resolution;
        float yPerStep = size.y / resolution;
        for (int y = 0; y < resolution + 1; y++)
        {
            for (int x = 0; x < resolution + 1; x++)
            {
                vertices.Add(new Vector3(x * xPerStep, transform.position.y, y * yPerStep));
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

        for (int i = 0; i < vertices.Count; i++)
        {
            float noiseValue = 0f;
            float amp = amplitude;
            float freq = frequency;
            Vector2 vertex = new Vector2(vertices[i].x, vertices[i].z);

            noiseValue = PerlinNoise(freq, amp, vertex);

            if (noiseModificator == NoiseModificator.domain_warping)
            {
                Vector2 warpedCoords1 = new Vector2(PerlinNoise(freq, amp, vertex + new Vector2(warpValues[0], warpValues[1])),
                PerlinNoise(freq, amp, vertex + new Vector2(warpValues[2], warpValues[3])));

                Vector2 warpedCoords2 = new Vector2(PerlinNoise(freq, amp, vertex + new Vector2(warpValues[4], warpValues[5])
                    + warpingImpact[0] * warpedCoords1),
                    PerlinNoise(freq, amp, vertex + new Vector2(warpValues[6], warpValues[7]) + warpingImpact[1] * warpedCoords1));

                noiseValue = PerlinNoise(freq, amp, vertex + (warpingImpact[2] * warpedCoords2));
            }


            vertices[i] = new Vector3(vertices[i].x, vertices[i].y + 0.5f + noiseValue * heightMultiplier, vertices[i].z);

        }
    }

    float PerlinNoise(float freq, float amp, Vector2 vertex)
    {
        float noiseValue = 0f;
        for (int o = 0; o < octaves; o++)
        {
            float xCoord = (float)vertex.x / (xSize - 1) * scale * freq + offset.x;
            float zCoord = (float)vertex.y / (ySize - 1) * scale * freq + offset.y;

            //noiseValue += Unity.Mathematics.noise.cellular(new Unity.Mathematics.float2(xCoord, zCoord)).y - 0.5f * amp;

            noiseValue += (Mathf.PerlinNoise(xCoord, zCoord) - 0.5f) * amp;
            //noiseValue += (Unity.Mathematics.noise.cnoise(new Unity.Mathematics.float2(xCoord, zCoord)) - 0.5f) * amp;
            float billowNoise = Mathf.Abs(noiseValue);
            float ridgeNoise = 0f - billowNoise;

            noiseValue = Mathf.Lerp(noiseValue, ridgeNoise, Mathf.Max(0f, sharpness));
            noiseValue = Mathf.Lerp(noiseValue, billowNoise, -Mathf.Min(0f, sharpness));



            amp *= gain;
            freq *= lacunarity;
        }
        return noiseValue;
    }

    float VoronoiNoise(float freq, float amp, Vector2 vertex)
    {
        float noiseValue = 0f;
        for (int o = 0; o < octaves; o++)
        {
            float xCoord = (float)vertex.x / (xSize - 1) * scale * freq + offset.x;
            float zCoord = (float)vertex.y / (ySize - 1) * scale * freq + offset.y;
            Vector3 noise = Noise.VoronoiNoise.Noise(xCoord, zCoord);
            //noiseValue += Noise.VoronoiNoise.CellHeight(noise.y, noise.z) * 5f;
            noiseValue += noise.x * amp;

            amp *= gain;
            freq *= lacunarity;
        }
        return noiseValue;
    }

    void GenerateTerrainVoronoi()
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            float noiseValue = 0f;
            float amp = amplitude;
            float freq = frequency;

            Vector2 vertex = new Vector2(vertices[i].x, vertices[i].z);

            noiseValue = VoronoiNoise(freq, amp, vertex);
            vertices[i] = new Vector3(vertices[i].x, vertices[i].y + 0.5f + noiseValue * heightMultiplier, vertices[i].z);
        }
    }

    void GenerateTerrainSimplex()
    {

        for (int i = 0; i < vertices.Count; i++)
        {
            float noiseValue = 0f;
            float amp = amplitude;
            float freq = frequency;

            Vector2 vertex = new Vector2(vertices[i].x, vertices[i].z);


            noiseValue = SimplexNoise(freq, amp, vertex);

            if (noiseModificator == NoiseModificator.domain_warping)
            {
                Vector2 warpedCoords1 = new Vector2(SimplexNoise(freq, amp, vertex + new Vector2(warpValues[0], warpValues[1])),
                SimplexNoise(freq, amp, vertex + new Vector2(warpValues[2], warpValues[3])));

                Vector2 warpedCoords2 = new Vector2(SimplexNoise(freq, amp, vertex + new Vector2(warpValues[4], warpValues[5])
                    + warpingImpact[0] * warpedCoords1),
                    SimplexNoise(freq, amp, vertex + new Vector2(warpValues[6], warpValues[7]) + warpingImpact[1] * warpedCoords1));

                noiseValue = SimplexNoise(freq, amp, vertex + (warpingImpact[2] * warpedCoords2));
            }

            vertices[i] = new Vector3(vertices[i].x, vertices[i].y + 0.5f + noiseValue * heightMultiplier, vertices[i].z);

        }
    }

    float SimplexNoise(float freq, float amp, Vector2 vertex)
    {
        float noiseValue = 0f;

        for (int o = 0; o < octaves; o++)
        {
            float xCoord = (float)vertex.x / (xSize - 1) * scale * freq + offset.x;
            float zCoord = (float)vertex.y / (ySize - 1) * scale * freq + offset.y;

            Vector3 noise = Noise.SimplexNoise.Noise(xCoord, zCoord);
            //Vector3 noise = Unity.Mathematics.noise.srdnoise(new float2(xCoord, zCoord));
            if (derivativeModificator == DerivativeModificator.derivative_simplex)
            {
                Vector2 dsum = new Vector2(noise.y, noise.z);
                noiseValue += amp * noise.x * (1.0f / (1 + 150 * Vector2.Dot(dsum, dsum)));
            }
            else
            {
                noiseValue += (noise.x - 0.5f) * amp;
            }

            float billowNoise = Mathf.Abs(noiseValue);
            float ridgeNoise = 0f - billowNoise;

            noiseValue = Mathf.Lerp(noiseValue, ridgeNoise, Mathf.Max(0f, sharpness));
            noiseValue = Mathf.Lerp(noiseValue, billowNoise, -Mathf.Min(0f, sharpness));

            //Vector3 noise = Unity.Mathematics.noise.srdnoise(new float2(xCoord, zCoord));
            /*            float altitudeErosion = 5f;
                        float ridgeErosion = 2f;*/

            /* Vector2 rDsum = new Vector2(noise.y, noise.z) * ridgeErosion;

             amp *= Mathf.Lerp(gain, gain + Mathf.SmoothStep(0f, 1f, noiseValue), altitudeErosion);
             amp = amp * (1f - (ridgeErosion / (1f + Vector2.Dot(rDsum, rDsum))));*/

            amp *= gain;
            freq *= lacunarity;
        }
        return noiseValue;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Space) && !terrainGenerated)
        {
            GenerateTerrain();
            mesh.RecalculateNormals();
        }

        if (!Input.GetKey(KeyCode.Space))
        {
            terrainGenerated = false;
        }
    }

}