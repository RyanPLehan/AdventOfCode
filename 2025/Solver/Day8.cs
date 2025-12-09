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
        public string ID { get => $"{Coordinate.X} - {Coordinate.Y} - {Coordinate.Z}"; }
        public Coordinate Coordinate { get; init; }
        public Circuit? Circuit { get; set; } = null;
    }

    private class Circuit()
    {
        public IList<JunctionBox> JunctionBoxes = new List<JunctionBox>();
    }

    private class JunctionBoxDistance
    {
        public JunctionBox[] PairedJunctionBoxes { get; } = new JunctionBox[2];
        public double Distance { get; set; }
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
        IList<JunctionBoxDistance> jbDistances = CalcAllPossibleDistances(junctionBoxes).OrderBy(x => x.Distance).ToList();
        

        foreach (JunctionBoxDistance jbd in jbDistances)
        {
            // create new circuit if both don't already belong to one
            if (jbd.PairedJunctionBoxes[0].Circuit == null &&
                jbd.PairedJunctionBoxes[1].Circuit == null)
            {
                Circuit circuit = new Circuit();
                circuit.JunctionBoxes.Add(jbd.PairedJunctionBoxes[0]);
                circuit.JunctionBoxes.Add(jbd.PairedJunctionBoxes[1]);
                jbd.PairedJunctionBoxes[0].Circuit = circuit;
                jbd.PairedJunctionBoxes[1].Circuit = circuit;
                circuits.Add(circuit);
                continue;
            }

            // Check to see if both already belong to a circuit
            if (jbd.PairedJunctionBoxes[0].Circuit != null &&
                jbd.PairedJunctionBoxes[1].Circuit != null)
                continue;

            // Combine junction boxes to existing circuit
            if (jbd.PairedJunctionBoxes[0].Circuit != null)
            {
                jbd.PairedJunctionBoxes[0].Circuit.JunctionBoxes.Add(jbd.PairedJunctionBoxes[1]);
                jbd.PairedJunctionBoxes[1].Circuit = jbd.PairedJunctionBoxes[0].Circuit;
            }
            else
            {
                jbd.PairedJunctionBoxes[1].Circuit.JunctionBoxes.Add(jbd.PairedJunctionBoxes[0]);
                jbd.PairedJunctionBoxes[0].Circuit = jbd.PairedJunctionBoxes[1].Circuit;
            }
        }

        return circuits;
    }

    private static IList<JunctionBoxDistance> CalcAllPossibleDistances(IList<JunctionBox> junctionBoxes)
    {
        IList<JunctionBoxDistance> distances = new List<JunctionBoxDistance>();

        foreach (JunctionBox jb in junctionBoxes)
        {
            foreach (JunctionBox compareTo in junctionBoxes)
            {
                // Don't compare to itself
                if (jb == compareTo) continue;

                // Don't compare if two are already in list
                if (distances.Any(x => x.PairedJunctionBoxes.Contains(jb)) &&
                    distances.Any(x => x.PairedJunctionBoxes.Contains(compareTo))) continue;

                var distance = new JunctionBoxDistance() { Distance = CalcDistance(jb.Coordinate, compareTo.Coordinate) };
                distance.PairedJunctionBoxes[0] = jb;
                distance.PairedJunctionBoxes[1] = compareTo;
                distances.Add(distance);
            }
        }

        return distances;
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
