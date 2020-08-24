using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator {

	private Dictionary<Vector2Int, float[,]> heightMaps;
	private NoiseFilter noiseFilter;

	public WorldGenerator(NoiseFilter filter) {
		noiseFilter = filter;
		heightMaps = new Dictionary<Vector2Int, float[,]>();
	}

	public float[,] GetHeightMap(Vector3Int chunkIndex) {
		var flatIndex = GetVectorXZ(chunkIndex);

		if (!heightMaps.ContainsKey(flatIndex))
			CreateHeightMap(flatIndex);

		return heightMaps[flatIndex];
	}

	private void CreateHeightMap(Vector2Int flatIndex) {
		var heightMap = new float[Chunk.Dimensions.x, Chunk.Dimensions.z];
		var sampleBase = new Vector2(flatIndex.x * Chunk.Dimensions.x, flatIndex.y * Chunk.Dimensions.z);

		for (int z = 0; z < Chunk.Dimensions.z; z++) {
			for (int x = 0; x < Chunk.Dimensions.x; x++) {
				heightMap[x, z] = noiseFilter.EvaluateNoise(sampleBase + new Vector2(x, z));
			}
		}

		heightMaps.Add(flatIndex, heightMap);
	}

	public Vector2Int GetVectorXZ(Vector3Int vector) => new Vector2Int(vector.x, vector.z);
	public void Clear() => heightMaps.Clear();
	public void Remove(Vector2Int key) => heightMaps.Remove(key);
	public void Remove(Vector3Int chunk) => heightMaps.Remove(GetVectorXZ(chunk));
}
