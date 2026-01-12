
using UnityEngine;

//Poprawiæ
namespace Noise
{
    public class ValueNoise
    {
        public static float Noise(float x, float y)
        {
            int x0 = Mathf.FloorToInt(x);
            int x1 = x0 + 1;
            int y0 = Mathf.FloorToInt(y);
            int y1 = y0 + 1;


            float sx = SmoothStep(x - x0);
            float sy = SmoothStep(y - y0);


            float v00 = RandomValue(x0, y0);
            float v10 = RandomValue(x1, y0);
            float v01 = RandomValue(x0, y1);
            float v11 = RandomValue(x1, y1);


            float ix0 = Mathf.Lerp(v00, v10, sx);
            float ix1 = Mathf.Lerp(v01, v11, sx);


            return Mathf.Lerp(ix0, ix1, sy);
        }

        private static float SmoothStep(float t)
        {
            return t * t * (3f - 2f * t);
        }


        private static float RandomValue(int x, int y)
        {
            int n = x * 1619 + y * 31337 + 1337 * 1013;
            n = (n << 13) ^ n;
            int nn = (n * (n * n * 60493 + 19990303) + 1376312589);
            return 1.0f - ((nn & 0x7fffffff) / 1073741824.0f); 
        }
    }
}
