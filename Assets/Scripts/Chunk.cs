using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

public class Chunk {

	public const int ChunkSize = 16;

	public static Vector3Int Dimensions => new Vector3Int(ChunkSize, ChunkSize, ChunkSize);

	public Vector3Int ChunkIndex { get; }
	public Vector3Int ChunkCoord { get; }
	public bool IsEmpty { get; private set; }

	private Voxel[] voxels;

	public void UpdateIsEmpty() {
		foreach (var voxel in voxels) {
			if (voxel.IsVisible) {
				IsEmpty = false;
				return;
			}
		}

		IsEmpty = true;
	}

	public Chunk(Vector3Int chunkIndex) {
		voxels = new Voxel[Dimensions.x * Dimensions.y * Dimensions.z];

		ChunkIndex = chunkIndex;
		ChunkCoord = new Vector3Int(chunkIndex.x * Chunk.Dimensions.x, chunkIndex.y * Chunk.Dimensions.y, chunkIndex.z * Chunk.Dimensions.z);
	}

	public void SetVoxel(int index, Voxel voxel) {
		voxels[index] = voxel;
	}

	public bool ContainsIndex(int x, int y, int z) => x >= 0 && x < Dimensions.x && y >= 0 && y < Dimensions.y && z >= 0 && z < Dimensions.z;

	public Voxel this[Vector3Int index] {
		get => this[index.x, index.y, index.z];
		set => this[index.x, index.y, index.z] = value;
	}

	public Voxel this[int x, int y, int z] {
		get {
			if (!ContainsIndex(x, y, z))
				return new Voxel(0, 0, 0, 0);

			return voxels[FlattenIndex(x, y, z)];
		}
		set {
			if (!ContainsIndex(x, y, z))
				throw new IndexOutOfRangeException($"Index ({x}, {y}, {z}) is out of range of chunk with dimensions {Dimensions}");

			voxels[FlattenIndex(x, y, z)] = value;
		}
	}

	private int FlattenIndex(int x, int y, int z) {
		return z * Chunk.Dimensions.x * Chunk.Dimensions.y + y * Chunk.Dimensions.x + x;
	}

	public void FillRange(Vector3Int start, Vector3Int end, byte r, byte g, byte b, byte a = 255) {
		for (int z = start.z; z <= end.z; z++) {
			for (int y = start.y; y <= end.y; y++) {
				for (int x = start.x; x <= end.x; x++) {
					this[new Vector3Int(x, y, z)] = new Voxel(r, g, b, a);
				}
			}
		}
	}

	public MeshData GenerateMesh() {
		var builder = new MeshBuilder();

		Vector3Int start, position, size, m, n, offset;
		int direction, axisA, axisB;
		Voxel startVoxel;

		Vector3[] vertices;
		bool[,] merged;

		for (int face = 0; face < 6; face++) {
			bool isBackFace = face > 2;
			direction = face % 3;
			axisA = (face + 1) % 3;
			axisB = (face + 2) % 3;

			start = new Vector3Int();
			position = new Vector3Int();

			for (start[direction] = 0; start[direction] < Dimensions[direction]; start[direction]++) {
				merged = new bool[Dimensions[axisA], Dimensions[axisB]];

				for (start[axisA] = 0; start[axisA] < Dimensions[axisA]; start[axisA]++) {
					for (start[axisB] = 0; start[axisB] < Dimensions[axisB]; start[axisB]++) {
						startVoxel = this[start];

						if (merged[start[axisA], start[axisB]] || !startVoxel.IsVisible || !IsFaceVisible(start, direction, isBackFace))
							continue;

						size = new Vector3Int();
						for (position = start, position[axisB]++; (position[axisB] < Dimensions[axisB]) && (CompareColours(start, position, direction, isBackFace)) && (!merged[position[axisA], position[axisB]]); position[axisB]++) { }
						size[axisB] = position[axisB] - start[axisB];


						for (position = start, position[axisA]++; (position[axisA] < Dimensions[axisA]) && (CompareColours(start, position, direction, isBackFace)) && (!merged[position[axisA], position[axisB]]); position[axisA]++) {
							for (position[axisB] = start[axisB]; (position[axisB] < Dimensions[axisB]) && (CompareColours(start, position, direction, isBackFace)) && (!merged[position[axisA], position[axisB]]); position[axisB]++) { }

							if (position[axisB] - start[axisB] < size[axisB]) {
								break;
							} else {
								position[axisB] = start[axisB];
							}
						}

						size[axisA] = position[axisA] - start[axisA];

						m = new Vector3Int();
						n = new Vector3Int();

						m[axisA] = size[axisA];
						n[axisB] = size[axisB];

						offset = start;
						offset[direction] += isBackFace ? 0 : 1;

						vertices = new Vector3[] {
							offset,
							offset + m,
							offset + m + n,
							offset + n
						};

						builder.AddQuad(vertices, startVoxel.GetColor(), isBackFace);

						for (int a = 0; a < size[axisA]; a++) {
							for (int b = 0; b < size[axisB]; b++) {
								merged[start[axisA] + a, start[axisB] + b] = true;
							}
						}
					}
				}
			}
		}

		return builder.GetMeshData();
	}

	private bool CompareColours(Vector3Int a, Vector3Int b, int axis, bool isBackFace) {
		var blockA = this[a];
		var blockB = this[b];

		return blockA.Equals(blockB) && blockB.IsVisible && IsFaceVisible(b, axis, isBackFace);
	}

	private bool IsFaceVisible(Vector3Int voxelPos, int axis, bool isBackFace) {
		voxelPos[axis] += isBackFace ? -1 : 1;

		if (ContainsIndex(voxelPos.x, voxelPos.y, voxelPos.z)) {
			return !this[voxelPos].IsVisible;
		} else {
			var (neighbourChunk, newPos) = GetAdjacentCoords(voxelPos);

			return !RegionManager.Instance.GetChunk(neighbourChunk, false)[newPos].IsVisible;
		}
	}

	int CyclicMod(int a, int b) {
		return a - b * Mathf.FloorToInt((float)a / b);
	}

	private (Vector3Int, Vector3Int) GetAdjacentCoords(Vector3Int voxelPos) {
		var chunkIndex = ChunkIndex;

		for (int i = 0; i < 3; i++) {
			if (voxelPos[i] < 0) {
				chunkIndex[i] -= 1;
			} else if (voxelPos[i] >= Chunk.Dimensions[i]) {
				chunkIndex[i] += 1;
			}

			voxelPos[i] = CyclicMod(voxelPos[i], Chunk.Dimensions.x);
		}

		return (chunkIndex, voxelPos);
	}

	public byte[] Serialise() {
		var bytes = new List<byte>();

		foreach (var voxel in voxels)
			bytes.AddRange(voxel.ToBytes());

		return bytes.ToArray();
	}

	static ProfilerMarker HeightMap = new ProfilerMarker("World.HeightMap");
	static ProfilerMarker FillChunk = new ProfilerMarker("World.FillChunk");

	public static Chunk GenerateChunk(Vector3Int chunkIndex, WorldGenerator generator) {
		var chunk = new Chunk(chunkIndex);

		using (FillChunk.Auto()) {
			var chunkCoord = new Vector3Int(chunkIndex.x * Chunk.Dimensions.z, chunkIndex.y * Chunk.Dimensions.y, chunkIndex.z * Chunk.Dimensions.z);
			var (minimum, heights) = generator.GetHeightMap(chunkIndex);

			if (minimum < chunkCoord.y + 2 * Chunk.Dimensions.y) {
				for (int z = 0; z < Dimensions.z; z++) {
					for (int x = 0; x < Dimensions.x; x++) {
						var height = GetHeight(heights, x, z);

						for (int y = 0; y < Dimensions.y; y++) {
							var globalY = chunkCoord.y + y;
							var depth = GetDepth(globalY, height);

							if (depth > 0 && height < 0 && globalY <= 0) {
								chunk[x, y, z] = new Voxel(0, 255, 255, 100);
							} else if (depth < -3 || (depth <= 0 && height < 0)) {
								chunk[x, y, z] = new Voxel(105, 105, 105);
							} else if (depth <= 0) {
								chunk[x, y, z] = new Voxel(50, 205, 50);
							}

						}
					}
				}
			}
		}

		chunk.UpdateIsEmpty();

		return chunk;
	}

	public static int GetDepth(int worldY, int heightVal) {
		return worldY - heightVal;
	}

	private static int GetHeight(float[,] heightMap, int localX, int localZ) {
		return (int)heightMap[localX, localZ];
	}
}
