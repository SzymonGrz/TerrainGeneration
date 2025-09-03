using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LSystem
{
    // Start is called before the first frame update
    public string lSystem(string[] axioms, Rule[] rules, int iterations)
    {
        string route = axioms[Random.Range(0, axioms.Length)].Clone().ToString();
        string newRoute = "";
        for (int i = 0; i < iterations; i++)
        {
            int length = route.Length;
            foreach(char c in route)
            {
                var rule = rules.FirstOrDefault(r => r.getCharacter() == c);
                if (rule != null)
                {
                    newRoute = newRoute + rule.getValue();
                }
                else
                {
                    newRoute = newRoute + c;
                }
                
            }
            route = newRoute.Clone().ToString();
            newRoute = "";
        }
        return route;
    }

}
