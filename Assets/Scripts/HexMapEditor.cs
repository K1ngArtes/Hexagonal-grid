using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Helps to click on cells and paint them accordingly.
/// </summary>
public class HexMapEditor : MonoBehaviour
{
	/// <summary>
	/// Colors to paint cells with.
	/// </summary>
	public Color[] colors;

	/// <summary>
	/// Hexagonal grid to act upon.
	/// </summary>
	public HexGrid hexGrid;

	/// <summary>
	/// Current active color in the editor.
	/// </summary>
	private Color _activeColor;

	/// <summary>
	/// Current active elevation in the editor.
	/// </summary>
	private int _activeElevation;

	void Awake()
	{
		SelectColor(0);
	}

	void Update ()
	{
		// Check if Mouse is over the panel, not the grid
		if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
		{
			HandleInput();
		}
	}

	/// <summary>
	/// Finds point of collision with cell.
	/// </summary>
	void HandleInput ()
	{
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit))
		{
			EditCell(hexGrid.GetCell(hit.point));
		}
	}

	/// <summary>
	/// Set color and elevation of the cell.
	/// </summary>
	/// <param name="cell">Cell to edit.</param>
	private void EditCell(HexCell cell)
	{
		cell.cellColor = _activeColor;
		cell.Elevation = _activeElevation;
		hexGrid.Refresh();
	}


	/// <summary>
	/// Select color to paint cell with.
	/// </summary>
	/// <param name="index">Index of the color.</param>
	public void SelectColor(int index)
	{
		_activeColor = colors[index];
	}

	/// <summary>
	/// Select the elevation for the cell.
	/// </summary>
	/// <param name="elevation">Elevation to set for the cell.</param>
	public void SetElevation(float elevation)
	{
		Debug.Log("Current active elevation is " + _activeElevation);
		_activeElevation = (int) elevation;
		Debug.Log("Active elevation got changed to " + _activeElevation);
	}
}
