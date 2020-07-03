using System.Collections.Generic;

using UnityEngine;


public class Chunk
{
	// GameObject Components
	private GameObject chunkGO;
	private MeshFilter meshFilter;
	private MeshRenderer meshRenderer;
	private MeshCollider meshCollider;

	// Chunk Data
	public Vector3Int chunkPos;
	private float[,,] terrainMap;
	public List<CaveWorm> CaveWorms = new List<CaveWorm>();

	// Mesh Data
	private readonly List<Vector3> vertices = new List<Vector3>();
	private readonly List<int> triangles = new List<int>();

	// Flags
	public bool ShouldGenerateChunkData = false;
	public bool GeneratedCaveWorms = false;
	public bool GeneratedChunkData = false;
	public bool GeneratedMesh = false;


	// Constructor
	public Chunk(Vector3Int position, bool generateData)
	{
		this.chunkPos = position;
		this.ShouldGenerateChunkData = generateData;
	}

	// Generate Cave Worms
	public void GenerateCaveWorms()
	{
		int posOffset = 1000;
		int numWorms = 4;//Mathf.RoundToInt(GameManager.Instance.CaveWormNoiseGenerator.GetNoise(this.chunkPos.x, this.chunkPos.y, this.chunkPos.z).Map(-1, 1, GameManager.Instance.MinimumCaveWorms, GameManager.Instance.MaximumCaveWorms));
		for(int i = 0; i < numWorms; i++)
		{
			int posX = Mathf.RoundToInt(GameManager.Instance.CaveWormNoiseGenerator.GetNoise(this.chunkPos.x + (posOffset * i), this.chunkPos.y + (posOffset * i), this.chunkPos.z + (posOffset * i)).Map(-1, 1, 0, GameManager.Instance.ChunkSize));
			int posY = Mathf.RoundToInt(GameManager.Instance.CaveWormNoiseGenerator.GetNoise(this.chunkPos.x + (posOffset * i), this.chunkPos.y + (posOffset * i), this.chunkPos.z + (posOffset * i)).Map(-1, 1, 0, GameManager.Instance.ChunkSize));
			int posZ = Mathf.RoundToInt(GameManager.Instance.CaveWormNoiseGenerator.GetNoise(this.chunkPos.x + (posOffset * i), this.chunkPos.y + (posOffset * i), this.chunkPos.z + (posOffset * i)).Map(-1, 1, 0, GameManager.Instance.ChunkSize));
			Vector3Int newWormPos = new Vector3Int(posX, posY, posZ).InternalPosToWorldPos(this.chunkPos);
			CaveWorm newWorm = new CaveWorm(newWormPos, GameManager.Instance.CaveWormRadius);
			this.CaveWorms.Add(newWorm);
		}
	}

	// Generate Chunk Data
	public void GenerateChunkData()
	{
		this.PopulateTerrainMap();
	}

	// Carve Cave Worm
	public void CarveCaveWorm(Vector3Int position, int radius)
    {
		Vector3Int internalPos = position.WorldPosToInternalPos();
		if(this.GeneratedChunkData == true)
        {
			for(int x = internalPos.x - radius; x < internalPos.x + radius; x++)
            {
				for(int y = internalPos.y - radius; y < internalPos.y + radius; y++)
				{
					for(int z = internalPos.z - radius; z < internalPos.z + radius; z++)
					{
						Vector3Int nextPos = new Vector3Int(x, y, z);
						if(this.IsPositionWithinBounds(nextPos) && Vector3Int.Distance(nextPos, internalPos) <= radius)
                        {
							this.terrainMap[internalPos.x, internalPos.y, internalPos.z] = -1f;
						}
					}
				}
			}
        }
    }

	// Is Position Within Bounds
	private bool IsPositionWithinBounds(Vector3Int position)
    {
		if(position.x >= 0 && position.x < GameManager.Instance.ChunkSize && position.y >= 0 && position.y < GameManager.Instance.ChunkSize && position.z >= 0 && position.z < GameManager.Instance.ChunkSize)
        {
			return true;
        }
		else
        {
			return false;
        }
    }

	// Initialize Chunk Object
	public void InitializeChunkObject()
    {
		this.chunkGO = new GameObject($@"Chunk: {this.chunkPos}");
		this.chunkGO.transform.position = this.chunkPos.ChunkPosToWorldPos();
		this.chunkGO.transform.parent = GameManager.Instance.ChunkParentGO.transform;
		this.meshFilter = this.chunkGO.AddComponent<MeshFilter>();
		this.meshRenderer = this.chunkGO.AddComponent<MeshRenderer>();
		this.meshRenderer.material = GameManager.Instance.ChunkMaterial;
		this.meshCollider = this.chunkGO.AddComponent<MeshCollider>();
		this.CreateMeshData();
	}

	// Remesh Chunk
	public void RemeshChunk()
    {
		this.GenerateCaveWorms();
		this.GenerateChunkData();
		this.CreateMeshData();
	}

	// Populate Terrain Map
	private void PopulateTerrainMap()
	{
		this.terrainMap = new float[GameManager.Instance.ChunkSize + 1, GameManager.Instance.ChunkSize + 1, GameManager.Instance.ChunkSize + 1];
		// The data points for terrain are stored at the corners of our "cubes", so the terrainMap needs to be 1 larger than the width/height of our mesh.
		for(int x = 0; x < GameManager.Instance.ChunkSize + 1; x++)
		{
			for(int z = 0; z < GameManager.Instance.ChunkSize + 1; z++)
			{
				for(int y = 0; y < GameManager.Instance.ChunkSize + 1; y++)
				{
					Vector3Int worldPos = new Vector3Int(x, y, z).InternalPosToWorldPos(this.chunkPos);
					float noiseValue = GameManager.Instance.NoiseGenerator.GetNoise(worldPos.x, worldPos.y, worldPos.z) * GameManager.Instance.Multiplier;
					this.terrainMap[x, y, z] = noiseValue;
				}
			}
		}
		this.GeneratedChunkData = true;
	}

	// Create Mesh Data
	private void CreateMeshData()
	{
        this.ClearMeshData();
		for(int x = 0; x < GameManager.Instance.ChunkSize; x++)
		{
			for(int y = 0; y < GameManager.Instance.ChunkSize; y++)
			{
				for(int z = 0; z < GameManager.Instance.ChunkSize; z++)
				{
                    this.MarchCube(new Vector3Int(x, y, z));
				}
			}
		}
        this.BuildMesh();
	}

	// Clear Mesh Data
	private void ClearMeshData()
	{
		this.vertices.Clear();
		this.triangles.Clear();
	}

	// March Cube
	private void MarchCube(Vector3Int position)
	{
		// Sample terrain values at each corner of the cube.
		float[] cube = new float[8];
		for(int i = 0; i < 8; i++)
		{
			cube[i] = this.SampleTerrain(position + GameManager.CornerTable[i]);
		}
		// Get the configuration index of this cube.
		int configIndex = this.GetCubeConfiguration(cube);
		// If the configuration of this cube is 0 or 255 (completely inside the terrain or completely outside of it) we don't need to do anything.
		if(configIndex == 0 || configIndex == 255)
        {
            return;
        }
        // Loop through the triangles. There are never more than 5 triangles to a cube and only three vertices to a triangle.
        int edgeIndex = 0;
		for(int i = 0; i < 5; i++)
		{
			for(int p = 0; p < 3; p++)
			{
				// Get the current indice. We increment triangleIndex through each loop.
				int index = GameManager.TriangleTable[configIndex, edgeIndex];
				// If the current edgeIndex is -1, there are no more indices and we can exit the function.
				if(index == -1)
				{
					return;
				}
				// Get the vertices for the start and end of this edge.
				Vector3 vert1 = position + GameManager.CornerTable[GameManager.EdgeIndexes[index, 0]];
				Vector3 vert2 = position + GameManager.CornerTable[GameManager.EdgeIndexes[index, 1]];
				Vector3 vertPosition;
				if(GameManager.Instance.SmoothTerrain == true)
                {
					// Get the terrain values at either end of our current edge from the cube array created above.
					float vert1Sample = cube[GameManager.EdgeIndexes[index, 0]];
					float vert2Sample = cube[GameManager.EdgeIndexes[index, 1]];
					// Calculate the difference between the terrain values.
					float difference = vert2Sample - vert1Sample;
					// If the difference is 0, then the terrain passes through the middle.
					if(difference == 0)
                    {
						difference = GameManager.Instance.TerrainSurfaceCutoff;
                    }
					else
                    {
						difference = (GameManager.Instance.TerrainSurfaceCutoff - vert1Sample) / difference;
                    }
					// Calculate the point alog the edge that passes through.
					vertPosition = vert1 + ((vert2 - vert1) * difference);
				}
				else
                {
					// Get the midpoint of this edge.
					vertPosition = (vert1 + vert2) / 2f;
				}
                // Add to our vertices and triangles list and incremement the edgeIndex.
                this.vertices.Add(vertPosition);
                this.triangles.Add(this.vertices.Count - 1);
				edgeIndex++;
			}
		}
	}

	// Sample Terrain
	private float SampleTerrain(Vector3Int point)
	{
		return this.terrainMap[point.x, point.y, point.z];
	}

	// Get Cube Configuration
	private int GetCubeConfiguration(float[] cube)
	{
		// Starting with a configuration of zero, loop through each point in the cube and check if it is below the terrain surface.
		int configurationIndex = 0;
		for(int i = 0; i < 8; i++)
		{
			// If it is, use bit-magic to the set the corresponding bit to 1. So if only the 3rd point in the cube was below
			// the surface, the bit would look like 00100000, which represents the integer value 32.
			if(cube[i] > GameManager.Instance.TerrainSurfaceCutoff)
            {
                configurationIndex |= 1 << i;
            }
        }
		return configurationIndex;
	}

	// Build Mesh
    private void BuildMesh()
    {
        Mesh mesh = new Mesh
        {
            vertices = this.vertices.ToArray(),
            triangles = this.triangles.ToArray()
        };
        mesh.RecalculateNormals();
        this.meshFilter.mesh = mesh;
		this.meshCollider.sharedMesh = mesh;
		this.GeneratedMesh = true;
	}
}
