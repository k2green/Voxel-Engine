using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction {
	North,  // Positive z
	East,   // Positive x
	South,  // Negative z
	West    // Negative x
}

public static class DirectionExts {
	public static Vector2Int AsVector2(this Direction dir) {
		switch (dir) {
			case Direction.North: return new Vector2Int(0, 1);
			case Direction.East: return new Vector2Int(1, 0);
			case Direction.South: return new Vector2Int(0, -1);
			case Direction.West: return new Vector2Int(-1, 0);
			default: return Vector2Int.zero;
		}
	}

	public static Vector3Int AsVector3(this Direction dir) {
		switch (dir) {
			case Direction.North: return new Vector3Int(0, 0, 1);
			case Direction.East: return new Vector3Int(1, 0, 0);
			case Direction.South: return new Vector3Int(0, 0, -1);
			case Direction.West: return new Vector3Int(-1, 0, 0);
			default: return Vector3Int.zero;
		}
	}
}
