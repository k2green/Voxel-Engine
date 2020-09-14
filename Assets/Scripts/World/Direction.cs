using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction {
	North,	// Positive Z
	Up,		// Positive Y
	East,   // Positive X
	South,  // Negative Z
	Down,   // Negative Y
	West    // Negative X
}

public static class DirectionExts {
	public static Vector3Int AsVector(this Direction dir) {
		switch (dir) {
			case Direction.North: return new Vector3Int(0, 0, 1);
			case Direction.Up: return new Vector3Int(0, 1, 0);
			case Direction.East: return new Vector3Int(1, 0, 0);
			case Direction.South: return new Vector3Int(0, 0, -1);
			case Direction.Down: return new Vector3Int(0, -1, 0);
			case Direction.West: return new Vector3Int(-1, 0, 0);

			default: return Vector3Int.zero;
		}
	}
}
