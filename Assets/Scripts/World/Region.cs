using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Region {
	public const int RegionSize = 32;
	private readonly WorldGenerator worldGenerator;
	private Dictionary<Vector3Int, Chunk> regionChunks;

	public Region(WorldGenerator generator) {
		regionChunks = new Dictionary<Vector3Int, Chunk>();
		worldGenerator = generator;
	}

	public Chunk GetChunk(Vector3Int chunkIndex) {
		if (!regionChunks.ContainsKey(chunkIndex)) {
			regionChunks.Add(chunkIndex, new Chunk(chunkIndex));
		}

		return regionChunks[chunkIndex];
	}

	public Chunk this[Vector3Int index] {
		get => GetChunk(index);
		set {
			if (regionChunks.ContainsKey(index))
				regionChunks.Remove(index);

			regionChunks.Add(index, value);
		}
	}

	private byte[] Serialize() {
		var list = new List<byte>();

		foreach (var index in regionChunks.Keys) {
			list.AddRange(SerializeVector3Int(index));
			list.AddRange(regionChunks[index].Serialise());
		}

		return list.ToArray();
	}

	public void WriteTo(FileStream fileStream) {
		var bytes = Serialize();
		fileStream.Write(bytes, 0, bytes.Length);
	}

	private byte[] SerializeVector3Int(Vector3Int vector) {
		var bytes = new byte[12];

		for (int i = 0; i < 3; i++) {
			BitConverter.GetBytes(vector[i]).CopyTo(bytes, i * 4);
		}

		return bytes;
	}

	public static string GetRegionFileName(Vector3Int regionIndex) {
		return $"Region-{regionIndex.x}-{regionIndex.y}-{regionIndex.z}.bin";
	}

	public static Region FromBytes(byte[] bytes, WorldGenerator generator) {
		var region = new Region(generator);
		var currentIndex = 0;

		while (currentIndex < bytes.Length) {
			var index = DeserializeVector3Int(bytes, currentIndex);
			currentIndex += 12;

			var chunk = DeserializeChunk(bytes, currentIndex, index);
			currentIndex += Chunk.Dimensions.x * Chunk.Dimensions.y * Chunk.Dimensions.z * 4;

			region[index] = chunk;
		}

		return region;
	}

	private static Vector3Int DeserializeVector3Int(byte[] bytes, int startIndex) {
		var vector = new Vector3Int();

		for (int i = 0; i < 3; i++) {
			var elementIndex = startIndex + i * 4;
			vector[i] = BitConverter.ToInt32(bytes, elementIndex);
		}

		return vector;
	}

	private static Chunk DeserializeChunk(byte[] bytes, int startIndex, Vector3Int index) {
		var chunk = new Chunk(index);

		for (int i = 0; i < Chunk.Dimensions.x * Chunk.Dimensions.y * Chunk.Dimensions.z; i++) {
			var baseIndex = startIndex + i * 4;
			chunk.SetVoxel(i, new Voxel(bytes[baseIndex], bytes[baseIndex + 1], bytes[baseIndex + 2], bytes[baseIndex + 3]));
		}

		return chunk;
	}
}
