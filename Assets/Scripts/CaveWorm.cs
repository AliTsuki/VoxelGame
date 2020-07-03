using System.Collections.Generic;

using UnityEngine;


public class CaveWorm
{
    // Node List
    public List<Vector3Int> Nodes = new List<Vector3Int>();
    public int Radius;

    // Constructor
    public CaveWorm(Vector3Int position, int radius)
    {
        this.Nodes.Add(position);
        this.Radius = radius;
        this.GenerateNodes();
    }

    // Generate Nodes
    public void GenerateNodes()
    {
        int dirOffset = -1000;
        for(int currentNode = 0; currentNode < GameManager.Instance.MaxWormNodes - 1; currentNode++)
        {
            float dirX = GameManager.Instance.CaveWormDirectionNoiseGenerator.GetNoise(this.Nodes[currentNode].x + (dirOffset * 1), this.Nodes[currentNode].y + (dirOffset * 1), this.Nodes[currentNode].z + (dirOffset * 1));
            float dirY = GameManager.Instance.CaveWormDirectionNoiseGenerator.GetNoise(this.Nodes[currentNode].x + (dirOffset * 2), this.Nodes[currentNode].y + (dirOffset * 2), this.Nodes[currentNode].z + (dirOffset * 2));
            float dirZ = GameManager.Instance.CaveWormDirectionNoiseGenerator.GetNoise(this.Nodes[currentNode].x + (dirOffset * 3), this.Nodes[currentNode].y + (dirOffset * 3), this.Nodes[currentNode].z + (dirOffset * 3));
            Vector3 newWormDir = new Vector3(dirX, dirY, dirZ).normalized;
            Vector3Int newNodePos = (this.Nodes[currentNode] + (newWormDir * this.Radius)).RoundToInt();
            if(Vector3.Distance(this.Nodes[0], newNodePos) < GameManager.Instance.MaxWormChunkDistance * GameManager.Instance.ChunkSize)
            {
                this.Nodes.Add(newNodePos);
            }
            else
            {
                break;
            }
        }
    }
}
