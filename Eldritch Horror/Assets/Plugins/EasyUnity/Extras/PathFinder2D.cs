using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using NoxLibrary.Lists;

public class PathFinder2D
{
    protected const float Sqrt2 = 1.41421356237f;

    #region Temporary fields during calculations
    protected Dictionary<Vector2Int, PFCell> cells;
    protected MinBinaryHeap<PFCell> openList;
    protected Vector2Int From;
    protected Vector2Int To;
    #endregion

    protected Func<Vector2Int, float> _costFunc;
    protected float GetCost(Vector2Int pos) => _costFunc(pos);

    public PathFinder2D(Func<Vector2Int, bool> isWalkableFunc) => _costFunc = (Vector2Int pos) => isWalkableFunc(pos) ? 0 : 1000000;

    public PathFinder2D(Func<Vector2Int, float> costFunc) => _costFunc = costFunc;

    public Queue<Vector2Int> FindPath(Vector2Int from, Vector2Int to, bool allowDiagonals = true)
    {
        From = from;
        To = to;
        int startCapacity = Mathf.Abs(to.x - from.x) * Mathf.Abs(to.y - from.y);
        cells = new Dictionary<Vector2Int, PFCell>(startCapacity);
        openList = new MinBinaryHeap<PFCell>(startCapacity / 2);

        openList.Add(new PFCell(null, from, 0, GetHeuristicDistance(from, to, allowDiagonals)));

        while (openList.Count > 0)
        {
            PFCell cell = openList.Pop();
            cell.IsClosed = true;

            if (cell.Pos == to)
                return CreateQueue(cell);

            DiscoverCells(cell, allowDiagonals);
        }

        return CreateQueue(cells.Min().Value);
    }

    protected void DiscoverCells(PFCell originalCell, bool allowDiagonals)
    {
        for (int y = -1; y < 2; y++)
            for (int x = -1; x < 2; x++)
            {
                if ((y == 0 && x == 0) || (!allowDiagonals && x != 0 && y != 0)) continue;
                DiscoverCell(originalCell, new Vector2Int(x, y) + originalCell.Pos, allowDiagonals);
            }
    }

    protected void DiscoverCell(PFCell originalCell, Vector2Int targetPos, bool allowDiagonals)
    {
        if (!cells.TryGetValue(targetPos, out PFCell cell) || !cell.IsClosed)
        {
            float g = originalCell.G + GetHeuristicDistance(originalCell.Pos, targetPos, allowDiagonals) + GetCost(targetPos);
            if (cell == null)
            {
                cells[targetPos] = cell = new PFCell(originalCell, targetPos, g, GetHeuristicDistance(targetPos, To, allowDiagonals));
                openList.Add(cell);
            }
            else if (g < cell.G)
            {
                cell.Update(originalCell, targetPos, g);
                openList.SortUp(cell);
            }
        }
    }

    private float GetHeuristicDistance(Vector2Int from, Vector2Int to, bool allowDiagonals)
    {
        if (allowDiagonals)
        {
            Vector2Int delta = new Vector2Int(Math.Abs(to.x - from.x), Math.Abs(to.y - from.y));
            return delta.x > delta.y ? (Sqrt2 * delta.y) + (delta.x - delta.y) : (Sqrt2 * delta.x) + (delta.y - delta.x);
        }
        else return Math.Abs(to.x - from.x) + Math.Abs(to.y - from.y);
    }

    private Queue<Vector2Int> CreateQueue(PFCell cell)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        while (cell != null && cell.Pos != From)
        {
            queue.Enqueue(cell.Pos);
            cell = cell.PreviousCell;
        }

        return new Queue<Vector2Int>(queue.Reverse());
    }

    protected class PFCell : IComparable<PFCell> //PathFindingCell
    {
        public PFCell PreviousCell;
        public Vector2Int Pos;
        public bool IsClosed = false;

        float _g;

        /// <summary>
        /// The shortest distance calculated to the origin so far.
        /// </summary>
        public float G { get => _g; set { _g = value; F = _g + H; } }

        /// <summary>
        /// Heuristic distance to destination (immutable)
        /// </summary>
        public float H { get; } //Heuristic distance to destination

        /// <summary>
        /// Calculated combined distance from origin to destination passing through this cell. (F = G + H)
        /// </summary>
        public float F { get; private set; }

        public PFCell(PFCell previousCell, Vector2Int pos, float g, float h)
        {
            PreviousCell = previousCell;
            Pos = pos;
            H = h;
            G = g;
        }

        public void Update(PFCell previousCell, Vector2Int pos, float g)
        {
            PreviousCell = previousCell;
            Pos = pos;
            G = g;
        }

        public int CompareTo(PFCell p)
        {
            int compare = F.CompareTo(p.F);
            if (compare == 0)
                compare = H.CompareTo(p.H);
            return compare;
        }

        public int CompareTo(object other) => other is PFCell cell ? CompareTo(cell) : throw new ArgumentException("Object is not of type PFCell");

        public override string ToString() => $"PFCell: {Pos} G: {G} H: {H} F: {F}";
    }
}