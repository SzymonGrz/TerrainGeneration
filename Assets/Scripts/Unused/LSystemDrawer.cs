using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LSystemDrawer : MonoBehaviour
{
    private string lSystemString;
    [SerializeField] private float stepLength = 1f;
    [SerializeField] private float angle = 90f;

    [SerializeField] private GameObject roadVertical;
    [SerializeField] private GameObject roadHorizontal;

    [SerializeField] private Texture2D roadSpriteIntersection;
    [SerializeField] private Texture2D roadSpriteCorner;
    [SerializeField] private Texture2D roadSpriteJunction;
    [SerializeField] private Texture2D roadSpriteEnd;

    [SerializeField] private GameObject terrain;


    private Stack<TurtleState> stateStack;

    private HashSet<Vector3Int> drawnCells = new HashSet<Vector3Int>();
    private Dictionary<Vector3Int, GameObject> placedRoadObjects = new Dictionary<Vector3Int, GameObject>();

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

    private float minHeight;

    private int tileSize;

    void Start()
    {
        tileSize = (int)roadHorizontal.GetComponent<MeshRenderer>().bounds.size.x;
        LSystem creator = new LSystem();

        lSystemString = creator.lSystem(axioms, rules, iterations);

        Draw();
        //CorrectTiles();

    }

    void Draw()
    {
        stateStack = new Stack<TurtleState>();

        minHeight = getMinHeight();
       minHeight += 22;
       
        Vector3 currentPosition = new Vector3(140, minHeight, 300);
        Quaternion currentRotation = Quaternion.identity;
        float currentStepLength = stepLength; 

        foreach (char c in lSystemString)
        {
            if(c == 'F' || c == 'G')
            {
                Vector3 startPos = currentPosition;
                Vector3 endPos = currentPosition +
                                 currentRotation * Vector3.forward * (currentStepLength * tileSize);

                DrawLineOnTilemap(startPos, endPos);
                currentPosition = endPos;
            }
            else if (c == 'f')
            {
                currentPosition += currentRotation * Vector3.forward * (currentStepLength * tileSize);
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

    public void DrawLineOnTilemap(Vector3 start, Vector3 end)
    {
        int x0 = Mathf.RoundToInt(start.x / tileSize);
        int z0 = Mathf.RoundToInt(start.z / tileSize);
        int x1 = Mathf.RoundToInt(end.x / tileSize);
        int z1 = Mathf.RoundToInt(end.z / tileSize);

        int dx = Mathf.Abs(x1 - x0);
        int dz = Mathf.Abs(z1 - z0);
        int sx = (x0 < x1) ? 1 : -1;
        int sz = (z0 < z1) ? 1 : -1;
        int err = dx - dz;

        while (true)
        {
            Vector3Int cell = new Vector3Int(x0, 0, z0);

            if (!placedRoadObjects.ContainsKey(cell))
            {
                Vector3 worldPos = new Vector3(x0 * tileSize, minHeight, z0 * tileSize);
                GameObject obj; 
                if(dx >= dz)
                {
                    obj = Instantiate(roadHorizontal, worldPos, Quaternion.identity);
                }
                else
                {
                    obj = Instantiate(roadVertical, worldPos, Quaternion.identity);
                }
                obj.transform.SetParent(this.gameObject.transform);
                placedRoadObjects.Add(cell, obj);
                drawnCells.Add(cell);
            }
                


            

            if (x0 == x1 && z0 == z1) break;

            int e2 = 2 * err;
            if (e2 > -dz) { err -= dz; x0 += sx; }
            if (e2 < dx) { err += dx; z0 += sz; }
        }

    }

    public void CorrectTiles()
    {

        foreach (var cell in drawnCells)
        {
            var neighbors = GetNeighbors(cell);

            if(neighbors.Count == 1)
            {
                placedRoadObjects[cell].GetComponent<MeshRenderer>().material.SetTexture("_MainTex", roadSpriteEnd);
                placedRoadObjects[cell].transform.rotation = RotateEnd(cell, neighbors);
            }
            else if (neighbors.Count == 2)
            {
                if ((neighbors.Contains(Vector3Int.forward) && neighbors.Contains(Vector3Int.back)) ||
                    (neighbors.Contains(Vector3Int.left) && neighbors.Contains(Vector3Int.right)))
                {
                    continue;
                }
                else
                {
                    placedRoadObjects[cell].GetComponent<MeshRenderer>().material.SetTexture("_MainTex", roadSpriteCorner);
                    placedRoadObjects[cell].transform.rotation = RotateCorner(cell, neighbors);
                }
            }
            else if (neighbors.Count == 3)
            {
                placedRoadObjects[cell].GetComponent<MeshRenderer>().material.SetTexture("_MainTex", roadSpriteJunction);
                placedRoadObjects[cell].transform.rotation = RotateTJunction(cell, neighbors);
            }
            else if (neighbors.Count == 4)
            {
                placedRoadObjects[cell].GetComponent<MeshRenderer>().material.SetTexture("_MainTex", roadSpriteIntersection);
            }
        }
    }

    public List<Vector3Int> GetNeighbors(Vector3Int cell)
    {
        Vector3Int[] dirs = new Vector3Int[]
        {
            new Vector3Int(0, 0, 1),   // forward
            new Vector3Int(0, 0, -1),  // back
            new Vector3Int(-1, 0, 0),  // left
            new Vector3Int(1, 0, 0)    // right
        };

        List<Vector3Int> result = new List<Vector3Int>();
        foreach (var dir in dirs)
        {
            if (drawnCells.Contains(cell + dir))
            {
                result.Add(dir);
            }
        }
        return result;
    }

    private Quaternion RotateEnd(Vector3Int cell, List<Vector3Int> dirs)
    {
        if (dirs.Contains(Vector3Int.left) || dirs.Contains(Vector3Int.right))
        {
            return Quaternion.Euler(0, 0, 0);
        }

        else if (dirs.Contains(Vector3Int.back) || dirs.Contains(Vector3Int.forward))
        {
            return Quaternion.Euler(0, 90, 0);
        }
        else
        {
            return Quaternion.Euler(0, 0, 0);
        }
    }

    private Quaternion RotateCorner(Vector3Int cell, List<Vector3Int> dirs)
    {

        if (dirs.Contains(Vector3Int.back) && dirs.Contains(Vector3Int.right))
        {
            return Quaternion.Euler(0, 180, 0);
        }
        else if (dirs.Contains(Vector3Int.back) && dirs.Contains(Vector3Int.left))
        {
            return Quaternion.Euler(0, -90, 0);
        }
        else if (dirs.Contains(Vector3Int.forward) && dirs.Contains(Vector3Int.right))
        {
            return Quaternion.Euler(0, 90, 0);
        }

        else if (dirs.Contains(Vector3Int.left) && dirs.Contains(Vector3Int.forward))
        {
            return Quaternion.Euler(0, 0, 0);
        }
        else
        {
            return Quaternion.Euler(0, 0, 0);
        }
    }

    private Quaternion RotateTJunction(Vector3Int cell, List<Vector3Int> dirs)
    {
        if (!dirs.Contains(Vector3Int.back))
        {
            return Quaternion.Euler(0, 0, 0);
        }
        else if (!dirs.Contains(Vector3Int.left))
        {
            return Quaternion.Euler(0, 90, 0);
        }
        else if (!dirs.Contains(Vector3Int.forward))
        {
            return Quaternion.Euler(0, 180, 0);
        }
        else if (!dirs.Contains(Vector3Int.right))
        {
            return Quaternion.Euler(0, -90, 0);
        }
        else return Quaternion.Euler(0, 0, 0);
    } 

    private void findFlatTerrain()
    {
       /* TerrainData terrainData = terrain.terrainData;
        float width = terrainData.size.x;
        float height = terrainData.size.z;*/

      
    }

    private float getMinHeight()
    {
        Terrain t = terrain.GetComponent<Terrain>();
        TerrainData data = t.terrainData;

        int samplesX = 1000;
        int samplesZ = 1000;

        Vector3 terrainPos = t.transform.position;
        Vector3 terrainSize = data.size;

        int terrainMask = 1 << t.gameObject.layer;

        float minHeight = float.MaxValue;

        for (int i = 0; i < samplesX; i++)
        {
            for (int j = 0; j < samplesZ; j++)
            {
                float posX = terrainPos.x + (i / (float)(samplesX - 1)) * terrainSize.x;
                float posZ = terrainPos.z + (j / (float)(samplesZ - 1)) * terrainSize.z;

                Vector3 rayStart = new Vector3(posX, terrainPos.y + terrainSize.y + 100f, posZ);

                if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Infinity, terrainMask))
                {
                    if (hit.point.y < minHeight)
                        minHeight = hit.point.y;
                }
            }
        }

        return minHeight;
    }

}
