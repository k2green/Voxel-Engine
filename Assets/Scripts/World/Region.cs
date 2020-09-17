using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Region {

	public static int RegionSize => 32;
	public static int ChunkCount => RegionSize * RegionSize;
	public static Vector2Int RegionDimensions => new Vector2Int(RegionSize, RegionSize);

	private Chunk[] chunks;

	public Region() : this(new Chunk[ChunkCount]) {

	}

	private Region(Chunk[] chunks) {
		this.chunks = chunks;
	}

	public Chunk this[int x, int z] {
		get {
			x = Math.NFMod(x, RegionSize);
			z = Math.NFMod(z, RegionSize);

			return chunks[FlattenIndex(x, z)];
		}
		set {
			x = Math.NFMod(x, RegionSize);
			z = Math.NFMod(z, RegionSize);

			chunks[FlattenIndex(x, z)] = value;
		}
	}

	public Chunk FindOrGenerateChunk(Vector2Int globalChunkIndex, TerrainGenerator generator) {
		GenerateChunk(globalChunkIndex, generator);

		return this[globalChunkIndex];
	}

	public Chunk this[Vector2Int globalChunkIndex] {
		get => this[globalChunkIndex.x, globalChunkIndex.y];
		set => this[globalChunkIndex.x, globalChunkIndex.y] = value;
	}

	private static int FlattenIndex(int x, int z) => z * RegionSize + x;
	private static int FlattenIndex(Vector2Int localChunkIndex) => FlattenIndex(localChunkIndex.x, localChunkIndex.y);

	private static bool IsInRange(int x, int z) => x >= 0 && x < RegionSize && z >= 0 && z < RegionSize;
	private static bool IsInRange(Vector2Int localChunkIndex) => IsInRange(localChunkIndex.x, localChunkIndex.y);

	public static string GetRegionFileName(Vector2Int regionIndex) => $"Region-({regionIndex.x})-({regionIndex.y}).bin";

	public void GenerateChunk(Vector2Int globalChunkIndex, TerrainGenerator generator) {
		if (this[globalChunkIndex] == null) {
			this[globalChunkIndex] = generator.GenerateChunk(globalChunkIndex);
		}
	}

	public void SaveRegion(string path, Vector2Int regionIndex) {
		var dirInfo = new DirectoryInfo(path);
		var filePath = Path.Combine(path, GetRegionFileName(regionIndex));

		if (!dirInfo.Exists)
			dirInfo.Create();

		using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write)) {
			foreach (var chunk in chunks) {
				Chunk.WriteChunkToFile(fileStream, chunk);
			}
		}
	}

	public static Region LoadRegion(string path, Vector2Int regionIndex) {
		var filePath = Path.Combine(path, GetRegionFileName(regionIndex));
		var fileInfo = new FileInfo(filePath);

		if (fileInfo.Exists) {
			var chunks = new Chunk[ChunkCount];

			using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
				for (int i = 0; i < ChunkCount; i++) {
					chunks[i] = Chunk.ReadChunkFromFile(fileStream);
				}
			}

			return new Region(chunks);
		} else {
			return new Region();
		}
	}
}
