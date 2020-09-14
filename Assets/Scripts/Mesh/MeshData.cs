using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MeshData {

	public MeshData(Vector3[] vertices, int[] triangles, Color32[] colors) {
		Vertices = vertices;
		Triangles = triangles;
		Colors = colors;
	}

	public void ApplyTo(Mesh mesh) {
		mesh.Clear();

		mesh.vertices = Vertices;
		mesh.triangles = Triangles;
		mesh.colors32 = Colors;

		mesh.RecalculateNormals();
	}

	public Vector3[] Vertices { get; }
	public int[] Triangles { get; }
	public Color32[] Colors { get; }
}
