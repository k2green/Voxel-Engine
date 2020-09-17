using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(TerrainGenerator))]
public class RegionManager : MonoBehaviour {

	private static string worldsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Voxel Worlds");

	private TerrainGenerator generator;
	private Dictionary<Vector2Int, Region> loadedRegions;

	private void Start() {
		generator = GetComponent<TerrainGenerator>();
		loadedRegions = new Dictionary<Vector2Int, Region>();
	}

	public Region GetRegion(Vector2Int regionIndex) {
		if (!loadedRegions.ContainsKey(regionIndex))
			loadedRegions.Add(regionIndex, LoadRegion(regionIndex));

		return loadedRegions[regionIndex];
	}

	private Region LoadRegion(Vector2Int regionIndex) {
		return Region.LoadRegion(worldsDirectory, regionIndex);
	}
}
