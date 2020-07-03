using System.Collections.Generic;

using UnityEngine;


public static class World
{
    public static Dictionary<Vector3Int, Chunk> Chunks = new Dictionary<Vector3Int, Chunk>();


    // Awake is called when the script instance is being loaded.
    public static void Awake()
    {

    }

    // Start is called before the first frame update.
    public static void Start()
    {
        GenerateStartingChunks();
    }

    // Update is called once per frame.
    public static void Update()
    {

    }

    // Fixed update is called every fixed framerate frame.
    public static void FixedUpdate()
    {

    }

    // Regenerate Starting Chunks
    public static void RegenerateStartingChunks()
    {
        foreach(KeyValuePair<Vector3Int, Chunk> chunk in Chunks)
        {
            GameObject.Destroy(chunk.Value.ChunkGO);
        }
        Chunks.Clear();
        GenerateStartingChunks();
    }

    public static bool GetChunk(Vector3Int chunkPos, out Chunk chunk)
    {
        if(Chunks.ContainsKey(chunkPos))
        {
            chunk = Chunks[chunkPos];
            return true;
        }
        else
        {
            chunk = null;
            return false;
        }
    }

    // Generate Starting Chunks
    private static void GenerateStartingChunks()
    {
        Debug.Log("Generating Starting Chunks...");
        Debug.Log("Initializing Starting Chunks...");
        // Instantiate Starting Chunks with Generate flag set to true
        for(int x = 0; x < GameManager.Instance.StartingChunkArea; x++)
        {
            for(int y = 0; y < GameManager.Instance.StartingChunkArea; y++)
            {
                for(int z = 0; z < GameManager.Instance.StartingChunkArea; z++)
                {
                    Vector3Int newChunkPos = new Vector3Int(x, y, z);
                    Chunks.Add(newChunkPos, new Chunk(newChunkPos, true));
                }
            }
        }
        Debug.Log("Successfully Initialized Starting Chunks!");
        Debug.Log("Generating Cave Worms for Starting Chunks...");
        // Loop through all starting chunks and check a cube of max worm distance away for uninstantiated chunks and add them to a list for instantiation with generate flag set to false
        List<Chunk> chunksToAdd = new List<Chunk>();
        foreach(KeyValuePair<Vector3Int, Chunk> chunk in Chunks)
        {
            chunk.Value.GenerateCaveWorms();
            for(int x = chunk.Key.x - GameManager.Instance.MaxWormChunkDistance; x < chunk.Key.x + GameManager.Instance.MaxWormChunkDistance; x++)
            {
                for(int y = chunk.Key.y - GameManager.Instance.MaxWormChunkDistance; y < chunk.Key.y + GameManager.Instance.MaxWormChunkDistance; y++)
                {
                    for(int z = chunk.Key.z - GameManager.Instance.MaxWormChunkDistance; z < chunk.Key.z + GameManager.Instance.MaxWormChunkDistance; z++)
                    {
                        Vector3Int newPos = new Vector3Int(x, y, z);
                        if(Chunks.ContainsKey(newPos) == false)
                        {
                            Chunk wormOnlyChunk = new Chunk(newPos, false);
                            wormOnlyChunk.GenerateCaveWorms();
                            chunksToAdd.Add(wormOnlyChunk);
                        }
                    }
                }
            }
        }
        // Loop through all worm only chunks and add them to chunk dictionary
        foreach(Chunk chunk in chunksToAdd)
        {
            if(Chunks.ContainsKey(chunk.chunkPos) == false)
            {
                Chunks.Add(chunk.chunkPos, chunk);
            }
        }
        Debug.Log("Successfully Generated Cave Worms for Starting Chunks!");
        Debug.Log("Generating Chunk Data for Starting Chunks...");
        // Loop through all chunks and generate chunk data for ones flagged for such
        foreach(KeyValuePair<Vector3Int, Chunk> chunk in Chunks)
        {
            if(chunk.Value.ShouldGenerateChunkData == true)
            {
                chunk.Value.GenerateChunkData();
            }
        }
        Debug.Log("Successfully Generated Chunk Data for Starting Chunks!");
        if(GameManager.Instance.ShouldCarveWorms == true)
        {
            Debug.Log("Carving Cave Worms for Starting Chunks...");
            // Loop through all chunks and carve cave worms
            foreach(KeyValuePair<Vector3Int, Chunk> chunk in Chunks)
            {
                foreach(CaveWorm worm in chunk.Value.CaveWorms)
                {
                    foreach(Vector3Int node in worm.Nodes)
                    {
                        CarveCave(node, worm.Radius);
                    }
                }
            }
            Debug.Log("Successfully Carved Cave Worms for Starting Chunks!");
        }
        Debug.Log("Generating GameObjects and Meshes for Starting Chunks...");
        // Loop through all chunks and generate meshes for ones with chunk data
        foreach(KeyValuePair<Vector3Int, Chunk> chunk in Chunks)
        {
            if(chunk.Value.GeneratedChunkData == true)
            {
                chunk.Value.InitializeChunkObject();
            }
        }
        Debug.Log("Successfully Generated GameObjects and Meshes for Starting Chunks!");
    }

    // Carve Cave Worm
    private static void CarveCave(Vector3Int worldPos, int radius)
    {
        for(int x = worldPos.x - radius; x < worldPos.x + radius; x++)
        {
            for(int y = worldPos.y - radius; y < worldPos.y + radius; y++)
            {
                for(int z = worldPos.z - radius; z < worldPos.z + radius; z++)
                {
                    Vector3Int nextPos = new Vector3Int(x, y, z);
                    float distance = Vector3Int.Distance(nextPos, worldPos);
                    if(distance <= radius && GetChunk(nextPos.WorldPosToChunkPos(), out Chunk chunk) == true && chunk.GeneratedChunkData == true)
                    {
                        Vector3Int internalPos = nextPos.WorldPosToInternalPos();
                        float value = Mathf.SmoothStep(GameManager.Instance.CaveWormCarveValue, 0f, distance / radius);
                        chunk.ModifyTerrainMap(internalPos, value, true);
                    }
                }
            }
        }
    }
}
