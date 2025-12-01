using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Solver;

internal static class Day1
{
    private const int MAX_CLICKS = 100;
    private const int START_DIAL = 50;

    public static int DecodePasswordPart1()
    {
        int password = 0;
        int dial = START_DIAL;
        ProcessRotations((direction, clicks) =>
        {
            dial = CalcDialPosition(dial, clicks);
            if (dial == 0) password++;                      
        });

        return password;
    }

    public static int DecodePasswordPart2()
    {
        int password = 0;
        int dial = START_DIAL;
        ProcessRotations((direction, clicks) =>
        {
            password += Math.Abs(clicks) / MAX_CLICKS;          // Count for each complete rotation
            var prevDial = dial;
            dial = CalcDialPosition(dial, clicks);

            if (dial == 0 ||                                                    // Check if our new position is 0
                (direction == 'L' && prevDial != 0 && dial > prevDial) ||       // Moved Left past zero
                (direction == 'R' && prevDial > dial))                          // Moved Right past zero
                password++;
        });

        return password;
    }

    private static void ProcessRotations(Action<char, int> function)
    {
        string[] rotations = File.ReadAllLines("Day1SafeRotations.txt");

        foreach (string rotation in rotations)
        {
            char direction = rotation[0];
            int clicks = Int32.Parse(rotation.Substring(1));
            if (direction == 'L') clicks = -clicks;
            function(direction, clicks);
        }
    }

    private static int CalcDialPosition(int currentDialPosition, int clicks)
    {
        // This will position dial either positive or negative from 0.
        // Will be in the max range: -99 to 99
        var newDialPosition = (currentDialPosition + clicks) % MAX_CLICKS;

        // This will set the dial to a postive number
        return (newDialPosition < 0) ? MAX_CLICKS + newDialPosition : newDialPosition;
    }
}
