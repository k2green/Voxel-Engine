using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Chunk))]
public class ChunkEditor : Editor {

	public override void OnInspectorGUI() {
		var chunk = (Chunk)target;

		GUI.enabled = false;
		EditorGUILayout.Vector3IntField("Dimensions", Chunk.Dimensions);
		EditorGUILayout.Vector3Field("Chunk Index", chunk.ChunkIndex);
		EditorGUILayout.Vector3Field("Chunk Position", chunk.Position);
		GUI.enabled = true;
	}
}
