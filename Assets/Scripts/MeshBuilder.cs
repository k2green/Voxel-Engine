using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshBuilder {

	private List<Vector3> vertices;
	private List<int> triangles;
	private List<Color32> colors;

	public MeshBuilder() {
		vertices = new List<Vector3>();
		triangles = new List<int>();
		colors = new List<Color32>();
	}

	public void AddQuad(Vector3[] quadVertices, Color32 color, bool isBackFace) {
		if (quadVertices == null || quadVertices.Length != 4) throw new System.ArgumentException("A quad must have 4 vertices");

		vertices.AddRange(quadVertices);
		colors.AddRange(new Color32[] { color, color, color, color });

		if (isBackFace) {
			triangles.Add(vertices.Count - 2);
			triangles.Add(vertices.Count - 3);
			triangles.Add(vertices.Count - 4);

			triangles.Add(vertices.Count - 1);
			triangles.Add(vertices.Count - 2);
			triangles.Add(vertices.Count - 4);
		} else {
			triangles.Add(vertices.Count - 4);
			triangles.Add(vertices.Count - 3);
			triangles.Add(vertices.Count - 2);

			triangles.Add(vertices.Count - 4);
			triangles.Add(vertices.Count - 2);
			triangles.Add(vertices.Count - 1);
		}
	}

	public MeshData GetMeshData() {
		return new MeshData(vertices.ToArray(), triangles.ToArray(), colors.ToArray());
	}
}
