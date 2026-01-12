using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Rule
{ 
    [SerializeField] private char character;
    [SerializeField] private string[] values;
    [SerializeField] private bool isRandom = false;

    public char getCharacter()
    {
        return character;
    }
    public string getValue()
    {
        if (isRandom)
        {
            return values[Random.Range(0, values.Length)];
        }
        else
        {
            return values[0];
        }
    }
  
}
