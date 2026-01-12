using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Noise
{
    public class VoronoiNoise
    {
        public static Vector3 Noise(float x, float y)
        {
            Vector2 p = new Vector2(Mathf.Floor(x), Mathf.Floor(y));
            Vector2 f = new Vector2(x % 1, y % 1);

            Vector3 m = new Vector3(5f, 0f, 0f);

            
            for (int j = -1; j <= 1; j++)
            {
                for (int i = -1; i <= 1; i++)
                {
                    Vector2 g = new Vector2(i, j);
                    Vector3 o = RandomVector3(p + g);
                    Vector2 r = g - f + new Vector2(o.x, o.y);
                    float d = Vector2.Dot(r, r);
                    
                    if(d < m.x)
                    {
                        m.x = Mathf.Min(m.x, d);
                        Vector2 temp = p + g;
                        m.y = -2f * r.x;//temp.x;
                        m.z = -2f * r.y;//temp.y;
                    }    
                }
            }
            return m;
        }

        private static Vector3 RandomVector3(Vector2 input)
        {
            Vector3 q = new Vector3(
                Vector2.Dot(input, new Vector2(127.1f, 311.7f)),
                Vector2.Dot(input, new Vector2(269.5f, 183.3f)),
                Vector2.Dot(input, new Vector2(419.2f, 371.9f))
                );
            q.x = Mathf.Sin(q.x) * 43758.5453f;
            q.y = Mathf.Sin(q.y) * 43758.5453f;
            q.z = Mathf.Sin(q.z) * 43758.5453f;

            q.x = q.x % 1;
            q.y = q.y % 1;
            q.z = q.z % 1;

            return q ;
        }

        public static float CellHeight(float x, float y)
        {
            Vector2 k = new Vector2(x, y);
            float n = Vector2.Dot(k, new Vector2(12.9898f, 78.233f));
            float r = Mathf.Sin(n) * 43758.5453f;
            return r % 1;
        }
    }
}
