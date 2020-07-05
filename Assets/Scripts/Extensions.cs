using UnityEngine;


/// <summary>
/// Contains extension methods for system/unity structs.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Converts a chunk position to the world position for the center of that chunk.
    /// </summary>
    /// <param name="chunkPos">The chunk position in chunk coordinate system.</param>
    /// <returns>Returns the position in world coordinate system representing the center of the chunk.</returns>
    public static Vector3Int ChunkPosToWorldPos(this Vector3Int chunkPos)
    {
        Vector3Int worldPos = new Vector3Int
        {
            x = (chunkPos.x * GameManager.Instance.ChunkSize) + (GameManager.Instance.ChunkSize / 2),
            y = (chunkPos.y * GameManager.Instance.ChunkSize) + (GameManager.Instance.ChunkSize / 2),
            z = (chunkPos.z * GameManager.Instance.ChunkSize) + (GameManager.Instance.ChunkSize / 2)
        };
        return worldPos;
    }

    // World Pos To Chunk Pos
    public static Vector3Int WorldPosToChunkPos(this Vector3Int worldPos)
    {
        Vector3Int chunkPos = new Vector3Int(worldPos.x / GameManager.Instance.ChunkSize, worldPos.y / GameManager.Instance.ChunkSize, worldPos.z / GameManager.Instance.ChunkSize);
        if(worldPos.x < 0 && worldPos.x % GameManager.Instance.ChunkSize < 0)
        {
            chunkPos.x -= 1;
        }
        if(worldPos.y < 0 && worldPos.y % GameManager.Instance.ChunkSize < 0)
        {
            chunkPos.y -= 1;
        }
        if(worldPos.z < 0 && worldPos.z % GameManager.Instance.ChunkSize < 0)
        {
            chunkPos.z -= 1;
        }
        return chunkPos;
    }

    /// <summary>
    /// Converts an internal position to a world position using the chunk position the internal position is relative to.
    /// </summary>
    /// <param name="internalPos">The internal position of a chunk data value.</param>
    /// <param name="chunkPos">The position of the parent chunk in chunk coordinate system.</param>
    /// <returns>Returns the position in world coordinate system that corresponds to the given chunk internal coordinate system position.</returns>
    public static Vector3Int InternalPosToWorldPos(this Vector3Int internalPos, Vector3Int chunkPos)
    {
        Vector3Int worldPos = new Vector3Int
        {
            x = internalPos.x + (chunkPos.x * GameManager.Instance.ChunkSize),
            y = internalPos.y + (chunkPos.y * GameManager.Instance.ChunkSize),
            z = internalPos.z + (chunkPos.z * GameManager.Instance.ChunkSize)
        };
        return worldPos;
    }

    // World Pos To Internal Pos
    public static Vector3Int WorldPosToInternalPos(this Vector3Int worldPos)
    {
        Vector3Int worldPosDifference = new Vector3Int(worldPos.x / GameManager.Instance.ChunkSize, worldPos.y / GameManager.Instance.ChunkSize, worldPos.z / GameManager.Instance.ChunkSize);
        if(worldPos.x < 0 && worldPos.x % GameManager.Instance.ChunkSize < 0)
        {
            worldPosDifference.x -= 1;
        }
        worldPosDifference.x *= GameManager.Instance.ChunkSize;
        if(worldPos.y < 0 && worldPos.y % GameManager.Instance.ChunkSize < 0)
        {
            worldPosDifference.y -= 1;
        }
        worldPosDifference.y *= GameManager.Instance.ChunkSize;
        if(worldPos.z < 0 && worldPos.z % GameManager.Instance.ChunkSize < 0)
        {
            worldPosDifference.z -= 1;
        }
        worldPosDifference.z *= GameManager.Instance.ChunkSize;
        Vector3Int internalPos = new Vector3Int
        {
            x = worldPos.x - worldPosDifference.x,
            y = worldPos.y - worldPosDifference.y,
            z = worldPos.z - worldPosDifference.z
        };
        return internalPos;
    }

    /// <summary>
    /// Remaps a float input from one range of values to another range of values.
    /// </summary>
    /// <param name="input">The input to modify.</param>
    /// <param name="inMin">The minimum value of the input value's range.</param>
    /// <param name="inMax">The maximum value of the input value's range.</param>
    /// <param name="outMin">The minimum value of the output value's range.</param>
    /// <param name="outMax">The maximum value of the output value's range.</param>
    /// <returns>Returns the remapped value within the given output range.</returns>
    public static float Map(this float input, float inMin, float inMax, float outMin, float outMax)
    {
        float slope = (outMax - outMin) / (inMax - inMin);
        float intercept = outMin - (slope * inMin);
        return (slope * input) + intercept;
    }

    /// <summary>
    /// Converts a Vector3 to a Vector3Int by rounding the x, y, and z using Mathf.RoundToInt on each value.
    /// </summary>
    /// <param name="input">The Vector3 to convert to a Vector3Int.</param>
    /// <returns>Returns the rounded to integer version of the given Vector3.</returns>
    public static Vector3Int RoundToInt(this Vector3 input)
    {
        return new Vector3Int(Mathf.RoundToInt(input.x), Mathf.RoundToInt(input.y), Mathf.RoundToInt(input.z));
    }
}
