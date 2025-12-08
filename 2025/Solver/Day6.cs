using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Buffers;

namespace Solver;

internal static class Day6
{
    private static char[] Operators = { '+', '-', '*', '/' };

    private class MathProblem
    {
        public char Operator { get; set; }
        public IList<string> Operands { get; set; }
    }

    public static long SolveMathWorksheetPart1()
    {
        long answer = 0;

        ProcessWorksheet((mp) =>
        {
            long tmp = 0;
            bool isInitialized = false;
            foreach (string value in mp.Operands)
            {
                long parsedValue = Int64.Parse(value.Trim());

                if (!isInitialized)
                {
                    tmp = parsedValue;
                    isInitialized = true;
                    continue;
                }

                switch (mp.Operator)
                {
                    case '+':
                        tmp += parsedValue;
                        break;

                    case '-':
                        tmp -= parsedValue;
                        break;

                    case '*':
                        tmp *= parsedValue;
                        break;

                    case '/':
                        tmp /= parsedValue;
                        break;
                }
            }

            answer += tmp;
        });

        return answer;
    }

    public static long SolveMathWorksheetPart2()
    {
        long answer = 0;

        // Process each operator, but in column form from right to left to build the intended operator
        ProcessWorksheet((mp) =>
        {
            long tmp = 0;
            bool isInitialized = false;
            int rowCount = mp.Operands.Count;
            int colCount = mp.Operands[0].ToCharArray().Length;
            
            for (int col = colCount - 1; col >= 0; col--)
            {
                StringBuilder sb = new StringBuilder();
                for (int row = 0; row < rowCount; row++)
                    sb.Append(mp.Operands[row][col]);

                string op = sb.ToString().Trim();
                if (String.IsNullOrWhiteSpace(op)) continue;

                long operand = Int64.Parse(op);

                if (!isInitialized)
                {
                    tmp = operand;
                    isInitialized = true;
                    continue;
                }

                switch (mp.Operator)
                {
                    case '+':
                        tmp += operand;
                        break;

                    case '-':
                        tmp -= operand;
                        break;

                    case '*':
                        tmp *= operand;
                        break;

                    case '/':
                        tmp /= operand;
                        break;
                }
            }

            answer += tmp;
        });

        return answer;
    }

    private static void ProcessWorksheet(Action<MathProblem> function)
    {
        string[] worksheet = File.ReadAllLines("Day6MathWorksheet.txt");

        // Assumptions
        //  Last row will contain the operator
        //  The columns are not fixed length
        //  The definition of a column is that for a specific column there is a space ' ' for every single row

        // Work from top down, left to right
        int rowCount = worksheet.Length;
        int colCount = worksheet[0].Length;
        bool isNewMathProblem = false;
        StringBuilder[] operandsBuilder = CreateOperandsBuilder(rowCount - 1);
        MathProblem mp = new MathProblem();

        for (int col = 0; col < colCount; col++)
        {
            int delimiterCnt = 0;

            if (isNewMathProblem)
            {
                isNewMathProblem = false;
                operandsBuilder = CreateOperandsBuilder(rowCount - 1);
                mp = new MathProblem();
            }

            for (int row = 0; row < rowCount; row++)
            {
                char value = worksheet[row].Substring(col, 1)[0];

                if (value == ' ')  delimiterCnt++;
                if (row < rowCount - 1) operandsBuilder[row].Append(value);                // aways append unless it is an operator / last row
                if (row == rowCount - 1 && Operators.Contains(value)) mp.Operator = value;
            }

            if (delimiterCnt == rowCount ||
                col == colCount - 1)
            {
                isNewMathProblem = true;
                mp.Operands = operandsBuilder.Select(x => x.ToString()).ToList();
                function(mp);
            }
        }
    }

    private static StringBuilder[] CreateOperandsBuilder(int size)
    {
        StringBuilder[] stringBuilders = new StringBuilder[size];
        for (int i = 0; i < size; i++)
            stringBuilders[i] = new StringBuilder();

        return stringBuilders;
    }
}