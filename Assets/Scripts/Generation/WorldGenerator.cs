using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour {

	private Dictionary<Vector2Int, (float, float[,])> heightMaps;

	public NoiseFilter baseNoiseFilter;
	public NoiseFilter[] noiseFilters;

	private void Start() {
		heightMaps = new Dictionary<Vector2Int, (float, float[,])>();
	}

	public (float, float[,]) GetHeightMap(Vector3Int chunkIndex) {
		var flatIndex = GetVectorXZ(chunkIndex);

		if (!heightMaps.ContainsKey(flatIndex))
			CreateHeightMap(flatIndex);

		return heightMaps[flatIndex];
	}

	private void LateUpdate() {
		Clear();
	}

	private void CreateHeightMap(Vector2Int flatIndex) {
		float min = float.MaxValue;
		var heightMap = new float[Chunk.Dimensions.x, Chunk.Dimensions.z];
		var sampleBase = new Vector2(flatIndex.x * Chunk.Dimensions.x, flatIndex.y * Chunk.Dimensions.z);

		for (int z = 0; z < Chunk.Dimensions.z; z++) {
			for (int x = 0; x < Chunk.Dimensions.x; x++) {
				heightMap[x, z] = EvaluateFilters(sampleBase + new Vector2(x, z));

				if (heightMap[x, z] < min)
					min = heightMap[x, z];
			}
		}

		heightMaps.Add(flatIndex, (min, heightMap));
	}

	private float EvaluateFilters(Vector2 point) {
		float baseFactor;
		float baseHeight;

		if (baseNoiseFilter == null) {
			baseFactor = 1;
			baseHeight = 0;
		} else {
			float noise = baseNoiseFilter.EvaluateNoise(point);
			baseFactor = Mathf.Max(0, noise);
			baseHeight = noise * baseNoiseFilter.settings.strength;
		}

		float height = 0;

		if (noiseFilters != null) {
			foreach (NoiseFilter filter in noiseFilters) {
				height += Mathf.Max(0, filter.EvaluateNoise(point)) * filter.settings.strength;
			}
		}

		return baseHeight + height * baseFactor;
	}

	public Vector2Int GetVectorXZ(Vector3Int vector) => new Vector2Int(vector.x, vector.z);
	public void Clear() => heightMaps.Clear();
	public void Remove(Vector2Int key) => heightMaps.Remove(key);
	public void Remove(Vector3Int chunk) => heightMaps.Remove(GetVectorXZ(chunk));
}
