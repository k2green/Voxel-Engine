using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshHandler {

	public static MeshData GreedyMesh(Voxel[] voxels, Vector3Int blockDimensions) {
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

			for (start[direction] = 0; start[direction] < blockDimensions[direction]; start[direction]++) {
				merged = new bool[blockDimensions[axisA], blockDimensions[axisB]];

				for (start[axisA] = 0; start[axisA] < blockDimensions[axisA]; start[axisA]++) {
					for (start[axisB] = 0; start[axisB] < blockDimensions[axisB]; start[axisB]++) {
						startVoxel = voxels[FlattenIndex(start, blockDimensions)];

						if (merged[start[axisA], start[axisB]] || !startVoxel.IsVisible || !IsFaceVisible(voxels, blockDimensions, start, direction, isBackFace))
							continue;

						size = new Vector3Int();
						for (position = start, position[axisB]++; (position[axisB] < blockDimensions[axisB]) && (CompareColours(voxels, blockDimensions, start, position, direction, isBackFace)) && (!merged[position[axisA], position[axisB]]); position[axisB]++) { }
						size[axisB] = position[axisB] - start[axisB];


						for (position = start, position[axisA]++; (position[axisA] < blockDimensions[axisA]) && (CompareColours(voxels, blockDimensions, start, position, direction, isBackFace)) && (!merged[position[axisA], position[axisB]]); position[axisA]++) {
							for (position[axisB] = start[axisB]; (position[axisB] < blockDimensions[axisB]) && (CompareColours(voxels, blockDimensions, start, position, direction, isBackFace)) && (!merged[position[axisA], position[axisB]]); position[axisB]++) { }

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

	private static bool IsFaceVisible(Voxel[] voxels, Vector3Int dimensions, Vector3Int voxelPos, int axis, bool isBackFace) {
		voxelPos[axis] += isBackFace ? -1 : 1;

		if (IsInRange(voxelPos, dimensions)) {
			return !voxels[FlattenIndex(voxelPos, dimensions)].IsVisible;
		} else {
			// TODO: Implement global face checking
			throw new NotImplementedException();
		}
	}

	private static bool CompareColours(Voxel[] voxels, Vector3Int dimensions, Vector3Int a, Vector3Int b, int axis, bool isBackFace) {
		var blockA = voxels[FlattenIndex(a, dimensions)];
		var blockB = voxels[FlattenIndex(b, dimensions)];

		return blockA.Equals(blockB) && blockB.IsVisible && IsFaceVisible(voxels, dimensions, b, axis, isBackFace);
	}

	private static int FlattenIndex(Vector3Int pos, Vector3Int dimensions) => FlattenIndex(pos.x, pos.y, pos.z, dimensions);
	private static int FlattenIndex(int x, int y, int z, Vector3Int dimensions) {
		return (z * dimensions.y * dimensions.x) + (y * dimensions.x) + x;
	}
	private static bool IsInRange(Vector3Int pos, Vector3Int dimensions) => IsInRange(pos.x, pos.y, pos.z, dimensions);
	private static bool IsInRange(int x, int y, int z, Vector3Int dimensions) =>
		x >= 0 && x < dimensions.x &&
		y >= 0 && y < dimensions.y &&
		z >= 0 && z < dimensions.z;
}
