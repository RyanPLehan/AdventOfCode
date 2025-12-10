using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Solver;

internal static class Day2
{
    public static long SolvePart1()
    {
        long sum = 0;

        ProcessProductIds((id) =>
        {
            var idLen = id.Length;
            if (idLen % 2 == 0 &&
                id.Substring(0, idLen / 2) == id.Substring(idLen / 2))
                sum += Int64.Parse(id);
        });

        return sum;
    }

    public static long SolvePart2()
    {
        long sum = 0;

        ProcessProductIds((id) =>
        {
            var idLen = id.Length;
            int maxGrabLen = idLen / 2;     // At most it can be 1/2 of the length of the text
            bool isMatch = true;            // Assume we have a pattern match
            int patternRepeatedCnt = 0;

            for (int grabLen = 1; grabLen <= maxGrabLen; grabLen++)
            {
                isMatch = true;
                patternRepeatedCnt = 1;                 // Account for 1st pattern match that we skip over in the loop
                var pattern = id.Substring(0, grabLen);

                // Check to make sure we can compare every part of the string
                if (idLen % grabLen != 0) continue;

                // Now compare pattern to rest of parts of string                
                for (int i = grabLen; i <= idLen - grabLen; i+=grabLen)
                {
                    if (!pattern.Equals(id.Substring(i, grabLen)))
                    {
                        isMatch = false;
                        break;
                    }
                    patternRepeatedCnt++;
                }

                // Break early if we have at least one complete pattern match
                if (isMatch) break;
            }

            if (isMatch && patternRepeatedCnt > 1) sum += Int64.Parse(id);
        });

        return sum;
    }


    private static void ProcessProductIds(Action<string> function)
    {
        string productIds = File.ReadAllText("Day2ProductIDs.txt");
        string[] productIdRanges = productIds.Split(',');
        foreach (string idRange in productIdRanges)
        {
            var tempIds = idRange.Split('-');
            long beginId = Int64.Parse(tempIds[0]);
            long endId = Int64.Parse(tempIds[1]);

            for (long id = beginId; id <= endId; id++)
            {
                function(id.ToString());
            }
        }
    }
}
