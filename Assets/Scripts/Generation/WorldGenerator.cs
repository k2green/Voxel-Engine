using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator {

	private Dictionary<Vector2Int, float> heightMap;
	private NoiseFilter noiseFilter;

	public WorldGenerator(NoiseFilter filter) {
		noiseFilter = filter;
		heightMap = new Dictionary<Vector2Int, float>();
	}

	public float GetHeight(Vector2Int point) {
		if (!heightMap.ContainsKey(point))
			heightMap.Add(point, noiseFilter.EvaluateNoise(point));

		return heightMap[point];
	}

	public Chunk GenerateChunk(Vector3Int chunkIndex) {
		var chunk = new Chunk();
		var chunkCoords = new Vector3Int(chunkIndex.x * Chunk.Dimensions.x, chunkIndex.y * Chunk.Dimensions.y, chunkIndex.z * Chunk.Dimensions.z);

		for (int z = 0; z < Chunk.Dimensions.z; z++) {
			for (int x = 0; x < Chunk.Dimensions.x; x++) {
				int height = (int)GetHeight(new Vector2Int(chunkCoords.x + x, chunkCoords.z + z));

				if (height >= chunkCoords.y + Chunk.Dimensions.y) {
					chunk.FillRange(new Vector3Int(x, 0, z), new Vector3Int(x, Chunk.Dimensions.y - 1, z), 105, 105, 105);
				} else if (height >= chunkCoords.y) {
					int localHeight = height - chunkCoords.y;
					chunk.FillRange(new Vector3Int(x, 0, z), new Vector3Int(x, localHeight, z), 105, 105, 105);
				}
			}
		}

		return chunk;
	}

	public void Clear() => heightMap.Clear();
}
