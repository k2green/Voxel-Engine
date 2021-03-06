﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Voxel {

	public byte R { get; }
	public byte G { get; }
	public byte B { get; }
	public byte A { get; }

	public bool IsVisible => A > 0;
	public bool IsTransparent => A < byte.MaxValue;
	public bool IsSolid => !IsTransparent;

	public Voxel(byte r, byte g, byte b, byte a = byte.MaxValue) {
		R = r;
		G = g;
		B = b;
		A = a;
	}

	public IEnumerable<byte> ToBytes() => new byte[] { R, G, B, A };

	public Voxel(Color32 color) : this(color.r, color.g, color.b, color.a) { }

	public Color32 GetColor() => new Color32(R, G, B, A);
}
