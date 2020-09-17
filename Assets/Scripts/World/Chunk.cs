using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Chunk {

	public static Vector3Int ChunkDimensions => new Vector3Int(16, 256, 16);
	public static int VoxelCount => ChunkDimensions.x * ChunkDimensions.y * ChunkDimensions.z;

	private Voxel[] voxels;


	public Chunk() : this(new Voxel[VoxelCount]) { }

	private Chunk(Voxel[] voxels) {
		this.voxels = voxels;
	}

	public Voxel this[int x, int y, int z] {
		get {
			if (!IsInRange(x, y, z))
				return new Voxel(0, 0, 0, 0);

			return voxels[FlattenIndex(x, y, z)];
		}
		set {
			if (!IsInRange(x, y, z))
				throw new IndexOutOfRangeException($"Index ({x}, {y}, {z}) is out of range of chunk with dimensions {ChunkDimensions}");

			voxels[FlattenIndex(x, y, z)] = value;
		}
	}

	public Voxel this[Vector3Int index] {
		get => this[index.x, index.y, index.z];
		set => this[index.x, index.y, index.z] = value;
	}

	private void SerializeVoxels(FileStream stream) {
		for (int i = 0; i < VoxelCount; i++) {
			voxels[i].WriteTo(stream);
		}
	}

	public static void WriteChunkToFile(FileStream stream, Chunk chunk) {
		if (chunk == null) {
			stream.Write(new byte[] { 0 }, 0, 1);
		} else {
			stream.Write(new byte[] { 1 }, 0, 1);

			chunk.SerializeVoxels(stream);
		}
	}

	private static int FlattenIndex(Vector3Int pos) => FlattenIndex(pos.x, pos.y, pos.z);
	private static int FlattenIndex(int x, int y, int z) {
		return z * ChunkDimensions.y * ChunkDimensions.x + y * ChunkDimensions.x + x;
	}

	private static bool IsInRange(Vector3Int pos) => IsInRange(pos.x, pos.y, pos.z);
	private static bool IsInRange(int x, int y, int z) =>
		x >= 0 && x < ChunkDimensions.x &&
		y >= 0 && y < ChunkDimensions.y &&
		z >= 0 && z < ChunkDimensions.z;

	private static Voxel[] DeserializeVoxels(FileStream stream) {
		var voxels = new Voxel[VoxelCount];

		for (int i = 0; i < voxels.Length; i++) {
			voxels[i] = Voxel.FromStream(stream);
		}

		return voxels;
	}

	public static Chunk ReadChunkFromFile(FileStream stream) {
		var chunkFlag = new byte[1];
		stream.Read(chunkFlag, 0, 1);

		if ((chunkFlag[0] & 1) == 0) {
			return null;
		} else {
			return new Chunk(DeserializeVoxels(stream));
		}
	}
}
