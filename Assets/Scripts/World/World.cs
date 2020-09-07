using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

[RequireComponent(typeof(RegionManager))]
public class World : MonoBehaviour {

	public static World Instance { get; private set; }

	static ProfilerMarker UpdateChunks = new ProfilerMarker("World.UpdateChunks");
	static ProfilerMarker UnloadChunks = new ProfilerMarker("World.UnloadChunks");

	public int loadRange = 3;

	private RegionManager regionManager;
	private ChunkPool chunkPool;
	private Dictionary<Vector3Int, ChunkObject> visibleChunks;
	private HashSet<Vector3Int> markedForUnload;

	private void Start() {
		Instance = this;

		regionManager = GetComponent<RegionManager>();

		chunkPool = new ChunkPool((int)Mathf.Pow(loadRange * 2, 3), transform);
		visibleChunks = new Dictionary<Vector3Int, ChunkObject>();
		markedForUnload = new HashSet<Vector3Int>();
	}

	public Voxel GetVoxelAt(Vector3 worldPos) {
		var chunkIndex = WorldToChunkCoord(worldPos);
		var posInChunk = ModElements(FloorToInt(worldPos), Chunk.Dimensions);

		return regionManager.GetChunk(chunkIndex)[posInChunk];
	}

	public void LoadChunk(Vector3Int chunkIndex) {
		CreateChunkObject(chunkIndex);
	}

	public void UnloadChunk(Vector3Int chunkIndex) {
		visibleChunks[chunkIndex].Clear();
		visibleChunks.Remove(chunkIndex);
	}

	private void LoadAround(Vector3Int chunkIndex) {
		for (int z = chunkIndex.z - loadRange; z <= chunkIndex.z + loadRange; z++) {
			for (int y = chunkIndex.y - loadRange; y <= chunkIndex.y + loadRange; y++) {
				for (int x = chunkIndex.x - loadRange; x <= chunkIndex.x + loadRange; x++) {
					var currentIndex = new Vector3Int(x, y, z);

					if (Vector3.Distance(currentIndex, chunkIndex) <= loadRange && !visibleChunks.ContainsKey(currentIndex)) {
						LoadChunk(currentIndex);
					}
				}
			}
		}
	}

	private void Update() {
		var playerChunk = regionManager.GetPlayerChunk();

		LoadAround(playerChunk);

		UpdateChunks.Begin();
		foreach (Vector3Int chunkIndex in visibleChunks.Keys) {
			if (Vector3.Distance(chunkIndex, playerChunk) >= loadRange) {
				markedForUnload.Add(chunkIndex);
			} else {
				var chunkObj = visibleChunks[chunkIndex];

				if (chunkObj.IsDirty) {
					chunkObj.UpdateMesh();
				}
			}
		}
		UpdateChunks.End();

		UnloadChunks.Begin();
		foreach (Vector3Int chunkIndex in markedForUnload) {
			UnloadChunk(chunkIndex);
		}

		markedForUnload.Clear();
		UnloadChunks.End();
	}

	private bool CreateChunkObject(Vector3Int chunkIndex) {
		var chunk = regionManager.GetChunk(chunkIndex);

		if (!chunk.IsEmpty && chunkPool.TryGetPooledObject(out var chunkObj)) {
			var worldPos = ChunkToWorldCoord(chunkIndex);

			chunkObj.transform.position = worldPos;
			chunkObj.gameObject.SetActive(true);

			chunkObj.Setup(chunk);

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
