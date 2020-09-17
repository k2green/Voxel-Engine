using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseSettings {
	[Header("Global Settings")]
	public float strength = 1;
	public float heightOffset = 0;
	public float scale = 50;
	public Vector2 offset;

	[Header("Layer Settings")]
	[Range(1, 8)]
	public int numberOfLayers = 1;

	public float baseFrequency = 1;
	public float frequencyScale = 2;
	public float persistance = .5f;

	public bool clipNegative = false;
}
