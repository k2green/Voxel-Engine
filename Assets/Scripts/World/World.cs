using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour {

	public static World Instance { get; private set; }

	public NoiseFilter noiseFilter;
	public Transform playerTransform;
	public int loadRange = 3;

	private WorldGenerator worldGen;
	private ChunkPool chunkPool;
	private Dictionary<Vector3Int, Chunk> loadedChunks;
	private Dictionary<Vector3Int, ChunkObject> visibleChunks;
	private HashSet<Vector3Int> markedForUnload;

	private void Start() {
		Instance = this;

		worldGen = new WorldGenerator(noiseFilter);
		chunkPool = new ChunkPool((int)Mathf.Pow(loadRange * 2, 3), transform);
		loadedChunks = new Dictionary<Vector3Int, Chunk>();
		visibleChunks = new Dictionary<Vector3Int, ChunkObject>();
		markedForUnload = new HashSet<Vector3Int>();
	}

	public Voxel GetVoxelAt(Vector3 worldPos) {
		var chunkIndex = WorldToChunkCoord(worldPos);

		if (!loadedChunks.ContainsKey(chunkIndex))
			return new Voxel(0, 0, 0, 0);

		var posInChunk = ModElements(FloorToInt(worldPos), Chunk.Dimensions);
		return loadedChunks[chunkIndex][posInChunk];
	}

	public void LoadChunk(Vector3Int chunkIndex) {
		loadedChunks.Add(chunkIndex, new Chunk(chunkIndex));
		loadedChunks[chunkIndex].LoadChunk(worldGen);
	}

	public void UnloadChunk(Vector3Int chunkIndex) {
		if (visibleChunks.ContainsKey(chunkIndex)) {
			visibleChunks[chunkIndex].Clear();
			visibleChunks.Remove(chunkIndex);
		}

		loadedChunks.Remove(chunkIndex);
	}

	private void RecursiveLoad(Vector3Int playerChunkIndex) {
		var visitedSet = new HashSet<Vector3Int>();
		RecursiveLoad(playerChunkIndex, playerChunkIndex, ref visitedSet);
	}

	private void RecursiveLoad(Vector3Int chunkIndex, Vector3Int playerChunkIndex, ref HashSet<Vector3Int> visited) {
		if (visited.Contains(chunkIndex) || Vector3.Distance(chunkIndex, playerChunkIndex) >= loadRange) return;

		if (!loadedChunks.ContainsKey(chunkIndex))
			LoadChunk(chunkIndex);

		visited.Add(chunkIndex);

		RecursiveLoad(chunkIndex + new Vector3Int(1, 0, 0), playerChunkIndex, ref visited);
		RecursiveLoad(chunkIndex + new Vector3Int(0, 1, 0), playerChunkIndex, ref visited);
		RecursiveLoad(chunkIndex + new Vector3Int(0, 0, 1), playerChunkIndex, ref visited);

		RecursiveLoad(chunkIndex + new Vector3Int(-1, 0, 0), playerChunkIndex, ref visited);
		RecursiveLoad(chunkIndex + new Vector3Int(0, -1, 0), playerChunkIndex, ref visited);
		RecursiveLoad(chunkIndex + new Vector3Int(0, 0, -1), playerChunkIndex, ref visited);
	}

	private void Update() {
		var playerChunk = WorldToChunkCoord(playerTransform.position);

		RecursiveLoad(playerChunk);

		foreach (Vector3Int chunkIndex in loadedChunks.Keys) {
			if (Vector3.Distance(chunkIndex, playerChunk) >= loadRange) {
				markedForUnload.Add(chunkIndex);
			} else {
				if (!visibleChunks.ContainsKey(chunkIndex))
					if (!CreateChunkObject(chunkIndex))
						continue;

				var chunkObj = visibleChunks[chunkIndex];

				if (chunkObj.IsDirty) {
					chunkObj.UpdateMesh();
				}
			}
		}

		foreach (Vector3Int chunkIndex in markedForUnload) {
			UnloadChunk(chunkIndex);
		}

		markedForUnload.Clear();
	}

	private bool CreateChunkObject(Vector3Int chunkIndex) {
		if (chunkPool.TryGetPooledObject(out var chunkObj)) {
			var worldPos = ChunkToWorldCoord(chunkIndex);

			chunkObj.transform.position = worldPos;
			chunkObj.gameObject.SetActive(true);

			chunkObj.Setup(loadedChunks[chunkIndex]);

			visibleChunks.Add(chunkIndex, chunkObj);
			return true;
		}

		return false;
	}

	private Vector3Int ChunkToWorldCoord(Vector3Int chunkIndex) => MultElements(chunkIndex, Chunk.Dimensions);
	private Vector3Int WorldToChunkCoord(Vector3 worldCoord) => DivElements(worldCoord, Chunk.Dimensions);

	private Vector3Int FloorToInt(Vector3 vector) => new Vector3Int(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y), Mathf.FloorToInt(vector.z));
	private Vector3Int ModElements(Vector3Int a, Vector3Int b) => new Vector3Int(a.x % b.x, a.y % b.y, a.z % b.z);
	private Vector3Int MultElements(Vector3Int a, Vector3Int b) => new Vector3Int(a.x * b.x, a.y * b.y, a.z * b.z);
	private Vector3Int DivElements(Vector3 a, Vector3 b) => FloorToInt(new Vector3(a.x / b.x, a.y / b.y, a.z / b.z));
}
