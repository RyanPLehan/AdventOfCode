using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection.Metadata;

namespace Solver;

internal static class Day9
{
    private class Tile
    {
        public int X { get; init; }
        public int Y { get; init; }
    }

    private class Rectangle
    {
        public Rectangle(Tile tile1, Tile tile2)
        {
            Tile1 = tile1;
            Tile2 = tile2;
            Area = (long)(Math.Abs(tile1.X - tile2.X) + 1) * (long)(Math.Abs(tile1.Y - tile2.Y) + 1);
        }

        public long Area { get; private set; }
        public Tile Tile1 { get; private set; }
        public Tile Tile2 { get; private set; }
        public Tile Tile3 { get => new Tile() { X = Tile1.X, Y = Tile2.Y }; }
        public Tile Tile4 { get => new Tile { X = Tile2.X, Y = Tile1.Y }; }
        }

    private class RightTriangle
    {
        public RightTriangle(Tile rightAngle) { RightAngle = rightAngle; }
        public Tile RightAngle { get; init; }                       // This is the 90 angle
        public Tile? HorizontalAcuteAngle { get; set; } = null;     // This is the Tile to the Left or Right of the Right Angle
        public Tile? VerticalAcuteAngle { get; set; } = null;       // This is the Tile to the Top or Bottom of the Right Angle
    }

    public static long SolvePart1()
    {
        var redTiles = GetRedTiles();
        var rectangles = CreateEveryPossibleRectangle(redTiles);
        var rectangle = rectangles.OrderByDescending(x => x.Area).First();
        return rectangle.Area;
    }


    // Idea
    // Get list of coordates that form right triangles that form the perimeter
    // Starting with the largest area, with the opposit corners, create the other two corners
    // Check to see if all 4 corners are in the perimeter
    public static long SolvePart2()
    {
        var redTiles = GetRedTiles();
        var rectangles = CreateEveryPossibleRectangle(redTiles);

        // Create Perimeter
        var topLeftTile = redTiles.OrderBy(row => row.Y).ThenBy(col => col.X).First();
        var rightTriangle = new RightTriangle(topLeftTile);
        Dictionary<Tile, RightTriangle> rightTriangles = new Dictionary<Tile, RightTriangle>();
        rightTriangles.TryAdd(rightTriangle.RightAngle, rightTriangle);
        var perimeter = CreatePerimeter(rightTriangle, redTiles, rightTriangles);

        long area = 0;
        foreach (var rectangle in rectangles.OrderByDescending(x => x.Area))
        {
            bool inPerimeter = IsTileWithinPerimeter(rectangle.Tile1, perimeter);
            inPerimeter = inPerimeter && IsTileWithinPerimeter(rectangle.Tile2, perimeter);
            inPerimeter = inPerimeter && IsTileWithinPerimeter(rectangle.Tile3, perimeter);
            inPerimeter = inPerimeter && IsTileWithinPerimeter(rectangle.Tile4, perimeter);
            if (inPerimeter)
            {
                area = rectangle.Area;
                break;
            }
        }

        return area;
    }

    private static IList<Tile> GetRedTiles()
    {
        IList<Tile> redTiles = new List<Tile>();
        string[] tiles = File.ReadAllLines("Day9RedTiles.txt");

        foreach (var tile in tiles)
        {
            var col_row = tile.Split(',');
            redTiles.Add(new Tile() { X = int.Parse(col_row[0]), Y = int.Parse(col_row[1]) });
        }

        return redTiles;
    }


    private static IList<Rectangle> CreateEveryPossibleRectangle(IList<Tile> redTiles)
    {
        IList<Rectangle> rectangles = new List<Rectangle>();

        for(int i = 0; i < redTiles.Count - 1; i++)
        {
            for(int j = i + 1; j < redTiles.Count; j++)
                rectangles.Add(new Rectangle(redTiles[i], redTiles[j]));
        }

        return rectangles;
    }


    // Idea:
    // Map out all red tiles along the perimeter.  This is done by assuming that a "corner" will form one and only one right angle
    // When a "corner" is found, map the coordinate and move to the next coordinate that formed the right angle.
    // Moving to the next coordinate involves walking in a clockwise direction
    private static IList<RightTriangle> CreatePerimeter(RightTriangle rightTriangle,
                                                        IList<Tile> redTiles,
                                                        Dictionary<Tile, RightTriangle> rightTriangles)
    {
        // Check to see if we need to move left/right
        if (rightTriangle.HorizontalAcuteAngle == null)
        {
            Tile? acuteAngleTile = null;
            RightTriangle nextRightTriangle = null;

            // Check right (clockwise) first
            acuteAngleTile = redTiles.Where(t => t.Y == rightTriangle.RightAngle.Y &&         // Same Row
                                                 t.X > rightTriangle.RightAngle.X)            // Greater Column
                                     .OrderBy(t => t.X)                                       // Next Greater Column
                                     .FirstOrDefault();

            // Check left
            if (acuteAngleTile == null)
            {
                acuteAngleTile = redTiles.Where(t => t.Y == rightTriangle.RightAngle.Y &&         // Same Row
                                                     t.X < rightTriangle.RightAngle.X)            // Less than Column
                                         .OrderByDescending(t => t.X)                             // Next lesser Column
                                         .FirstOrDefault();
            }

            if (acuteAngleTile == null) throw new Exception("Horizontal Acute Angle Tile not found");

            rightTriangle.HorizontalAcuteAngle = acuteAngleTile;
            rightTriangles.TryGetValue(acuteAngleTile, out nextRightTriangle);
            if (nextRightTriangle == null) nextRightTriangle = new RightTriangle(acuteAngleTile);
            nextRightTriangle.HorizontalAcuteAngle = rightTriangle.RightAngle;
            rightTriangles.TryAdd(acuteAngleTile, nextRightTriangle);
            CreatePerimeter(nextRightTriangle, redTiles, rightTriangles);
        }

        // Check to see if we need to move Top / Bottom
        if (rightTriangle.VerticalAcuteAngle == null)
        {
            Tile? acuteAngleTile = null;
            RightTriangle nextRightTriangle = null;

            // Check bottom (clockwise) first
            acuteAngleTile = redTiles.Where(t => t.X == rightTriangle.RightAngle.X &&         // Same Column
                                                 t.Y > rightTriangle.RightAngle.Y)            // Greater Row
                                     .OrderBy(t => t.Y)                                       // Next Greater Row
                                     .FirstOrDefault();

            // Check Top
            if (acuteAngleTile == null)
            {
                acuteAngleTile = redTiles.Where(t => t.X == rightTriangle.RightAngle.X &&         // Same Column
                                                     t.Y < rightTriangle.RightAngle.Y)            // Less than Row
                                         .OrderByDescending(t => t.Y)                             // Next lesser Row
                                         .FirstOrDefault();
            }

            if (acuteAngleTile == null) throw new Exception("Veritical Acute Angle Tile not found");

            rightTriangle.VerticalAcuteAngle = acuteAngleTile;
            rightTriangles.TryGetValue(acuteAngleTile, out nextRightTriangle);
            if (nextRightTriangle == null) nextRightTriangle = new RightTriangle(acuteAngleTile);
            nextRightTriangle.VerticalAcuteAngle = rightTriangle.RightAngle;
            rightTriangles.TryAdd(acuteAngleTile, nextRightTriangle);
            CreatePerimeter(nextRightTriangle, redTiles, rightTriangles);
        }

        // At this point both Horizontal and Vertical acute angles are found
        return rightTriangles.Values.ToList();
    }


    // Idea
    // To check to see if tile is within a right triangle, will use the two acute angles to create an area using the Rectangle.
    // Then check to see if the tile is within the 4 corners
    private static bool IsTileWithinPerimeter(Tile tile, IList<RightTriangle> perimeter)
    {
        bool result = false;

        foreach(var rt in perimeter)
        {
            Tile? topLeft = null;
            Tile? topRight = null;
            Tile? bottomLeft = null;
            Tile? bottomRight = null;

            if (rt.HorizontalAcuteAngle.Y < rt.VerticalAcuteAngle.Y)
            {
                if (rt.HorizontalAcuteAngle.X < rt.VerticalAcuteAngle.X)
                {
                    topLeft = rt.HorizontalAcuteAngle;
                    bottomRight = rt.VerticalAcuteAngle;
                    topRight = new Tile() { Y = topLeft.Y, X = bottomRight.X };
                    bottomLeft = new Tile() { Y = bottomRight.Y, X = topLeft.X };
                }
                else
                {
                    topRight = rt.HorizontalAcuteAngle;
                    bottomLeft = rt.VerticalAcuteAngle;
                    topLeft = new Tile() { Y = topRight.Y, X = bottomLeft.X };
                    bottomRight = new Tile() { Y = bottomLeft.Y, X = topRight.X };
                }
            }

            if (rt.VerticalAcuteAngle.Y < rt.HorizontalAcuteAngle.Y)
            {
                if (rt.VerticalAcuteAngle.X < rt.HorizontalAcuteAngle.X)
                {
                    topLeft = rt.VerticalAcuteAngle;
                    bottomRight = rt.HorizontalAcuteAngle;
                    topRight = new Tile() { Y = topLeft.Y, X = bottomRight.X };
                    bottomLeft = new Tile() { Y = bottomRight.Y, X = topLeft.X };
                }
                else
                {
                    topRight = rt.HorizontalAcuteAngle;
                    bottomLeft = rt.VerticalAcuteAngle;
                    topLeft = new Tile() { Y = topRight.Y, X = bottomLeft.X };
                    bottomRight = new Tile() { Y = bottomLeft.Y, X = topRight.X };
                }
            }


            result = topLeft.Y <= tile.Y && topLeft.X <= tile.X &&
                     topRight.Y <= tile.Y && topRight.X >= tile.X &&
                     bottomLeft.Y >= tile.Y && bottomLeft.X <= tile.X &&
                     bottomRight.Y >= tile.Y && bottomRight.X >= tile.X;

            if (result) break;
        }

        return result;
    }
}
