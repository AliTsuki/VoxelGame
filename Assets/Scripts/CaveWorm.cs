using System.Collections.Generic;

using UnityEngine;


/// <summary>
/// The class representing a Perlin Worm used to generate caves.
/// </summary>
public class CaveWorm
{
    /// <summary>
    /// Class representing one segment of a cave worm that contains a list of points/values.
    /// </summary>
    public class Segment
    {
        /// <summary>
        /// Struct containing a position in world coordinate system and a float value for that position.
        /// </summary>
        public struct Point
        {
            /// <summary>
            /// The position of this point in world coordinate system.
            /// </summary>
            public Vector3Int WorldPosition { get; private set; }
            /// <summary>
            /// The value of this data point.
            /// </summary>
            public float Value { get; private set; }

            /// <summary>
            /// Creates a new point using given position and value.
            /// </summary>
            /// <param name="worldPosition">The position in world coordinate system for this point.</param>
            /// <param name="value">The value of this point.</param>
            public Point(Vector3Int worldPosition, float value)
            {
                this.WorldPosition = worldPosition;
                this.Value = value;
            }
        }

        /// <summary>
        /// The position of this Segment's center node in world coordinate system.
        /// </summary>
        public Vector3Int WorldPosition { get; private set; }
        /// <summary>
        /// List of all points contained in this segment.
        /// </summary>
        public List<Point> Points { get; private set; } = new List<Point>();

        /// <summary>
        /// Creates a new segment with the given position in world coordinate system as its center node.
        /// </summary>
        /// <param name="worldPosition">The position of the center node of this segment in world coordinate system.</param>
        public Segment(Vector3Int worldPosition)
        {
            this.WorldPosition = worldPosition;
        }
    }

    /// <summary>
    /// List of all the segments contained in this cave worm.
    /// </summary>
    public List<Segment> Segments { get; private set; } = new List<Segment>();
    /// <summary>
    /// The radius measured in world coordinate system that represents how wide the cave should be around each node.
    /// </summary>
    public int Radius { get; private set; }

    /// <summary>
    /// Creates a new Cave Worm with the head at the given position and radius as given.
    /// Immediately runs the Generate Segments method and fills its list of segments with points.
    /// </summary>
    /// <param name="position">The position in world coordinate system representing the start of the worm.</param>
    /// <param name="radius">The radius in world coordinate system representing the size of the cave's walls from the center of each node.</param>
    public CaveWorm(Vector3Int position, int radius)
    {
        Segment newSegment = new Segment(position);
        this.Segments.Add(newSegment);
        this.Radius = radius;
        this.GenerateSegments();
    }

    /// <summary>
    /// Generates all segments for this worm. It works by looping through the maximum number of possible segments,
    /// breaking out if the next segment is more than the max distance in MaxWormChunkDistance. For each segment it 
    /// samples 3 different locations of the noise generator and uses that as a normalized vector3 representing
    /// the direction between the current segment and the next segment, it then places the next segment in that direction
    /// at radius amount of positions away from the current segment.
    /// </summary>
    public void GenerateSegments()
    {
        int dirOffset = -1000;
        for(int currentSegment = 0; currentSegment < GameManager.Instance.MaxWormSegments - 1; currentSegment++)
        {
            float dirX = GameManager.Instance.CaveWormDirectionNoiseGenerator.GetNoise(this.Segments[currentSegment].WorldPosition.x + (dirOffset * 1), this.Segments[currentSegment].WorldPosition.y + (dirOffset * 1), this.Segments[currentSegment].WorldPosition.z + (dirOffset * 1));
            float dirY = GameManager.Instance.CaveWormDirectionNoiseGenerator.GetNoise(this.Segments[currentSegment].WorldPosition.x + (dirOffset * 2), this.Segments[currentSegment].WorldPosition.y + (dirOffset * 2), this.Segments[currentSegment].WorldPosition.z + (dirOffset * 2));
            float dirZ = GameManager.Instance.CaveWormDirectionNoiseGenerator.GetNoise(this.Segments[currentSegment].WorldPosition.x + (dirOffset * 3), this.Segments[currentSegment].WorldPosition.y + (dirOffset * 3), this.Segments[currentSegment].WorldPosition.z + (dirOffset * 3));
            Vector3 newWormDir = new Vector3(dirX, dirY, dirZ).normalized;
            Vector3Int newSegmentPos = (this.Segments[currentSegment].WorldPosition + (newWormDir * this.Radius)).RoundToInt();
            if(Vector3.Distance(this.Segments[0].WorldPosition, newSegmentPos) < GameManager.Instance.MaxWormChunkDistance * GameManager.Instance.ChunkSize)
            {
                this.Segments.Add(new Segment(newSegmentPos));
            }
            else
            {
                break;
            }
        }
        foreach(Segment segment in this.Segments)
        {
            for(int x = segment.WorldPosition.x - this.Radius; x < segment.WorldPosition.x + this.Radius; x++)
            {
                for(int y = segment.WorldPosition.y - this.Radius; y < segment.WorldPosition.y + this.Radius; y++)
                {
                    for(int z = segment.WorldPosition.z - this.Radius; z < segment.WorldPosition.z + this.Radius; z++)
                    {
                        Vector3Int nextWorldPos = new Vector3Int(x, y, z);
                        float distance = Vector3Int.Distance(segment.WorldPosition, nextWorldPos);
                        if(distance <= this.Radius)
                        {
                            float value = Mathf.SmoothStep(GameManager.Instance.CaveWormCarveValue, 0f, distance / this.Radius);
                            segment.Points.Add(new Segment.Point(nextWorldPos, value));
                        }
                    }
                }
            }
        }
    }
}
