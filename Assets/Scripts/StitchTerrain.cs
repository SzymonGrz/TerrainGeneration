using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StitchTerrain : MonoBehaviour
{

    Terrain terrain;

    // Start is called before the first frame update
    void Start()
    {
        terrain = GetComponent<Terrain>();
        Terrain left = GameObject.Find("Chunk_4").GetComponent<Terrain>();
        Terrain bottom = GameObject.Find("Chunk_5").GetComponent<Terrain>();
        terrain.SetNeighbors(left,null,null,bottom );
        StitchToBottom(terrain, bottom);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StitchToBottom(Terrain terrain, Terrain bottomTerrain)
    {
        TerrainData terrainData = terrain.terrainData;
        int resolution = terrainData.heightmapResolution;
        float[,] edgeValues = bottomTerrain.terrainData.GetHeights(0, resolution - 1, resolution - 1, 1);
        terrainData.SetHeights(0, 0, edgeValues);
    }
}
