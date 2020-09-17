using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NoiseFilter : ScriptableObject {

	public NoiseSettings settings;

	public virtual float EvaluateNoise(Vector2 position, bool useStrength = true) {
		float noiseVal = 0;
		float frequency = settings.baseFrequency;
		float amplitude = 1;

		for (int i = 0; i < settings.numberOfLayers; i++) {
			float val = Evaluate(position / settings.scale * frequency + settings.offset);

			noiseVal += val * amplitude;
			frequency *= settings.frequencyScale;
			amplitude *= settings.persistance;
		}

		float outputVal = noiseVal - settings.heightOffset;

		if (settings.clipNegative)
			outputVal = Mathf.Max(0, outputVal);

		if (useStrength)
			outputVal += settings.strength;

		return outputVal;
	}

	protected abstract float Evaluate(Vector2 point);
}
