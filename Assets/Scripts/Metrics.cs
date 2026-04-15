using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

[System.Serializable]
public struct ExperimentResultDW
{
    public float w0, w1, w2;
    public float stdDev;
    public float avgGradient;
    public float varGradient;
    public float ruggedness;
    public float meanAbsCurvature;
    public float skewness;
}

public struct ExperimentResultErosion
{
    public float iterations;
    public float stdDev;
    public float avgGradient;
    public float varGradient;
    public float ruggedness;
    public float meanAbsCurvature;
    public float skewness;
    public float erosionScore; // stdDev slope / mean slope
}

public struct ExperimentResultNoise
{
    public float parameter;
    public float stdDev;
    public float avgGradient;
    public float varGradient;
    public float ruggedness;
    public float meanAbsCurvature;
    public float skewness;
}

public struct ExperimentResultLsystem
{
    public string parameter;
    public int numberOfSegments;
    public float totalRiverLength;
    public int numberOfSplits;
    public int numberOfSegmentsOnMap;
    public float riverLengthOnMap;
    public float mapCoverage;
}


public class Metrics
{


    static List<float> gradients;

    public static float StdDev(Vector3[] vertices)
    {
        float sum = 0f;
        foreach (var v in vertices)
        {
            sum += v.y;
        }

        float mean = sum / vertices.Length;

        float variance = 0f;
        foreach (var v in vertices)
        {
            float diff = v.y - mean;
            variance += diff * diff;
        }

        variance /= vertices.Length;
        float stdDev = Mathf.Sqrt(variance);

        return stdDev;
    }

    public static float StdDev(float[] vertices)
    {
        float sum = 0f;
        foreach (var v in vertices)
        {
            sum += v;
        }

        float mean = sum / vertices.Length;

        float variance = 0f;
        foreach (var v in vertices)
        {
            float diff = v - mean;
            variance += diff * diff;
        }

        variance /= vertices.Length;
        float stdDev = Mathf.Sqrt(variance);

        return stdDev;
    }

    public static float MeanGrad(Vector3[] vertices, int planeResolution, float cellSize)
    {

        gradients = new List<float>();
        float totalGradient = 0f;
        int count = 0;

        for (int y = 0; y < planeResolution; y++)
        {
            for (int x = 0; x < planeResolution; x++)
            {
                float h = vertices[y * (planeResolution + 1) + x].y;
                float hx = vertices[y * (planeResolution + 1) + (x + 1)].y;
                float hy = vertices[(y + 1) * (planeResolution + 1) + x].y;

                float dx = (hx - h) / cellSize;
                float dy = (hy - h) / cellSize;

                float gradient = Mathf.Sqrt(dx * dx + dy * dy);
                gradients.Add(gradient);

                totalGradient += gradient;
                count++;
            }
        }

        float avgGradient = totalGradient / count;
        return avgGradient;
    }

    public static float MeanGrad(float[] vertices, int planeResolution, float cellSize)
    {

        gradients = new List<float>();
        float totalGradient = 0f;
        int count = 0;

        for (int y = 0; y < planeResolution; y++)
        {
            for (int x = 0; x < planeResolution; x++)
            {
                float h = vertices[y * (planeResolution + 1) + x];
                float hx = vertices[y * (planeResolution + 1) + (x + 1)];
                float hy = vertices[(y + 1) * (planeResolution + 1) + x];

                float dx = (hx - h) / cellSize;
                float dy = (hy - h) / cellSize;

                float gradient = Mathf.Sqrt(dx * dx + dy * dy);
                gradients.Add(gradient);

                totalGradient += gradient;
                count++;
            }
        }

        float avgGradient = totalGradient / count;
        return avgGradient;
    }

    public static float GradientVariance()
    {
        float meanG = gradients.Average();

        float varG = 0f;
        foreach (var g in gradients)
        {
            float diff = g - meanG;
            varG += diff * diff;
        }

        varG /= gradients.Count;
        return varG;
    }

    public static float MeanHeight(float[] h, int planeResolution)
    {
        float sum = 0f;

        for (int i = 0; i < h.Length; i++)
        {
            sum += h[i];
        }

        return sum / h.Length;
    }

    public static float Ruggedness(float[] h, int planeResolution)
    {
        float sum = 0f;
        int count = 0;

        for (int y = 0; y < planeResolution; y++)
        {
            for (int x = 0; x < planeResolution; x++)
            {
                float h0 = h[y * (planeResolution + 1) + x];
                float hx = h[y * (planeResolution + 1) + (x + 1)];
                float hy = h[(y + 1) * (planeResolution + 1) + x];

                sum += Mathf.Abs(h0 - hx);
                sum += Mathf.Abs(h0 - hy);

                count += 2;
            }
        }

        return sum / count;
    }

    public static float MeanAbsCurvature(float[] h, int planeResolution)
    {
        float sum = 0f;
        int count = 0;

        for (int y = 1; y < planeResolution; y++)
        {
            for (int x = 1; x < planeResolution; x++)
            {
                float center = h[y * (planeResolution + 1) + x];

                float left = h[y * (planeResolution + 1) + (x - 1)];
                float right = h[y * (planeResolution + 1) + (x + 1)];
                float up = h[(y - 1) * (planeResolution + 1) + x];
                float down = h[(y + 1) * (planeResolution + 1) + x];

                float lap = (left + right + up + down) - 4f * center;

                sum += Mathf.Abs(lap);
                count++;
            }
        }

        return sum / count;
    }

    public static float HeightSkewness(float[] h, int planeResolution)
    {
        float mean = MeanHeight(h, planeResolution);
        float std = StdDev(h);

        float sum = 0f;
        int count = 0;

        int total = (planeResolution + 1) * (planeResolution + 1);

        for (int i = 0; i < total; i++)
        {
            float d = (h[i] - mean) / std;
            sum += d * d * d;
            count++;
        }

        return sum / count;
    }

    public static float ErosionScore(float[] h, int planeResolution)
    {
        float sum = 0f;
        float sumSq = 0f;
        int count = 0;

        for (int y = 1; y < planeResolution; y++)
        {
            for (int x = 1; x < planeResolution; x++)
            {
                float center = h[y * (planeResolution + 1) + x];

                float[] neighbors = new float[]
                {
                    h[y * (planeResolution + 1) + (x - 1)],
                    h[y * (planeResolution + 1) + (x + 1)],
                    h[(y - 1) * (planeResolution + 1) + x],
                    h[(y + 1) * (planeResolution + 1) + x]
                };

                float s = Mathf.Max(neighbors);

                sum += s;
                sumSq += s * s;
                count++;
                
            }
        }

        float mean = sum / count;
        float variance = (sumSq / count) - (mean * mean);
        float stdDev = Mathf.Sqrt(variance);

        if (mean == 0f) return 0f;

        return stdDev / mean;
    }

    public static void SaveDWToCSV(List<ExperimentResultDW> results, string filename)
    {
        StringBuilder sb = new StringBuilder();

        // nag³ówek kolumn
        sb.AppendLine("w0;w1;w2;stdDev;avgGradient;varGradient;ruggedness;meanAbsCurvature;skewness");

        foreach (var r in results)
        {
            sb.AppendLine(
                r.w0 + ";" +
                r.w1 + ";" +
                r.w2 + ";" +
                r.stdDev + ";" +
                r.avgGradient + ";" +
                r.varGradient + ";" +
                r.ruggedness + ";" +
                r.meanAbsCurvature + ";" +
                r.skewness
            );
        }

        string path = Application.dataPath + "/Tests/DomainWarping/" + filename;
        StreamWriter sw = new StreamWriter(path);
        sw.WriteLine(sb.ToString() );
        sw.Close(); 

    }

    public static void SaveErosionToCSV(List<ExperimentResultErosion> results, string filename)
    {
        StringBuilder sb = new StringBuilder();

        // nag³ówek kolumn
        sb.AppendLine("iterations;stdDev;avgGradient;varGradient;ruggedness;meanAbsCurvature;skewness;erosionScore");

        foreach (var r in results)
        {
            sb.AppendLine(
                r.iterations + ";" +
                r.stdDev + ";" +
                r.avgGradient + ";" +
                r.varGradient + ";" +
                r.ruggedness + ";" +
                r.meanAbsCurvature + ";" +
                r.skewness + ";" +
                r.erosionScore
            );
        }

        string path = Application.dataPath + "/Tests/" + filename;
        StreamWriter sw = new StreamWriter(path);
        sw.WriteLine(sb.ToString());
        sw.Flush();
        sw.Close();

    }
    public static void SaveNoiseToCSV(List<ExperimentResultNoise> results, string filename, string parameterName)
    {
        StringBuilder sb = new StringBuilder();

        // nag³ówek kolumn
        sb.AppendLine(parameterName + ";stdDev;avgGradient;varGradient;ruggedness;meanAbsCurvature;skewness");

        foreach (var r in results)
        {
            sb.AppendLine(
                r.parameter + ";" +
                r.stdDev + ";" +
                r.avgGradient + ";" +
                r.varGradient + ";" +
                r.ruggedness + ";" +
                r.meanAbsCurvature + ";" +
                r.skewness
            );
        }

        string path = Application.dataPath + "/Tests/" + filename;
        StreamWriter sw = new StreamWriter(path);
        sw.WriteLine(sb.ToString());
        sw.Flush();
        sw.Close();

    }

    public static void SaveLSystemToCSV(List<ExperimentResultLsystem> results, string filename, string parameterName)
    {
        StringBuilder sb = new StringBuilder();

        // nag³ówek kolumn
        sb.AppendLine(parameterName + ";numberOfSegments;totalRiverLength;numberOfSplits;numberOFSegmentsOnMap;" +
            "riverLengthOnMap;mapCoverage");

        foreach (var r in results)
        {
            sb.AppendLine(
                r.parameter + ";" +
                r.numberOfSegments + ";" +
                r.totalRiverLength + ";" +
                r.numberOfSplits + ";" +
                r.numberOfSegmentsOnMap + ";" +
                r.riverLengthOnMap + ";" +
                r.mapCoverage
            );
        }

        string path = Application.dataPath + "/Tests/" + filename;
        StreamWriter sw = new StreamWriter(path);
        sw.WriteLine(sb.ToString());
        sw.Flush();
        sw.Close();
    }

    public static void SaveTimeToCSV(List<(float,float)> results, string filename, string parameterName)
    {
        StringBuilder sb = new StringBuilder();

        // nag³ówek kolumn
        sb.AppendLine(parameterName + ";time");

        foreach (var r in results)
        {
            sb.AppendLine(
                r.Item1 + ";" +
                r.Item2
            );
        }

        string path = Application.dataPath + "/Tests/Time/" + filename;
        StreamWriter sw = new StreamWriter(path);
        sw.WriteLine(sb.ToString());
        sw.Flush();
        sw.Close();
    }

}
