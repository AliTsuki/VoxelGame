using UnityEditor;
using UnityEditor.SceneManagement;

using UnityEngine;


/// <summary>
/// Class describing how to render the custom inspector window for the GameManager script object.
/// </summary>
[CustomEditor(typeof(GameManager)), System.Serializable]
public class GameManagerEditor : Editor
{
    private GameManager gm;


    // OnEnable is called when the object is enabled and becomes active.
    public void OnEnable()
    {
		this.gm = (GameManager)this.target;
	}

    // OnInspectorGUI is called every time the custom inspector window is modified.
    public override void OnInspectorGUI()
    {
        this.serializedObject.Update();
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
		this.gm.RoomFrequency = EditorGUILayout.FloatField("Frequency:", this.gm.RoomFrequency);
		this.gm.RoomOctaves = EditorGUILayout.IntField("Octaves:", this.gm.RoomOctaves);
		this.gm.RoomLacunarity = EditorGUILayout.FloatField("Lacunarity:", this.gm.RoomLacunarity);
		this.gm.RoomPersistence = EditorGUILayout.FloatField("Persistence:", this.gm.RoomPersistence);
		this.gm.RoomMultiplier = EditorGUILayout.FloatField("Multiplier:", this.gm.RoomMultiplier);
		// Cave Worm Settings
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Cave Worm Noise Settings", BoldCenteredStyle);
		this.gm.ShouldCarveWorms = EditorGUILayout.Toggle("Should Carve Worms?:", this.gm.ShouldCarveWorms);
		EditorGUILayout.LabelField("Position Noise Map", EditorStyles.boldLabel);
		this.gm.CaveWormPositionNoiseType = (FastNoise.NoiseType)EditorGUILayout.EnumPopup("Cave Worm Pos Noise Type:", this.gm.CaveWormPositionNoiseType);
		this.gm.CaveWormPositionNoiseInterpolation = (FastNoise.Interp)EditorGUILayout.EnumPopup("Cave Worm Pos Noise Interpolation:", this.gm.CaveWormPositionNoiseInterpolation);
		this.gm.CaveWormPositionFrequency = EditorGUILayout.FloatField("Cave Worm Pos Frequency:", this.gm.CaveWormPositionFrequency);
		EditorGUILayout.LabelField("Direction Noise Map", EditorStyles.boldLabel);
		this.gm.CaveWormDirectionNoiseType = (FastNoise.NoiseType)EditorGUILayout.EnumPopup("Cave Worm Dir Noise Type:", this.gm.CaveWormDirectionNoiseType);
		this.gm.CaveWormDirectionNoiseInterpolation = (FastNoise.Interp)EditorGUILayout.EnumPopup("Cave Worm Dir Noise Interpolation:", this.gm.CaveWormDirectionNoiseInterpolation);
		this.gm.CaveWormDirectionFrequency = EditorGUILayout.FloatField("Cave Worm Dir Frequency:", this.gm.CaveWormDirectionFrequency);
		EditorGUILayout.LabelField("Number, Length, Radius, Values", EditorStyles.boldLabel);
		this.gm.MinimumCaveWorms = EditorGUILayout.IntField("Minimum Cave Worms:", this.gm.MinimumCaveWorms);
		this.gm.MaximumCaveWorms = EditorGUILayout.IntField("Maximum Cave Worms:", this.gm.MaximumCaveWorms);
		this.gm.MaxWormChunkDistance = EditorGUILayout.IntField("Maximum Worm Chunk Distance:", this.gm.MaxWormChunkDistance);
		this.gm.MaxWormNodes = EditorGUILayout.IntField("Maximum Worm Nodes:", this.gm.MaxWormNodes);
		this.gm.CaveWormRadius = EditorGUILayout.IntField("Cave Worm Radius:", this.gm.CaveWormRadius);
		this.gm.CaveWormCarveValue = EditorGUILayout.FloatField("Cave Worm Carve Value:", this.gm.CaveWormCarveValue);
		// Regenerate Button
		EditorGUILayout.Space();
		if(GUILayout.Button("Regenerate Starting Chunks"))
		{
			GameManager.Instance.RegenerateStartingChunks();
		}
		if(GUI.changed)
		{
			EditorUtility.SetDirty(this.gm);
			EditorSceneManager.MarkSceneDirty(this.gm.gameObject.scene);
		}
	}
}
