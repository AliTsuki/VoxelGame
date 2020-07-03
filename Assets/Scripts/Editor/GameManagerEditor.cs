using UnityEditor;

using UnityEngine;


[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    private GameManager gm;


    // OnInspectorGUI is called every time the custom inspector window is modified.
    public override void OnInspectorGUI()
    {
        this.serializedObject.Update();
        this.gm = (GameManager)this.target;
		GUIStyle BoldCenteredStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, };
		// References
		EditorGUILayout.LabelField("References", BoldCenteredStyle);
		this.gm.ChunkParentGO = (GameObject)EditorGUILayout.ObjectField("Chunk Parent:", this.gm.ChunkParentGO, typeof(GameObject), true);
		this.gm.ChunkMaterial = (Material)EditorGUILayout.ObjectField("Chunk Material:", this.gm.ChunkMaterial, typeof(Material), false);
		// Chunk Data
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Chunk Settings", BoldCenteredStyle);
		this.gm.ChunkSize = EditorGUILayout.IntField("Chunk Size:", this.gm.ChunkSize);
		this.gm.StartingChunkArea = EditorGUILayout.IntField("Starting Chunk Area:", this.gm.StartingChunkArea);
		this.gm.TerrainSurfaceCutoff = EditorGUILayout.FloatField("Terrain Cutoff:", this.gm.TerrainSurfaceCutoff);
		this.gm.Seed = EditorGUILayout.IntField("Seed:", this.gm.Seed);
		this.gm.SmoothTerrain = EditorGUILayout.Toggle("Smooth Terrain:", this.gm.SmoothTerrain);
		// Cave Settings
		// Noise Settings
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Noise Settings", BoldCenteredStyle);
		this.gm.NoiseType = (FastNoise.NoiseType)EditorGUILayout.EnumPopup("Noise Type:", this.gm.NoiseType);
		this.gm.NoiseInterpolation = (FastNoise.Interp)EditorGUILayout.EnumPopup("Noise Interpolation:", this.gm.NoiseInterpolation);
		this.gm.FractalType = (FastNoise.FractalType)EditorGUILayout.EnumPopup("Fractal Type:", this.gm.FractalType);
		this.gm.Frequency = EditorGUILayout.FloatField("Frequency:", this.gm.Frequency);
		this.gm.Octaves = EditorGUILayout.IntField("Octaves:", this.gm.Octaves);
		this.gm.Lacunarity = EditorGUILayout.FloatField("Lacunarity:", this.gm.Lacunarity);
		this.gm.Persistence = EditorGUILayout.FloatField("Persistence:", this.gm.Persistence);
		this.gm.Multiplier = EditorGUILayout.FloatField("Multiplier:", this.gm.Multiplier);
		// Cave Worm Settings
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Cave Worm Noise Settings", BoldCenteredStyle);
		this.gm.ShouldCarveWorms = EditorGUILayout.Toggle("Should Carve Worms?:", this.gm.ShouldCarveWorms);
		this.gm.CaveWormNoiseType = (FastNoise.NoiseType)EditorGUILayout.EnumPopup("Cave Worm Noise Type:", this.gm.CaveWormNoiseType);
		this.gm.CaveWormNoiseInterpolation = (FastNoise.Interp)EditorGUILayout.EnumPopup("Cave Worm Noise Interpolation:", this.gm.CaveWormNoiseInterpolation);
		this.gm.CaveWormFrequency = EditorGUILayout.FloatField("Cave Worm Frequency:", this.gm.CaveWormFrequency);
		this.gm.MinimumCaveWorms = EditorGUILayout.IntField("Minimum Cave Worms:", this.gm.MinimumCaveWorms);
		this.gm.MaximumCaveWorms = EditorGUILayout.IntField("Maximum Cave Worms:", this.gm.MaximumCaveWorms);
		this.gm.MaxWormChunkDistance = EditorGUILayout.IntField("Maximum Worm Chunk Distance:", this.gm.MaxWormChunkDistance);
		this.gm.MaxWormNodes = EditorGUILayout.IntField("Maximum Worm Nodes:", this.gm.MaxWormNodes);
		this.gm.CaveWormRadius = EditorGUILayout.IntField("Cave Worm Radius:", this.gm.CaveWormRadius);
		// Remesh Button
		EditorGUILayout.Space();
		if(GUILayout.Button("Remesh Current Chunks"))
		{
			GameManager.Instance.RemeshChunks();
		}
	}
}
