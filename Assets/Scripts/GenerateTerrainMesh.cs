using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


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

        Vector3 t = this.gameObject.transform.position;
        //center = new Vector2(t.x + 500f, t.z + 500f);

        //GenerateTerrain();
        mesh.RecalculateNormals();

        //Debug.Log(mesh.triangles.Length);

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

}