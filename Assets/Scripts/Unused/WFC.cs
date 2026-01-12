using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WFC : MonoBehaviour
{
    /*
     0 - undeclared
     1 - mountains
     2 - forest
     3 - plains
     4 - water
     5 - deepwater
     6 - volcano
    */

    public TileBase[] tiles;
    private Tilemap tilemap;

    public float cellSize = 1f;

    public int width = 10;
    public int height = 10;

    public int types = 7;

    public int range = 1;

    public float[] weights = { 1, 1, 1, 1, 1, 1, 1 };

    int[,] map;
    Dictionary<int, int[]> rules;

    float[] cellEntropy;

    public int islandRange = 200;


    enum Type 
    { 
        linear,
        WFC,
        WFC_entropy,
    }

    [SerializeField]
    Type algorithm = Type.WFC;


    // Start is called before the first frame update
    void Start()
    {

        float total = weights.Sum();
        for (int i = 0; i < weights.Length; i++)
            weights[i] /= total;

        rules = new Dictionary<int, int[]>();
        rules.Add(0, new int[] { 0, 0, 0, 0, 0, 0, 0 });
        rules.Add(1, new int[] { 0, 0, 0, 1, 1, 1, 0 });
        rules.Add(2, new int[] { 0, 0, 0, 0, 1, 1, 1 });
        rules.Add(3, new int[] { 0, 1, 0, 0, 0, 1, 1 });
        rules.Add(4, new int[] { 0, 1, 1, 0, 0, 0, 1 });
        rules.Add(5, new int[] { 0, 1, 1, 1, 0, 0, 1 });
        rules.Add(6, new int[] { 0, 0, 1, 1, 1, 1, 0 });

        map = new int[width, height];
        cellEntropy = new float[width*height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[x, y] = 0;
                //cellEntropy[x+width* y] = calculateEntropy(x, y);

                if (Mathf.Sqrt((Mathf.Pow((float)(x - width * 0.5), 2) + Mathf.Pow((float)(y - width * 0.5), 2))) > islandRange)
                {
                    map[x, y] = 5;
                }
                updateEntropy(x, y);
            }
        }
        //updateEntropy();




        /*map[5, 5] = 2;*/

        tilemap = GetComponent<Tilemap>();

        


    }

    // Update is called once per frame
    void Update()
    {
        if (algorithm == Type.WFC)
        {
            if (!leastConflicts())
                GenerateMap();
        }
        else if(algorithm == Type.linear)
        {
            if (!leastConflictsLinear())
                GenerateMap();
        }
        else if (algorithm == Type.WFC_entropy)
        {
            if (!leastConflictsEntropy())
                GenerateMap();
        }
    }

    void GenerateMap()
    {
        int rows = width;
        int cols = height;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                int tileIndex = map[y, x];
                TileBase tileToPlace = tiles[tileIndex];

                Vector3Int position = new Vector3Int(x, -y, 0);

                tilemap.SetTile(position, tileToPlace);
            }
        }
    }

    bool leastConflicts()
    {
        bool success = true;
        int x, y;
        int conflicts;
        int tries = 8;
        for(int i = 0; i < width*height; i++)
        {
            x = UnityEngine.Random.Range(0, width);
            y = UnityEngine.Random.Range(0, height);
            conflicts = checkConflicts(x, y);
            if(conflicts > 0 || map[x, y] == 0)
            {
                success = false;
                int bestType = 0;
                int leastConflicts = 100;
                int tempT, tempC;
                for(int j = 0; j < tries; j++)
                {
                    tempT = 1 + UnityEngine.Random.Range(0, types - 1);
                    map[x, y] = tempT;
                    tempC = checkConflicts(x, y);
                    if(tempC < leastConflicts)
                    {
                        bestType = tempT;
                        leastConflicts = tempC;
                    }
                }
                map[x, y] = bestType;
            }
        }
        return success;
    }

    bool leastConflictsLinear()
    {
        bool success = true;
        int x = 0;
        int y = 0;
        int conflicts = 0;
        int tries = 8;
        for (int i = 0; i < width; i++)
        {
            for (int k = 0; k < height; k++)
            {
                x = i;
                y = k;
                conflicts = checkConflicts(x, y);
                if (conflicts > 0 || map[x, y] == 0)
                {
                    success = false;
                    int bestType = 0;
                    int leastConflicts = 100;
                    int tempT, tempC;
                    for (int j = 0; j < tries; j++)
                    {
                        tempT = 1 + UnityEngine.Random.Range(0, types - 1);
                        map[x, y] = tempT;
                        tempC = checkConflicts(x, y);
                        if (tempC < leastConflicts)
                        {
                            bestType = tempT;
                            leastConflicts = tempC;
                        }
                    }
                    map[x, y] = bestType;
                }
            }
        }
       
        return success;
    }

    int checkConflicts(int x, int y)
    {
        int conflicts = 0;
        int tx, ty;

        for(int dx = -range; dx <=range; dx++)
        {
            for(int dy = -range; dy <=range; dy++)
            {
                tx = (dx + x + width) % width;
                ty = (dy + y + height) % height;
                conflicts += rules[map[x, y]][map[tx, ty]];
            }
        }


        return conflicts;
    }


    private void updateEntropy(int x, int y)
    {
        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = -range; dy <= range; dy++)
            {
                int tx = (dx + x + width) % width;
                int ty = (dy + y + height) % height;

                cellEntropy[tx+ty*width] = calculateEntropy(tx, ty);
            }
        }
    }


    (int, int) pickCoords()
    {
        int x = 0;
        int y = 0;
        float minEntropy = float.MaxValue;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float entropy = cellEntropy[i + j * width];
                if (entropy < minEntropy && entropy > 1f) // 1f = ustalona komórka
                {
                    minEntropy = entropy;
                    x = i;
                    y = j;
                }
            }
        }

        return (x, y);
    }

    float calculateEntropy(int x, int y)
    {


        float Z = 0f;
        float sum = 0f;
        for (int type = 1; type < types; type++)
        {
            if (rules[map[x, y]][type] == 0)
            {
                float w = weights[type];
                Z += w;
                sum += w * Mathf.Log(w);
            }
        }

        if (Z == 0) return 0f;

        float entropy = Mathf.Log(Z) - (sum / Z);
        entropy += UnityEngine.Random.value * 1e-3f;
        return entropy;
    }

    int collapseCell(int x, int y)
    {
        List<int> possibleValues = Enumerable.Range(1, types - 1).ToList();

        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = -range; dy <= range; dy++)
            {
                int tx = (dx + x + width) % width;
                int ty = (dy + y + height) % height;

                int neighborType = map[tx, ty];
                if (neighborType != 0)
                {
                    for (int k = possibleValues.Count - 1; k >= 0; k--)
                    {
                        int candidate = possibleValues[k];
                        if (rules[neighborType][candidate] == 1)
                        {
                            possibleValues.RemoveAt(k);
                        }
                    }
                }
            }
        }

        if (possibleValues.Count == 0)
        {
            makeHole(x, y);
            updateEntropy(x, y);
            return 0;
        }

        int index = UnityEngine.Random.Range(0, possibleValues.Count);
        return possibleValues[index];
    }

    void makeHole(int x, int y)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int tx = (dx + x + width) % width;
                int ty = (dy + y + height) % height;

                map[tx, ty] = 0;
                updateEntropy(tx, ty);
            }
        }
    }


    bool leastConflictsEntropy()
    {
        bool success = true;

        (int x, int y) = pickCoords();
        if (map[x, y] == 0 || cellEntropy[x + y * width] != 1f)
        {
            success = false;
            int bestType = collapseCell(x, y);
            map[x, y] = bestType;
            updateEntropy(x, y);
        }
        return success;
    }




}
