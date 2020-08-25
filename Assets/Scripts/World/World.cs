using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

public class World : MonoBehaviour {

	public static World Instance { get; private set; }

	static ProfilerMarker s_LoadChunks = new ProfilerMarker("World.LoadChunks");
	static ProfilerMarker s_UpdateChunks = new ProfilerMarker("World.UpdateChunks");
	static ProfilerMarker s_UnloadChunks = new ProfilerMarker("World.UnloadChunks");

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

	private void LoadAround(Vector3Int chunkIndex) {
		s_LoadChunks.Begin();

		for(int z = chunkIndex.z - loadRange; z <= chunkIndex.z + loadRange; z++) {
			for (int y = chunkIndex.y - loadRange; y <= chunkIndex.y + loadRange; y++) {
				for (int x = chunkIndex.x - loadRange; x <= chunkIndex.x + loadRange; x++) {
					var currentIndex = new Vector3Int(x, y, z);

					if(Vector3.Distance(currentIndex, chunkIndex) <= loadRange && !loadedChunks.ContainsKey(currentIndex)) {
						LoadChunk(currentIndex);
					}
				}
			}
		}

		s_LoadChunks.End();
	}

	private void Update() {
		var playerChunk = WorldToChunkCoord(playerTransform.position);

		LoadAround(playerChunk);

		s_UpdateChunks.Begin();
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
		s_UpdateChunks.End();

		s_UnloadChunks.Begin();
		foreach (Vector3Int chunkIndex in markedForUnload) {
			UnloadChunk(chunkIndex);
		}

		markedForUnload.Clear();
		s_UnloadChunks.End();
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
