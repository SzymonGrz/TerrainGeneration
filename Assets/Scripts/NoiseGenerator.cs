using System;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGenerator: MonoBehaviour
{
    [Header("Plane Generation")]
    [SerializeField] private int xSize;
    [SerializeField] private int ySize;
    [SerializeField] private int planeResolution;

    private Vector3[] vertices;

    [SerializeField] private GameObject plane;
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
    //enum NoiseModificator
    //{
    //    domain_warping,
    //    turbulence,
    //    none,
    //}

    enum DerivativeModificator
    {
        derivative_simplex,
        none,
    }

    [SerializeField] private NoiseType noiseType = NoiseType.perlin;

    [SerializeField] private bool domainWarping = false;
    [SerializeField] private bool turbulence = false;

    //[SerializeField] private NoiseModificator noiseModificator = NoiseModificator.none;
    [SerializeField] private DerivativeModificator derivativeModificator = DerivativeModificator.none;
    [SerializeField] private float derivativeMultiplication = 150;

    [SerializeField][Range(-1, 1)] private float sharpness;

    [Header("Domain Warping")]
    [SerializeField] private float[] warpingImpact;
    [SerializeField] private float[] warpValues = new float[] { 0, 0, 5.2f, 1.3f, 1.7f, 9.2f, 8.3f, 2.8f };

    [Header("Experimental")]
    [SerializeField][Range(0, 1)] private float interpolation;
    [SerializeField] private bool isTerrace = false;
    [SerializeField] private bool testing = false;
    [SerializeField] private int riverRadius;

    private Vector2 center;

    bool terrainGenerated = false;

    private float[] rivermask;
    private float[] distanceField;

    public void Start()
    {

        mesh = plane.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;

        rivermask = LSystemRiver.getRiverMask();
        if(rivermask == null)
        {
            rivermask = new float[vertices.Length];
        }

        distanceField = BlurMask(rivermask, planeResolution, 6, riverRadius);
        GenerateTerrain();

        mesh.RecalculateNormals();
    }

    public void GenerateTerrain()
    {
        //ClearTerrainHeights();
        if (noiseType == NoiseType.perlin)
        {
            //--------------Pomiar czasu--------------------------
            //for (int i = 0; i < 5; i++)
            //{
              //  System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
                //GenerateTerrainPerlin();
                //long end = sw.ElapsedMilliseconds;
                //Debug.Log(end);
            //}
            //-------------------------------------------------------

            GenerateTerrainPerlin();
        }
        else if (noiseType == NoiseType.simplex)
        {
            //---------------Pomiar czasu---------------------
            //GenerateTerrainSimplex();
            //System.GC.Collect();
            //System.GC.WaitForPendingFinalizers();
            //System.GC.Collect();
            //for (int i = 0; i < 5; i++)
            //{
            //    System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            //    GenerateTerrainSimplex();
            //    long end = sw.ElapsedMilliseconds;
            //    sw.Stop();
            //    Debug.Log(end);
            //}
            //-------------------------------------------------

            GenerateTerrainSimplex();
        }
        else if (noiseType == NoiseType.voronoi)
        {
            //for (int i = 0; i < 5; i++)
            {
                //System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
                GenerateTerrainVoronoi();
                //long end = sw.ElapsedMilliseconds;
                //Debug.Log(end);
            }
        }
        else
        {

        }
        mesh.vertices = vertices;

        if (!testing)
            return;

        float cellSize = xSize / planeResolution;

        //float[] values = { 0, 5, 10, 20, 40 };

        //float[] heights;

        //List<ExperimentResultDW> results = new List<ExperimentResultDW>();

        //foreach (float w0 in values)
        //{
        //    foreach (float w1 in values)
        //    {
        //        foreach (float w2 in values)
        //        {
        //            warpingImpact = new float[] { w0, w1, w2 };

        //            ClearTerrainHeights();
        //            GenerateTerrainSimplex();

        //            heights = vertices.Select(p => p.y).ToArray();

        //            float stdDev = Metrics.StdDev(vertices);
        //            float avgGradient = Metrics.MeanGrad(vertices, planeResolution, cellSize);
        //            float varG = Metrics.GradientVariance();
        //            float ruggedness = Metrics.Ruggedness(heights, planeResolution);
        //            float meanAbsCurvature = Metrics.MeanAbsCurvature(heights, planeResolution);
        //            float skewness = Metrics.HeightSkewness(heights, planeResolution);

        //            results.Add(new ExperimentResultDW
        //            {
        //                w0 = w0,
        //                w1 = w1,
        //                w2 = w2,
        //                stdDev = stdDev,
        //                avgGradient = avgGradient,
        //                varGradient = varG,
        //                ruggedness = ruggedness,
        //                meanAbsCurvature = meanAbsCurvature,
        //                skewness = skewness,
        //            });


        //        }
        //    }
        //}
        //Metrics.SaveDWToCSV(results, "domainWarping.csv");

        //float[] heights;

        float[] values = { 1 };
        List<(float, float)> results = new List<(float, float)> ();
        //List<ExperimentResultNoise> results = new List<ExperimentResultNoise>();
        string parameterName = "DomainWarping";

        for (int i = 0; i < values.Length; i++)
        {
            //----------PARAMETR-----------

            sharpness = values[i];

            //--------------------------------

            for (int j = 0; j < 10; j++)
            {
                ClearTerrainHeights();

                float start = Time.realtimeSinceStartup;
                GenerateTerrainSimplex();
                start -= Time.realtimeSinceStartup;
                results.Add((values[i], -start*1000));

            }

            //--------------Odchylenie standardowe-----------------------

            //heights = vertices.Select(p => p.y).ToArray();

            //float stdDev = Metrics.StdDev(vertices);
            ////--------------Œredni gradient-------------

            //float avgGradient = Metrics.MeanGrad(vertices, planeResolution, cellSize);
            ////----------Wariancja gradientów--------------------

            //float varG = Metrics.GradientVariance();

            //float ruggedness = Metrics.Ruggedness(heights, planeResolution);
            //float meanAbsCurvature = Metrics.MeanAbsCurvature(heights, planeResolution);
            //float skewness = Metrics.HeightSkewness(heights, planeResolution);

            //results.Add(
            //    new ExperimentResultNoise
            //    {
            //        parameter = values[i],
            //        stdDev = stdDev,
            //        avgGradient = avgGradient,
            //        varGradient = varG,
            //        ruggedness = ruggedness,
            //        meanAbsCurvature = meanAbsCurvature,
            //        skewness = skewness,
            //    });
        }

        Metrics.SaveTimeToCSV(results, "/SimplexNoise/time" + planeResolution +"DomainWarping" + ".csv", parameterName);

        //Metrics.SaveNoiseToCSV(results, "/SimplexNoise/BaseAmplitude5/" + parameterName + ".csv", parameterName);

    }

    private void ClearTerrainHeights()
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vertices[i].x, transform.position.y, vertices[i].z);
        }
    }

    void GenerateTerrainPerlin()
    {

        

        for (int i = 0; i < vertices.Length; i++)
        {
            float noiseValue = 0f;
            float amp = amplitude;
            float freq = frequency;
            Vector2 vertex = new Vector2(vertices[i].x, vertices[i].z);

            noiseValue = PerlinNoise(freq, amp, vertex, i);

            //----------------------------
            //test area        

            //noiseValue = Mathf.Max(PerlinNoise(freq, amp, vertex), SimplexNoise(freq, amp, vertex));
            //noiseValue = Mathf.Lerp(noiseValue, SimplexNoise(freq, amp, vertex), Mathf.Max(0f, interpolation));
            //noiseValue = Mathf.SmoothStep(PerlinNoise(freq, amp, vertex), SimplexNoise(freq, amp, vertex), Mathf.Max(0f, interpolation));

            //noiseValue = Mathf.Lerp(noiseValue, VoronoiNoise(freq, amp, vertex), Mathf.Max(0f, interpolation));
            //noiseValue = noiseValue * VoronoiNoise(freq, amp, vertex);
            //noiseValue = 0.65f*noiseValue + 0.35f*VoronoiNoise(freq, amp, vertex);

            //noiseValue = 0.65f * noiseValue + 0.35f * ValueNoise(freq, amp, vertex);

            //----------------------------

            if (domainWarping)
            {
                Vector2 warpedCoords1 = new Vector2(PerlinNoise(freq, amp, vertex + new Vector2(warpValues[0], warpValues[1]), i),
                PerlinNoise(freq, amp, vertex + new Vector2(warpValues[2], warpValues[3]), i));

                Vector2 warpedCoords2 = new Vector2(PerlinNoise(freq, amp, vertex + new Vector2(warpValues[4], warpValues[5])
                    + warpingImpact[0] * warpedCoords1, i),
                    PerlinNoise(freq, amp, vertex + new Vector2(warpValues[6], warpValues[7]) + warpingImpact[1] * warpedCoords1, i));

                noiseValue = PerlinNoise(freq, amp, vertex + (warpingImpact[2] * warpedCoords2), i);
            }

            //-------------EXPERIMENTAL---------------------
            //float riverInfluence = rivermask[i];
            //float noiseMultiplier = Mathf.SmoothStep(1f, 0f, riverInfluence);
            //noiseValue *= noiseMultiplier;
            //noiseValue -= riverDepth * riverInfluence;

            //-----------------------------------------

            vertices[i] = new Vector3(vertices[i].x, vertices[i].y + 0.5f + noiseValue * heightMultiplier, vertices[i].z);

        }

        //----------------------------------------

        if (isTerrace)
        {

            float minNoise = float.MaxValue;
            float maxNoise = float.MinValue;

            foreach (var vertex in vertices)
            {
                if (vertex.y < minNoise) minNoise = vertex.y;
                if (vertex.y > maxNoise) maxNoise = vertex.y;
            }

            for (int j = 0; j < vertices.Length; j++)
            {
                float noiseValue = vertices[j].y;
                int steps = 20;
                float normalizedNoise = (noiseValue - minNoise) / (maxNoise - minNoise);


                float terraced = Mathf.Floor(normalizedNoise * steps) / steps;

                float finalY = Mathf.Lerp(minNoise, maxNoise, terraced);

                vertices[j] = new Vector3(vertices[j].x, finalY, vertices[j].z);
            }
        }
        //-------------------------------------------------------

    }

    float PerlinNoise(float freq, float amp, Vector2 vertex, int index)
    {

        float noiseValue = 0f;
        for (int o = 0; o < octaves; o++)
        {
            float xCoord = (float)vertex.x / (xSize - 1) * scale * freq + offset.x;
            float zCoord = (float)vertex.y / (ySize - 1) * scale * freq + offset.y;

            //---------------------------------
            //Test area


            //float distance = Vector2.Distance(center, new Vector2(vertex.x, vertex.y));

            //Vector2 direction = new Vector2(0.5f, 0.5f).normalized;
            //float maxT =
            //    Mathf.Abs(direction.x) * xSize +
            //    Mathf.Abs(direction.y) * ySize;

            //float t = Vector2.Dot(vertex, direction);
            //float factor = Mathf.Clamp01(t / maxT);

            //---------------------------------

            float tempNoiseValue = (Mathf.PerlinNoise(xCoord, zCoord) - 0.5f);

            if (turbulence)
            {
                float turbulence = Mathf.Abs(tempNoiseValue);
                noiseValue += turbulence * amp;
            }
            else
            {
                //---------------------------------------
                //Test area

                //float ridge = (1f - Mathf.Abs(tempNoiseValue));
                //noiseValue += ridge * amp * noiseValue;


                //noiseValue += tempNoiseValue * amp * modulation;
                //noiseValue += tempNoiseValue * amp * factor;

                //---------------------------------------


                noiseValue += tempNoiseValue * amp;
            }
            float billowNoise = Mathf.Abs(noiseValue);
            float ridgeNoise = 1f - billowNoise;

            noiseValue = Mathf.Lerp(noiseValue, ridgeNoise, Mathf.Max(0f, sharpness));
            noiseValue = Mathf.Lerp(noiseValue, billowNoise, -Mathf.Min(0f, sharpness));

            float distance01 = 1f - distanceField[index];
            distance01 = Mathf.Pow(distance01, 3f);

            noiseValue *= distance01;

            amp *= gain;
            freq *= lacunarity;

            //--------------------------------
            //Test area

            //noiseValue = Mathf.Clamp(noiseValue, 0f, 1f);
            //noiseValue = Mathf.Sin(noiseValue);


            //noiseValue = Mathf.Cos(noiseValue);
            //noiseValue = Mathf.Tan(noiseValue);
            //noiseValue = Mathf.Asin(noiseValue);
            //noiseValue = Mathf.Atan(noiseValue);
            //noiseValue = (float)System.Math.Tanh((double)noiseValue);

            //--------------------------------
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

            float tempNoiseValue = noise.x;

            if (turbulence)
            {
                float turbulence = Mathf.Abs(tempNoiseValue);
                noiseValue += turbulence * amp;
            }
            else
            {
                noiseValue += tempNoiseValue * amp;
            }

            amp *= gain;
            freq *= lacunarity;
        }
        return noiseValue;
    }

    void GenerateTerrainVoronoi()
    {
        for (int i = 0; i < vertices.Length; i++)
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

        for (int i = 0; i < vertices.Length; i++)
        {
            float noiseValue = 0f;
            float amp = amplitude;
            float freq = frequency;

            Vector2 vertex = new Vector2(vertices[i].x, vertices[i].z);


            noiseValue = SimplexNoise(freq, amp, vertex, i);

            if (domainWarping)
            {
                Vector2 warpedCoords1 = new Vector2(SimplexNoise(freq, amp, vertex + new Vector2(warpValues[0], warpValues[1]), i),
                SimplexNoise(freq, amp, vertex + new Vector2(warpValues[2], warpValues[3]), i));

                Vector2 warpedCoords2 = new Vector2(SimplexNoise(freq, amp, vertex + new Vector2(warpValues[4], warpValues[5])
                    + warpingImpact[0] * warpedCoords1, i),
                    SimplexNoise(freq, amp, vertex + new Vector2(warpValues[6], warpValues[7]) + warpingImpact[1] * warpedCoords1, i));

                noiseValue = SimplexNoise(freq, amp, vertex + (warpingImpact[2] * warpedCoords2), i);
            }

            vertices[i] = new Vector3(vertices[i].x, vertices[i].y + 0.5f + noiseValue * heightMultiplier, vertices[i].z);

        }
    }

    float SimplexNoise(float freq, float amp, Vector2 vertex, int index)
    {
        float noiseValue = 0f;

        for (int o = 0; o < octaves; o++)
        {
            float xCoord = (float)vertex.x / (xSize - 1) * scale * freq + offset.x;
            float zCoord = (float)vertex.y / (ySize - 1) * scale * freq + offset.y;

            float tempNoise = 0f;

            Vector3 noise = Noise.SimplexNoise.Noise(xCoord, zCoord);

            if (derivativeModificator == DerivativeModificator.derivative_simplex)
            {
                Vector2 dsum = new Vector2(noise.y, noise.z);
                tempNoise = amp * noise.x * (1.0f / (1 + derivativeMultiplication * Vector2.Dot(dsum, dsum)));
            }

            if (turbulence)
            {
                float turbulence = (Mathf.Abs(noise.x) - 0.5f) * amp;
                tempNoise = turbulence;
            }
            else
            {
                tempNoise = (noise.x - 0.5f) * amp;
            }

            noiseValue += tempNoise;

            float billowNoise = Mathf.Abs(noiseValue);
            float ridgeNoise = 1f - billowNoise;

            noiseValue = Mathf.Lerp(noiseValue, ridgeNoise, Mathf.Max(0f, sharpness));
            noiseValue = Mathf.Lerp(noiseValue, billowNoise, -Mathf.Min(0f, sharpness));

            //Vector3 noise = Unity.Mathematics.noise.srdnoise(new float2(xCoord, zCoord));
            /*            float altitudeErosion = 5f;
                        float ridgeErosion = 2f;*/

            /* Vector2 rDsum = new Vector2(noise.y, noise.z) * ridgeErosion;

             amp *= Mathf.Lerp(gain, gain + Mathf.SmoothStep(0f, 1f, noiseValue), altitudeErosion);
             amp = amp * (1f - (ridgeErosion / (1f + Vector2.Dot(rDsum, rDsum))));*/

            float distance01 = 1f - distanceField[index];
            distance01 = Mathf.Pow(distance01, 3f);

            noiseValue *= distance01;

            amp *= gain;
            freq *= lacunarity;
        }
        return noiseValue;
    }

    float ValueNoise(float freq, float amp, Vector2 vertex)
    {
        float noiseValue = 0f;
        for (int o = 0; o < 1; o++)
        {
            float xCoord = (float)vertex.x / (xSize - 1) * scale * freq + offset.x;
            float zCoord = (float)vertex.y / (ySize - 1) * scale * freq + offset.y;
            float noise = Noise.ValueNoise.Noise(xCoord, zCoord);

            noiseValue += noise * amp;


            amp *= gain;
            freq *= lacunarity;
        }
        return noiseValue;
    }

    float[] BlurMask(float[] mask, int size, int iterations, int radius)
    {
        float[] current = (float[])mask.Clone();
        float[] temp = new float[mask.Length];

        for (int it = 0; it < iterations; it++)
        {
            for (int z = 0; z < size; z++)
            {
                for (int x = 0; x < size; x++)
                {
                    float sum = 0f;
                    float count = 0f;

                    for (int dz = -radius; dz <= radius; dz++)
                    {
                        for (int dx = -radius; dx <= radius; dx++)
                        {
                            int nx = x + dx;
                            int nz = z + dz;

                            if (nx < 0 || nz < 0 || nx >= size || nz >= size)
                                continue;

                            int ni = nz * size + nx;

                            sum += current[ni];
                            count++;
                        }
                    }

                    int i = z * size + x;
                    temp[i] = sum / count;
                }
            }

            // swap
            var swap = current;
            current = temp;
            temp = swap;
        }

        return current;
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

    private void OnValidate()
    {
        if (warpingImpact.Length != 3)
        {
            Debug.LogWarning("Don't change the array size!");
            System.Array.Resize(ref warpingImpact, 3);
        }

        if (warpValues.Length != 8)
        {
            Debug.LogWarning("Don't change the array size!");
            System.Array.Resize(ref warpValues, 8);
        }
    }

}
