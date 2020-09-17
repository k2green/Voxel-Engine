using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Math {

	public static int NFMod(int a, int b) {
		return a - b * Div(a, b);
	}

	public static Vector2Int NFMod(Vector2Int a, Vector2Int b) => new Vector2Int(NFMod(a.x, b.x), NFMod(a.y, b.y));

	public static int Div(float a, float b) => Mathf.FloorToInt(a / b);
	public static Vector2Int Div(Vector2 vector, float b) => new Vector2Int(Div(vector.x, b), Div(vector.y, b));
	public static Vector2Int Div(Vector2 a, Vector2 b) => new Vector2Int(Div(a.x, b.x), Div(a.y, b.y));
	public static Vector2Int Div(Vector2 a, Vector3 b) => Div(a, new Vector2(b.x, b.z));
}
