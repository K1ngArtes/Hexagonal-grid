using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Representation of the hexagon mesh.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour {

	Mesh _hexMesh;
	List<Vector3> _vertices;
	List<int> _triangles;
	List<Color> _cellColors;
	MeshCollider _meshCollider;

	void Awake()
	{
		GetComponent<MeshFilter>().mesh = _hexMesh = new Mesh();
		_meshCollider = gameObject.AddComponent<MeshCollider>();
		_hexMesh.name = "Hex Mesh";
		_vertices = new List<Vector3>();
		_cellColors = new List<Color>();
		_triangles = new List<int>();
	}

	/// <summary>
	/// Clear all vertices and triangles. Recalculate them again.
	/// </summary>
	/// <param name="cells">Array of cells, which will get recalculated.</param>
	public void Triangulate(HexCell[] cells)
	{
		_hexMesh.Clear();
		_vertices.Clear();
		_triangles.Clear();
		_cellColors.Clear();
		for (int i = 0; i < cells.Length; i++)
		{
			Triangulate(cells[i]);
		}
		_hexMesh.vertices = _vertices.ToArray();
		// Used vertex color. Not sure why.
		_hexMesh.colors = _cellColors.ToArray();
		_hexMesh.triangles = _triangles.ToArray();
		_hexMesh.RecalculateNormals();
		// meshCollider.sharedMesh is the mesh object used for collision detection.
		_meshCollider.sharedMesh = _hexMesh;
	}
	
	/// <summary>
	/// Recalculate single hexagonal cell.
	/// </summary>
	/// <param name="cell">Cell to recalculate.</param>
	void Triangulate(HexCell cell)
	{
		for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
		{
			Triangulate(d, cell);
		}
	}

	/// <summary>
	/// Recalculate single triangle of the cell based on its direction.
	/// </summary>
	/// <param name="direction">Direction of the triangle relative to cell center.</param>
	/// <param name="cell">Cell to recalculate triangle of.</param>
	void Triangulate(HexDirection direction, HexCell cell)
	{
		
		//	  -v3--v4-	
		//	  \|    |/
		//	   v1--v2
		//		\  /   
		//		 \/
		
		// 1) Add main triangle (center, v1, v2)
		Vector3 center = cell.transform.localPosition;
		Vector3 v1 = center + HexMetrics.GetFirstSolidCorner(direction);
		Vector3 v2 = center + HexMetrics.GetSecondSolidCorner(direction);
		AddTriangle(center, v1, v2);
		AddTriangleColor(cell.cellColor);

		// 2) Build a rectangular bridge. If is used so the bridge is built only once.
		// Also build two triangles left
		if(direction <= HexDirection.SE)
		{
			TriangulateConnection(direction, cell, v1, v2);
		}
		
		// Assign color to two last vertices from neighbor cell
		// Each vertex of hexagon is shared by three hexagons
		HexCell prevNeighbor = cell.GetNeighbor(direction.Previous()) ?? cell;
		HexCell neighbor = cell.GetNeighbor(direction) ?? cell;
		HexCell nextNeighbor = cell.GetNeighbor(direction.Next()) ?? cell;
		
		// 3) Fill the gaps left
//		AddTriangle(v1, center + HexMetrics.GetFirstCorner(direction), v3);
//		AddTriangleColor(
//			cell.cellColor,
//			(cell.cellColor + prevNeighbor.cellColor + neighbor.cellColor) / 3f,
//			bridgeColor
//		);
//		
//		AddTriangle(v2, v4, center + HexMetrics.GetSecondCorner(direction));
//		AddTriangleColor(
//			cell.cellColor,
//			bridgeColor,
//			(cell.cellColor + prevNeighbor.cellColor + neighbor.cellColor) / 3f
//		);
	}

	/// <summary>
	/// Create a rectangular bridge from two triangles and complete triangle left.
	/// </summary>
	/// <param name="direction">Direction of the neighbor.</param>
	/// <param name="cell">Current cell.</param>
	/// <param name="v1">First vertex of solid color.</param>
	/// <param name="v2">Second vertex of solid color.</param>
	void TriangulateConnection(HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2)
	{
		HexCell neighbor = cell.GetNeighbor(direction);

		if (neighbor == null)
		{
			return;
		}
		
		Vector3 bridge = HexMetrics.GetBridge(direction);
		Vector3 v3 = v1 + bridge;
		Vector3 v4 = v2 + bridge;
		v3.y = v4.y = neighbor.Elevation * HexMetrics.ElevationStep;

		// Only create terraces for slopes
		if (cell.GetEdgeType(direction) == HexEdgeType.Slope)
		{
			TriangulateEdgeTerraces(v1, v2, cell, v3, v4, neighbor);
		}
		else
		{
			AddQuad(v1, v2, v3, v4);
			AddQuadColor(cell.cellColor, neighbor.cellColor);	
		}

		// Create triangle left
		HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
		// direction <= HexDirection.E is to avoid overlapping triangles
		if (direction <= HexDirection.E && nextNeighbor != null)
		{
			Vector3 v5 = v2 + HexMetrics.GetBridge(direction.Next());
			v5.y = nextNeighbor.Elevation * HexMetrics.ElevationStep;

			if (cell.Elevation <= neighbor.Elevation)
			{
				if (cell.Elevation <= nextNeighbor.Elevation)
				{
					TriangulateCorner(v2, cell, v4, neighbor, v5, nextNeighbor);
				}
				else
				{
					TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
				}
			}
			else if (neighbor.Elevation <= nextNeighbor.Elevation)
			{
				TriangulateCorner(v4, neighbor, v5, nextNeighbor, v2, cell);
			}
			else
			{
				TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
			}
		}
	}

	/// <summary>
	/// Create an edge terrace.
	/// </summary>
	/// <param name="beginLeft">Bottom left vertex of the bridge.</param>
	/// <param name="beginRight">Bottom right vertex of the bridge.</param>
	/// <param name="beginCell">Cell to start slope from.</param>
	/// <param name="endLeft">Top left vertex of the bridge.</param>
	/// <param name="endRight">Top right vertex of the bridge.</param>
	/// <param name="endCell">Cell to end slope on.</param>
	void TriangulateEdgeTerraces(
		Vector3 beginLeft, Vector3 beginRight, HexCell beginCell,
		Vector3 endLeft, Vector3 endRight, HexCell endCell)
	{
		// Create first small slope
		Vector3 v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, 1);
		Vector4 v4 = HexMetrics.TerraceLerp(beginRight, endRight, 1);
		Color c2 = HexMetrics.TerraceLerp(beginCell.cellColor, endCell.cellColor, 1);
		AddQuad(beginLeft, beginRight, v3, v4);
		AddQuadColor(beginCell.cellColor, c2);
		
		// Create all small slopes that are left except the last one
		for(int i = 2; i < HexMetrics.TerraceSteps; i++)
		{
			Vector3 v1 = v3;
			Vector3 v2 = v4;
			Color c1 = c2;
			v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, i);
			v4 = HexMetrics.TerraceLerp(beginRight, endRight, i);
			c2 = HexMetrics.TerraceLerp(beginCell.cellColor, endCell.cellColor, i);
			AddQuad(v1, v2, v3, v4);
			AddQuadColor(c1, c2);
		}
		
		// Create last small slope
		AddQuad(v3, v4, endLeft, endRight);
		AddQuadColor(c2, endCell.cellColor);
	}

	void TriangulateCorner(
		Vector3 bottom, HexCell bottomCell,
		Vector3 left, HexCell leftCell,
		Vector3 right, HexCell rightCell)
	{
		HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
		HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

		if (leftEdgeType == HexEdgeType.Slope)
		{
			if (rightEdgeType == HexEdgeType.Slope)
			{
				TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
				return;
			}
			if (rightEdgeType == HexEdgeType.Flat)
			{
				TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
				return;
			}
			TriangulateCornerTerracesCliff(bottom, bottomCell, left, leftCell, right, rightCell);
			return;
		}
		if (rightEdgeType == HexEdgeType.Slope)
		{
			if (leftEdgeType == HexEdgeType.Flat)
			{
				TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
				return;
			}
		}

		AddTriangle(bottom, left, right);
		AddTriangleColor(bottomCell.cellColor, rightCell.cellColor, leftCell.cellColor);
	}

	void TriangulateCornerTerraces(
		Vector3 begin, HexCell beginCell,
		Vector3 left, HexCell leftCell,
		Vector3 right, HexCell rightCell
	)
	{
		Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
		Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);
		Color c3 = HexMetrics.TerraceLerp(beginCell.cellColor, leftCell.cellColor, 1);
		Color c4 = HexMetrics.TerraceLerp(beginCell.cellColor, rightCell.cellColor, 1);
		
		AddTriangle(begin, v3, v4);
		AddTriangleColor(beginCell.cellColor, c3, c4);
		
		for(int i = 2; i < HexMetrics.TerraceSteps; i++)
		{
			Vector3 v1 = v3;
			Vector3 v2 = v4;
			Color c1 = c3;
			Color c2 = c4;
			v3 = HexMetrics.TerraceLerp(begin, left, i);
			v4 = HexMetrics.TerraceLerp(begin, right, i);
			c3 = HexMetrics.TerraceLerp(beginCell.cellColor, leftCell.cellColor, i);
			c4 = HexMetrics.TerraceLerp(beginCell.cellColor, rightCell.cellColor, i);
			
			AddQuad(v1, v2, v3, v4);
			AddQuadColor(c1, c2, c3, c4);
		}
		
		AddQuad(v3, v4, left, right);
		AddQuadColor(c3, c4, leftCell.cellColor, rightCell.cellColor);
	}

	void TriangulateCornerTerracesCliff(
		Vector3 begin, HexCell beginCell,
		Vector3 left, HexCell leftCell,
		Vector3 right, HexCell rightCell
	)
	{
		float b = 1f / (rightCell.Elevation - beginCell.Elevation);
		Vector3 boundary = Vector3.Lerp(begin, right, b);
		Color boundaryColor = Color.Lerp(beginCell.cellColor, rightCell.cellColor, b);
		
		AddTriangle(begin, left, boundary);
		AddTriangleColor(beginCell.cellColor, leftCell.cellColor, boundaryColor);
	}

	/// <summary>
	/// Create a single triangle.
	/// </summary>
	/// <param name="v1">Position of first vertex.</param>
	/// <param name="v2">Position of second vertex.</param>
	/// <param name="v3">Position of third vertex.</param>
	void AddTriangle (Vector3 v1, Vector3 v2, Vector3 v3) {
		int vertexIndex = _vertices.Count;
//		_vertices.Add(Perturb(v1));
//		_vertices.Add(Perturb(v2));
//		_vertices.Add(Perturb(v3));
		_vertices.Add(v1);
		_vertices.Add(v2);
		_vertices.Add(v3);
		_triangles.Add(vertexIndex);
		_triangles.Add(vertexIndex + 1);
		_triangles.Add(vertexIndex + 2);
	}

	/// <summary>
	/// Create a quad for a color blended region of the triangle.
	/// </summary>
	/// <param name="v1">Position of bottom left vertex.</param>
	/// <param name="v2">Position of bottom right vertex.</param>
	/// <param name="v3">Position of top left vertex.</param>
	/// <param name="v4">Position of top right vertex.</param>
	void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
	{
		int vertexIndex = _vertices.Count;
//		_vertices.Add(Perturb(v1));
//		_vertices.Add(Perturb(v2));
//		_vertices.Add(Perturb(v3));
//		_vertices.Add(Perturb(v4));
		_vertices.Add(v1);
		_vertices.Add(v2);
		_vertices.Add(v3);
		_vertices.Add(v4);
		// First triangle of the quad
		_triangles.Add(vertexIndex);
		_triangles.Add(vertexIndex + 2);
		_triangles.Add(vertexIndex + 1);
		// Second triangle of the quad
		_triangles.Add(vertexIndex + 1);
		_triangles.Add(vertexIndex + 2);
		_triangles.Add(vertexIndex + 3);
	}

	/// <summary>
	/// Add 4 differnet colors to quad vertices.
	/// </summary>
	/// <param name="c1">Color for vertex v1.</param>
	/// <param name="c2">Color for vertex v2.</param>
	/// <param name="c3">Color for vertex v3.</param>
	/// <param name="c4">Color for vertex v4.</param>
	void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
	{
		_cellColors.Add(c1);
		_cellColors.Add(c2);
		_cellColors.Add(c3);
		_cellColors.Add(c4);
	}
	
	/// <summary>
	/// Assign color to triangle vertices based only on cell and its direction neighbor.
	/// </summary>
	/// <param name="c1">Color to paint v1 and v2 with</param>
	/// <param name="c2">Color to paint v3 and v4 with</param>
	void AddQuadColor(Color c1, Color c2)
	{
		_cellColors.Add(c1);
		_cellColors.Add(c1);
		_cellColors.Add(c2);
		_cellColors.Add(c2);
	}

	/// <summary>
	/// Apply the same color to all vertices of the mesh triangle.
	/// </summary>
	/// <param name="color">Color to apply to verteces.</param>
	void AddTriangleColor(Color color)
	{
		_cellColors.Add(color);
		_cellColors.Add(color);
		_cellColors.Add(color);
	}
	
	/// <summary>
	/// Apply 3 different colors to each of the vertexes of the triangular mesh.
	/// </summary>
	/// <param name="c1">Color to apply to first vertex.</param>
	/// <param name="c2">Color to apply to second vertex.</param>
	/// <param name="c3">Color to apply to third vertex.</param>
	void AddTriangleColor(Color c1, Color c2, Color c3)
	{
		_cellColors.Add(c1);
		_cellColors.Add(c2);
		_cellColors.Add(c3);
	}

	Vector3 Perturb(Vector3 vertexPosition)
	{
		Vector4 sample = HexMetrics.SampleNoise(vertexPosition);
		// * 2f - 1f is used so that perturbation will go both ways
		vertexPosition.x += (sample.x * 2f - 1f) * HexMetrics.cellPerturbStrength;
		vertexPosition.y += (sample.y * 2f - 1f) * HexMetrics.cellPerturbStrength;
		vertexPosition.z += (sample.z * 2f - 1f) * HexMetrics.cellPerturbStrength;
		return vertexPosition;
	}
}
