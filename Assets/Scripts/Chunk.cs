using System;
using System.Collections.Generic;

using UnityEngine;


public class Chunk
{
	[Flags]
	public enum Neighbors
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
	public Vector3Int ChunkPos;
	private float[,,] chunkData;
	public List<CaveWorm> CaveWorms = new List<CaveWorm>();

	// Mesh Data
	private readonly List<Vector3> vertices = new List<Vector3>();
	private readonly List<int> triangles = new List<int>();

	// Flags
	public bool ShouldGenerateChunkData = false;
	public bool HasGeneratedCaveWorms = false;
	public bool HasGeneratedChunkData = false;
	public bool HasGeneratedMesh = false;


	// Constructor
	public Chunk(Vector3Int chunkPos, bool generateData)
	{
		this.ChunkPos = chunkPos;
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
			int posX = Mathf.RoundToInt(GameManager.Instance.CaveWormPositionNoiseGenerator.GetNoise((this.ChunkPos.x * this.ChunkPos.x) + (posOffset * 1 * i), this.ChunkPos.y + (posOffset * 1 * i), this.ChunkPos.z + (posOffset * 1 * i)).Map(-1, 1, 0, GameManager.Instance.ChunkSize));
			int posY = Mathf.RoundToInt(GameManager.Instance.CaveWormPositionNoiseGenerator.GetNoise(this.ChunkPos.x + (posOffset * 2 * i), (this.ChunkPos.y * this.ChunkPos.y) + (posOffset * 2 * i), this.ChunkPos.z + (posOffset * 2 * i)).Map(-1, 1, 0, GameManager.Instance.ChunkSize));
			int posZ = Mathf.RoundToInt(GameManager.Instance.CaveWormPositionNoiseGenerator.GetNoise(this.ChunkPos.x + (posOffset * 3 * i), this.ChunkPos.y + (posOffset * 3 * i), (this.ChunkPos.z * this.ChunkPos.z) + (posOffset * 3 * i)).Map(-1, 1, 0, GameManager.Instance.ChunkSize));
			Vector3Int newWormPos = new Vector3Int(posX, posY, posZ).InternalPosToWorldPos(this.ChunkPos);
			CaveWorm newWorm = new CaveWorm(newWormPos, GameManager.Instance.CaveWormRadius);
			this.CaveWorms.Add(newWorm);
		}
		this.HasGeneratedCaveWorms = true;
	}

	// Generate Chunk Data
	public void GenerateChunkData()
	{
		// The data points for terrain are stored at the corners of our "cubes", so the terrainMap needs to be 1 larger than the width/height of our mesh.
		this.chunkData = new float[GameManager.Instance.ChunkSize + 1, GameManager.Instance.ChunkSize + 1, GameManager.Instance.ChunkSize + 1];
		for(int x = 0; x < GameManager.Instance.ChunkSize + 1; x++)
		{
			for(int z = 0; z < GameManager.Instance.ChunkSize + 1; z++)
			{
				for(int y = 0; y < GameManager.Instance.ChunkSize + 1; y++)
				{
					Vector3Int worldPos = new Vector3Int(x, y, z).InternalPosToWorldPos(this.ChunkPos);
					float value = GameManager.Instance.NoiseGenerator.GetNoise(worldPos.x, worldPos.y, worldPos.z) * GameManager.Instance.Multiplier;
					this.chunkData[x, y, z] = value;
				}
			}
		}
		this.HasGeneratedChunkData = true;
	}

	// Get Chunk Data
	public float GetChunkData(Vector3Int internalPos)
	{
		return this.chunkData[internalPos.x, internalPos.y, internalPos.z];
	}

	// Set Chunk Data
	public void SetChunkData(Vector3Int internalPos, float value)
	{
		this.chunkData[internalPos.x, internalPos.y, internalPos.z] += value;
	}

	// Notify Neighbors
	public void NotifyNeighbors(Vector3Int internalPos, float value)
    {
		// TODO: Seams still sometimes appearing
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
				Vector3Int newChunkPos = this.ChunkPos;
				Vector3Int newInternalPos = internalPos;
				if(neighbor.HasFlag(Neighbors.xNeg))
				{
					newChunkPos.x = this.ChunkPos.x - 1;
					newInternalPos.x = GameManager.Instance.ChunkSize;
				}
				else if(neighbor.HasFlag(Neighbors.xPos))
                {
					newChunkPos.x = this.ChunkPos.x + 1;
					newInternalPos.x = 0;
                }
				if(neighbor.HasFlag(Neighbors.yNeg))
				{
					newChunkPos.y = this.ChunkPos.y - 1;
					newInternalPos.y = GameManager.Instance.ChunkSize;
				}
				else if(neighbor.HasFlag(Neighbors.yPos))
				{
					newChunkPos.y = this.ChunkPos.y + 1;
					newInternalPos.y = 0;
				}
				if(neighbor.HasFlag(Neighbors.zNeg))
				{
					newChunkPos.z = this.ChunkPos.z - 1;
					newInternalPos.z = GameManager.Instance.ChunkSize;
				}
				else if(neighbor.HasFlag(Neighbors.zPos))
				{
					newChunkPos.z = this.ChunkPos.z + 1;
					newInternalPos.z = 0;
				}
				if(World.GetChunk(newChunkPos, out Chunk chunk) == true && chunk.HasGeneratedChunkData == true)
				{
					chunk.SetChunkData(newInternalPos, value);
				}
			}
		}
	}

	// Instantiate Chunk Game Object
	public void InstantiateChunkGameObject()
    {
		this.ChunkGO = new GameObject($@"Chunk: {this.ChunkPos}");
		this.ChunkGO.transform.position = this.ChunkPos.ChunkPosToWorldPos();
		this.ChunkGO.transform.parent = GameManager.Instance.ChunkParentGO.transform;
		this.meshFilter = this.ChunkGO.AddComponent<MeshFilter>();
		this.meshRenderer = this.ChunkGO.AddComponent<MeshRenderer>();
		this.meshRenderer.material = GameManager.Instance.ChunkMaterial;
		this.meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
		this.meshCollider = this.ChunkGO.AddComponent<MeshCollider>();
		this.CreateMesh();
		this.AssignMesh();
	}

	// Clear Mesh Data
	private void ClearMeshData()
	{
		this.vertices.Clear();
		this.triangles.Clear();
	}

	// Create Mesh
	private void CreateMesh()
	{
        this.ClearMeshData();
		for(int x = 0; x < GameManager.Instance.ChunkSize; x++)
		{
			for(int y = 0; y < GameManager.Instance.ChunkSize; y++)
			{
				for(int z = 0; z < GameManager.Instance.ChunkSize; z++)
				{
                    this.BuildMesh(new Vector3Int(x, y, z));
				}
			}
		}
	}

	// Build Mesh using Marching Cubes
	private void BuildMesh(Vector3Int internalPos)
	{
		// Sample terrain values at each corner of the cube.
		float[] cube = new float[8];
		for(int i = 0; i < 8; i++)
		{
			cube[i] = this.GetChunkData(internalPos + GameManager.CornerTable[i]);
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
				// Get the current index. We increment triangleIndex through each loop.
				int index = GameManager.TriangleTable[configIndex, edgeIndex];
				// If the current edgeIndex is -1, there are no more indices and we can exit the function.
				if(index == -1)
				{
					return;
				}
				// Get the vertices for the start and end of this edge.
				Vector3 vert1 = internalPos + GameManager.CornerTable[GameManager.EdgeIndexes[index, 0]];
				Vector3 vert2 = internalPos + GameManager.CornerTable[GameManager.EdgeIndexes[index, 1]];
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
					// Calculate the point along the edge that passes through.
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

	// Get Cube Configuration
	private int GetCubeConfiguration(float[] cube)
	{
		// Starting with a configuration of zero, loop through each point in the cube and check if it is below the terrain surface.
		int configurationIndex = 0;
		for(int i = 0; i < 8; i++)
		{
			// If it is, set the corresponding bit to 1.
			if(cube[i] > GameManager.Instance.TerrainSurfaceCutoff)
            {
                configurationIndex |= 1 << i;
            }
        }
		return configurationIndex;
	}

	// Assign Mesh
    private void AssignMesh()
    {
        Mesh mesh = new Mesh
        {
            vertices = this.vertices.ToArray(),
            triangles = this.triangles.ToArray()
        };
        mesh.RecalculateNormals();
        this.meshFilter.mesh = mesh;
		this.meshCollider.sharedMesh = mesh;
		this.HasGeneratedMesh = true;
	}
}
