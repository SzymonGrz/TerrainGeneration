using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSystemRiver : MonoBehaviour
{
    // Start is called before the first frame update
    private string lSystemString;
    [SerializeField] private float[] stepLengths;
    [SerializeField] private float[] angles;

    [SerializeField] private GameObject terrain;

    private Mesh mesh;
    private Vector3[] vertices;
    [SerializeField] private float radius = 2;
    [SerializeField] private float depth = 10;
    private List<Vector3> riverPoints;
    Transform t;

    private class Segment
    {
        public Vector3 a;
        public Vector3 b;
        public Segment(Vector3 a, Vector3 b) { this.a = a; this.b = b; }
    }

    private List<Segment> riverSegments = new List<Segment>();


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
        riverPoints = new List<Vector3>();

        Draw();
        CreateRiver();
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        //CorrectTiles();

    }

    void Draw()
    {
        stateStack = new Stack<TurtleState>();

        int vertexIndex = vertices.Length /2 ;
        Vector3 currentPosition = vertices[vertexIndex];
        Quaternion currentRotation = Quaternion.identity;
        float currentStepLength = stepLengths[UnityEngine.Random.Range(0, stepLengths.Length)];

        foreach (char c in lSystemString)
        {
            float angle = angles[UnityEngine.Random.Range(0, angles.Length)];
            if (c == 'F' || c == 'G')
            {
                Vector3 startPos = currentPosition;
                Vector3 endPos = currentPosition +
                                 currentRotation * Vector3.forward * currentStepLength;

                riverPoints.Add(endPos);

                currentPosition = endPos;
            }
            else if (c == 'f')
            {
                currentPosition += currentRotation * Vector3.forward * (currentStepLength);
            }
            else if (c == '+')
            {
                currentRotation *= Quaternion.Euler(0, -angle, 0);
            }
            else if (c == '-')
            {
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
        for (int i = 0; i < riverPoints.Count - 1; i++)
            riverSegments.Add(new Segment(riverPoints[i], riverPoints[i + 1]));


        for (int i = 0; i < vertices.Length; i++)
        {
            //Vector3 worldV = t.TransformPoint(vertices[i]);
            Vector2 v2 = new Vector2(vertices[i].x, vertices[i].z);
            float maxFalloff = 0f;

            foreach (var seg in riverSegments)
            {
                float dist = DistPointSegment(
                    v2,
                    new Vector2(seg.a.x, seg.a.z),
                    new Vector2(seg.b.x, seg.b.z)
                );

                if (dist < radius)
                {
                    float t = dist / radius;
                    float falloff = Mathf.SmoothStep(1f, 0f, t);
                    maxFalloff = Mathf.Max(maxFalloff, falloff);
                    
                }
            }
            vertices[i].y -= depth * maxFalloff;
        }

    }

    float DistPointSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
        Vector2 proj = a + t * ab;
        return Vector2.Distance(p, proj);
    }



}
