using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using static UnityEditor.Progress;
using static UnityEngine.Rendering.DebugUI;

public class ThermalErosion : MonoBehaviour
{
    [SerializeField] private GameObject terrain;
    [SerializeField] private int mapSize = 256;
    [SerializeField] private int iterations = 10;
    [SerializeField] private float threshold = 10f;
    [SerializeField][Range(0, 1f)] private float c = 0.1f;
    private Vector3[] vertices;
    private float range;
    private float maxDelta;

    [Header("Testing")]
    [SerializeField] private bool isTesting;

    //[Header("Experimental")]
    //[SerializeField][Range(0, 0.01f)] private float thresholdMult = 0.005f;
    //[SerializeField][Range(0, 0.1f)] private float deltaMult = 0.01f;



    // Start is called before the first frame update
    void Start()
    {
        Mesh mesh = terrain.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        float[] heights = vertices.Select(p => p.y).ToArray();
        float[] newHeights;

        //float minH = float.MaxValue;
        //float maxH = float.MinValue;

        //for (int i = 0; i < heights.Length; i++)
        //{
        //    float h = heights[i];
        //    if (h < minH) minH = h;
        //    if (h > maxH) maxH = h;
        //}

        //range = maxH - minH;
        //if (range < 1e-6f) range = 1e-6f;
        //threshold = range * thresholdMult;
        //c =  (c / iterations) * 8;
        //maxDelta = range * deltaMult;

        //for (int i = 0; i < 5; i++)
        //{
        //    System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        //    Erode(heights, mapSize, iterations);
        //    long end = sw.ElapsedMilliseconds;
        //    Debug.Log(end);
        //}

        if(!isTesting)
        {
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

        float[] testValues = {100, 200, 500};

        //List<ExperimentResultErosion> results = new List<ExperimentResultErosion>();
        List<(float, float)> results = new List<(float, float)> ();
        //results.Clear();

        foreach (float testValue in testValues)
        {
            //--------PARAMETR-------------

            //erosion

            //threshold = testValue;
            iterations = (int)testValue;


            for (int i = 0; i < 5; i++)
            {
                //-----------------------------

                heights = vertices.Select(p => p.y).ToArray();



                float start = Time.realtimeSinceStartup;
                Erode(heights, mapSize, (int)testValue);
                start -= Time.realtimeSinceStartup;
                results.Add((testValue, -start * 1000));
            }

        }

        Metrics.SaveTimeToCSV(results, "/ThermalErosion/time" + mapSize + ".csv", "Iterations");

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].y = newHeights[i];
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();



    }


    public float[] Erode(float[] heights, int size, int iterations)
    {

        //----------------BADANIA---------------
        List<ExperimentResultErosion> results = new List<ExperimentResultErosion>();
        //----------------------------------

        float[] newHeights = (float[])heights.Clone();
        int width = (int)Mathf.Sqrt(heights.Length);

        

        for (int i = 0; i < iterations; i++)
        {
            float[] deltas = new float[heights.Length];

            for (int j = 0; j < heights.Length; j++)
            {
                int x = j % width;
                int y = j / width;

                int[] dx = { -1, 1, 0, 0};
                int[] dy = { 0, 0, -1, 1};

                for (int k = 0; k < dx.Length; k++)
                {
                    int nx = x + dx[k];
                    int ny = y + dy[k];

                    if (nx >= 0 && nx < width && ny >= 0 && ny < width)
                    {
                        int neighbor = ny * width + nx;
                        float slope = heights[j] - heights[neighbor];
                        
                        if (slope > threshold)
                        {
                            float transport = c * (slope - threshold);
                            deltas[j] -= transport;
                            deltas[neighbor] += transport;
                        }
                    }
                }
            }
            
            for (int j = 0; j < heights.Length; j++)
            {
                heights[j] += deltas[j];
            }

            //-----------------BADANIA---------------------------------
        //    int[] steps = { 1, 5, 10, 20, 50, 100, 200, 400, 600, 800, 1000 };

        //    if (steps.Contains(i))
        //    {
        //        float stdDev = Metrics.StdDev(heights);
        //        float avgGradient = Metrics.MeanGrad(heights, mapSize, 2);
        //        float varG = Metrics.GradientVariance();
        //        float ruggedness = Metrics.Ruggedness(heights, mapSize);
        //        float meanAbsCurvature = Metrics.MeanAbsCurvature(heights, mapSize);
        //        float skewness = Metrics.HeightSkewness(heights, mapSize);
        //        float erosionScore = Metrics.ErosionScore(heights, mapSize);

        //        results.Add(new ExperimentResultErosion
        //        {
        //            iterations = i,
        //            stdDev = stdDev,
        //            avgGradient = avgGradient,
        //            varGradient = varG,
        //            ruggedness = ruggedness,
        //            meanAbsCurvature = meanAbsCurvature,
        //            skewness = skewness,
        //            erosionScore = erosionScore,
        //        });
        //    }

        //    //--------------------------------------------

        }
        //string parameterName = "Threshold";
        //Metrics.SaveErosionToCSV(results, "ThermalErosion/New/thermalErosion" + parameterName + threshold + ".csv");

        return heights;
    }
}


