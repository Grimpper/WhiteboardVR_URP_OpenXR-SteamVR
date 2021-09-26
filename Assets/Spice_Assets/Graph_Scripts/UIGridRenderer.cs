using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class UIGridRenderer : Graphic
{
    [Header("Grid properties")]
    [SerializeField] private Vector2Int gridSize = new Vector2Int(1, 1);
    public float thickness = 1.5f;
    [Range(0, 10)] public int horizontalDashes = 2;
    [Range(0, 10)] public int verticalDashes = 2;
    [SerializeField] private bool equalDashes = false;
    

    enum CellType { Solid, Dashed }
    [Header("Cell properties")]
    [SerializeField] private CellType cellType = CellType.Solid;

    private delegate void Function(int x, int y, int index, VertexHelper vh);

    private Function[] functions;

    private const int SolidCellVertices = 8;
    private const int CornerVertices = 24;
    private const int DashVertices = 4;

    [Space]
    [SerializeField] private bool debug = false;

    public Vector2Int GridSize
    {
        set
        {
            gridSize = value;
            UpdateGeometry();
        }
    }

    private float width;
    private float height;
    private float cellWidth;
    private float cellHeight;

    private float distance;
    private float horizontalDashWidth;
    private float verticalDashWidth;

    private Function GetFunction() => functions[(int) cellType];
    
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        functions = new Function[] { DrawCell, DrawDashedCell };

        Function drawFunction = GetFunction();
        
        vh.Clear();
        
        width = rectTransform.rect.width;
        height = rectTransform.rect.height;

        cellWidth = width / gridSize.x;
        cellHeight = height / gridSize.y;

        int cellIndex = 0;
        for (int rowIndex = 0; rowIndex < gridSize.y; rowIndex++)
        {
            for (int columnIndex = 0; columnIndex < gridSize.x; columnIndex++)
            {
                drawFunction(columnIndex, rowIndex, cellIndex, vh);
                cellIndex++;
            }
        }
    }

    private void DrawCell(int columnIndex, int rowIndex, int cellIndex, VertexHelper vh)
    {
        float xPos = cellWidth * columnIndex;
        float yPos = cellHeight * rowIndex;
        
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = debug ? new Color32(0, 255, 0, 255) : (Color32)color;
        
        distance = thickness / Mathf.Sqrt(2f);

        AddCornerVertices(ref vertex, ref vh, xPos, yPos);
        AddInternalCornerVertices(ref vertex, ref vh, xPos, yPos);
        AddCellTriangles(ref vh, cellIndex);
    }
    
    /// <summary>
    /// Draw the dashed borders of a cell given a column and a row.
    /// In horizontalDashWidth and verticalDashWidth the dash count is multiplied by 2 and then added 2
    /// because it ends and starts with half a dash to form the corners of the cross. So 1 is added for the half lines
    /// and the other one is added since the blank spaces take the space of a dash and there is always 1 more than the
    /// dashes. In the draw below we can se 3 dashes + 2 half dashes in the crosses + 4 blanks which adds up to 8
    /// + - - - +  
    /// </summary>
    /// <param name="columnIndex">Horizontal index of cell in a 2d grid</param>
    /// <param name="rowIndex">Vertical index of cell in a 2d grid</param>
    /// <param name="cellIndex">Current cell being drawn</param>
    /// <param name="vh">Array that holds all the vertices</param>
    private void DrawDashedCell(int columnIndex, int rowIndex, int cellIndex, VertexHelper vh)
    {
        float xPos = cellWidth * columnIndex;
        float yPos = cellHeight * rowIndex;
        
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = debug ? new Color32(0, 255, 0, 255) : (Color32)color;
        
        distance = thickness / Mathf.Sqrt(2f);
        
        horizontalDashWidth = cellWidth / (horizontalDashes * 2 + 2);

        if (equalDashes) 
            verticalDashes = RecalculateDashCount(horizontalDashWidth, cellHeight);
        
        verticalDashWidth = cellHeight / (verticalDashes * 2 + 2);

        AddCornerVertices(ref vertex, ref vh, xPos, yPos);
        AddInternalCornerVertices(ref vertex, ref vh, xPos, yPos);
        AddMiddleVertices(ref vertex, ref vh, xPos, yPos);
        AddInternalMiddleVertices(ref vertex, ref vh, xPos, yPos);
        AddDashesVertices(ref vertex, ref vh, xPos, yPos);

        AddCornerCellTriangles(ref vh, cellIndex);
        AddDashesTriangles(ref vh, cellIndex);
    }

    private int RecalculateDashCount(float dashWidth, float dashContainerSize) 
        => Mathf.CeilToInt((dashContainerSize / dashWidth - 2) / 2);

    private void AddCornerVertices(ref UIVertex vertex, ref VertexHelper vh, float xPos, float yPos)
    {
        vertex.position = new Vector3(xPos, yPos);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos, yPos + cellHeight);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + cellWidth, yPos + cellHeight);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + cellWidth, yPos);
        vh.AddVert(vertex);
    }

    private void AddInternalCornerVertices(ref UIVertex vertex, ref VertexHelper vh, float xPos, float yPos)
    {
        
        vertex.position = new Vector3(xPos + distance, yPos + distance);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + distance, yPos + cellHeight - distance);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + cellWidth - distance, yPos + cellHeight - distance);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + cellWidth - distance, yPos + distance);
        vh.AddVert(vertex);
    }
    
    private void AddMiddleVertices(ref UIVertex vertex, ref VertexHelper vh, float xPos, float yPos)
    {
        vertex.position = new Vector3(xPos, yPos + verticalDashWidth / 2);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos, yPos + cellHeight - verticalDashWidth / 2);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + horizontalDashWidth / 2, yPos + cellHeight);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + cellWidth - horizontalDashWidth / 2, yPos + cellHeight);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos + cellWidth, yPos + cellHeight - verticalDashWidth / 2);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos + cellWidth, yPos + verticalDashWidth / 2);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos + cellWidth - horizontalDashWidth / 2, yPos);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos + horizontalDashWidth / 2, yPos);
        vh.AddVert(vertex);
    }
    
    private void AddInternalMiddleVertices(ref UIVertex vertex, ref VertexHelper vh, float xPos, float yPos)
    {
        vertex.position = new Vector3(xPos + distance, yPos + verticalDashWidth / 2);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + distance, yPos + cellHeight - verticalDashWidth / 2);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + horizontalDashWidth / 2, yPos + cellHeight - distance);
        vh.AddVert(vertex);

        vertex.position = new Vector3(xPos + cellWidth - horizontalDashWidth / 2, yPos + cellHeight - distance);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos + cellWidth - distance, yPos + cellHeight - verticalDashWidth / 2);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos + cellWidth - distance, yPos + verticalDashWidth / 2);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos + cellWidth - horizontalDashWidth / 2, yPos + distance);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xPos + horizontalDashWidth / 2, yPos + distance);
        vh.AddVert(vertex);
    }
    
    private void AddDashesVertices(ref UIVertex vertex, ref VertexHelper vh, float xPos, float yPos)
    {
        for (int i = 0; i < horizontalDashes; i++)
        {
            if (debug) vertex.color = new Color32(0, 0, 255, 255);
            AddHorizontalDashesVertices(ref vertex, ref vh, xPos, yPos, horizontalDashWidth, i);
        }
        
        for (int i = 0; i < verticalDashes; i++)
        {
            if (debug) vertex.color = new Color32( 255, 0, 0, 255);
            AddVerticalDashesVertices(ref vertex, ref vh, xPos, yPos, verticalDashWidth, i);
        }
    }
    
    private void AddHorizontalDashesVertices(ref UIVertex vertex, ref VertexHelper vh, float xPos, float yPos, 
        float dashWidth, float dashIndex)
    {
        float xDashPos = xPos + horizontalDashWidth / 2 + horizontalDashWidth + horizontalDashWidth * 2 * dashIndex;
        
        // Bottom dashes vertices
        float yDashPosBottom = yPos;
        AddHorizontalDashVertices(ref vertex, ref vh, xDashPos, yDashPosBottom, dashWidth);
        
        // Top dashes vertices
        float yDashPosTop = yPos + cellHeight - distance;
        AddHorizontalDashVertices(ref vertex, ref vh, xDashPos, yDashPosTop, dashWidth);
    }

    private void AddHorizontalDashVertices(ref UIVertex vertex, ref VertexHelper vh, float xDashPos, float yDashPos, 
        float dashWidth)
    {
        vertex.position = new Vector3(xDashPos, yDashPos);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xDashPos, yDashPos + distance);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xDashPos + dashWidth, yDashPos + distance);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xDashPos + dashWidth, yDashPos);
        vh.AddVert(vertex);
    }

    private void AddVerticalDashesVertices(ref UIVertex vertex, ref VertexHelper vh, float xPos, float yPos, 
        float dashWidth, float dashIndex)
    {
        float yDashPos = yPos + verticalDashWidth / 2 + verticalDashWidth + verticalDashWidth * 2 * dashIndex;
        
        // Left dashes vertices
        float xDashPosLeft = xPos;
        AddVerticalDashVertices(ref vertex, ref vh, xDashPosLeft, yDashPos, dashWidth);
        
        // Right dashes vertices
        float xDashPosRight = xPos + cellWidth - distance;
        AddVerticalDashVertices(ref vertex, ref vh, xDashPosRight, yDashPos, dashWidth);
    }
    
    private void AddVerticalDashVertices(ref UIVertex vertex, ref VertexHelper vh, float xDashPos, float yDashPos, 
        float dashWidth)
    {
        vertex.position = new Vector3(xDashPos, yDashPos);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xDashPos, yDashPos + dashWidth);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xDashPos + distance, yDashPos + dashWidth);
        vh.AddVert(vertex);
        
        vertex.position = new Vector3(xDashPos + distance, yDashPos);
        vh.AddVert(vertex);
    }

    private void AddCornerCellTriangles(ref VertexHelper vh, int index)
    {
        int cellOffset = index * (CornerVertices + DashVertices * horizontalDashes * 2 + DashVertices * verticalDashes * 2);
        
        // Left bottom Corner
        vh.AddTriangle(cellOffset + 15, cellOffset + 4, cellOffset + 23);
        vh.AddTriangle(cellOffset + 0, cellOffset + 4, cellOffset + 15);
        vh.AddTriangle(cellOffset + 0, cellOffset + 8, cellOffset + 4);
        vh.AddTriangle(cellOffset + 8, cellOffset + 16, cellOffset + 4);
        
        // Left top Corner
        vh.AddTriangle(cellOffset + 9, cellOffset + 5, cellOffset + 17);
        vh.AddTriangle(cellOffset + 9, cellOffset + 1, cellOffset + 5);
        vh.AddTriangle(cellOffset + 1, cellOffset + 10, cellOffset + 5);
        vh.AddTriangle(cellOffset + 5, cellOffset + 10, cellOffset + 18);
        
        // Right top Corner
        vh.AddTriangle(cellOffset + 19, cellOffset + 11, cellOffset + 6);
        vh.AddTriangle(cellOffset + 11, cellOffset + 2, cellOffset + 6);
        vh.AddTriangle(cellOffset + 2, cellOffset + 12, cellOffset + 6);
        vh.AddTriangle(cellOffset + 20, cellOffset + 6, cellOffset + 12);
        
        // Right bottom Corner
        vh.AddTriangle(cellOffset + 21, cellOffset + 13, cellOffset + 7);
        vh.AddTriangle(cellOffset + 13, cellOffset + 3, cellOffset + 7);
        vh.AddTriangle(cellOffset + 7, cellOffset + 3, cellOffset + 14);
        vh.AddTriangle(cellOffset + 14, cellOffset + 22, cellOffset + 7);
    }
    
    private void AddDashesTriangles(ref VertexHelper vh, int cellIndex)
    {
        int cellOffset = cellIndex * (CornerVertices + DashVertices * horizontalDashes * 2 + DashVertices * verticalDashes * 2);

        for (int dashIndex = 0; dashIndex < horizontalDashes; dashIndex++)
        {
            int dashOffset = dashIndex * DashVertices;
            
            // Bottom dashes
            int bottomDashesStart = cellOffset + CornerVertices;
            AddDashTriangle(ref vh, dashOffset, bottomDashesStart);

            // Top dashes
            int topDashesStart = bottomDashesStart + DashVertices * horizontalDashes;
            AddDashTriangle(ref vh, dashOffset, topDashesStart);
        }
        
        for (int dashIndex = 0; dashIndex < verticalDashes; dashIndex++)
        {
            int dashOffset = dashIndex * DashVertices;
            
            // Left dashes
            int leftDashesStart = cellOffset + CornerVertices + DashVertices * horizontalDashes * 2;
            AddDashTriangle(ref vh, dashOffset, leftDashesStart);
        
            // Right dashes
            int rightDashesStart = leftDashesStart + DashVertices * verticalDashes;
            AddDashTriangle(ref vh, dashOffset, rightDashesStart);
        }
    }

    private void AddDashTriangle(ref VertexHelper vh, int dashOffset, int dashStart)
    {
        vh.AddTriangle(dashStart + dashOffset + 0, dashStart + dashOffset + 1, dashStart + dashOffset + 3);
        vh.AddTriangle(dashStart + dashOffset + 1, dashStart + dashOffset + 2, dashStart + dashOffset + 3);
    } 
        
    private void AddCellTriangles(ref VertexHelper vh, int index)
    {
        int cellOffset = index * SolidCellVertices;
        
        // Left Edge
        vh.AddTriangle(cellOffset + 0, cellOffset + 1, cellOffset + 5);
        vh.AddTriangle(cellOffset + 5, cellOffset + 4, cellOffset + 0);
        
        // Top Edge
        vh.AddTriangle(cellOffset + 1, cellOffset + 2, cellOffset + 6);
        vh.AddTriangle(cellOffset + 6, cellOffset + 5, cellOffset + 1);
        
        // Right Edge
        vh.AddTriangle(cellOffset + 2, cellOffset + 3, cellOffset + 7);
        vh.AddTriangle(cellOffset + 7, cellOffset + 6, cellOffset + 2);
        
        // Bottom Edge
        vh.AddTriangle(cellOffset + 3, cellOffset + 0, cellOffset + 4);
        vh.AddTriangle(cellOffset + 4, cellOffset + 7, cellOffset + 3);
    }
}
