using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;

namespace Solver;

internal static class Day4
{
    private static bool[,]? _boolGrid = null;

    public static int SumAccessiblePaperRollsPart1()
    {
        const int MAX_ROW = 3;
        const int MAX_COL = 3;
        const int CENTER_ROW_COL = 1;
        const int MAX_ADJACENT_ALLOWED = 3;

        int accessCount = 0;
        ProcessPaperGrid((boolGrid) =>
        {
            int adjacentRollCount = 0;
            if (boolGrid[1, 1])
            {
                for (int row = 0; row < MAX_ROW; row++)
                    for (int col = 0; col < MAX_COL; col++)
                        if (boolGrid[row, col] && 
                            !(row == CENTER_ROW_COL && col == CENTER_ROW_COL)) adjacentRollCount++;

                if (adjacentRollCount <= MAX_ADJACENT_ALLOWED) accessCount++;
            }

            return false;
        });

        return accessCount;
    }

    public static int SumAccessiblePaperRollsPart2()
    {
        const int MAX_ROW = 3;
        const int MAX_COL = 3;
        const int CENTER_ROW_COL = 1;
        const int MAX_ADJACENT_ALLOWED = 3;

        bool atLeastOneRemoved = true;
        int accessCount = 0;
        while (atLeastOneRemoved)
        {
            atLeastOneRemoved = false;
            ProcessPaperGrid((boolGrid) =>
            {
                bool remove = false;
                int adjacentRollCount = 0;
                if (boolGrid[1, 1])
                {
                    for (int row = 0; row < MAX_ROW; row++)
                        for (int col = 0; col < MAX_COL; col++)
                            if (boolGrid[row, col] &&
                                !(row == CENTER_ROW_COL && col == CENTER_ROW_COL)) adjacentRollCount++;

                    if (adjacentRollCount <= MAX_ADJACENT_ALLOWED)
                    {
                        accessCount++;
                        remove = true;
                    }
                }

                atLeastOneRemoved |= remove;
                return remove;
            });
        }

        return accessCount;
    }

    #region Solution
    // Read in file and Create a 3x3 boolean grid where the mid center contains (or not) the roll of paper (true)
    //      [Top left]  [Top Center]    [Top Right]
    //      [Mid Left]  [Mid Center]    [Mid Center]
    //      [Bot Left]  [Bot Center]    [Bot Center]

    // Then, if the Mid Center has a roll paper (ie true), then check each adjecent cell and count any occurance of another roll of paper
    // Check count to determine if fork lift and move roll of paper
    #endregion

    // Create a 3x3 grid where the mid center contains (or not) the roll of paper
    //      [Top left]  [Top Center]    [Top Right]
    //      [Mid Left]  [Mid Center]    [Mid Center]
    //      [Bot Left]  [Bot Center]    [Bot Center]

    // The delegated function should return true if the paper roll is to be removed
    private static void ProcessPaperGrid(Func<bool[,], bool> function)
    {
        if (_boolGrid == null)
            _boolGrid = BuildGrid();

        int rowCount = _boolGrid.GetUpperBound(0) + 1;
        int colCount = _boolGrid.GetUpperBound(1) + 1;

        IList<(int row, int col)> paperRollsToBeRemoved = new List<(int row, int col)>();

        // Iterate through each space in the grid and create a 3x3 grid where the grid area in question is in the center (2,2) of the 3x3 grid
        int maxSpaces = rowCount * colCount;
        for (int spacePosition = 0; spacePosition < maxSpaces; spacePosition++)
        {
            // Calculate row, col of where space is located in boolGrid
            int row = spacePosition / rowCount;
            int col = spacePosition % colCount;

            // Create grid and manaully all 9 cells
            bool[,] forkLiftGrid = new bool[3, 3];

            // Top Left, Center, Right
            forkLiftGrid[0, 0] = row - 1 >= 0 && col - 1 >= 0 ? _boolGrid[row - 1, col - 1] : false;
            forkLiftGrid[0, 1] = row - 1 >= 0 ? _boolGrid[row - 1, col] : false;
            forkLiftGrid[0, 2] = row - 1 >= 0 && col + 1 < colCount ? _boolGrid[row - 1, col + 1] : false;

            // Mid Left, Center, Right
            forkLiftGrid[1, 0] = col - 1 >= 0 ? _boolGrid[row, col - 1] : false;
            forkLiftGrid[1, 1] = _boolGrid[row, col];
            forkLiftGrid[1, 2] = col + 1 < colCount ? _boolGrid[row, col + 1] : false;

            // Bottom Left, Center, Right
            forkLiftGrid[2, 0] = row + 1 < rowCount && col - 1 >= 0 ? _boolGrid[row + 1, col - 1] : false;
            forkLiftGrid[2, 1] = row + 1 < rowCount ? _boolGrid[row + 1, col] : false;
            forkLiftGrid[2, 2] = row + 1 < rowCount && col + 1 < colCount ? _boolGrid[row + 1, col + 1] : false;

            bool removeRoll = function(forkLiftGrid);

            if (removeRoll) paperRollsToBeRemoved.Add((row,col));
        }

        // Now remove paperRolls from overall grid
        foreach (var coordinate in paperRollsToBeRemoved)
            _boolGrid[coordinate.row, coordinate.col] = false;
    }

    // This is a one time build of the boolean grid from the file
    private static bool[,] BuildGrid()
    {
        string[] paperGrid = File.ReadAllLines("Day4PaperGrid.txt");

        // Create a true NxM grid of boolean values
        // An indication of true represents a roll of paper
        int rowCount = paperGrid.Length;
        int colCount = paperGrid[0].Length;
        bool[,] boolGrid = new bool[rowCount, colCount];

        for (int row = 0; row < rowCount; row++)
        {
            char[] paperRow = paperGrid[row].ToCharArray();
            if (colCount != paperRow.Length) throw new Exception(String.Format("Column length mis-match (row: {0})", row));

            for (int col = 0; col < colCount; col++)
                boolGrid[row, col] = paperRow[col] == '@' ? true : false;
        }

        return boolGrid;
    }
}
