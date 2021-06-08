using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static GlobalData;

public static class GlobalFunctions
{

    public static Vector3 LocalToWord(Vector3Int localplace)
    {
        float w = (Mathf.Sqrt(3) * tileRadius) / pixelPerUnit;
        float h = 0.75f * (2 * tileRadius / (float)pixelPerUnit);
        float offset = 0;
        if (Math.Abs(localplace.y) % 2 == 1)
        {
            offset = w / 2;
        }
        return new Vector3(localplace.x * w + offset, localplace.y * h, localplace.z);
    }

    public static List<Vector3Int> Neighbours(Vector3Int centre)
    {
        List<Vector3Int> cube_direction = new List<Vector3Int>() { new Vector3Int(+1, -1, 0), new Vector3Int(+1, 0, -1), new Vector3Int(0, +1, -1), new Vector3Int(-1, +1, 0), new Vector3Int(-1, 0, +1), new Vector3Int(0, -1, +1) };
        List<Vector3Int> neighbours = new List<Vector3Int>();
        foreach (Vector3Int n in cube_direction)
        {
            neighbours.Add(CubeToOffset(OffsetToCube(centre) + n));
        }
        return neighbours;
    }

    public static List<Vector3Int> CircleCoords(int radiusinner, int radiusouter, Vector3Int centre)//radiusinner=1,radiusouter=1 for melee 
    {
        List<Vector3Int> circlecoords = new List<Vector3Int>();
        List<Vector3Int> cube_direction = new List<Vector3Int>() { new Vector3Int(+1, -1, 0), new Vector3Int(+1, 0, -1), new Vector3Int(0, +1, -1), new Vector3Int(-1, +1, 0), new Vector3Int(-1, 0, +1), new Vector3Int(0, -1, +1) };
        for (int r = radiusinner; r <= radiusouter; r++)
        {
            Vector3Int cube = OffsetToCube(centre) + cube_direction[4] * r;
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < r; j++)
                {
                    circlecoords.Add(CubeToOffset(cube));
                    cube = cube + cube_direction[i];
                }
            }
        }
        return circlecoords;
    }

    public static int Rolldamage(int baseDamage, int dice, int number, int cover)
    {
        int r = 0;
        for (var i = 0; i < number; i++)
        {
            if (cover < 0)
            {
                int roll = 0;
                for (int j = 0; j < cover - 1; j--)
                {
                    roll = Math.Max(UnityEngine.Random.Range(1, dice + 1), roll);
                }
                r = r + roll;
            }
            else
            {
                int roll = dice;
                for (int j = 0; j < cover + 1; j++)
                {
                    roll = Math.Min(UnityEngine.Random.Range(1, dice + 1), roll);
                }
                r = r + roll;
            }
        }
        return r + baseDamage;
    }

    public static int Distance(Vector3Int a, Vector3Int b)
    {
        Vector3Int ac = OffsetToCube(a);
        Vector3Int bc = OffsetToCube(b);
        return CubeDistance(ac, bc);
    }

    private static Vector3Int OffsetToCube(Vector3Int a)
    {
        int x = a.x - (a.y - (a.y & 1)) / 2;
        int z = a.y;
        int y = -x - z;
        return new Vector3Int(x, y, z);
    }

    private static Vector3Int CubeToOffset(Vector3Int a)
    {
        int x = a.x + (a.z - (a.z & 1)) / 2;
        int y = a.z;
        return new Vector3Int(x, y, 0);
    }

    private static int CubeDistance(Vector3Int a, Vector3Int b)
    {
        return (Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y) + Math.Abs(a.z - b.z)) / 2;
    }
}