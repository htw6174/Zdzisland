using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TextureCreator))]
public class TextureCreatorInspector : Editor {

	private TextureCreator creator;

	private void OnEnable () {
		creator = target as TextureCreator;
		Undo.undoRedoPerformed += RefreshCreator;
	}

	private void OnDisable () {
		Undo.undoRedoPerformed -= RefreshCreator;
	}

	private void RefreshCreator () {
		if (Application.isPlaying) {
			creator.FillHeights();
		}
	}

	public override void OnInspectorGUI () {
		EditorGUI.BeginChangeCheck();
		DrawDefaultInspector();
        if(GUILayout.Button("Update Terrain"))
        {
            creator.FillHeights();
            //RefreshCreator();
        }
        if(GUILayout.Button("Update Details"))
        {
            creator.FillDetails();
        }
        if (GUILayout.Button("Randomize"))
        {
            creator.RandomizePosition();
        }
        if (GUILayout.Button("Place Trees"))
        {
            creator.PlaceTrees(0);
        }

		if (creator.liveHeightmap && EditorGUI.EndChangeCheck()) {
            creator.FillHeights();
        }
        if (creator.liveTexture && EditorGUI.EndChangeCheck())
        {
            creator.FillTexture();
        }
    }
}
