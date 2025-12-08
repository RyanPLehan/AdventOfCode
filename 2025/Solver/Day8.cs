using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Solver;

internal static class Day8
{
    private struct Coordinate
    {
        public Coordinate(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public int X { get; init; }
        public int Y { get; init; }
        public int Z { get; init; }
    }

    private class JunctionBox()
    {
        public Coordinate Coordinate { get; init; }
        public Circuit Circuit { get; set; }
    }

    private class Circuit()
    {
        IList<JunctionBox> JunctionBoxes = new List<JunctionBox>();
    }

    private static IList<JunctionBox> GetJunctionBoxes()
    {
        string[] coordinates = File.ReadAllLines("Day8JunctionBoxes.txt");
        IList<JunctionBox> junctionBoxes  = new List<JunctionBox>();

        foreach (string coordinate in coordinates)
        {
            string[] parsedCoodinate = coordinate.Split(',');
            int x = Int32.Parse(parsedCoodinate[0]);
            int y = Int32.Parse(parsedCoodinate[1]);
            int z = Int32.Parse(parsedCoodinate[2]);
            JunctionBox jb = new JunctionBox() { Coordinate = new Coordinate(x, y, z)};
            junctionBoxes.Add(jb);
        }

        return junctionBoxes;
    }

    // Use Euclidean Distance Equation (https://en.wikipedia.org/wiki/Euclidean_distance)
    private static double CalcDistance(Coordinate coordinate1, Coordinate coordinate2)
    {
        long deltaX = coordinate1.X - coordinate2.X;
        long deltaY = coordinate1.Y - coordinate2.Y;
        long deltaZ = coordinate1.Z - coordinate2.Z;
        return Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2) + Math.Pow(deltaZ, 2));
    }
}
