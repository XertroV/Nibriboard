﻿using System;
using System.Configuration;
namespace Nibriboard.RippleSpace
{
	/// <summary>
	/// Represents a location in absolute plane-space.
	/// </summary>
	public class LocationReference : Reference
	{
		public ChunkReference ContainingChunk {
			get {
				return new ChunkReference(
					Plane,
					X / Plane.ChunkSize,
					Y / Plane.ChunkSize
				);
			}
		}
		public LocationReference(Plane inPlane, int inX, int inY) : base(inPlane, inX, inY)
		{
			
		}

		public override bool Equals(object obj)
		{
			ChunkReference otherChunkReference = obj as ChunkReference;
			if (otherChunkReference == null)
				return false;
			
			if(X == otherChunkReference.X && Y == otherChunkReference.Y &&
			   Plane == otherChunkReference.Plane)
			{
				return true;
			}
			return false;
		}

		public override string ToString()
		{
			return $"LocationReference: {base.ToString()}";
		}

		public static LocationReference Parse(Plane plane, string source)
		{
			// TODO: Decide if this is the format that we want to use for location references
			if (!source.StartsWith("LocationReference:"))
				throw new InvalidDataException($"Error: That isn't a valid location reference. Location references start with 'ChunkReference:'.");

			// Trim the extras off the reference
			source = source.Substring("LocationReference:".Length);
			source = source.Trim("() \v\t\r\n".ToCharArray());

			int x = source.Substring(0, source.IndexOf(","));
			int y = source.Substring(source.IndexOf(",") + 1);
			return new LocationReference(
				plane,
				x,
				y
			);
		}
	}
}
