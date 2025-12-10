using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection.Metadata.Ecma335;

namespace Solver;

internal static class Day5
{
    private class IngredientRange
    {
        public long Begin { get; set; }
        public long End { get; set; }
    }

    public static int SolvePart1()
    {
        int freshCnt = 0;
        var ingredientRanges = GetIngredientRanges();

        ProcessIngredients((ingredientId) =>
        {
            foreach (var range in ingredientRanges)
            {
                if (ingredientId >= range.Begin && ingredientId <= range.End)
                {
                    freshCnt++;
                    break;
                }
            }
        });


        return freshCnt;
    }

    

    public static long SolvePart2()
    {
        // We sort so that there is no need to worry about working ranges from both ends
        var ingredientRanges = GetIngredientRanges().OrderBy(x => x.Begin).ToList();
        var noOverlappingRanges = new List<IngredientRange>();

        // Build new range where no values overlap
        for (int i = 0; i < ingredientRanges.Count; i++)
        {
            bool hasBeenAdded = false;

            for (int j = 0; j < noOverlappingRanges.Count; j++)
            {
                // Check to see if entire range is already within another range
                if (ingredientRanges[i].Begin <= noOverlappingRanges[j].End &&
                    ingredientRanges[i].End <= noOverlappingRanges[j].End)
                {
                    hasBeenAdded = true;
                    break;
                }

                // Check to see if the begin values are in another range
                // Since we have already ordered in ascending order by Begin, no need to compare Begin to Begin
                if (ingredientRanges[i].Begin <= noOverlappingRanges[j].End &&
                ingredientRanges[i].End > noOverlappingRanges[j].End)
                {
                    // Just Extend the range of the current no over lapping range set
                    noOverlappingRanges[j].End = ingredientRanges[i].End;
                    hasBeenAdded = true;
                    break;
                }
            }

            if (!hasBeenAdded)
                noOverlappingRanges.Add(new IngredientRange()
                {
                    Begin = ingredientRanges[i].Begin,
                    End = ingredientRanges[i].End,
                });
        }

        // Now count each value within the ranges
        return noOverlappingRanges.Select(x => x.End - x.Begin + 1)
                                  .Sum();
    }
    


    private static IList<IngredientRange> GetIngredientRanges()
    {
        // Create 2 separate list (Ingredients Range and individual Ingredient (fresh/spoiled)
        string[] db = File.ReadAllLines("Day5IngredientDB.txt");

        IList<IngredientRange> ingredientRanges = new List<IngredientRange>();
        foreach (string line in db)
        {
            if (String.IsNullOrWhiteSpace(line)) break;

            string[] rangeValues = line.Split('-');
            ingredientRanges.Add(new IngredientRange()
            {
                Begin = Int64.Parse(rangeValues[0]),
                End = Int64.Parse(rangeValues[1])
            });
        }

        return ingredientRanges;
    }

    private static void ProcessIngredients(Action<long> function)
    {
        string[] db = File.ReadAllLines("Day5IngredientDB.txt");

        IList<long> ingredientIds = new List<long>();
        bool foundBreakMarker = false;
        foreach (string line in db)
        {
            if (!String.IsNullOrWhiteSpace(line) && !foundBreakMarker) 
                continue;

            if (String.IsNullOrWhiteSpace(line))
            {
                foundBreakMarker = true;
                continue;
            }

            ingredientIds.Add(Int64.Parse(line));
        }

        foreach (long ingredientId in ingredientIds)
            function(ingredientId);
    }
}
