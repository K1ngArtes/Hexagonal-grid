using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Holds basic parameters of hexagonal cell.
/// </summary>
public static class HexMetrics
{
	public const float cellPerturbStrength = 5f;

	public const float noiseScale = 0.003f;
	
	/// <summary>
	/// Horizontal distance from center of the hex to its corner.
	/// </summary>
	public const float outerRadius = 10f;

	/// <summary>
	/// Number of terraces per single slope.
	/// </summary>
	public const int TerracesPerSlop = 2;

	/// <summary>
	/// Number of smaller slopes when big slope is divided.
	/// </summary>
	public const int TerraceSteps = TerracesPerSlop * 2 + 1;

	/// <summary>
	/// Length of a single step.
	/// </summary>
	public const float HorizontalTerraceStepSize = 1f / TerraceSteps;
	
	/// <summary>
	/// Hight of a single step.
	/// </summary>
	public const float VerticalTerraceStepsize = 1f / (TerracesPerSlop + 1);
	
	/// <summary>
	/// Representation of how high each elevation step has to be.
	/// </summary>
	public const float ElevationStep = 5f;
	
	/// <summary>
	/// Distance from center of the hex to its edge.
	/// </summary>
	public const float innerRadius = outerRadius * 0.866025404f;

	/// <summary>
	/// Percentage of solid color regon of the hexagon.
	/// </summary>
	public const float SolidFactor = 0.75f;

	/// <summary>
	/// Percentage of not solid, blended region of the hexagon (close to edges).
	/// </summary>
	public const float BlendFactor = 1f - SolidFactor;

	/// <summary>
	/// 6 corners of the hexagon with the 7th being the same as the first
	/// for easier mesh triangles creation.
	/// </summary>
	public static Vector3[] corners =
	{
		new Vector3(0f, 0f, outerRadius),
		new Vector3(innerRadius, 0f, 0.5f * outerRadius),
		new Vector3(innerRadius, 0f, -0.5f * outerRadius),
		new Vector3(0f, 0f, -outerRadius),
		new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
		new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
		new Vector3(0f, 0f, outerRadius)
	};

	public static Texture2D noiseSource;

	/// <summary>
	/// Helps to find position of second vertex of the triangle.
	/// </summary>
	/// <param name="direction">Direction of the triangle to find vertex of.</param>
	/// <returns>Position of second vertex of the triangle.</returns>
	public static Vector3 GetFirstCorner(HexDirection direction)
	{
		return corners[(int)direction];
	}

	/// <summary>
	/// Helps to find position of third vertex of the triangle.
	/// </summary>
	/// <param name="direction">Direction of the triangle to find vertex of.</param>
	/// <returns>Position of third vertex of the triangle.</returns>
	public static Vector3 GetSecondCorner(HexDirection direction)
	{
		return corners[(int)direction + 1];
	}

	/// <summary>
	/// Retrieve first corner of the solid Hexagon at specified position.
	/// </summary>
	/// <param name="direction">Direction of the triangle to find corners of.</param>
	/// <returns>Position of the corner of inner hexagon.</returns>
	public static Vector3 GetFirstSolidCorner(HexDirection direction)
	{
		return corners[(int) direction] * SolidFactor;
	}

	/// <summary>
	/// Retrieve second corner of the solid Hexagon at specified position.
	/// </summary>
	/// <param name="direction">Direction of the triangle to find corners of.</param>
	/// <returns>Position of the corner of inner hexagon.</returns>
	public static Vector3 GetSecondSolidCorner(HexDirection direction)
	{
		return corners[(int) direction + 1] * SolidFactor;
	}

	/// <summary>
	/// Bridge is the vector in direction from v1 to v3 or from v2 to v4 in mesh triangle.
	/// </summary>
	/// <param name="direction">Direction of the triangle.</param>
	/// <returns>Bridge vector.</returns>
	public static Vector3 GetBridge(HexDirection direction)
	{
		return (corners[(int) direction] + corners[(int) direction + 1]) *
		        BlendFactor;
	}

	/// <summary>
	/// Find position of top left or top right vertex of single edge step quad.
	/// </summary>
	/// <param name="a">Vertex to start vector from.</param>
	/// <param name="b">Vertex to aim towards.</param>
	/// <param name="step">Which step on the terrace to find.</param>
	/// <returns></returns>
	public static Vector3 TerraceLerp(Vector3 a, Vector3 b, int step)
	{
		// Interpolation is (1 - t)a + t*b = a + t(b - a) which is a movement from a to b along (b - a)
		float horizontalStep = step * HorizontalTerraceStepSize;
		a.x += (b.x - a.x) * horizontalStep;
		a.z += (b.z - a.z) * horizontalStep;

		// (step + 1) / 2 integer division
		float verticalStep = ((step + 1) / 2) * VerticalTerraceStepsize;
		a.y += (b.y - a.y) * verticalStep;
		return a;
	}
	
	/// <summary>
	/// Interpolate between two colors from two points and step on a slope.
	/// </summary>
	/// <param name="a">First vertex.</param>
	/// <param name="b">Second vertex.</param>
	/// <param name="step">Number of the step on the slope.</param>
	/// <returns></returns>
	public static Color TerraceLerp(Color a, Color b, int step)
	{
		float horizontalStepSize = step * HorizontalTerraceStepSize;
		return Color.Lerp(a, b, horizontalStepSize);
	}

	public static Vector4 SampleNoise(Vector3 position)
	{
		return noiseSource.GetPixelBilinear(position.x * noiseScale, position.z * noiseScale);
	}

	/// <summary>
	/// Return type of the edge.
	/// </summary>
	/// <param name="elevation1">Elevation of the first cell.</param>
	/// <param name="elevation2">Elevation of the second cell.</param>
	/// <returns>Type of edge between two cells.</returns>
	public static HexEdgeType GetEdgeType(int elevation1, int elevation2)
	{
		if (elevation1 == elevation2)
		{
			return HexEdgeType.Flat;
		}
		int delta = elevation2 - elevation1;
		if (delta == 1 || delta == -1)
		{
			return HexEdgeType.Slope;
		}
		return HexEdgeType.Cliff;
	}
}
