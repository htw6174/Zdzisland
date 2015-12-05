using UnityEngine;
using System.Collections;

public class TerrainCreator : MonoBehaviour {

    Terrain active;
    float[,] samples;

    void Awake()
    {
        active = Terrain.activeTerrain;
        int width = active.terrainData.heightmapWidth;
        Debug.Log(width);
        samples = new float[width, width];
        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < width; x++)
            {
                samples[y, x] = ((Mathf.Sin((float)x / 20f) + 1f) + (Mathf.Cos((float)y / 20f) + 1f)) / 30f;
            }
        }
        active.terrainData.SetHeights(0, 0, samples);
    }
}
