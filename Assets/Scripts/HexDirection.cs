using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Representation of directions relative to the cell.
/// </summary>
public enum HexDirection
{
	NE, E, SE, SW, W, NW
}

/// <summary>
/// Set of useful functions on directions.
/// </summary>
public static class HexDirectionExtensions
{
	/// <summary>
	/// Finds opposite direction to provided one.
	/// </summary>
	/// <param name="direction">Direction to find opposite of.</param>
	/// <returns>Opposite direction of provided one.</returns>
	public static HexDirection Opposite(this HexDirection direction)
	{
		return (int) direction < 3 ? (direction + 3) : (direction - 3);
	}

	/// <summary>
	/// Returns previous (anticlockwise) direction to current one.
	/// </summary>
	/// <param name="direction">Current direction.</param>
	/// <returns>Previous direction.</returns>
	public static HexDirection Previous(this HexDirection direction)
	{
		return direction == HexDirection.NE ? HexDirection.NW : (direction - 1);
	}

	/// <summary>
	/// Returns next (clockwise) direction to current one.
	/// </summary>
	/// <param name="direction">Current direction.</param>
	/// <returns>Next direction.</returns>
	public static HexDirection Next(this HexDirection direction)
	{
		return direction == HexDirection.NW ? HexDirection.NE : (direction + 1);
	}
}