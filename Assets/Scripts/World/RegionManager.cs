using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Profiling;
using UnityEngine;

[RequireComponent(typeof(WorldGenerator))]
public class RegionManager : MonoBehaviour {

	public static RegionManager Instance { get; private set; }

	public string worldName = "World";
	public Transform playerTransform;
	public bool regenerate = false;

	private WorldGenerator worldGenerator;
	private Dictionary<Vector3Int, Region> regions;

	private static string worldsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Voxel Worlds");

	// Start is called before the first frame update
	void Start() {
		Instance = this;
		worldGenerator = GetComponent<WorldGenerator>();
		regions = new Dictionary<Vector3Int, Region>();
	}

	// Update is called once per frame
	void LateUpdate() {
		var markedForUnload = new HashSet<Vector3Int>();
		var playerChunk = GetPlayerChunk();

		foreach (var index in regions.Keys) {
			var centreChunk = (index + Vector3.one * .5f) * Region.RegionSize;

			if (Vector3.Distance(centreChunk, playerChunk) > Region.RegionSize)
				markedForUnload.Add(index);
		}

		foreach (var index in markedForUnload) {
			UnloadRegion(index);
		}
	}

	private void OnDestroy() {
		foreach (var index in regions.Keys)
			SaveRegion(index);
	}

	public Vector3Int GetPlayerChunk() {
		var chunkIndex = new Vector3Int();

		for (int i = 0; i < 3; i++) {
			chunkIndex[i] = Mathf.FloorToInt(playerTransform.position[i] / Chunk.Dimensions[i]);
		}

		return chunkIndex;
	}

	public Chunk GetChunk(Vector3Int chunkIndex, bool generate = true) {
		var regionIndex = ChunkToRegion(chunkIndex);

		if (!regions.ContainsKey(regionIndex)) {
			if(generate) {
				LoadRegion(regionIndex);
			} else {
				return new Chunk(chunkIndex);
			}
		}

		return regions[regionIndex].GetChunk(chunkIndex, worldGenerator);
	}

	private void LoadRegion(Vector3Int regionIndex) {
		if (regions.ContainsKey(regionIndex)) return;

		if (!regenerate) {
			var worldPath = Path.Combine(worldsDirectory, worldName);
			var worldDirInfo = new DirectoryInfo(worldPath);

			if (worldDirInfo.Exists) {
				var regionFile = Path.Combine(worldPath, Region.GetRegionFileName(regionIndex));
				var fileInfo = new FileInfo(regionFile);

				if (fileInfo.Exists) {
					Debug.Log("Loading region");
					var bytes = File.ReadAllBytes(regionFile);
					regions.Add(regionIndex, Region.FromBytes(bytes, regionIndex));

					return;
				}
			}
		}

		regions.Add(regionIndex, new Region(regionIndex));
	}

	private void UnloadRegion(Vector3Int regionIndex) {
		if (!regions.ContainsKey(regionIndex)) return;

		SaveRegion(regionIndex);
		regions.Remove(regionIndex);
	}

	private void SaveRegion(Vector3Int regionIndex) {
		var worldPath = Path.Combine(worldsDirectory, worldName);
		var worldDirInfo = new DirectoryInfo(worldPath);

		if (!worldDirInfo.Exists)
			worldDirInfo.Create();

		string regionFile = Path.Combine(worldPath, Region.GetRegionFileName(regionIndex));

		using (var stream = File.OpenWrite(regionFile))
			regions[regionIndex].WriteTo(stream);
	}

	private Vector3Int ChunkToRegion(Vector3Int chunkIndex) {
		return FloorToInt((Vector3)chunkIndex / Region.RegionSize);
	}

	private Vector3Int FloorToInt(Vector3 vector) {
		return new Vector3Int(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y), Mathf.FloorToInt(vector.z));
	}
}
