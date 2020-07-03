using System;
using System.Collections.Generic;

using UnityEngine;


public class Chunk
{
	[Flags]
	private enum Neighbors
    {
		None = 0,
		xNeg = 1,
		xPos = 2,
		yNeg = 4,
		yPos = 8,
		zNeg = 16,
		zPos = 32
    }

	// GameObject Components
	public GameObject ChunkGO { get; private set; }
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
		// TODO: Change number of worms to be based on noise and not the same value for every chunk
		int numWorms = GameManager.Instance.MinimumCaveWorms;
		for(int i = 0; i < numWorms; i++)
		{
			int posX = Mathf.RoundToInt(GameManager.Instance.CaveWormPositionNoiseGenerator.GetNoise((this.chunkPos.x * this.chunkPos.x) + (posOffset * 1 * i), this.chunkPos.y + (posOffset * 1 * i), this.chunkPos.z + (posOffset * 1 * i)).Map(-1, 1, 0, GameManager.Instance.ChunkSize + 1));
			int posY = Mathf.RoundToInt(GameManager.Instance.CaveWormPositionNoiseGenerator.GetNoise(this.chunkPos.x + (posOffset * 2 * i), (this.chunkPos.y * this.chunkPos.y) + (posOffset * 2 * i), this.chunkPos.z + (posOffset * 2 * i)).Map(-1, 1, 0, GameManager.Instance.ChunkSize + 1));
			int posZ = Mathf.RoundToInt(GameManager.Instance.CaveWormPositionNoiseGenerator.GetNoise(this.chunkPos.x + (posOffset * 3 * i), this.chunkPos.y + (posOffset * 3 * i), (this.chunkPos.z * this.chunkPos.z) + (posOffset * 3 * i)).Map(-1, 1, 0, GameManager.Instance.ChunkSize + 1));
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

	// Modify Terrain Map
	public void ModifyTerrainMap(Vector3Int internalPos, float value, bool originalRequest)
	{
		this.terrainMap[internalPos.x, internalPos.y, internalPos.z] += value;
		if(originalRequest == true)
        {
			this.NotifyNeighbors(internalPos, value);
		}
	}

	// Notify Neighbors
	private void NotifyNeighbors(Vector3Int internalPos, float value)
    {
		// TODO: Seams still sometimes appearing, maybe drop notify neighbors method and replace with an edge copy method that averages the values for every edge location with neighboring chunk edges
		// Check if this position is neighboring any other chunks, get how many/which directions
		List<Neighbors> neighbors = new List<Neighbors>();
		if(internalPos.x == 0)
		{
			neighbors.Add(Neighbors.xNeg);
		}
		else if(internalPos.x == GameManager.Instance.ChunkSize)
		{
			neighbors.Add(Neighbors.xPos);
		}
		if(internalPos.y == 0)
		{
			neighbors.Add(Neighbors.yNeg);
		}
		else if(internalPos.y == GameManager.Instance.ChunkSize)
		{
			neighbors.Add(Neighbors.yPos);
		}
		if(internalPos.z == 0)
		{
			neighbors.Add(Neighbors.zNeg);
		}
		else if(internalPos.z == GameManager.Instance.ChunkSize)
		{
			neighbors.Add(Neighbors.zPos);
		}
		// If on face (1): single neighbor (x), if on edge (2): triple neighbor (x, y, xy), if on corner (3): sept neighbor (x, y, z, xy, yz, xz, xyz)
		// Edge
		if(neighbors.Count == 2)
        {
			neighbors.Add(Neighbors.None | neighbors[0] | neighbors[1]);
        }
		// Corner
		if(neighbors.Count == 3)
		{
			neighbors.Add(Neighbors.None | neighbors[0] | neighbors[1]);
			neighbors.Add(Neighbors.None | neighbors[1] | neighbors[2]);
			neighbors.Add(Neighbors.None | neighbors[0] | neighbors[2]);
			neighbors.Add(Neighbors.None | neighbors[0] | neighbors[1] | neighbors[2]);
        }
		if(neighbors.Count > 0)
		{
			foreach(Neighbors neighbor in neighbors)
			{
				Vector3Int newChunkPos = this.chunkPos;
				Vector3Int newInternalPos = internalPos;
				if(neighbor.HasFlag(Neighbors.xNeg))
				{
					newChunkPos.x = this.chunkPos.x - 1;
					newInternalPos.x = GameManager.Instance.ChunkSize;
				}
				else if(neighbor.HasFlag(Neighbors.xPos))
                {
					newChunkPos.x = this.chunkPos.x + 1;
					newInternalPos.x = 0;
                }
				if(neighbor.HasFlag(Neighbors.yNeg))
				{
					newChunkPos.y = this.chunkPos.y - 1;
					newInternalPos.y = GameManager.Instance.ChunkSize;
				}
				else if(neighbor.HasFlag(Neighbors.yPos))
				{
					newChunkPos.y = this.chunkPos.y + 1;
					newInternalPos.y = 0;
				}
				if(neighbor.HasFlag(Neighbors.zNeg))
				{
					newChunkPos.z = this.chunkPos.z - 1;
					newInternalPos.z = GameManager.Instance.ChunkSize;
				}
				else if(neighbor.HasFlag(Neighbors.zPos))
				{
					newChunkPos.z = this.chunkPos.z + 1;
					newInternalPos.z = 0;
				}
				if(World.GetChunk(newChunkPos, out Chunk chunk) == true && chunk.GeneratedChunkData == true)
				{
					chunk.ModifyTerrainMap(newInternalPos, value, false);
				}
			}
		}
	}

	// Is Position Within Bounds
	private bool IsPositionWithinBounds(Vector3Int position)
    {
		if(position.x >= 0 && position.x < GameManager.Instance.ChunkSize + 1 && position.y >= 0 && position.y < GameManager.Instance.ChunkSize + 1 && position.z >= 0 && position.z < GameManager.Instance.ChunkSize + 1)
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
		this.ChunkGO = new GameObject($@"Chunk: {this.chunkPos}");
		this.ChunkGO.transform.position = this.chunkPos.ChunkPosToWorldPos();
		this.ChunkGO.transform.parent = GameManager.Instance.ChunkParentGO.transform;
		this.meshFilter = this.ChunkGO.AddComponent<MeshFilter>();
		this.meshRenderer = this.ChunkGO.AddComponent<MeshRenderer>();
		this.meshRenderer.material = GameManager.Instance.ChunkMaterial;
		this.meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
		this.meshCollider = this.ChunkGO.AddComponent<MeshCollider>();
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
