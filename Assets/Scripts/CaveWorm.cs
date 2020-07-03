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
        int currentNode = 0;
        while(this.Nodes.Count < GameManager.Instance.MaxWormNodes)
        {
            int dirX = Mathf.RoundToInt(GameManager.Instance.CaveWormNoiseGenerator.GetNoise(this.Nodes[currentNode].x + dirOffset, this.Nodes[currentNode].y + dirOffset, this.Nodes[currentNode].z + dirOffset));
            int dirY = Mathf.RoundToInt(GameManager.Instance.CaveWormNoiseGenerator.GetNoise(this.Nodes[currentNode].x + dirOffset, this.Nodes[currentNode].y + dirOffset, this.Nodes[currentNode].z + dirOffset));
            int dirZ = Mathf.RoundToInt(GameManager.Instance.CaveWormNoiseGenerator.GetNoise(this.Nodes[currentNode].x + dirOffset, this.Nodes[currentNode].y + dirOffset, this.Nodes[currentNode].z + dirOffset));
            Vector3 newWormDir = new Vector3(dirX, dirY, dirZ).normalized;
            Vector3Int newNodePos = (this.Nodes[currentNode] + (newWormDir * this.Radius)).RoundToInt();
            if(Vector3.Distance(this.Nodes[0], newNodePos) < GameManager.Instance.MaxWormChunkDistance * GameManager.Instance.ChunkSize)
            {
                this.Nodes.Add(newNodePos);
                currentNode++;
            }
            else
            {
                break;
            }
        }
    }
}
