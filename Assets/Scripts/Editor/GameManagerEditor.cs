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
		// Noise Settings
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Room Noise Settings", BoldCenteredStyle);
		this.gm.NoiseType = (FastNoise.NoiseType)EditorGUILayout.EnumPopup("Room Noise Type:", this.gm.NoiseType);
		this.gm.NoiseInterpolation = (FastNoise.Interp)EditorGUILayout.EnumPopup("Room Noise Interpolation:", this.gm.NoiseInterpolation);
		this.gm.FractalType = (FastNoise.FractalType)EditorGUILayout.EnumPopup("Room Fractal Type:", this.gm.FractalType);
		this.gm.RoomFrequency = EditorGUILayout.FloatField("Room Frequency:", this.gm.RoomFrequency);
		this.gm.RoomOctaves = EditorGUILayout.IntField("Room Octaves:", this.gm.RoomOctaves);
		this.gm.RoomLacunarity = EditorGUILayout.FloatField("Room Lacunarity:", this.gm.RoomLacunarity);
		this.gm.RoomPersistence = EditorGUILayout.FloatField("Room Persistence:", this.gm.RoomPersistence);
		this.gm.RoomMultiplier = EditorGUILayout.FloatField("Room Value Multiplier:", this.gm.RoomMultiplier);
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
		this.gm.MaxWormSegments = EditorGUILayout.IntField("Maximum Worm Segments:", this.gm.MaxWormSegments);
		this.gm.CaveWormRadius = EditorGUILayout.IntField("Cave Worm Radius:", this.gm.CaveWormRadius);
		this.gm.CaveWormCarveValue = EditorGUILayout.FloatField("Cave Worm Carve Value:", this.gm.CaveWormCarveValue);
		// Ore Settings
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Ore Noise Settings", BoldCenteredStyle);
		this.gm.OreNoiseType = (FastNoise.NoiseType)EditorGUILayout.EnumPopup("Ore Noise Type:", this.gm.OreNoiseType);
		this.gm.OreNoiseInterpolation = (FastNoise.Interp)EditorGUILayout.EnumPopup("Ore Noise Interpolation:", this.gm.OreNoiseInterpolation);
		this.gm.OreFrequency = EditorGUILayout.FloatField("Ore Frequency:", this.gm.OreFrequency);
		this.gm.OreCutoff = EditorGUILayout.FloatField("Ore Cutoff:", this.gm.OreCutoff);
		// Regenerate Button
		EditorGUILayout.Space();
		if(GUILayout.Button("Regenerate Starting Chunks"))
		{
			GameManager.Instance.RegenerateStartingChunks();
		}
		if(GUI.changed && EditorApplication.isPlaying == false)
		{
			EditorUtility.SetDirty(this.gm);
			EditorSceneManager.MarkSceneDirty(this.gm.gameObject.scene);
		}
	}
}
