using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErosionGenerator : MonoBehaviour
{
    [SerializeField] private Terrain terrain;
    private TerrainData terrainData;

    private struct Droplet
    {
        int x;
        int y;
        float height;
        float waterCapacity;
        float sedimentCapacity;
        float sediment;
        float radius;

       /* public Droplet(int _x, int _y, float _height)
        {
            x = _x;
            y = _y;
            height = _height;
            sedimentCapacity = 2f;
            waterCapacity = 2f;
            sediment = 0f;
            radius = 1f;
        }*/
    }

    void Start()
    {
        terrainData = terrain.terrainData;
    }

    void Update()
    {
        
    }

    /*void Erode(float[,] heights, int size)
    {
        int randomX = Random.Range(0, size);
        int randomY = Random.Range(0, size);
        float randomHeight = heights[randomX, randomY];
        Droplet droplet = new Droplet(randomX, randomY, randomHeight);

        for(int lifetime = 0; lifetime < 30; lifetime++)
        {
           // Vector2 direction;

        }

    }*/

}
