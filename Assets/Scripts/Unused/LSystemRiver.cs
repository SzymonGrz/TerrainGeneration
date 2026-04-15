using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LSystemRiver : MonoBehaviour
{
    
    private string lSystemString;
    [SerializeField] private float[] stepLengths;
    [SerializeField] private float[] angles;

    [SerializeField] private GameObject terrain;
    [SerializeField] private int planeResolution;
    [SerializeField] private int xSize;
    [SerializeField] private int ySize;

    private Mesh mesh;
    private Vector3[] vertices;
    [SerializeField] private float radius = 2;
    [SerializeField] private float depth = 10;
    Transform t;

    private static float[] riverMask;

    private class Segment
    {
        public Vector3 a;
        public Vector3 b;
        public Segment(Vector3 a, Vector3 b) { this.a = a; this.b = b; }
    }

    private List<Vector2> riverSegmentsA = new List<Vector2>();
    private List<Vector2> riverSegmentsB = new List<Vector2>();


    private Stack<TurtleState> stateStack;

    struct TurtleState
    {
        public Vector3 position;
        public Quaternion rotation;
        public float stepLength;

        public TurtleState(Vector3 pos, Quaternion rot, float step)
        {
            position = pos;
            rotation = rot;
            stepLength = step;
        }
    }


    [SerializeField] private string[] axioms;
    [SerializeField] private Rule[] rules;
    [SerializeField] private int iterations = 1;
    [SerializeField] private bool shorterRoads = false;
    [SerializeField][Range(0f, 1f)] private float lengthFactor = 0.5f;


    void Start()
    {
        LSystem creator = new LSystem();

        lSystemString = creator.lSystem(axioms, rules, iterations);

        mesh = terrain.GetComponent<MeshFilter>().mesh;
        t = terrain.GetComponent<MeshFilter>().transform;
        vertices = mesh.vertices;
        Draw();
        CreateRiver();
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        //Experiment();
        ExperimentTime();


    }

    void Draw()
    {
        stateStack = new Stack<TurtleState>();



        Vector3 currentPosition = new Vector3(
                mesh.bounds.center.x,
                mesh.bounds.center.y,
                mesh.bounds.min.z
        );

        Quaternion currentRotation = Quaternion.identity;
        float currentStepLength = stepLengths[UnityEngine.Random.Range(0, stepLengths.Length)];

        foreach (char c in lSystemString)
        {
            
            if (c == 'F' || c == 'G')
            {
                Vector3 startPos = currentPosition;
                Vector3 endPos = currentPosition +
                                 currentRotation * Vector3.forward * currentStepLength;

                riverSegmentsA.Add(new Vector2(currentPosition.x, currentPosition.z));
                riverSegmentsB.Add(new Vector2(endPos.x, endPos.z));

                currentPosition = endPos;
            }
            else if (c == 'f')
            {
                currentPosition += currentRotation * Vector3.forward * (currentStepLength);
            }
            else if (c == '+')
            {
                float angle = angles[UnityEngine.Random.Range(0, angles.Length)];
                currentRotation *= Quaternion.Euler(0, -angle, 0);
            }
            else if (c == '-')
            {
                float angle = angles[UnityEngine.Random.Range(0, angles.Length)];
                currentRotation *= Quaternion.Euler(0, angle, 0);
            }
            else if (c == '[')
            {
                stateStack.Push(new TurtleState(currentPosition, currentRotation, currentStepLength));

                if (shorterRoads)
                    currentStepLength *= lengthFactor;
            }
            else if (c == ']')
            {
                var state = stateStack.Pop();
                currentPosition = state.position;
                currentRotation = state.rotation;
                currentStepLength = state.stepLength;
            }
        }
    }

    private void CreateRiver()
    {
        riverMask = new float[vertices.Length];

        float maxFalloff;

        for (int i = 0; i < vertices.Length; i++)
        {
            maxFalloff = 0f;
            Vector2 v2 = new Vector2(vertices[i].x, vertices[i].z);

            for (int j = 0; j < riverSegmentsA.Count; j++)
            {

                //Vector2 segA = new Vector2(riverSegments[j].a.x, riverSegments[j].a.z);
                //Vector2 segB = new Vector2(riverSegments[j].b.x, riverSegments[j].b.z);

                float dist = DistPointSegment(v2, riverSegmentsA[j], riverSegmentsB[j]);


                if (dist < radius)
                {
                    float t = dist / radius;
                    //maxFalloff = 1;
                    float falloff = Mathf.Exp(-t * t * 4f); 
                    if (falloff > maxFalloff)
                    {
                        maxFalloff = falloff;
                    }

                }
                
            }
            if (maxFalloff > 0f)
            {
                vertices[i].y -= depth * maxFalloff;
                riverMask[i] = Mathf.Max(riverMask[i], maxFalloff);
            }
        }

    }

    float DistPointSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
        Vector2 proj = a + t * ab;
        return Vector2.Distance(p, proj);
    }

    public static float[] getRiverMask()
    {
        return riverMask;
    }

    private void Experiment()
    {

        List<ExperimentResultLsystem> results = new List<ExperimentResultLsystem>();
        //string[] values = {"A", "B", "C", "D", "E"};

        string[] values = { "Test1", "Test2", "Test3", "Test4", "Test5" };

        //float[][] tableValues =
        //{
        //    new float[] {22, 24, 25, 26, 28, 30},
        //    new float[] {22, 28, 34, 40, 46, 52},
        //    new float[] {10, 20, 40, 55, 70, 80}
        //};

        //float[][] tableValues =
        //{
        //    new float[] {22, 24, 25, 26, 28, 30},
        //    new float[] {22, 28, 34, 40, 46, 52},
        //    new float[] {10, 20, 40, 55, 70, 80}
        //};

        LSystem creator = new LSystem();

        string parameterName = "RulesC-Complex";

        //for (int j = 0; j < tableValues.Length; j++)
        {
            for (int k = 0; k < 5; k++)
            {
                //iterations = (int)values[j];
                //stepLengths = tableValues[j];
                riverMask = new float[vertices.Length];
                riverSegmentsA.Clear();
                riverSegmentsB.Clear();
                stateStack.Clear();

                lSystemString = creator.lSystem(axioms, rules, iterations);
                Draw();
                CreateRiver();

                int numberOfSegments = riverSegmentsA.Count;
                int numberOfSegmentsOnMap = 0;

                float totalRiverLength = 0f;
                float riverLengthOnMap = 0f;

                float cellSize = 2f;

                Dictionary<(float, float), List<Vector2>> segments = new Dictionary<(float, float), List<Vector2>>();

                List<Vector2> points = new List<Vector2>(riverSegmentsA);
                points.AddRange(riverSegmentsB);

                for (int i = 0; i < riverSegmentsA.Count; i++)
                {
                    float segmentLength = Vector2.Distance(riverSegmentsA[i], riverSegmentsB[i]);

                    if ((riverSegmentsA[i].x >= 0 && riverSegmentsA[i].x < xSize
                        && riverSegmentsA[i].y >= 0 && riverSegmentsA[i].y < ySize)
                        && (riverSegmentsB[i].x >= 0 && riverSegmentsB[i].x < xSize
                        && riverSegmentsB[i].y >= 0 && riverSegmentsB[i].y < ySize))
                    {

                        riverLengthOnMap += segmentLength;
                        numberOfSegmentsOnMap++;
                    }
                    totalRiverLength += segmentLength;

                }

                for (int i = 0; i < points.Count; i++)
                {
                    float cellX = Mathf.Floor(points[i].x / cellSize);
                    float cellY = Mathf.Floor(points[i].y / cellSize);

                    if (segments.ContainsKey((cellX, cellY)))
                    {
                        segments[(cellX, cellY)].Add(points[i]);
                    }
                    else
                    {
                        segments.Add((cellX, cellY), new List<Vector2>());
                    }
                }

                int numberOfSplits = 0;

                foreach (var segment in segments)
                {
                    if (segment.Value.Count >= 3)
                    {
                        numberOfSplits++;
                    }
                }

                float riverCount = 0f;
                float threshold = 0.1f;

                if (riverMask != null)
                {
                    for (int i = 0; i < riverMask.Length; i++)
                    {
                        if (riverMask[i] > threshold)
                        {
                            riverCount++;
                        }
                    }
                }

                float coverage = riverCount / riverMask.Length;

                results.Add(new ExperimentResultLsystem
                {
                    parameter = values[k],
                    numberOfSegments = numberOfSegments,
                    totalRiverLength = totalRiverLength,
                    numberOfSplits = numberOfSplits,
                    numberOfSegmentsOnMap = numberOfSegmentsOnMap,
                    riverLengthOnMap = riverLengthOnMap,
                    mapCoverage = coverage,
                });
            }

        }

        Metrics.SaveLSystemToCSV(results, "LSystem/" + parameterName + ".csv", parameterName);
        //Debug.Log("number of segments: " + numberOfSegments + " total river length: " + totalRiverLength +
        //    " number of splits: " + numberOfSplits + " number of segments on map: " + numberOfSegmentsOnMap +
        //    " river length on map: " + riverLengthOnMap + " map coverage: " + coverage);
    }

    private void ExperimentTime()
    {
        LSystem creator = new LSystem();

        List<(float, float)> results = new List<(float, float)> ();

        float[] values = { 1, 2, 4, 6, 8 };

        for (int i = 0; i < values.Length; i++) {

            iterations = (int)values[i];
            for (int k = 0; k < 3; k++)
            {
                //iterations = (int)values[j];
                //stepLengths = tableValues[j];
                riverMask = new float[vertices.Length];
                riverSegmentsA.Clear();
                riverSegmentsB.Clear();
                stateStack.Clear();

                lSystemString = creator.lSystem(axioms, rules, iterations);
                float start = Time.realtimeSinceStartup;
                Draw();
                CreateRiver();
                start -= Time.realtimeSinceStartup;

                results.Add((iterations, -start * 1000));

            }
        }

        Metrics.SaveTimeToCSV(results, "/LSystem/timeSlow" + ".csv", "Iterations");
    }

}
