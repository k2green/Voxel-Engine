using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PeakNoise", menuName = "Noise Filters/Peak Noise")]
public class PeakNoise : NoiseFilter {

	private float weight;

	public override float EvaluateNoise(Vector2 position) {
		weight = 1;

		return base.EvaluateNoise(position);
	}

	protected override float Evaluate(Vector2 point) {
		float perlin = Mathf.PerlinNoise(point.x, point.y) * 2 - 1;
		float peaks = 1 - Mathf.Abs(perlin);
		float value = peaks * peaks * weight;

		weight = value;

		return value;
	}
}
