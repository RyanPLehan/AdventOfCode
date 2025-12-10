using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Solver;

internal static class Day9
{
    private class RedTile
    {
        public int X { get; init; }
        public int Y { get; init; }
    }

    private class Rectangle
    {
        public Rectangle(RedTile redTile1, RedTile redTile2)
        {
            RedTile1 = redTile1;
            RedTile2 = redTile2;
            Area = (long)(Math.Abs(redTile1.X - redTile2.X) + 1) * (long)(Math.Abs(redTile1.Y - redTile2.Y) + 1);
        }

        public long Area { get; private set; }
        public RedTile RedTile1 { get; private set; }
        public RedTile RedTile2 { get; private set; }
        
    }

    public static long SolvePart1()
    {
        var redTiles = GetRedTiles();
        var rectangles = CreateEveryPossibleRectangle(redTiles);
        var rectangle = rectangles.OrderByDescending(x => x.Area).First();
        return rectangle.Area;
    }

    private static IList<Rectangle> CreateEveryPossibleRectangle(IList<RedTile> redTiles)
    {
        IList<Rectangle> rectangles = new List<Rectangle>();

        for(int i = 0; i < redTiles.Count - 1; i++)
        {
            for(int j = i + 1; j < redTiles.Count; j++)
                rectangles.Add(new Rectangle(redTiles[i], redTiles[j]));
        }

        return rectangles;
    }

    private static IList<RedTile> GetRedTiles()
    {
        IList<RedTile> redTiles = new List<RedTile>();
        string[] tiles = File.ReadAllLines("Day9RedTiles.txt");

        foreach(var tile in tiles)
        {
            var col_row = tile.Split(',');
            redTiles.Add(new RedTile() { X = int.Parse(col_row[1]), Y = int.Parse(col_row[0]) });
        }

        return redTiles;
    }
}
