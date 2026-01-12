using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

public class ThermalErosion : MonoBehaviour
{
    [SerializeField] private GameObject terrain;
    [SerializeField] private int mapSize = 256;
    [SerializeField] private float threshold = 10f;
    [SerializeField] private int iterations = 10;
    [SerializeField][Range(0, 0.25f)] private float c = 0.1f;
    private Vector3[] vertices;
    private float range;
    private float maxDelta;

    //[Header("Experimental")]
    //[SerializeField][Range(0, 0.01f)] private float thresholdMult = 0.005f;
    //[SerializeField][Range(0, 0.1f)] private float deltaMult = 0.01f;



    // Start is called before the first frame update
    void Start()
    {
        Mesh mesh = terrain.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        float[] heights = vertices.Select(p => p.y).ToArray();

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

        float[] newHeights = Erode(heights, mapSize, iterations);

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].y = newHeights[i];
        }
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        float minH = float.MaxValue;
        float maxH = float.MinValue;

        for (int i = 0; i < newHeights.Length; i++)
        {
            float h = newHeights[i];
            if (h < minH) minH = h;
            if (h > maxH) maxH = h;
        }

        range = maxH - minH;
        Debug.Log(range);

    }
    public float[] Erode(float[] heights, int size, int iterations)
    {
        float[] newHeights = (float[])heights.Clone();
        int width = (int)Mathf.Sqrt(heights.Length);

        

        for (int i = 0; i < iterations; i++)
        {
            float[] deltas = new float[heights.Length];

            for (int j = 0; j < heights.Length; j++)
            {
                int x = j % width;
                int y = j / width;

                int[] dx = { -1, 1, 0, 0, -1, 1, -1, 1};
                int[] dy = { 0, 0, -1, 1, -1, -1, 1, 1};

                int[] neighbors = new int[] { -1, -1, -1, -1, -1, -1, -1, -1};
                float totalNeighbors = 0f;
                int validNeighbors = 0;

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
                            neighbors[k] = neighbor;
                            totalNeighbors += (slope - threshold);
                            validNeighbors++;
                        }
                    }
                }

                if(validNeighbors == 0)
                {
                    continue;
                }

                float totalDelta = c * totalNeighbors;
                float deltaPerNeighbor = totalDelta / validNeighbors;
                //newHeights[j] -= totalDelta;
                deltas[j] -= totalDelta;

                foreach (int n in neighbors)
                {
                    if (n >= 0)
                    {
                        //newHeights[n] += deltaPerNeighbor;
                        deltas[n] += deltaPerNeighbor;
                    }
                }

            }
            
            for (int j = 0; j < newHeights.Length; j++)
            {
                newHeights[j] += deltas[j];
            }
            heights = (float[])newHeights.Clone();
        }
        return newHeights;
    }
}


