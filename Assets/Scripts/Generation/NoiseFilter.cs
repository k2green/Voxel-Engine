using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NoiseFilter : ScriptableObject {

	public NoiseSettings settings;

	public float EvaluateNoise(Vector2 position) {
		float noiseVal = 0;
		float frequency = settings.baseScale;
		float amplitude = 1;

		for(int i = 0; i < settings.numberOfLayers; i++) {
			float val = Evaluate(position * frequency + settings.offset);
			noiseVal += val * amplitude;

			frequency *= settings.scaleFactor;
			amplitude *= settings.persistance;
		}

		return noiseVal * settings.strength;
	}

	protected abstract float Evaluate(Vector2 point);
}
