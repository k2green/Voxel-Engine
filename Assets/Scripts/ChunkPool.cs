using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkPool {

	private List<ChunkObject> pooledObjects;

	public ChunkPool(int poolSize, Transform parentTransform = null) {
		pooledObjects = new List<ChunkObject>();

		for (int i = 0; i < poolSize; i++) {
			pooledObjects.Add(CreateChunkObject(parentTransform));
		}
	}

	private ChunkObject CreateChunkObject(Transform parentTransform) {
		var gameObj = new GameObject("Chunk");
		gameObj.transform.parent = parentTransform;

		gameObj.AddComponent<MeshFilter>();
		gameObj.AddComponent<MeshRenderer>();
		gameObj.AddComponent<MeshCollider>();

		var chunkObj = gameObj.AddComponent<ChunkObject>();
		chunkObj.Initialise();

		return chunkObj;
	}

	public bool TryGetPooledObject(out ChunkObject chunk) {
		chunk = null;

		foreach (var obj in pooledObjects) {
			if (!obj.gameObject.activeInHierarchy) {
				chunk = obj;
				return true;
			}
		}

		return false;
	}
}
