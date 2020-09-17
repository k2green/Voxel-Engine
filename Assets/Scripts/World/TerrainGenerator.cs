using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {
	public int baseHeight = Chunk.ChunkDimensions.y / 2;
	public NoiseFilter[] baseNoiseFilters;
	public NoiseFilter[] featureNoiseFilters;

	private (float, float) EvaluateMaskFilters(Vector2Int position) {
		float mask = 1;
		float height = 0;

		if (baseNoiseFilters != null) {
			foreach (var filter in baseNoiseFilters) {
				float noise = filter.EvaluateNoise(position, false);
				mask *= Mathf.Max(0, noise);
				height += noise * filter.settings.strength;
			}
		}

		return (mask, height);
	}

	private float EvaluateFeatureFilters(Vector2Int position) {
		float value = 0;

		if (featureNoiseFilters != null) {
			foreach (var filter in featureNoiseFilters) {
				value += filter.EvaluateNoise(position);
			}
		}

		return value;
	}

	public float EvaluateHeight(Vector2Int position) {
		var (factor, baseNoiseHeight) = EvaluateMaskFilters(position);
		var height = EvaluateFeatureFilters(position);

		return baseHeight + baseNoiseHeight + factor * height;
	}

	public Chunk GenerateChunk(Vector2Int chunkIndex) {
		var chunk = new Chunk();
		var globIndex = new Vector2Int(chunkIndex.x * Chunk.ChunkDimensions.x, chunkIndex.y * Chunk.ChunkDimensions.z);

		for (int x = 0; x < Chunk.ChunkDimensions.x; x++) {
			for (int z = 0; z < Chunk.ChunkDimensions.z; z++) {
				int height = (int)EvaluateHeight(globIndex + new Vector2Int(x, z));

				for (int y = 0; y < Chunk.ChunkDimensions.y; x++) {
					int depth = y - height;

					if (depth <= 0) {
						if (depth > -3) {
							chunk[x, y, z] = new Voxel(50, 205, 50);
						} else {
							chunk[x, y, z] = new Voxel(205, 205, 205);
						}
					} else if (y <= baseHeight) {
						chunk[x, y, z] = new Voxel(0, 0, 255);
					}
				}
			}
		}

		return chunk;
	}
}
