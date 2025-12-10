using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;

namespace Solver;

internal static class Day7
{
    private struct Coordinate
    {
        public Coordinate(int row, int col)
        {
            Row = row;
            Column = col;
        }

        public int Row { get; init; }
        public int Column { get; init; }
    }

    private class TachyonNode
    {
        public Coordinate CreationCoordinate;
        public string ID { get => $"{CreationCoordinate.Row} - {CreationCoordinate.Column}"; }
        public TachyonNode? LeftNode { get; set; } = null;
        public TachyonNode? RightNode { get; set; } = null;
        public TachyonNode? MergedNode { get; set; } = null;
    }


    public static int SolvePart1()
    {
        // Use hashset to keep list of visited nodes
        HashSet<string> nodeIds = new HashSet<string>();
        int splitCnt = 0;

        IDictionary<Coordinate, TachyonNode> distinctNodes = CreateBinaryTree();
        foreach(var node in distinctNodes.Values)
        {
            string nodeId = node.ID;
            if (!nodeIds.Contains(nodeId))
            {
                nodeIds.Add(nodeId);
                if (node.LeftNode != null && node.RightNode != null) splitCnt++;
            }
        }

        return splitCnt;
    }

    // Idea: For each split on each tree level, there are 2 paths
    // Therefore the max total will be SUM += Splits(n) * 2: where Splits(n) is the number of splits on level n
    public static long SolvePart2()
    {
        IDictionary<Coordinate, TachyonNode> distinctNodes = CreateBinaryTree();
        var root = distinctNodes.Values.First();
        IDictionary<string, long> visitedNodePathCount = new Dictionary<string, long>();
        long pathCnt = CalcTotalPathsRecursive(root, visitedNodePathCount);

        return pathCnt;
    }



    private static IDictionary<Coordinate, TachyonNode> CreateBinaryTree()
    {
        // Build Binary Tree
        // Caveat: A sharable node can exist between a Left/Right Node of a parent node on the same level
        //         The sharable node is denoted by it's coordinates

        string[] grid = File.ReadAllLines("Day7TachyonBeam.txt");
        TachyonNode? rootNode = null;

        int rowCount = grid.Length;
        int colCount = grid[0].Length;
        IDictionary<Coordinate, TachyonNode> distinctNodes = new Dictionary<Coordinate, TachyonNode>();

        // Build Binary Tree
        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < colCount; col++)
            {
                char c = grid[row][col];
                if (c == 'S')
                    rootNode = CreateOrFindNode(new Coordinate(row, col), distinctNodes);

                if (c == '^')
                {
                    TachyonNode? parentNode = FindParentNode(new Coordinate(row, col), distinctNodes);
                    if (parentNode == null) throw new Exception("Missing Parent Node");

                    // Create or use Existing node for Left/Right side
                    parentNode.LeftNode = CreateOrFindNode(new Coordinate(row, col - 1), distinctNodes);
                    parentNode.RightNode = CreateOrFindNode(new Coordinate(row, col + 1), distinctNodes);
                }
            }
        }

        // This will perform a special merge of where Tachyon Beams paths merge.  This will denoted by a CoParenting situation
        foreach (TachyonNode node in distinctNodes.Values)
        {
            if (node.LeftNode == null && node.RightNode == null)
            {
                TachyonNode? mergeNode = null;
                int row = node.CreationCoordinate.Row+1;
                int col = node.CreationCoordinate.Column;
                int maxRow = distinctNodes.Select(x => x.Key.Row).Max();
                while (row <= maxRow && !distinctNodes.TryGetValue(new Coordinate(row++, col), out mergeNode))
                    ;

                node.MergedNode = mergeNode;
            }
        }

        /*
        // Traverse Binary Tree Breadth first using recursion
        List<List<TachyonNode>> traversedNodes = new List<List<TachyonNode>>();
        TraverseLevelOrderRecursive(rootNode, 0, traversedNodes);

        foreach (var level in traversedNodes)
            foreach (TachyonNode node in level)
                function(node);
        */

        return distinctNodes;
    }


    // Traverse Binary Tree in Breadth (Level Order) first 
    private static void TraverseLevelOrderRecursive(TachyonNode? node, int level, List<List<TachyonNode>> visitedNodes)
    {
        if (node == null) return;

        // Add new level 
        if (visitedNodes.Count <= level) visitedNodes.Add(new List<TachyonNode>());

        // Add current node's id to the visited list
        visitedNodes[level].Add(node);

        // Recurse Left and Right
        TraverseLevelOrderRecursive(node.LeftNode, level + 1, visitedNodes);
        TraverseLevelOrderRecursive(node.RightNode, level + 1, visitedNodes);
    }



    // Traverse Binary Tree in Depth first - Pre-Order (root, left, right)
    // Use Dynamic programming so that if a node has already been visited, then just return the total paths from that point forward
    private static long CalcTotalPathsRecursive(TachyonNode? node, IDictionary<string, long> visitedNodePathCount)
    {
        long pathCnt = 0;

        // This use case should never be reached b/c every split will have a left and right child node
        if (node == null) return 0;

        // Check to see if node is already in list        
        if (visitedNodePathCount.TryGetValue(node.ID, out pathCnt)) return pathCnt;

        // Count terminal node as 1
        if (node.LeftNode == null && node.RightNode == null)
        {
            if (node.MergedNode == null)
            {
                visitedNodePathCount.Add(node.ID, 1);
                return 1;
            }
            else
                pathCnt += CalcTotalPathsRecursive(node.MergedNode, visitedNodePathCount);
        }

        // Recurse Left and Right
        pathCnt += CalcTotalPathsRecursive(node.LeftNode, visitedNodePathCount);
        pathCnt += CalcTotalPathsRecursive(node.RightNode, visitedNodePathCount);
        visitedNodePathCount.Add(node.ID, pathCnt);

        return pathCnt;
    }




    private static TachyonNode CreateOrFindNode(Coordinate coordinate, IDictionary<Coordinate, TachyonNode> distinctNodes)
    {
        TachyonNode? node = null;

        if (!distinctNodes.TryGetValue(coordinate, out node))
        {
            node = new TachyonNode() { CreationCoordinate = coordinate };
            distinctNodes.Add(coordinate, node);
        }

        return node;
    }

    private static TachyonNode? FindParentNode(Coordinate coordinate, IDictionary<Coordinate, TachyonNode> distinctNodes)
    {
        TachyonNode? node = null;
        int row = coordinate.Row;
        int col = coordinate.Column;

        while (row >= 0 && !distinctNodes.TryGetValue(new Coordinate(row--, col), out node))
            ;

        return node;
    }
}
