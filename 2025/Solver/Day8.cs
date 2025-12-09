using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.ComponentModel;
using System.Text.Json;

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
        public Circuit? Circuit { get; set; } = null;
    }

    private class Circuit()
    {
        public IList<JunctionBox> JunctionBoxes = new List<JunctionBox>();
    }


    public static long GetSizeOfLargestCircuitsPart1(int topLargestCircuits)
    {
        IList<Circuit> circuits = CreateCircuits();
        IList<Circuit> largestCircuits = circuits.OrderByDescending(x => x.JunctionBoxes.Count)
                                                 .Take(topLargestCircuits)
                                                 .ToList();

        long total = largestCircuits[0].JunctionBoxes.Count;
        for (int i = 1; i < largestCircuits.Count; i++)
            total *= largestCircuits[i].JunctionBoxes.Count;

        return total;
    }

    private static IList<Circuit> CreateCircuits()
    {
        IList<Circuit> circuits = new List<Circuit>();
        IList<JunctionBox> junctionBoxes = GetJunctionBoxes();

        foreach (JunctionBox jb in junctionBoxes)
        {
            // if already in circuit, just move on to next one
            if (jb.Circuit != null) continue;

            IDictionary<JunctionBox, double> distances = new Dictionary<JunctionBox, double>();
            foreach (JunctionBox compareTo in junctionBoxes)
            {
                // Don't compare to itself
                if (jb == compareTo) continue;

                distances.Add(compareTo, CalcDistance(jb.Coordinate, compareTo.Coordinate));
            }

            // Sort by distance
            var closestJunctionBox = distances.OrderBy(x => x.Value)
                                              .Select(x => x.Key)
                                              .First();

            // Check to see if closest junction box is already in a circuit
            // if so, just add to the existing circuit
            if (closestJunctionBox.Circuit != null)
            {
                closestJunctionBox.Circuit.JunctionBoxes.Add(jb);
                jb.Circuit = closestJunctionBox.Circuit;
            }
            else
            {
                Circuit circuit = new Circuit();
                circuits.Add(circuit);
                circuit.JunctionBoxes.Add(jb);
                circuit.JunctionBoxes.Add(closestJunctionBox);
                jb.Circuit = circuit;
                closestJunctionBox.Circuit = circuit;
            }
        }

        return circuits;
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
