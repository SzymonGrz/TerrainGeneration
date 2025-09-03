using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Tile : MonoBehaviour 
{
    public Sprite sprite;

    Tile(int value)
    {
        GetComponent<SpriteRenderer>().sprite = sprite;
    }

    public static explicit operator Tile(TileBase v)
    {
        throw new NotImplementedException();
    }

    /*void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }*/
}
