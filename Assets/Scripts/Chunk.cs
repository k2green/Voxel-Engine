using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk {

	public const int ChunkSize = 16;

	public static Vector3Int Dimensions => new Vector3Int(ChunkSize, ChunkSize, ChunkSize);

	public Vector3Int ChunkIndex { get; }
	public Vector3Int ChunkCoord { get; }

	private Voxel[,,] voxels;


	public bool IsEmpty() {
		foreach (var voxel in voxels) {
			if (voxel.IsVisible)
				return false;
		}

		return true;
	}

	public Chunk(Vector3Int chunkIndex) {
		voxels = new Voxel[Dimensions.x, Dimensions.y, Dimensions.z];

		ChunkIndex = chunkIndex;
		ChunkCoord = new Vector3Int(chunkIndex.x * Chunk.Dimensions.x, chunkIndex.y * Chunk.Dimensions.y, chunkIndex.z * Chunk.Dimensions.z);
	}

	public void LoadChunk(WorldGenerator generator) {
		var chunkCoord = new Vector3Int(ChunkIndex.x * Chunk.Dimensions.x, ChunkIndex.y * Chunk.Dimensions.y, ChunkIndex.z * Chunk.Dimensions.z);
		var heightMap = generator.GetHeightMap(ChunkIndex);

		for (int z = 0; z < Chunk.Dimensions.z; z++) {
			for (int x = 0; x < Chunk.Dimensions.x; x++) {
				int height = (int)heightMap[x, z];

				if (height >= chunkCoord.y + Chunk.Dimensions.y + 3) {
					FillRange(new Vector3Int(x, 0, z), new Vector3Int(x, 15, z), 105, 105, 105);
				} else if (height >= chunkCoord.y + Chunk.Dimensions.y) {
					var newHeight = height - chunkCoord.y - 3;
					FillRange(new Vector3Int(x, 0, z), new Vector3Int(x, newHeight, z), 105, 105, 105);
					FillRange(new Vector3Int(x, newHeight + 1, z), new Vector3Int(x, 15, z), 50, 205, 50);
				} else if (height >= chunkCoord.y + 3) {
					var newHeight = height - chunkCoord.y - 3;
					FillRange(new Vector3Int(x, 0, z), new Vector3Int(x, newHeight, z), 105, 105, 105);
					FillRange(new Vector3Int(x, newHeight + 1, z), new Vector3Int(x, newHeight + 3, z), 50, 205, 50);
				} else if (height >= chunkCoord.y) {
					var newHeight = height - chunkCoord.y;
					FillRange(new Vector3Int(x, 0, z), new Vector3Int(x, newHeight, z), 50, 205, 50);
				}
			}
		}
	}

	public bool ContainsIndex(Vector3Int index) =>
		index.x >= 0 && index.x < Dimensions.x &&
		index.y >= 0 && index.y < Dimensions.y &&
		index.z >= 0 && index.z < Dimensions.z;

	public Voxel this[Vector3Int index] {
		get {
			if (!ContainsIndex(index))
				return new Voxel(0, 0, 0, 0);

			return voxels[index.x, index.y, index.z];
		}
		set {
			if (!ContainsIndex(index))
				throw new IndexOutOfRangeException($"Index {index} is out of range of chunk with dimensions {Dimensions}");

			voxels[index.x, index.y, index.z] = value;
		}
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
		return this[voxelPos].IsTransparent;
	}
}
