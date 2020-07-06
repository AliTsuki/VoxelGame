using System.Collections.Generic;

using UnityEngine;


/// <summary>
/// Static class containing references to everything that exists in the game world. Controls chunk spawning/despawning.
/// </summary>
public static class World
{
    /// <summary>
    /// Dictionary of every chunk that exists in the game world.
    /// </summary>
    private readonly static Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();


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

    /// <summary>
    /// Removes all chunks and then generates them anew.
    /// </summary>
    public static void RegenerateStartingChunks()
    {
        RemoveAllChunks();
        GenerateStartingChunks();
    }

    /// <summary>
    /// Get a reference to a currently existing chunk by referencing the chunk coordinate system position.
    /// </summary>
    /// <param name="chunkPos">The position in chunk coordinate system to retrieve a chunk from.</param>
    /// <param name="chunk">The reference to the chunk at that position, if one exists.</param>
    /// <returns>Returns true if a chunk exists at the given position.</returns>
    public static bool GetChunk(Vector3Int chunkPos, out Chunk chunk)
    {
        if(chunks.ContainsKey(chunkPos))
        {
            chunk = chunks[chunkPos];
            return true;
        }
        else
        {
            chunk = null;
            return false;
        }
    }

    /// <summary>
    /// Removes all currently existing chunks by destroying their GameObjects and clearing the chunk dictionary.
    /// </summary>
    private static void RemoveAllChunks()
    {
        foreach(KeyValuePair<Vector3Int, Chunk> chunk in chunks)
        {
            GameObject.Destroy(chunk.Value.ChunkGO);
        }
        chunks.Clear();
    }

    /// <summary>
    /// Generates a cube of chunks of StartingChunkArea cubed in size. First creates chunk references in memory, 
    /// then creates cave worms for each chunk, then creates chunk data for each chunk, then carves the caves through
    /// the chunk data using the cave worms, and finally generates a GameObject for each chunk and performs
    /// marching cubes algorithm to mesh each chunk based on the chunk data.
    /// </summary>
    private static void GenerateStartingChunks()
    {
        Debug.Log("Generating Starting Chunks...");
        //------------------------------------------------------------------------------------------
        // Instantiate Starting Chunks with Generate flag set to true.
        Debug.Log("Initializing Starting Chunks...");
        for(int x = 0; x < GameManager.Instance.StartingChunkArea; x++)
        {
            for(int y = 0; y < GameManager.Instance.StartingChunkArea; y++)
            {
                for(int z = 0; z < GameManager.Instance.StartingChunkArea; z++)
                {
                    Vector3Int newChunkPos = new Vector3Int(x, y, z);
                    chunks.Add(newChunkPos, new Chunk(newChunkPos, true));
                }
            }
        }
        Debug.Log("Successfully Initialized Starting Chunks!");
        //------------------------------------------------------------------------------------------
        if(GameManager.Instance.ShouldCarveWorms == true)
        {
            // Loop through all starting chunks and check a cube of max worm distance away for uninstantiated chunks and add them to a list for instantiation with generate flag set to false.
            Debug.Log("Generating Cave Worms for Starting Chunks...");
            List<Chunk> chunksToAdd = new List<Chunk>();
            foreach(KeyValuePair<Vector3Int, Chunk> chunk in chunks)
            {
                chunk.Value.GenerateCaveWorms();
                for(int x = chunk.Key.x - GameManager.Instance.MaxWormChunkDistance; x < chunk.Key.x + GameManager.Instance.MaxWormChunkDistance; x++)
                {
                    for(int y = chunk.Key.y - GameManager.Instance.MaxWormChunkDistance; y < chunk.Key.y + GameManager.Instance.MaxWormChunkDistance; y++)
                    {
                        for(int z = chunk.Key.z - GameManager.Instance.MaxWormChunkDistance; z < chunk.Key.z + GameManager.Instance.MaxWormChunkDistance; z++)
                        {
                            Vector3Int newPos = new Vector3Int(x, y, z);
                            if(chunks.ContainsKey(newPos) == false)
                            {
                                Chunk wormOnlyChunk = new Chunk(newPos, false);
                                wormOnlyChunk.GenerateCaveWorms();
                                chunksToAdd.Add(wormOnlyChunk);
                            }
                        }
                    }
                }
            }
            // Loop through all worm only chunks and add them to chunk dictionary.
            foreach(Chunk chunk in chunksToAdd)
            {
                if(chunks.ContainsKey(chunk.ChunkPos) == false)
                {
                    chunks.Add(chunk.ChunkPos, chunk);
                }
            }
            Debug.Log("Successfully Generated Cave Worms for Starting Chunks!");
        }
        //------------------------------------------------------------------------------------------
        // Loop through all chunks and generate chunk data for ones flagged for such.
        Debug.Log("Generating Chunk Data for Starting Chunks...");
        foreach(KeyValuePair<Vector3Int, Chunk> chunk in chunks)
        {
            if(chunk.Value.ShouldGenerateChunkData == true)
            {
                chunk.Value.GenerateChunkData();
            }
        }
        Debug.Log("Successfully Generated Chunk Data for Starting Chunks!");
        //------------------------------------------------------------------------------------------
        if(GameManager.Instance.ShouldCarveWorms == true)
        {        
            // Loop through all chunks and carve cave worms.
            Debug.Log("Carving Cave Worms for Starting Chunks...");
            foreach(KeyValuePair<Vector3Int, Chunk> chunk in chunks)
            {
                foreach(CaveWorm worm in chunk.Value.CaveWorms)
                {
                    foreach(CaveWorm.Segment segment in worm.Segments)
                    {
                        foreach(CaveWorm.Segment.Point point in segment.Points)
                        {
                            if(GetChunk(point.WorldPosition.WorldPosToChunkPos(), out Chunk chunkOfPoint) == true && chunkOfPoint.HasGeneratedChunkData == true)
                            {
                                chunkOfPoint.SetChunkDataValue(point.WorldPosition.WorldPosToInternalPos(), point.Value, true);
                            }
                        }
                    }
                }
            }
            Debug.Log("Successfully Carved Cave Worms for Starting Chunks!");
        }
        //------------------------------------------------------------------------------------------
        // Loop through all chunks and generate meshes for ones with chunk data.
        Debug.Log("Generating GameObjects and Meshes for Starting Chunks...");
        foreach(KeyValuePair<Vector3Int, Chunk> chunk in chunks)
        {
            if(chunk.Value.HasGeneratedChunkData == true)
            {
                chunk.Value.InstantiateChunkGameObject();
            }
        }
        Debug.Log("Successfully Generated GameObjects and Meshes for Starting Chunks!");
        //------------------------------------------------------------------------------------------
    }
}
