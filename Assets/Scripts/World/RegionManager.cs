using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RegionManager : MonoBehaviour {

	public NoiseFilter noiseFilter;
	public string worldName = "World";

	private WorldGenerator worldGenerator;
	private Dictionary<Vector3Int, Region> regions;
	private Transform playerTransform;

	private static string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);

	// Start is called before the first frame update
	void Start() {
		worldGenerator = new WorldGenerator(noiseFilter);
		regions = new Dictionary<Vector3Int, Region>();
	}

	// Update is called once per frame
	void Update() {

	}

	public Chunk GetChunk(Vector3Int chunkIndex) {
		var regionIndex = ChunkToRegion(chunkIndex);

		if (!regions.ContainsKey(regionIndex)) {
			LoadRegion(regionIndex);
		}

		return regions[regionIndex].GetChunk(chunkIndex);
	}

	private void LoadRegion(Vector3Int regionIndex) {
		if (regions.ContainsKey(regionIndex)) return;

		var worldPath = Path.Combine(documentsPath, worldName);
		var worldDirInfo = new DirectoryInfo(worldPath);

		if (worldDirInfo.Exists) {
			var regionFile = Path.Combine(worldPath, Region.GetRegionFileName(regionIndex));
			var fileInfo = new FileInfo(regionFile);

			if (fileInfo.Exists) {
				var bytes = File.ReadAllBytes(regionFile);
				regions.Add(regionIndex, Region.FromBytes(bytes, worldGenerator));

				return;
			}
		}

		regions.Add(regionIndex, new Region(worldGenerator));
	}

	private void UnloadRegion(Vector3Int regionIndex) {
		if (!regions.ContainsKey(regionIndex)) return;

		SaveRegion(regionIndex);
		regions.Remove(regionIndex);
	}

	private void SaveRegion(Vector3Int regionIndex) {
		var worldPath = Path.Combine(documentsPath, worldName);
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
