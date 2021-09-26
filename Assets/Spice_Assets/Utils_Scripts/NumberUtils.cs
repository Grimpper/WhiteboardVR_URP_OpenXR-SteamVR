using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public static class NumberUtils
{
    public enum Unit
    {
        Y = 24,
        Z = 21,
        E = 18,
        P = 15,
        T = 12,
        G = 9,
        M = 6,
        k = 3,
        h = 2,
        D = 1,
        Unitary = 0,
        d = -1,
        c = -2,
        m = -3,
        u = -6,
        n = -9,
        p = -12,
        f = -15,
        a = -18,
        z = -21,
        y = -24,
    }

    private static string MagnitudeToString(this Unit magnitude) 
        => magnitude == Unit.Unitary ? "" : magnitude.ToString();

    public const string
        Radians = "rad",
        Degrees = "°",
        Time = "s",
        Voltage = "V",
        Intensity = "I";
    
    public struct Line
    {
        public float slope;
        public float intercept;
        public Vector2 normal;
        public Vector2 dir;

        public Line(Vector2 pointA, Vector2 dir)
        {
            this.dir = dir.normalized;
            slope = GetSlope(dir);
            intercept = GetIntercept(slope, pointA);
            normal = GetNormalVector(dir);
        }

        public Vector2 GetPoint() => new Vector2(0, intercept);
    }

    public static int GetSignificantFigurePos(float number)
    {
        float absNumber = Math.Abs(number);

        if (absNumber == 0) 
            return 0;
        
        int decimalPlaces = 0;

        if (absNumber < 1)
        {
            while (absNumber < 1)
            {
                absNumber *= 10f;
                decimalPlaces--;
            }
        }
        else
        {
            while (absNumber > 10)
            {
                absNumber /= 10f;
                decimalPlaces++;
            }
        }

        return decimalPlaces;
    }

    public static float GetDotProduct(Vector2 vectorA, Vector2 vectorB) =>
        vectorA.x * vectorB.x + vectorA.y * vectorB.y;
    
    public static float GetSlope(Vector2 vector) => vector.y / vector.x;
    
    public static float GetIntercept(float slope, Vector2 point) => point.y - slope * point.x;
    
    public static Vector2 LineIntersection(Line lineA, Line lineB)
    {
        float xPos = (lineB.intercept - lineA.intercept) / (lineA.slope - lineB.slope);
        float yPos = lineA.slope * xPos + lineA.intercept;

        return new Vector2(xPos, yPos);
    }

    public static float GetAngleFromVector(Vector2 vector, string unit = Radians) =>
        unit switch
        {
            Degrees => (float)(Math.Atan(vector.y / vector.x) * 180 / Math.PI),
            Radians => (float)Math.Atan(vector.y / vector.x),
            _ => 0
        };
    
    public static float GetAngleBetweenVectors(Vector2 vectorA, Vector2 vectorB, string unit = Radians) =>
        unit switch
        {
            Degrees => (float)(Math.Acos(GetDotProduct(vectorA, vectorB) / (vectorA.magnitude * vectorB.magnitude)) 
                                                                                                    * 180 / Math.PI),
            Radians => (float)Math.Acos(GetDotProduct(vectorA, vectorB) / (vectorA.magnitude * vectorB.magnitude)),
            _ => 0
        };

    public static float GetAngleBisectorAngle(Vector2 vectorA, Vector2 vectorB, string unit = Radians)
    {
        float alphaAngle = GetAngleBetweenVectors(vectorA, vectorB);
        float betaAngle = Mathf.PI - alphaAngle;

        float lineBAngle = GetAngleFromVector(vectorB);
        float bisectorAngle = lineBAngle + alphaAngle + betaAngle / 2f;

        if (unit == Degrees)
            return bisectorAngle * 180 / Mathf.PI;
        
        if (unit == Radians)
            return bisectorAngle;
        
        return 0;

    }

    public static Vector2 GetNormalVector(Vector2 vector) => new Vector2(-vector.y, vector.x).normalized;

    public static float GetMagnitude(Unit unit) => Mathf.Pow(10, (int) unit);
    
    public static string GetUnit(string variableName, Unit magnitude)
    {
        if (variableName.Equals("time"))
        {
            return " (" + magnitude.MagnitudeToString() + Time + ")";
        }

        if (variableName.StartsWith("v"))
        {
            return " (" + magnitude.MagnitudeToString() + Voltage + ")";
        }

        if (variableName.StartsWith("i"))
        {
            return " (" + magnitude.MagnitudeToString() + Intensity + ")";
        }

        return "";
    }
}
