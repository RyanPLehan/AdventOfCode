using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace Solver;

internal static class Day3
{
    public static long SolvePart1A()
    {
        long jolts = 0;
        ProcessBatteryBanks((batteries) =>
        {
            int bankSize = batteries.Count();
            long maxJolts = 0;
            for (int i = 0; i < bankSize - 1; i++)
                for (int j = i+1; j < bankSize; j++)
                {
                    long tmp = Int64.Parse(String.Format("{0}{1}", batteries[i], batteries[j]));
                    maxJolts = tmp > maxJolts ? tmp : maxJolts;
                }

            jolts += maxJolts;
        });

        return jolts;
    }

    public static long SolvePart1B()
    {
        const int MAX_BATTERIES_ON = 2;
        long totalJoltz = 0;

        ProcessBatteryBanks((batteries) =>
        {
            // This array will hold the index of the battery to be turn ON
            int[] onBatteries = CreateOnBatteryList(batteries, MAX_BATTERIES_ON);
            long joltz = CalcJolts(batteries, onBatteries);
            totalJoltz += joltz;
            //Debug.WriteLine($"Battery Bank: {String.Join("", batteries)} - Jolts: {joltz}");
        });

        return totalJoltz;
    }


    public static long SolvePart2()
    {
        const int MAX_BATTERIES_ON = 12;
        long totalJoltz = 0;

        ProcessBatteryBanks((batteries) =>
        {
            // This array will hold the index of the battery to be turn ON
            int[] onBatteries = CreateOnBatteryList(batteries, MAX_BATTERIES_ON);
            long joltz = CalcJolts(batteries, onBatteries);
            totalJoltz += joltz;
            //Debug.WriteLine($"Battery Bank: {String.Join("", batteries)} - Jolts: {joltz}");
        });

        return totalJoltz;
    }

    #region Solution #3
    // Pretend there is a switch box of x size (ie 3)
    // This switch box is labeled as OnBatteries
    // The switch box only contains the index of the batteries to be turned on
    // Starting from some index, traverse from Left to Right in the list of batteries
    // Compare the jolt size to the one previous to see which one is the largest
    // Swap IFF (if and only if), the current is the largest AND there is enough batteries in the list to fil the switch box

    // Example with Max of on switches = 3
    // Start with the following
    // Jolts:   8 9 7 8 2
    // Index:   0 1 2 3 4

    // Iteration 1
    // Jolts:   8 9 7 8 2
    // Index:   0 1 2 3 4
    // Switch:  1           = 9  (Starting with index 0, Largest number is 9 with 3 remaining slots)
    //
    // Iteration 2
    // Jolts:   8 9 7 8 2
    // Index:   0 1 2 3 4
    // Switch:  1 3         = 98  (Starting with index 2, Largest number is 8 with 1 remaining slot)
    //
    // Iteration 3
    // Jolts:   8 9 7 8 2
    // Index:   0 1 2 3 4
    // Switch:  1 3 4       = 982 (Starting with index 4, Largest (and only number left) is 2 with zero slots remaining
    //

    private static int[] CreateOnBatteryList(char[] batteries, int maxOnBatterySize)
    {
        // This array will hold the index of the battery to be turn ON
        int[] onBatteries = new int[maxOnBatterySize];

        int startingIndex = 0;
        for (int i = 0; i < maxOnBatterySize; i++)
        {
            // Initiailize On Battery value with starting index for comparison
            onBatteries[i] = startingIndex;

            for (int b = startingIndex; b < batteries.Length; b++)
            {
                int remainingBatteries = batteries.Length - (b + 1);
                // Compare Jolts of current battery to the battery that is currently being index in the onBatteries
                // Make sure there are enough batteries left to fill onbattery switch
                if (batteries[b] > batteries[onBatteries[i]] &&
                    remainingBatteries >= maxOnBatterySize - (i + 1))
                    onBatteries[i] = b;
            }
            startingIndex = onBatteries[i] + 1;
        }

        return onBatteries;
    }


    private static long CalcJolts(char[] batteries, int[] onBatteries)
    {
        StringBuilder sb = new StringBuilder(50);

        for (int i = 0; i < onBatteries.Length; i++)
            sb.Append(batteries[onBatteries[i]]);

        return Int64.Parse(sb.ToString());
    }
    #endregion

    #region Solution #2
    // Note: This results in 100^12 computations
    // Pretend there is a switch box of x size (ie 3)
    // This switch box is labeled as OnBatteries
    // The switch box only contains the index of the batteries to be turned on
    // Therefore, only have to deal with a container of size 3
    // Example with Max of on switches = 3
    // Start with the following
    // Jolts:   8 7 9 5 2 
    // Index:   0 1 2 3 4
    // Switch:  0 1 2       = 879
    //
    // Iteration 1
    // Jolts:   8 7 9 5 2 
    // Index:   0 1 2 3 4
    // Switch:  0 1 3       = 875
    //
    // Iteration 2
    // Jolts:   8 7 9 5 2
    // Index:   0 1 2 3 4
    // Switch:  0 1 4       = 872
    //
    // Iteration 3
    // Jolts:   8 7 9 5 2
    // Index:   0 1 2 3 4
    // Switch:  0 2 3       = 895
    //
    // Iteration 4
    // Jolts:   8 7 9 5 2
    // Index:   0 1 2 3 4
    // Switch:  0 2 4       = 892
    //
    // Iteration 5
    // Jolts:   8 7 9 5 2
    // Index:   0 1 2 3 4
    // Switch:  0 3 4       = 852
    //
    // Iteration 6
    // Jolts:   8 7 9 5 2
    // Index:   0 1 2 3 4
    // Switch:  1 2 3       = 795
    //
    // Iteration 7
    // Jolts:   8 7 9 5 2
    // Index:   0 1 2 3 4
    // Switch:  1 2 4       = 792
    //
    // Iteration 8
    // Jolts:   8 7 9 5 2
    // Index:   0 1 2 3 4
    // Switch:  1 3 4       = 752
    //
    // Iteration 9
    // Jolts:   8 7 9 5 2
    // Index:   0 1 2 3 4
    // Switch:  2 3 4       = 952
    //

    /*
    public static long SumBatteryJoltsPart2()
    {
        const int MAX_BATTERIES_ON = 12;
        long totalJoltz = 0;

        ProcessBatteryBanks((batteries) =>
        {
            // This array will hold the index of the battery to be turn ON
            int[] onBatteries = new int[MAX_BATTERIES_ON];
            long joltz = CalcJoltsRecursive(batteries, onBatteries, 0, 0);
            totalJoltz += joltz;
            //Debug.WriteLine($"Battery Bank: {String.Join("", batteries)} - Jolts: {joltz}");
        });

        return totalJoltz;
    }

    // The values in the onBatteries array IS the index of the battery that is ON
    private static long CalcJoltsRecursive(char[] batteries, int[] onBatteries, int currentOnBatteryIndex, int initialValue)
    {
        long maxJolts = 0;

        // Once we exceed our OnBatteries boundry, calculate
        if (currentOnBatteryIndex == onBatteries.Length) 
            return CalcJolts(batteries, onBatteries);

        // Based upon the value of the index, calculate the end index of the batteries array
        int endBatteriesIndex = batteries.Length - onBatteries.Length + currentOnBatteryIndex;

        for (int i = initialValue; i <= endBatteriesIndex; i++)
        {
            onBatteries[currentOnBatteryIndex] = i;
            long jolts = CalcJoltsRecursive(batteries, onBatteries, currentOnBatteryIndex + 1, i + 1);
            maxJolts = (jolts > maxJolts) ? jolts : maxJolts;
        }

        return maxJolts;
    }
    private static long CalcJolts(char[] batteries, int[] onBatteries)
    {
        StringBuilder sb = new StringBuilder(50);

        for (int i = 0; i < onBatteries.Length; i++)
            sb.Append(batteries[onBatteries[i]]);

        return Int64.Parse(sb.ToString());
    }
    */
    #endregion


    #region Solution #1
    // Note: This results in 100^12 computations
    // Pretend there is a on/off switch under each battery
    // Given a maximum number of switches to turn on, iterate through all batteries fliping each one on/off
    // After each iteration, combine all jolt values of batteries that have their switch on
    // Turn jolt values into a number and compare to current largest value
    // When turning on/off switches:
    //    Start with the left most switches (max num of switches) turned on
    //    Then with the right most on switch, move it down the line
    // Example with Max of on switches = 3
    // Start with the following
    // Jolts:   8 7 9 5 2 
    // Index:   0 1 2 3 4
    // Switch:  1 1 1 0 0    = 879
    //
    // Iteration 1
    // Jolts:   8 7 9 5 2 
    // Index:   0 1 2 3 4
    // Switch:  1 1 0 1 0    = 875
    //
    // Iteration 2
    // Jolts:   8 7 9 5 2
    // Index:   0 1 2 3 4
    // Switch:  1 1 0 0 1    = 872
    //
    // Iteration 3
    // Jolts:   8 7 9 5 2
    // Index:   0 1 2 3 4
    // Switch:  1 0 1 1 0    = 895
    //
    // Iteration 4
    // Jolts:   8 7 9 5 2
    // Index:   0 1 2 3 4
    // Switch:  1 0 1 0 1    = 892
    //
    // Iteration 5
    // Jolts:   8 7 9 5 2
    // Index:   0 1 2 3 4
    // Switch:  1 0 0 1 1    = 852
    //
    // Iteration 6
    // Jolts:   8 7 9 5 2
    // Index:   0 1 2 3 4
    // Switch:  0 1 1 1 0    = 795
    //
    // Iteration 7
    // Jolts:   8 7 9 5 2
    // Index:   0 1 2 3 4
    // Switch:  0 1 1 0 1    = 792
    //
    // Iteration 8
    // Jolts:   8 7 9 5 2
    // Index:   0 1 2 3 4
    // Switch:  0 1 0 1 1    = 752
    //
    // Iteration 9
    // Jolts:   8 7 9 5 2
    // Index:   0 1 2 3 4
    // Switch:  0 0 1 1 1    = 952
    //

    /*
    private static long CalcJolts(char[] batteries, int maxBatteriesOn)
    {
        long maxJolts = 0;
        bool[] switches = new bool[batteries.Length];

        // Turn on the first set of switches
        for (int i = 0; i < maxBatteriesOn; i++)
            switches[i] = true;

        // Calc on our initial set of ON switches
        maxJolts = CalcJolts(batteries, switches);

        // Start with the right most ON switch and push it down from left to right
        int startPosition = maxBatteriesOn - 1;
        int lastPosition = switches.Length - 1;

        while (startPosition >= 0)
        {
            for (int i = startPosition; i < lastPosition; i++)
            {
                switches[i] = false;
                switches[i + 1] = true;
                long tmp = CalcJolts(batteries, switches);
                maxJolts = tmp > maxJolts ? tmp : maxJolts;
            }

            startPosition--;
            lastPosition--;
        }

        return maxJolts;
    }


    private static long CalcJolts(char[] batteries, bool[] switches)
    {
        StringBuilder sb = new StringBuilder(50);

        for (int i = 0; i < switches.Length; i++)
            if (switches[i]) sb.Append(batteries[i]);

        return Int64.Parse(sb.ToString());
    }
    */
    #endregion


    private static void ProcessBatteryBanks(Action<char[]> function)
    {
        string[] batteryBanks = File.ReadAllLines("Day3BatteryBanks.txt");
        foreach (string batteryBank in batteryBanks)
            function(batteryBank.ToCharArray());
    }
}
