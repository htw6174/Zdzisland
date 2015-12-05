using UnityEngine;

public class TextureCreator : MonoBehaviour {

    public Vector3 position;

	public float frequency = 1f;

	[Range(1, 8)]
	public int octaves = 1;

	[Range(1f, 4f)]
	public float lacunarity = 2f;

	[Range(0f, 1f)]
	public float persistence = 0.5f;

	[Range(1, 3)]
	public int dimensions = 3;

    [Range(0f, 1f)]
    public float amplitude = 1f;

    [Range(-1f, 1f)]
    public float offset = 0f;

    private float terrainHeight;

	public NoiseMethodType type;

    public int detailDensity = 10;

    [Range(1f, 90f)]
    public float cliffAngle = 50f;

    [Range(1f, 90f)]
    public float grassGrowthAngle = 30f;

    public AnimationCurve cliffTransition;

    [Range(0, 1000)]
    public int treeFrequency;

    public bool liveHeightmap = false;

    public bool liveTexture = false;

    private TerrainData data;

    private float[,] heights;

    private float[,,] texture;

    private int[,] details;
	
	private void OnEnable () {
		//FillHeights();
	}

	//private void Update () {
	//	if (transform.hasChanged) {
	//		transform.hasChanged = false;
	//		FillHeights();
	//	}
	//}
	
    public void RandomizePosition()
    {
        position.x = Random.Range(-99f, 99f);
        position.y = Random.Range(-99f, 99f);
        position.z = Random.Range(-99f, 99f);
        FillHeights();
        FillTexture();
    }

	public void FillHeights ()
    {
        int resolution = Terrain.activeTerrain.terrainData.heightmapWidth;
        terrainHeight = Terrain.activeTerrain.terrainData.heightmapHeight;
        heights = new float[resolution, resolution];

        Vector3 point00 = position + new Vector3(-0.5f,-0.5f);
		Vector3 point10 = position + new Vector3( 0.5f,-0.5f);
		Vector3 point01 = position + new Vector3(-0.5f, 0.5f);
		Vector3 point11 = position + new Vector3( 0.5f, 0.5f);

        NoiseMethod method = Noise.methods[(int)type][dimensions - 1];
		float stepSize = 1f / resolution;
		for (int y = 0; y < resolution; y++) {
			Vector3 point0 = Vector3.Lerp(point00, point01, (y + 0.5f) * stepSize);
			Vector3 point1 = Vector3.Lerp(point10, point11, (y + 0.5f) * stepSize);
			for (int x = 0; x < resolution; x++) {
				Vector3 point = Vector3.Lerp(point0, point1, (x + 0.5f) * stepSize);
				float sample = Noise.Sum(method, point, frequency, octaves, lacunarity, persistence);
				if (type != NoiseMethodType.Value) {
					sample = sample * 0.5f + 0.5f;
				}
                heights[y, x] = (sample * amplitude) + (offset);
			}
		}
        Terrain.activeTerrain.terrainData.SetHeights(0, 0, heights);
	}

    public void FillTexture()
    {
        int resolution = Terrain.activeTerrain.terrainData.alphamapWidth;
        texture = new float[resolution, resolution, 2];
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                Vector3 normal = Terrain.activeTerrain.terrainData.GetInterpolatedNormal((float)x / resolution, (float)y / resolution);
                float angle = Vector3.Angle(Vector3.up, normal);
                float slope = Mathf.Clamp(angle / cliffAngle, 0, 1);
                //if (x % 100 == 0 && y % 100 == 0)
                //{
                //    Debug.Log(angle);
                //    Debug.Log(slope);
                //}
                texture[y, x, 0] = 1f - cliffTransition.Evaluate(slope);
                texture[y, x, 1] = cliffTransition.Evaluate(slope);
            }
        }
        Terrain.activeTerrain.terrainData.SetAlphamaps(0, 0, texture);
    }

    public void FillDetails()
    {
        int resolution = Terrain.activeTerrain.terrainData.detailWidth;
        details = new int[resolution, resolution];

        Vector3 point00 = position + new Vector3(-0.5f, -0.5f);
        Vector3 point10 = position + new Vector3(0.5f, -0.5f);
        Vector3 point01 = position + new Vector3(-0.5f, 0.5f);
        Vector3 point11 = position + new Vector3(0.5f, 0.5f);

        NoiseMethod method = Noise.methods[(int)type][dimensions - 1];
        float stepSize = 1f / resolution;
        for (int y = 0; y < resolution; y++)
        {
            Vector3 point0 = Vector3.Lerp(point00, point01, (y + 0.5f) * stepSize);
            Vector3 point1 = Vector3.Lerp(point10, point11, (y + 0.5f) * stepSize);
            for (int x = 0; x < resolution; x++)
            {
                Vector3 normal = Terrain.activeTerrain.terrainData.GetInterpolatedNormal((float)x / resolution, (float)y / resolution);
                float angle = Vector3.Angle(Vector3.up, normal);
                if(angle < grassGrowthAngle)
                {
                    Vector3 point = Vector3.Lerp(point0, point1, (x + 0.5f) * stepSize);
                    float sample = Noise.Sum(method, point, frequency, octaves, lacunarity, persistence);
                    if (type != NoiseMethodType.Value)
                    {
                        sample = sample * 0.5f + 0.5f;
                    }
                    details[y, x] = (int)((1f - sample) * detailDensity);
                }
                else
                {
                    details[y, x] = 0;
                }
            }
        }
        Terrain.activeTerrain.terrainData.SetDetailLayer(0, 0, 0, details);
    }

    public void PlaceTrees(int prototypeIndex)
    {
        //clear trees
        Terrain.activeTerrain.terrainData.treeInstances = new TreeInstance[treeFrequency];

        //place trees
        Vector3 maxPosition = Terrain.activeTerrain.terrainData.size;
        for(int i = 0; i < treeFrequency; i++)
        {
            TreeInstance instance = new TreeInstance();
            instance.heightScale = 1f;
            instance.widthScale = 1f;
            instance.color = Color.white;
            instance.lightmapColor = Color.white;
            instance.position = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            instance.prototypeIndex = prototypeIndex;
            Terrain.activeTerrain.AddTreeInstance(instance);
            Debug.Log("Placed tree #" + (i + 1));
        }
    }
}