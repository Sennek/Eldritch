using UnityEngine;
using System.Collections.Generic;

public interface IPathfindable2D
{
    Queue<Vector2> FindPath(Vector2 origin, Vector2 destination);
    IPFCell GetIPFCell(Vector2 pos);
    int Width { get; }
    int Height { get; }
}

public interface IPFCell
{
    bool IsWalkable { get; }
}