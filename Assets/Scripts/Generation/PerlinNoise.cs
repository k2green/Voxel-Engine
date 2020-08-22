using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="PerlinNoise", menuName ="Noise Filters/Perlin Noise", order = 1)]
public class PerlinNoise : NoiseFilter {
	protected override float Evaluate(Vector2 point) => Mathf.Clamp01(Mathf.PerlinNoise(point.x, point.y));
}
