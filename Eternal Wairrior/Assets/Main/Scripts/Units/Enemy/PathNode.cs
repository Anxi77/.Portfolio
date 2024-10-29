using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    public Vector2 worldPosition;
    public int gridX;
    public int gridY;

    public float gCost;  // ���������κ����� �Ÿ�
    public float hCost;  // ��ǥ�������� ���� �Ÿ�
    public float fCost => gCost + hCost;

    public bool isWalkable;
    public PathNode parent;

    public PathNode(Vector2 worldPos, int x, int y, bool walkable)
    {
        worldPosition = worldPos;
        gridX = x;
        gridY = y;
        isWalkable = walkable;
    }
}
