using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseSettings {
	[Range(1, 8)]
	public int numberOfLayers = 1;
	public float strength = 1;
	public float baseScale = 1;
	public float scaleFactor = 2;
	public float persistance = .5f;
	public Vector2 offset;
}
