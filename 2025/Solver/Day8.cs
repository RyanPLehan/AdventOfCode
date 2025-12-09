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
        public string ID { get => $"{Coordinate.X} - {Coordinate.Y} - {Coordinate.Z}"; }
        public Coordinate Coordinate { get; init; }
        //public Circuit? Circuit { get; set; } = null;
    }

    private class PairedJunctionBoxes
    {
        public string Key
        { get => (JBox1.Coordinate.X < JBox2.Coordinate.X) ? $"{JBox1.ID} : {JBox2.ID}" : $"{JBox1.ID} : {JBox2.ID}"; }

        public JunctionBox JBox1 { get; init; }
        public JunctionBox JBox2 { get; init; }
        public double Distance { get; init; }
    }

    private class Circuit()
    {
        private IDictionary<string, PairedJunctionBoxes> _connections = new Dictionary<string, PairedJunctionBoxes>();

        public ImmutableArray<PairedJunctionBoxes> Connections { get => _connections.Values.ToImmutableArray(); }
        public bool ContainsConnection(PairedJunctionBoxes connection) => _connections.ContainsKey(connection.Key);
        public bool ContainsJunctionBox(JunctionBox junctionBox) => _connections.Values.Any(x => x.JBox1 == junctionBox || x.JBox2 == junctionBox);

        public void AddConnection(PairedJunctionBoxes connection)
        {
            _connections.Add(connection.Key, connection);
        }

        public int DistinctJunctionBoxCount 
        {
            get
            {
                IList<JunctionBox> list = new List<JunctionBox>();
                foreach (var connection in _connections.Values)
                {
                    list.Add(connection.JBox1);
                    list.Add(connection.JBox2);
                }

                return list.Distinct().Count();
            }
        }
    }


    public static long GetSizeOfLargestCircuitsPart1(int topLargestCircuits)
    {
        IList<Circuit> circuits = CreateCircuits();
        Debug.WriteLine($"Total unique junction boxes: {circuits.Select(x => x.DistinctJunctionBoxCount).Sum()}");

        
        Debug.WriteLine("");
        Debug.WriteLine("Circuits");
        foreach (var circuit in circuits)
        {
            foreach (var pjb in circuit.Connections)
                Debug.WriteLine($"JBox1: {pjb.JBox1.ID}  JBox2: {pjb.JBox2.ID}   Distance: {pjb.Distance}");

            Debug.WriteLine("");
        }
        

        IList<Circuit> largestCircuits = circuits.OrderByDescending(x => x.Connections.Length)
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

    private static IList<Circuit> CreateCircuits(int maxConnections = 0)
    {
        IList<Circuit> circuits = new List<Circuit>();
        IList<JunctionBox> junctionBoxes = GetJunctionBoxes();

        IList<PairedJunctionBoxes> pairedJBoxList = CalcDistances(junctionBoxes).OrderBy(x => x.Distance)
                                                                                .ToList();

        int totalAdded = 0;
        for (int i = 0; 
             i < pairedJBoxList.Count && ((maxConnections == 0) || (maxConnections > 0 && totalAdded <= maxConnections));
             i++)
        {
            PairedJunctionBoxes pjb = pairedJBoxList[i];

            // Check to see if they are both in the same circuit
            if (circuits.Where(x => x.ContainsJunctionBox(pjb.JBox1) && x.ContainsJunctionBox(pjb.JBox2)).Any())
            {
                foreach (Circuit c in circuits.Where(x => x.ContainsJunctionBox(pjb.JBox1) && x.ContainsJunctionBox(pjb.JBox2)))
                    Debug.WriteLine("In Both");

                continue;
            }
                

            totalAdded++;

            Circuit? circuit = null;
            // Add to existing circuit, if available
            circuit = circuits.Where(x => x.ContainsJunctionBox(pjb.JBox1) &&
                                          !x.ContainsJunctionBox(pjb.JBox2))
                              .FirstOrDefault();

            if (circuit != null)
            {
                circuit.AddConnection(pjb);
                continue;
            }

            // Add to existing circuit, if available
            circuit = circuits.Where(x => x.ContainsJunctionBox(pjb.JBox2) &&
                                          !x.ContainsJunctionBox(pjb.JBox1))
                              .FirstOrDefault();

            if (circuit != null)
            {
                circuit.AddConnection(pjb);
                continue;
            }


            // Whole new Circuit
            circuit = new Circuit();
            circuit.AddConnection(pjb);
            circuits.Add(circuit);
        }

        return circuits;
    }

    private static IList<PairedJunctionBoxes> CalcDistances(IList<JunctionBox> junctionBoxes)
    {
        IList<PairedJunctionBoxes> pairedJBoxes = new List<PairedJunctionBoxes>();
        for (int i = 0; i < junctionBoxes.Count - 1; i++)
        {
            IDictionary<JunctionBox, double> distances = new Dictionary<JunctionBox, double>();
            for (int j = i+1; j < junctionBoxes.Count; j++)
            {
                var jb1 = junctionBoxes[i];
                var jb2 = junctionBoxes[j];

                // Don't compare to itself
                if (jb1 == jb2) continue;

                // Produces every combination
                PairedJunctionBoxes pairedJB = new PairedJunctionBoxes()
                {
                    JBox1 = jb1,
                    JBox2 = jb2,
                    Distance = CalcDistance(jb1.Coordinate, jb2.Coordinate),
                };
                pairedJBoxes.Add(pairedJB);

                // Only take the first shortest distance
                /*
                var shortestJBox = distances.OrderBy(x => x.Value).First();
                PairedJunctionBoxes pairedJB = new PairedJunctionBoxes()
                {
                    JBox1 = jb1,
                    JBox2 = shortestJBox.Key,
                    Distance = shortestJBox.Value,
                };
                pairedJBoxes.Add(pairedJB);
                */
            }
        }

        return pairedJBoxes;
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
