using System.Collections.Generic;
using UnityEngine;

public class TerrainGen : MonoBehaviour
{
    [System.Serializable]
    public struct TileSet
    {
        public string name;
        [Range(0f, 1f)] public float minHeight;

        [Header("Prefabs")]
        public GameObject center;
        public GameObject straight;
        public GameObject corner;
        public GameObject invertedCorner;
    }

    [Header("Grid Settings")]
    public int mapWidth = 50;
    public int mapHeight = 50;
    public float tileSize = 2f;

    [Header("Island Tweak Parameters")]
    [Range(1f, 20f)] public float islandSize = 5f;       // Lower = Large massive continents, Higher = Smaller islands
    [Range(0.1f, 0.9f)] public float waterLevel = 0.45f; // Higher = Fewer/smaller islands (more water), Lower = More land
    public bool useIslandFalloff = true;                // Forces the edges of the map to be water (creates a central island cluster)

    [Header("Biome Layers (Order from lowest to highest)")]
    public List<TileSet> biomes;

    private float[,] heightMap;
    private GameObject[,] spawnedTiles;

    void Start()
    {
        GenerateTerrain();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            GenerateTerrain();
        }
    }

    [ContextMenu("Regenerate")]
    public void GenerateTerrain()
    {
        // Clear old islands if regenerating in editor
        ClearOldTerrain();

        heightMap = new float[mapWidth, mapHeight];
        spawnedTiles = new GameObject[mapWidth, mapHeight];

        float seedX = Random.Range(0f, 50000f);
        float seedZ = Random.Range(0f, 50000f);

        // Step 1: Generate Heights using Perlin Noise
        for (int x = 0; x < mapWidth; x++)
        {
            for (int z = 0; z < mapHeight; z++)
            {
                float xCoord = (float)x / mapWidth * islandSize + seedX;
                float zCoord = (float)z / mapHeight * islandSize + seedZ;

                float noise = Mathf.PerlinNoise(xCoord, zCoord);

                if (useIslandFalloff)
                {
                    noise *= CalculateFalloff(x, z);
                }

                heightMap[x, z] = noise;
            }
        }

        // Step 2: Place Tiles based on Height and Biome Rules
        CalculateAndPlaceTiles();

        meshCombiner combiner = GetComponent<meshCombiner>();
        if (combiner != null)
        {
            combiner.CombineSelectedMeshes();
        }
    }

    // Creates a gradient that slopes down to 0 at the map edges, forcing water to surround your islands
    float CalculateFalloff(int x, int z)
    {
        float xv = x / (float)mapWidth * 2 - 1;
        float zv = z / (float)mapHeight * 2 - 1;
        float value = Mathf.Max(Mathf.Abs(xv), Mathf.Abs(zv));

        // Classic falloff formula
        float a = 3f;
        float b = 2.2f;
        float falloff = Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
        return 1f - falloff;
    }
    void CalculateAndPlaceTiles()
    {
        // Calculate the offsets once before the loops start
        float halfWidth = (mapWidth * tileSize) / 2f;
        float halfHeight = (mapHeight * tileSize) / 2f;
        float pivotOffset = tileSize / 2f;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int z = 0; z < mapHeight; z++)
            {
                float currentHeight = heightMap[x, z];

                if (currentHeight < waterLevel) continue;

                TileSet activeBiome = GetBiomeForHeight(currentHeight);

                // Cardinal Neighbors
                bool N = (z + 1 < mapHeight) && (heightMap[x, z + 1] >= waterLevel);
                bool S = (z - 1 >= 0) && (heightMap[x, z - 1] >= waterLevel);
                bool E = (x + 1 < mapWidth) && (heightMap[x + 1, z] >= waterLevel);
                bool W = (x - 1 >= 0) && (heightMap[x - 1, z] >= waterLevel);

                // Diagonal Neighbors
                bool NE = (x + 1 < mapWidth && z + 1 < mapHeight) && (heightMap[x + 1, z + 1] >= waterLevel);
                bool NW = (x - 1 >= 0 && z + 1 < mapHeight) && (heightMap[x - 1, z + 1] >= waterLevel);
                bool SE = (x + 1 < mapWidth && z - 1 >= 0) && (heightMap[x + 1, z - 1] >= waterLevel);
                bool SW = (x - 1 >= 0 && z - 1 >= 0) && (heightMap[x - 1, z - 1] >= waterLevel);

                // Count cardinal land neighbors to detect 1x1 islands and caps
                int landNeighborCount = (N ? 1 : 0) + (S ? 1 : 0) + (E ? 1 : 0) + (W ? 1 : 0);

                // REMOVE CAPS AND 1x1: Skip spawning anything if it's an isolated chunk or a dead-end tile
                if (landNeighborCount <= 1) continue;

                // Centered position calculation
                Vector3 position = new Vector3(
                    (x * tileSize) - halfWidth + pivotOffset,
                    0,
                    (z * tileSize) - halfHeight + pivotOffset
                );

                GameObject prefabToSpawn = null;
                Quaternion rotation = Quaternion.identity;

                // 1. Inverted Corners
                if (N && E && W && !NW) { prefabToSpawn = activeBiome.invertedCorner; rotation = Quaternion.Euler(0, 90, 0); }
                else if (N && S && E && !NE) { prefabToSpawn = activeBiome.invertedCorner; rotation = Quaternion.Euler(0, 180, 0); }
                else if (S && E && W && !SE) { prefabToSpawn = activeBiome.invertedCorner; rotation = Quaternion.Euler(0, 270, 0); }
                else if (N && S && W && !SW) { prefabToSpawn = activeBiome.invertedCorner; rotation = Quaternion.Euler(0, 0, 0); }

                // 2. Center Tile
                else if (N && S && E && W) { prefabToSpawn = activeBiome.center; }

                // 3. Straight Coasts
                else if (N && S && E && !W) { prefabToSpawn = activeBiome.straight; rotation = Quaternion.Euler(0, 90, 0); }
                else if (N && S && !E && W) { prefabToSpawn = activeBiome.straight; rotation = Quaternion.Euler(0, 270, 0); }
                else if (N && !S && E && W) { prefabToSpawn = activeBiome.straight; rotation = Quaternion.Euler(0, 0, 0); }
                else if (!N && S && E && W) { prefabToSpawn = activeBiome.straight; rotation = Quaternion.Euler(0, 180, 0); }

                // 4. Outer Corners
                else if (!N && !E && S && W) { prefabToSpawn = activeBiome.corner; rotation = Quaternion.Euler(0, 270, 0); }
                else if (!N && !W && S && E) { prefabToSpawn = activeBiome.corner; rotation = Quaternion.Euler(0, 180, 0); }
                else if (!S && !W && N && E) { prefabToSpawn = activeBiome.corner; rotation = Quaternion.Euler(0, 90, 0); }
                else if (!S && !E && N && W) { prefabToSpawn = activeBiome.corner; rotation = Quaternion.Euler(0, 0, 0); }

                // Spawn and track
                if (prefabToSpawn != null)
                {
                    GameObject tile = Instantiate(prefabToSpawn, position, rotation, this.transform);
                    spawnedTiles[x, z] = tile;
                }
            }
        }
    }

    TileSet GetBiomeForHeight(float height)
    {
        // Loop backwards from highest biome to lowest to find the correct layer
        for (int i = biomes.Count - 1; i >= 0; i--)
        {
            if (height >= biomes[i].minHeight)
                return biomes[i];
        }
        return biomes[0];
    }

    void ClearOldTerrain()
    {
        if (spawnedTiles == null) return;
        for (int x = 0; x < mapWidth; x++)
        {
            for (int z = 0; z < mapHeight; z++)
            {
                if (spawnedTiles[x, z] != null)
                {
                    Destroy(spawnedTiles[x, z]);
                }
            }
        }
    }
}

