using System;
using System.Collections.Generic;

using UnityEngine;


/// <summary>
/// Class describing a chunk of the game world, with all its internal data and meshes.
/// </summary>
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
	/// <summary>
	/// The position of this chunk in chunk coordinate system.
	/// </summary>
	public Vector3Int ChunkPos;
	/// <summary>
	/// The values of each point in the chunk.
	/// </summary>
	private float[,,] chunkData;
	/// <summary>
	/// List of all the cave worms that started in this chunk.
	/// </summary>
	public List<CaveWorm> CaveWorms = new List<CaveWorm>();

	// Mesh Data
	/// <summary>
	/// List of all vertices for the chunk mesh.
	/// </summary>
	private readonly List<Vector3> vertices = new List<Vector3>();
	/// <summary>
	/// List of all triangle vertex indices for the chunk mesh.
	/// </summary>
	private readonly List<int> triangles = new List<int>();

	// Flags
	/// <summary>
	/// Should this chunk generate chunk data or just cave worms for neighboring chunks.
	/// </summary>
	public bool ShouldGenerateChunkData = false;
	/// <summary>
	/// Has this chunk generated cave worms.
	/// </summary>
	public bool HasGeneratedCaveWorms = false;
	/// <summary>
	/// Has this chunk generated chunk data.
	/// </summary>
	public bool HasGeneratedChunkData = false;
	/// <summary>
	/// Has this chunk generated a mesh.
	/// </summary>
	public bool HasGeneratedMesh = false;


	/// <summary>
	/// Creates a new chunk at the given position in chunk coordinate system, and marks whether chunk should create chunk data.
	/// </summary>
	/// <param name="chunkPos">The position of this chunk in chunk coordinate system.</param>
	/// <param name="generateData">Should this chunk generate chunk data?</param>
	public Chunk(Vector3Int chunkPos, bool generateData)
	{
		this.ChunkPos = chunkPos;
		this.ShouldGenerateChunkData = generateData;
	}

	/// <summary>
	/// Generates cave worms for this chunk. Works by looping through the amount of worms to create and getting
	/// a random position for each worm to start using remapped 3d noise, then starts a cave worm at that position.
	/// </summary>
	public void GenerateCaveWorms()
	{
		int posOffset = 1000;
		// TODO: Change number of worms to be based on noise and not the same value for every chunk.
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

	/// <summary>
	/// Generates a 3D field of floating point values for each internal position in the chunk using a noise generator.
	/// </summary>
	public void GenerateChunkData()
	{
		// The size of the chunk data is 1 larger than the chunk size because the marching cubes sample the corners of each cube.
		this.chunkData = new float[GameManager.Instance.ChunkSize + 1, GameManager.Instance.ChunkSize + 1, GameManager.Instance.ChunkSize + 1];
		for(int x = 0; x <= GameManager.Instance.ChunkSize; x++)
		{
			for(int z = 0; z <= GameManager.Instance.ChunkSize; z++)
			{
				for(int y = 0; y <= GameManager.Instance.ChunkSize; y++)
				{
					Vector3Int worldPos = new Vector3Int(x, y, z).InternalPosToWorldPos(this.ChunkPos);
					float value = GameManager.Instance.NoiseGenerator.GetNoise(worldPos.x, worldPos.y, worldPos.z) * GameManager.Instance.RoomMultiplier;
					this.chunkData[x, y, z] = value;
				}
			}
		}
		this.HasGeneratedChunkData = true;
	}

	/// <summary>
	/// Gets the value of the chunk data at the given position in chunk internal coordinate system.
	/// </summary>
	/// <param name="internalPos">The position you wish to sample in chunk internal coordinate system.</param>
	/// <returns>Returns the floating point value of that position from the 3D float field of chunk data.</returns>
	public float GetChunkData(Vector3Int internalPos)
	{
		return this.chunkData[internalPos.x, internalPos.y, internalPos.z];
	}

	/// <summary>
	/// Sets the value of the chunk data at the given position in chunk internal coordinate system.
	/// </summary>
	/// <param name="internalPos">The position you wish to modify in chunk internal coordinate system.</param>
	/// <param name="value">The amount you wish to add to the value at the given position in the 3D float field of chunk data.</param>
	/// <param name="notifyNeighbors">Should chunk neighbors be notified of this change?</param>
	public void SetChunkData(Vector3Int internalPos, float value, bool notifyNeighbors)
	{
		this.chunkData[internalPos.x, internalPos.y, internalPos.z] += value;
		if(notifyNeighbors == true)
        {
			this.NotifyNeighbors(internalPos, value);
		}
	}

	/// <summary>
	/// If the given internal position is on a face/edge/corner this lets the neighboring chunks that share that face/edge/corner update their data to match.
	/// </summary>
	/// <param name="internalPos">The position in chunk internal coordinate system to use for notifying neighbor chunks.</param>
	/// <param name="value">The value to notify neighbor chunks of.</param>
	private void NotifyNeighbors(Vector3Int internalPos, float value)
    {
		// Check if this position is neighboring any other chunks, get how many/which directions.
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
		// If on face (1): single neighbor (x), if on edge (2): triple neighbor (x, y, xy), if on corner (3): sept neighbor (x, y, z, xy, yz, xz, xyz).
		// Edge
		if(neighbors.Count == 2)
        {
			neighbors.Add(neighbors[0] | neighbors[1]);
        }
		// Corner
		else if(neighbors.Count == 3)
		{
			neighbors.Add(neighbors[0] | neighbors[1]);
			neighbors.Add(neighbors[1] | neighbors[2]);
			neighbors.Add(neighbors[0] | neighbors[2]);
			neighbors.Add(neighbors[0] | neighbors[1] | neighbors[2]);
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
					chunk.SetChunkData(newInternalPos, value, false);
				}
			}
		}
	}

	/// <summary>
	/// Instantiates a chunk GameObject, adds the necessary components, creates and assigns a mesh to the GameObject.
	/// </summary>
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

	/// <summary>
	/// Clears all mesh data for this chunk.
	/// </summary>
	private void ClearMeshData()
	{
		this.vertices.Clear();
		this.triangles.Clear();
	}

	/// <summary>
	/// Creates a mesh by first clearing any existing mesh data then looping through all points in the chunk and performing marching cubes to build a mesh.
	/// </summary>
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

	/// <summary>
	/// Builds a mesh for this chunk by using the marching cubes algorithm to check each corner of each cube
	/// contained in the chunk to get which triangles configuration to use for the mesh.
	/// </summary>
	/// <param name="internalPos">The position in chunk internal coordinate system for which to check a cube and make a mesh.</param>
	private void BuildMesh(Vector3Int internalPos)
	{
		// Get chunk data for each corner of this cube.
		float[] cube = new float[8];
		for(int i = 0; i < 8; i++)
		{
			cube[i] = this.GetChunkData(internalPos + GameManager.CornerTable[i]);
		}
		// Get the configuration index of this cube.
		int cubeConfigIndex = this.GetCubeConfiguration(cube);
		// If the configuration of this cube is 0 or 255 (completely inside the terrain or completely outside of it) we don't need to do anything.
		if(cubeConfigIndex == 0 || cubeConfigIndex == 255)
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
				int index = GameManager.TriangleTable[cubeConfigIndex, edgeIndex];
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

	/// <summary>
	/// Get the cube configuration index for marching cubes algorithm.
	/// </summary>
	/// <param name="cube">A float array representing the chunk data values at each corner of a cube.</param>
	/// <returns>Returns an integer representing the index to use to get the triangles/vertices to mesh this cube.</returns>
	private int GetCubeConfiguration(float[] cube)
	{
		// Loop through each point in the cube and check if it is below the terrain surface.
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

	/// <summary>
	/// Assigns the array of vertices and triangle indices to a mesh object and assign that mesh object to this GameObject's components.
	/// </summary>
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
