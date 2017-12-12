using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Representation of the single hexagonal cell.
/// </summary>
public class HexCell : MonoBehaviour
{
	/// <summary>
	/// Hexagonal coordinates of the cell.
	/// </summary>
    public HexCoordinates coordinates;

	/// <summary>
	/// Color this cell is painted with.
	/// </summary>
	public Color cellColor;

	/// <summary>
	/// Rectangular transform of UI label of coordinates.
	/// </summary>
	public RectTransform uiRect;

	/// <summary>
	/// Elevation of the cell.
	/// </summary>
	private int _elevation;
	public int Elevation
	{
		get { return _elevation; }
		set
		{
			_elevation = value;
			Vector3 position = transform.localPosition;
			position.y = value * HexMetrics.ElevationStep;
			transform.localPosition = position;

			// Set position of coordinate labels
			Vector3 uiPosition = uiRect.localPosition;
			uiPosition.z = _elevation * -HexMetrics.ElevationStep;
			uiRect.localPosition = uiPosition;
		}
	}
	
	/// <summary>
	/// Neighbors of the cell.
	/// </summary>
	[SerializeField]
	private HexCell[] neighbors;

	/// <summary>
	/// Retrieve neighbor of current cell at certain direction.
	/// </summary>
	/// <param name="direction">Direction of neighbor relative to current cell.</param>
	/// <returns>Neighbor of cell.</returns>
	public HexCell GetNeighbor(HexDirection direction)
	{
		return neighbors[(int)direction];
	}

	/// <summary>
	/// Set neighbor of current cell at specified direction.
	/// </summary>
	/// <param name="direction">Direction of the neighbor relative to current cell.</param>
	/// <param name="cell">Neighbor to set.</param>
	public void SetNeighbor(HexDirection direction, HexCell cell)
	{
		neighbors[(int) direction] = cell;
		cell.neighbors[(int) direction.Opposite()] = this;
	}

	/// <summary>
	/// Find type of edge at particular direction.
	/// </summary>
	/// <param name="direction">Direction of the edge.</param>
	/// <returns>Type of the edge at this direction.</returns>
	public HexEdgeType GetEdgeType(HexDirection direction)
	{
		return HexMetrics.GetEdgeType(_elevation, neighbors[(int) direction]._elevation);
	}

	public HexEdgeType GetEdgeType(HexCell otherCell)
	{
		return HexMetrics.GetEdgeType(_elevation, otherCell._elevation);
	}
}
