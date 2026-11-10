using NUnit.Framework;
using UnityEngine;
using WhereTheSoundingsEnd.Inventory;

namespace WhereTheSoundingsEnd.Tests
{
    public sealed class CargoGridGeometryTests
    {
        [Test]
        public void RotateAndNormalize_QuarterTurnKeepsShapeInsidePositiveCoordinates()
        {
            Vector2Int[] source =
            {
                Vector2Int.zero,
                Vector2Int.right,
                Vector2Int.up
            };

            Vector2Int[] rotated = CargoGridGeometry.RotateAndNormalize(source, 1);

            Assert.That(rotated.Length, Is.EqualTo(3));
            Assert.That(Contains(rotated, new Vector2Int(0, 0)), Is.True);
            Assert.That(Contains(rotated, new Vector2Int(0, 1)), Is.True);
            Assert.That(Contains(rotated, new Vector2Int(1, 1)), Is.True);
        }

        [Test]
        public void CanPlace_RejectsOutOfBoundsOverlapAndBlockedCells()
        {
            bool[,] occupied = new bool[4, 4];
            bool[,] blocked = new bool[4, 4];
            Vector2Int[] domino = { Vector2Int.zero, Vector2Int.right };

            Assert.That(CargoGridGeometry.CanPlace(4, 4, occupied, blocked, domino, new Vector2Int(0, 0)), Is.True);
            Assert.That(CargoGridGeometry.CanPlace(4, 4, occupied, blocked, domino, new Vector2Int(3, 0)), Is.False);

            occupied[1, 1] = true;
            Assert.That(CargoGridGeometry.CanPlace(4, 4, occupied, blocked, domino, new Vector2Int(0, 1)), Is.False);

            occupied[1, 1] = false;
            blocked[0, 1] = true;
            Assert.That(CargoGridGeometry.CanPlace(4, 4, occupied, blocked, domino, new Vector2Int(0, 1)), Is.False);
        }

        [Test]
        public void BuildBlockedGrid_IgnoresCellsOutsideTheHold()
        {
            Vector2Int[] cells =
            {
                new Vector2Int(1, 2),
                new Vector2Int(-1, 0),
                new Vector2Int(9, 9)
            };

            bool[,] blocked = CargoGridGeometry.BuildBlockedGrid(4, 4, cells);

            Assert.That(blocked[1, 2], Is.True);
            Assert.That(blocked[0, 0], Is.False);
            Assert.That(blocked[3, 3], Is.False);
        }

        private static bool Contains(Vector2Int[] cells, Vector2Int target)
        {
            for (int i = 0; i < cells.Length; i++)
                if (cells[i] == target) return true;
            return false;
        }
    }
}
