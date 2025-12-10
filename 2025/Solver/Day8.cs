using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Diagnostics;

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
        public string ID { get => $"{Coordinate.X}.{Coordinate.Y}.{Coordinate.Z}"; }
        public Coordinate Coordinate { get; init; }
    }

    private class Connection
    {
        public string Key
        { get => (JBox1.Coordinate.X < JBox2.Coordinate.X) ? $"{JBox1.ID} : {JBox2.ID}" : $"{JBox1.ID} : {JBox2.ID}"; }

        public JunctionBox JBox1 { get; init; }
        public JunctionBox JBox2 { get; init; }
        public double Distance { get; init; }
    }

    private class Circuit()
    {
        public List<Connection> Connections { get; private set; } = new List<Connection>();

        public bool IsMerged { get; private set; } = false;

        public bool ContainsConnection(Connection connection) => 
            Connections.Where(x => x.Key == connection.Key).Any();

        public bool ContainsJunctionBox(JunctionBox junctionBox) =>
            Connections.Where(x => x.JBox1.ID == junctionBox.ID || x.JBox2.ID == junctionBox.ID).Any();

        public int DistinctJunctionBoxCount 
        {
            get
            {
                HashSet<string> hs = new HashSet<string>();
                foreach (var connection in Connections)
                {
                    hs.Add(connection.JBox1.ID);
                    hs.Add(connection.JBox2.ID);
                }

                return hs.Count();
            }
        }

        public void Merge(Circuit circuit)
        {
            if (circuit.IsMerged) return;

            this.Connections.AddRange(circuit.Connections);
            circuit.IsMerged = true;
            circuit.Connections.Clear();
        }
    }


    public static long SolvePart1()
    {
        const int topLargestCircuits = 3;
        const int maxConnections = 1000;            // Use 10 for Sample, use 1000 for puzzle
        IList<Circuit> circuits = CreateCircuits(maxConnections);     
        IList<Circuit> largestCircuits = circuits.OrderByDescending(x => x.Connections.Count)
                                                 .Take(topLargestCircuits)
                                                 .ToList();

        long total = 0;
        for (int i = 0; i < largestCircuits.Count; i++)
        {
            if (i == 0) total = largestCircuits[i].DistinctJunctionBoxCount;
            else total *= largestCircuits[i].DistinctJunctionBoxCount;
        }
           
        return total;
    }

    public static long SolvePart2()
    {
        Connection conn = GetLastConnectionToFormSingleCircuit();
        return (long)conn.JBox1.Coordinate.X * (long)conn.JBox2.Coordinate.X;
    }


    private static IList<Circuit> CreateCircuits(int maxConnections)
    {
        IList<Circuit> circuits = new List<Circuit>();
        IList<JunctionBox> junctionBoxes = GetJunctionBoxes();
        IList<Connection> connections = CalcAllDistances(junctionBoxes).OrderBy(x => x.Distance).ToList();

        for (int i = 0; i < connections.Count && i < maxConnections; i++)
        {
            Circuit? circuit = null;
            var conn = connections[i];

            // Check to see if pair is already in the same circuit
            if (circuits.Where(x => x.ContainsJunctionBox(conn.JBox1) &&
                                    x.ContainsJunctionBox(conn.JBox2))
                        .Any())
                continue;


            // This will add to existing circuit
            var mergeCircuits = circuits.Where(x => x.ContainsJunctionBox(conn.JBox1) || 
                                                    x.ContainsJunctionBox(conn.JBox2))
                                        .ToList();
            if (mergeCircuits.Any())
            {
                circuit = mergeCircuits[0];         // Get first one
                circuit.Connections.Add(conn);      // add connection

                // Now merge circuits into one
                for (int j = mergeCircuits.Count - 1; j > 0; j--)
                    circuit.Merge(mergeCircuits[j]);

                // Remove merged
                circuits = circuits.Where(x => !x.IsMerged).ToList();
            }
            else
            {
                // Whole new Circuit
                circuit = new Circuit();
                circuit.Connections.Add(conn);
                circuits.Add(circuit);
            }
        }

        // Make sure to not to return any merged circuits
        return circuits;
    }


    private static Connection GetLastConnectionToFormSingleCircuit()
    {
        IList<Circuit> circuits = new List<Circuit>();
        IList<JunctionBox> junctionBoxes = GetJunctionBoxes();
        IList<Connection> connections = CalcAllDistances(junctionBoxes).OrderBy(x => x.Distance).ToList();

        HashSet<JunctionBox> connectedJunctionBoxes = new HashSet<JunctionBox>();

        Connection conn = null;
        for (int i = 0; i < connections.Count; i++)
        {
            Circuit? circuit = null;
            conn = connections[i];
            
            connectedJunctionBoxes.Add(conn.JBox1);
            connectedJunctionBoxes.Add(conn.JBox2);

            // Check to see if pair is already in the same circuit
            if (circuits.Where(x => x.ContainsJunctionBox(conn.JBox1) &&
                                    x.ContainsJunctionBox(conn.JBox2))
                        .Any())
                continue;


            // This will add to existing circuit
            var mergeCircuits = circuits.Where(x => x.ContainsJunctionBox(conn.JBox1) ||
                                                    x.ContainsJunctionBox(conn.JBox2))
                                        .ToList();
            if (mergeCircuits.Any())
            {
                circuit = mergeCircuits[0];         // Get first one
                circuit.Connections.Add(conn);      // add connection

                // Now merge circuits into one
                for (int j = mergeCircuits.Count - 1; j > 0; j--)
                    circuit.Merge(mergeCircuits[j]);

                // Remove merged
                circuits = circuits.Where(x => !x.IsMerged).ToList();
            }
            else
            {
                // Whole new Circuit
                circuit = new Circuit();
                circuit.Connections.Add(conn);
                circuits.Add(circuit);
            }

            // Break when all junction boxes have been connected and there is only 1 circuit
            if (circuits.Count == 1 &&
                junctionBoxes.Count == connectedJunctionBoxes.Count)
                break;
        }

        // Make sure to not to return any merged circuits
        return conn;
    }






    private static IList<Connection> CalcAllDistances(IList<JunctionBox> junctionBoxes)
    {
        IList<Connection> connections = new List<Connection>();
        for (int i = 0; i < junctionBoxes.Count - 1; i++)
        {
            for (int j = i + 1; j < junctionBoxes.Count; j++)
            {
                var jb1 = junctionBoxes[i];
                var jb2 = junctionBoxes[j];

                // Don't compare to itself
                if (jb1 == jb2) continue;

                Connection conn = new Connection()
                {
                    JBox1 = jb1,
                    JBox2 = jb2,
                    Distance = CalcDistance(jb1.Coordinate, jb2.Coordinate),
                };

                connections.Add(conn);
            }
        }

        return connections;
    }

    private static IList<Connection> CalcShortestDistances(IList<JunctionBox> junctionBoxes)
    {
        IList<Connection> connections = new List<Connection>();
        for (int i = 0; i < junctionBoxes.Count - 1; i++)
        {
            IDictionary<JunctionBox, double> distances = new Dictionary<JunctionBox, double>();
            for (int j = i + 1; j < junctionBoxes.Count; j++)
            {
                var jb1 = junctionBoxes[i];
                var jb2 = junctionBoxes[j];

                // Don't compare to itself
                if (jb1 == jb2) continue;
                distances.Add(jb2, CalcDistance(jb1.Coordinate, jb2.Coordinate));
            }

            // Only take the first shortest distance
            var shortestJBox = distances.OrderBy(x => x.Value).First();
            Connection conn = new Connection()
            {
                JBox1 = junctionBoxes[i],
                JBox2 = shortestJBox.Key,
                Distance = shortestJBox.Value,
            };

            connections.Add(conn);
        }

        return connections;
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

        return Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2) + Math.Pow(deltaZ, 2);
        //return Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2) + Math.Pow(deltaZ, 2));
    }
}
