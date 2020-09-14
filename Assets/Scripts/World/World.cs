
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

[RequireComponent(typeof(RegionManager))]
public class World : MonoBehaviour {

	public static World Instance { get; private set; }

	static ProfilerMarker UpdateChunks = new ProfilerMarker("World.UpdateChunks");
	static ProfilerMarker ModifyChunks = new ProfilerMarker("World.ModifyChunks");
	static ProfilerMarker LoadChunks = new ProfilerMarker("World.LoadChunks");
	static ProfilerMarker UnloadChunks = new ProfilerMarker("World.UnloadChunks");

	public int loadRange = 3;

	private RegionManager regionManager;
	private ChunkPool chunkPool;
	private Dictionary<Vector3Int, ChunkObject> visibleChunks;

	private void Start() {
		Instance = this;

		regionManager = GetComponent<RegionManager>();

		chunkPool = new ChunkPool((int)Mathf.Pow(loadRange * 2, 3), transform);
		visibleChunks = new Dictionary<Vector3Int, ChunkObject>();
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

	private (IEnumerable<Vector3Int>, IEnumerable<Vector3Int>) GetChunksModifyLists(Vector3Int playerChunk) {
		var chunksToLoad = new HashSet<Vector3Int>();
		var chunksToUnload = new HashSet<Vector3Int>();

		foreach (var index in visibleChunks.Keys) {
			if (Vector3.Distance(index, playerChunk) > loadRange) {
				chunksToUnload.Add(index);
			} else {
				foreach (Direction direction in Enum.GetValues(typeof(Direction))) {
					var newIndex = index + direction.AsVector();

					if (!visibleChunks.ContainsKey(newIndex) && !chunksToLoad.Contains(newIndex) && Vector3.Distance(playerChunk, newIndex) <= loadRange) {
						chunksToLoad.Add(newIndex);
					}
				}
			}
		}

		return (chunksToLoad, chunksToUnload);
	}

	private void Update() {
		IEnumerable<Vector3Int> toLoad;
		IEnumerable<Vector3Int> toUnload;

		using (ModifyChunks.Auto()) {
			var playerChunk = regionManager.GetPlayerChunk();

			if (!visibleChunks.ContainsKey(playerChunk))
				LoadChunk(playerChunk);

			(toLoad, toUnload) = GetChunksModifyLists(playerChunk);
		}

		using (UnloadChunks.Auto())
			foreach (var index in toUnload)
				UnloadChunk(index);

		using (LoadChunks.Auto())
			foreach (var index in toLoad)
				LoadChunk(index);

		using (UpdateChunks.Auto()) {
			foreach (Vector3Int chunkIndex in visibleChunks.Keys) {
				var chunkObj = visibleChunks[chunkIndex];

				if (chunkObj.IsDirty) {
					chunkObj.UpdateMesh();
				}
			}
		}
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
