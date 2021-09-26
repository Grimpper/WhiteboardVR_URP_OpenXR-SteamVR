using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UILineRenderer : Graphic
{
    enum LineConnection { Intersect, Bisect }
    [SerializeField] private LineConnection lineConnection = LineConnection.Bisect;

    private delegate void Function(ref VertexHelper vh, Vector2 lastPoint, Vector2 point, Vector2 nextPoint);

    private Function[] functions;
    
    private Function GetFunction() => functions[(int) lineConnection];
    
    [SerializeField] private float thickness = 10f;

    [SerializeField] private Vector2Int gridSize;
    [SerializeField] private List<Vector2> points = new List<Vector2>();

    public Vector2Int GridSize
    {
        set
        {
            gridSize = value;
            UpdateGeometry();
        }
    }
    
    public List<Vector2> Points
    {
        set
        {
            points = value;
            UpdateGeometry();
        }
    }

    [SerializeField] private bool debug = false;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        functions = new Function[] { DrawIntersecting, DrawBisecting };

        Function drawFunction = GetFunction();
        
        vh.Clear();
        
        if (points.Count < 2) 
            return;

        DrawPointVerticesAlongNormal(ref vh, points[0], points[1], true);
        
        for (int i = 1; i < points.Count - 1; i++)
        {
            Vector2 lastPoint = points[i - 1];
            Vector2 point = points[i];
            Vector2 nextPoint = points[i + 1];

            drawFunction(ref vh, lastPoint, point, nextPoint);
        }

        DrawPointVerticesAlongNormal(ref vh, points[points.Count - 2], points[points.Count - 1], false);

        int lineCount = points.Count - 1;
        for (int i = 0; i < lineCount; i++)
        {
            int lineVertexOffset = i * 4;
            DrawLine(ref vh, lineVertexOffset);
        }

        int triangleCount = lineCount - 1;
        for (int i = 0; i < triangleCount; i++)
        {
            int triangleVertexOffset = i * 4 + 2;
            DrawTriangle(ref vh, triangleVertexOffset);
        }
    }

    private void DrawIntersecting(ref VertexHelper vh, Vector2 lastPoint, Vector2 point, Vector2 nextPoint)
    {
        NumberUtils.Line lineA = new NumberUtils.Line(lastPoint, point - lastPoint);
        NumberUtils.Line lineAPlus = GetLinePlus(lineA);
        NumberUtils.Line lineAMinus = GetLineMinus(lineA);
            
        NumberUtils.Line lineB = new NumberUtils.Line(point, nextPoint - point);
        NumberUtils.Line lineBPlus = GetLinePlus(lineB);
        NumberUtils.Line lineBMinus = GetLineMinus(lineB);

        Vector2 abPlusIntersection = NumberUtils.LineIntersection(lineAPlus, lineBPlus);
        Vector2 abMinusIntersection = NumberUtils.LineIntersection(lineAMinus, lineBMinus);
        
        if (float.IsNaN(abPlusIntersection.x) || float.IsNaN(abMinusIntersection.x)) 
            DrawPointVerticesAlongNormal(ref vh, point, nextPoint, true);
        else
        {
            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = debug ? Random.ColorHSV() : color;
            
            vertex.position = new Vector3(abPlusIntersection.x, abPlusIntersection.y);
            vh.AddVert(vertex);
            
            vertex.position = new Vector3(abMinusIntersection.x, abMinusIntersection.y);
            vh.AddVert(vertex);
        }
    }

    private void DrawBisecting(ref VertexHelper vh, Vector2 lastPoint, Vector2 point, Vector2 nextPoint)
    {
        NumberUtils.Line lineA = new NumberUtils.Line(lastPoint, point - lastPoint);
        NumberUtils.Line lineB = new NumberUtils.Line(point, nextPoint - point);

        float bisectorAngle = NumberUtils.GetAngleBisectorAngle(lineA.dir, lineB.dir);
        
        if (float.IsNaN(bisectorAngle))
        {
            DrawPointVerticesAlongNormal(ref vh, point, nextPoint, true);
            DrawPointVerticesAlongNormal(ref vh, point, nextPoint, true);
        }
        else
        {
            Vector2 bisectorUnitVector = new Vector2(Mathf.Cos(bisectorAngle), Mathf.Sin(bisectorAngle));
            Vector2 abMinusIntersection = point - thickness / 2 * bisectorUnitVector;

            Vector2 lineAPlusVertex = abMinusIntersection + thickness * lineA.normal;
            Vector2 lineBPlusVertex = abMinusIntersection + thickness * lineB.normal;

            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = debug ? new Color32(255, 0, 0, 255) : (Color32)color;
        
            vertex.position = lineAPlusVertex;
            vh.AddVert(vertex);
        
            vertex.color = debug ? new Color32(0, 255, 0, 255) : (Color32)color;
        
            vertex.position = abMinusIntersection;
            vh.AddVert(vertex);

            vertex.color = debug ? new Color32(0, 0, 255, 255) : (Color32)color;
        
            vertex.position = lineBPlusVertex;
            vh.AddVert(vertex);
        
            vertex.position = abMinusIntersection;
            vh.AddVert(vertex);
        }
        
        
        /*float bisectorAngle = NumberUtils.GetAngleBisectorAngle(lineA.dir, lineB.dir);

        if (float.IsNaN(bisectorAngle)) 
            DrawPointVerticesAlongNormal(ref vh, point, nextPoint, true);
        else
        {
            Vector2 bisectorUnitVector = new Vector2(Mathf.Cos(bisectorAngle), Mathf.Sin(bisectorAngle));

            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = debug ? Random.ColorHSV() : color;

            float xPos = point.x + thickness / 2 * bisectorUnitVector.x;
            float yPos = point.y + thickness / 2 * bisectorUnitVector.y;
            vertex.position = new Vector3(xPos, yPos);
            vh.AddVert(vertex);

            xPos = point.x - thickness / 2 * bisectorUnitVector.x;
            yPos = point.y - thickness / 2 * bisectorUnitVector.y;
            vertex.position = new Vector3(xPos, yPos);
            vh.AddVert(vertex);
        }*/
    }

    private NumberUtils.Line GetLinePlus(NumberUtils.Line line)
    {
        float xPosPlus = line.GetPoint().x + thickness / 2 * line.normal.x;
        float yPosPlus = line.GetPoint().y + thickness / 2 * line.normal.y;
        Vector2 pointPlus = new Vector2(xPosPlus, yPosPlus);
        return new NumberUtils.Line(pointPlus, line.dir);
    }
    
    private NumberUtils.Line GetLineMinus(NumberUtils.Line line)
    {
        float xPosMinus = line.GetPoint().x - thickness / 2 * line.normal.x;
        float yPosMinus = line.GetPoint().y - thickness / 2 * line.normal.y;
        Vector2 pointMinus = new Vector2(xPosMinus, yPosMinus);
        return new NumberUtils.Line(pointMinus, line.dir);
    }

    private void DrawPointVerticesAlongNormal(ref VertexHelper vh, Vector2 pointA, Vector2 pointB, 
        bool drawAtFirst)
    {
        ref Vector2 pointToDraw = ref pointB;
        
        if (drawAtFirst)
            pointToDraw = ref pointA;
        
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = debug ? new Color32(255, 255, 255, 255) : (Color32)color;

        Vector2 normal = NumberUtils.GetNormalVector(pointB - pointA);

        float xPos = pointToDraw.x + thickness / 2 * normal.x;
        float yPos = pointToDraw.y + thickness / 2 * normal.y;
        vertex.position = new Vector3(xPos, yPos);
        vh.AddVert(vertex);
        
        vertex.color = debug ? new Color32(255, 0, 255, 255) : (Color32)color;
        
        xPos = pointToDraw.x - thickness / 2 * normal.x;
        yPos = pointToDraw.y - thickness / 2 * normal.y;
        vertex.position = new Vector3(xPos, yPos);
        vh.AddVert(vertex);
    }

    private void DrawLine(ref VertexHelper vh, int vertexOffset)
    {
        vh.AddTriangle(vertexOffset + 0, vertexOffset + 2, vertexOffset + 1);
        vh.AddTriangle(vertexOffset + 2, vertexOffset + 3, vertexOffset + 1);
    }
    
    private void DrawTriangle(ref VertexHelper vh, int vertexOffset)
    {
        vh.AddTriangle(vertexOffset + 0, vertexOffset + 2, vertexOffset + 1);
    }
}
