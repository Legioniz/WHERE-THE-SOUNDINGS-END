using System;
using UnityEngine;

namespace WhereTheSoundingsEnd.Inventory
{
    /// <summary>
    /// Deterministic grid geometry shared by cargo placement, UI previews and automated tests.
    /// </summary>
    public static class CargoGridGeometry
    {
        public static Vector2Int[] RotateAndNormalize(Vector2Int[] source, int quarterTurns)
        {
            if (source == null || source.Length == 0)
                throw new ArgumentException("A cargo shape must contain at least one cell.", nameof(source));

            int turns = ((quarterTurns % 4) + 4) % 4;
            Vector2Int[] result = new Vector2Int[source.Length];
            int minX = int.MaxValue;
            int minY = int.MaxValue;

            for (int i = 0; i < source.Length; i++)
            {
                Vector2Int cell = source[i];
                for (int turn = 0; turn < turns; turn++)
                    cell = new Vector2Int(cell.y, -cell.x);

                result[i] = cell;
                minX = Mathf.Min(minX, cell.x);
                minY = Mathf.Min(minY, cell.y);
            }

            for (int i = 0; i < result.Length; i++)
                result[i] -= new Vector2Int(minX, minY);

            return result;
        }

        public static bool CanPlace(
            int width,
            int height,
            bool[,] occupied,
            bool[,] blocked,
            Vector2Int[] shape,
            Vector2Int anchor)
        {
            if (width <= 0 || height <= 0 || shape == null || shape.Length == 0)
                return false;

            if (occupied == null || occupied.GetLength(0) != width || occupied.GetLength(1) != height)
                throw new ArgumentException("Occupied grid dimensions do not match the cargo hold.");

            if (blocked != null && (blocked.GetLength(0) != width || blocked.GetLength(1) != height))
                throw new ArgumentException("Blocked grid dimensions do not match the cargo hold.");

            for (int i = 0; i < shape.Length; i++)
            {
                Vector2Int cell = anchor + shape[i];
                if (cell.x < 0 || cell.y < 0 || cell.x >= width || cell.y >= height)
                    return false;

                if (occupied[cell.x, cell.y])
                    return false;

                if (blocked != null && blocked[cell.x, cell.y])
                    return false;
            }

            return true;
        }

        public static bool[,] BuildBlockedGrid(int width, int height, Vector2Int[] blockedCells)
        {
            bool[,] blocked = new bool[width, height];
            if (blockedCells == null) return blocked;

            for (int i = 0; i < blockedCells.Length; i++)
            {
                Vector2Int cell = blockedCells[i];
                if (cell.x >= 0 && cell.y >= 0 && cell.x < width && cell.y < height)
                    blocked[cell.x, cell.y] = true;
            }

            return blocked;
        }
    }
}
