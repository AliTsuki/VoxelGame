﻿using System.Runtime.CompilerServices;

using UnityEngine;


public static class Extensions
{
    // Chunk Pos To World Pos
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

    // Internal Pos To World Pos
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
        Vector3Int internalPos = new Vector3Int(worldPos.x / GameManager.Instance.ChunkSize, worldPos.y / GameManager.Instance.ChunkSize, worldPos.z / GameManager.Instance.ChunkSize);
        if(worldPos.x < 0)
        {
            if(worldPos.x % GameManager.Instance.ChunkSize < 0)
            {
                internalPos.x -= 1;
            }
        }
        internalPos.x *= GameManager.Instance.ChunkSize;
        if(worldPos.y < 0)
        {
            if(worldPos.y % GameManager.Instance.ChunkSize < 0)
            {
                internalPos.y -= 1;
            }
        }
        internalPos.y *= GameManager.Instance.ChunkSize;
        if(worldPos.z < 0)
        {
            if(worldPos.z % GameManager.Instance.ChunkSize < 0)
            {
                internalPos.z -= 1;
            }
        }
        internalPos.z *= GameManager.Instance.ChunkSize;
        internalPos.x = worldPos.x - internalPos.x;
        internalPos.y = worldPos.y - internalPos.y;
        internalPos.z = worldPos.z - internalPos.z;
        return internalPos;
    }

    // Map
    public static float Map(this float input, float fromMin, float fromMax, float toMin, float toMax)
    {
        float slope = (toMax - toMin) / (fromMax - fromMin);
        float intercept = toMin - (slope * fromMin);
        return (slope * input) + intercept;
    }

    // Round To Int
    public static Vector3Int RoundToInt(this Vector3 input)
    {
        return new Vector3Int(Mathf.RoundToInt(input.x), Mathf.RoundToInt(input.y), Mathf.RoundToInt(input.z));
    }
}