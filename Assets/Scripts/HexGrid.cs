using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// Representation of hexagonal grid.
/// </summary>
public class HexGrid : MonoBehaviour {

	/// <summary>
	/// Number of hexagons on X-axis.
	/// </summary>
	public int width = 6;
	/// <summary>
	/// Number of hexagons on Z-axis.
	/// </summary>
	public int height = 6;
	/// <summary>
	/// Text prefab, which writes coordinates of the cell on top of it.
	/// </summary>
	public Text cellLabelPrefab;
	/// <summary>
	/// Prefab of the cell. In this case generated mesh of the single hexagonal cell.
	/// </summary>
	public HexCell cellPrefab;
	/// <summary>
	/// Default color of the cell.
	/// </summary>
	public Color defaultCellColor = Color.white;

	public Texture2D noiseSource;

	Canvas _gridCanvas;
	HexMesh _hexMesh;
	HexCell[] _cells;

	void Awake()
	{
		HexMetrics.noiseSource = noiseSource;
		_gridCanvas = GetComponentInChildren<Canvas>();
		_hexMesh = GetComponentInChildren<HexMesh>();

		_cells = new HexCell[height * width];

		for(int z = 0, i = 0; z < height; z++)
		{
			for(int x = 0; x < width; x++)
			{
				CreateCell(x, z, i++);
			}
		}
	}

	void Start()
	{
		_hexMesh.Triangulate(_cells);
	}

	private void OnEnable()
	{
		HexMetrics.noiseSource = noiseSource;
	}

	/// <summary>
	/// Triangulate the mesh.
	/// </summary>
	public void Refresh()
	{
		_hexMesh.Triangulate(_cells);
	}
	
	/// <summary>
	/// Get the cell at given position in the world.
	/// </summary>
	/// <param name="positionHit">Position where raycast hit the cell.</param>
	/// <returns>Cell being hit.</returns>
	public HexCell GetCell(Vector3 positionHit)
	{
		// Transform hit position from world space to local space
		positionHit = transform.InverseTransformPoint(positionHit);
		// Obtain the coordinates of the cell being hit
		HexCoordinates cellHitCoordinates = HexCoordinates.FromPositionHitToCellCoordinates(positionHit);
		int hitCellIndex = cellHitCoordinates.X + cellHitCoordinates.Z * width + cellHitCoordinates.Z / 2;
		return _cells[hitCellIndex];
	}

	/// <summary>
	/// Create hexagonal cell at certain positions.
	/// </summary>
	/// <param name="x">Number of hexgon on x-axis.</param>
	/// <param name="z">Number of hexgon on z-axis.</param>
	/// <param name="i">Index of the cell being created.</param>
	void CreateCell(int x, int z, int i)
	{
		// Position of the hex center realtive to the grid
		Vector3 position;

		// z/2 so that each consecutive row shifts to the right, -z/2 is integer division,
		// so that each even row is shifted back to its original place
		position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
		position.y = 0f;
		position.z = z * (HexMetrics.outerRadius * 1.5f);

		// Instantiate the cell an set its position
		HexCell cell = _cells[i] = Instantiate<HexCell>(cellPrefab);
		// Make this cell a child of the grid
		cell.transform.SetParent(transform, false);
		// Positions transform relative to the parent
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
		cell.cellColor = defaultCellColor;

		if (x > 0)
		{
			cell.SetNeighbor(HexDirection.W, _cells[i - 1]);
		}
		
		if (z > 0)
		{
			// Bitwise AND. This is one of the ways to check if number is even. Here binary AND is used.
			// If result yielded is zero, then number is even.
			if ((z & 1) == 0)
			{
				cell.SetNeighbor(HexDirection.SE, _cells[i - width]);
				if (x > 0)
				{
					cell.SetNeighbor(HexDirection.SW, _cells[i - width - 1]);
				}
			}
			else
			{
				cell.SetNeighbor(HexDirection.SW, _cells[i - width]);
				if (x < width - 1)
				{
					cell.SetNeighbor(HexDirection.SE, _cells[i - width + 1]);
				}
			}
		}
		
		// Write the coordinates of the cell as GUI
		Text label = Instantiate<Text>(cellLabelPrefab);
		label.rectTransform.SetParent(_gridCanvas.transform, false);
		label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
		label.text = cell.coordinates.ToStringOnSeparateLines();
		cell.uiRect = label.rectTransform;
	}
}
