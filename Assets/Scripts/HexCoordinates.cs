using UnityEngine;

/// <summary>
/// Representation of hexagonal coordinate system (x, y, z).
/// </summary>
[System.Serializable]
// Convert any coordinate system to hex coordinate system
public struct HexCoordinates
{
	[SerializeField]
	private int x, z;

	public int X
	{
		get
		{
			return x;
		}
	}
	public int Z
	{
		get
		{
			return z;
		}
	}
	public int Y
	{
		// X, Y and Z have to add up to one
		get { return -X - Z; }
	}

	public HexCoordinates (int x, int z)
	{
		this.x = x;
		this.z = z;
	}

	/// <summary>
	/// Converts offset coordinates to hexagonal coordinates.
	/// </summary>
	/// <param name="offsetX">X offset coordinate.</param>
	/// <param name="offsetZ">Z offset coordinate.</param>
	/// <returns>HexCoordinates of the cell</returns>
	public static HexCoordinates FromOffsetCoordinates(int offsetX, int offsetZ)
	{
		return new HexCoordinates(offsetX - offsetZ / 2, offsetZ);
	}

	/// <summary>
	/// Print hexagonal coordinates in form (x, y, z).
	/// </summary>
	/// <returns>String representation of Hex Coordinate.</returns>
	public override string ToString ()
	{
		return "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
	}

	/// <summary>
	/// Print hexagonal coordinates in form x + "\n" + y + "\n" + z.
	/// </summary>
	/// <returns>String representation of Hex Coordinate.</returns>
	public string ToStringOnSeparateLines()
	{
		return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
	}

	/// <summary>
	/// Point hit by the ray (local space) to Hex Coordinates.
	/// </summary>
	/// <param name="positionToTransform">Coordinate to be transformed to hex coordinate.</param>
	/// <returns>Hex Coordinates of the cell being hit.</returns>
	public static HexCoordinates FromPositionHitToCellCoordinates(Vector3 positionToTransform)
	{
		
		// Find x and y as z didn't exist. Produces correct result if Z = 0 
		float x = positionToTransform.x / (HexMetrics.innerRadius * 2f);
		float y = -x;

		// We have to shift as we move along z
		float offset = positionToTransform.z / (HexMetrics.outerRadius * 3f);
		x -= offset;
		y -= offset;

		// Result is needed to be rounded to int
		int xValueOfCell = Mathf.RoundToInt(x);
		int yValueOfCell = Mathf.RoundToInt(y);
		int zValueOfCell = Mathf.RoundToInt(-x - y);

		// Some rounding error occure when clicked too close to edges between hexagons
		// Not sure how it works yet
		if(xValueOfCell + yValueOfCell + zValueOfCell != 0)
		{
			float dX = Mathf.Abs(x - xValueOfCell);
			float dY = Mathf.Abs(y - yValueOfCell);
			float dZ = Mathf.Abs(-x - y - zValueOfCell);

			if(dX > dY && dX > dZ)
			{
				xValueOfCell = -yValueOfCell - zValueOfCell;
			}
			else if(dZ > dY)
			{
				zValueOfCell = -xValueOfCell - yValueOfCell;
			}
		}

		return new HexCoordinates(xValueOfCell, zValueOfCell);
	}
}
