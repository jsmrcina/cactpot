using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace cactpot_gui
{
    class BoardRepresentation
    {
        public byte[] points = new byte[9];

        private Dictionary<int, int> scoreToPayout = new Dictionary<int, int> {
           { 6, 10000 },
            { 7, 36 },
            { 8, 720 },
            { 9, 360 },
            { 10, 80 },
            { 11, 252 },
            { 12, 108 },
            { 13, 72 },
            { 14, 54 },
            { 15, 180 },
            { 16, 72 },
            { 17, 180 },
            { 18, 119 },
            { 19, 36 },
            { 20, 306 },
            { 21, 1080 },
            { 22, 144 },
            { 23, 1800 },
            { 24, 3600 }
        };

        public BoardRepresentation()
        {
            // 0 1 2
            // 3 4 5
            // 6 7 8
            for (int x = 0; x < 9; x++)
            {
                points[x] = 0;
            }
        }

        public BoardRepresentation(BoardRepresentation other)
        {
            AssignArray(other.points);
        }

        public BoardRepresentation(byte[] values)
        {
            AssignArray(values);
        }

        /*
         * Gets a list of all slots that are already uncovered (i.e. not set to zero in points array)
        */
        public int[] UncoveredSlots()
        {
            List<int> uncoveredSlots = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 };

            for (int x = 0; x < 9; x++)
            {
                if (points[x] == 0)
                {
                    uncoveredSlots.Remove(x);
                }
            }

            return uncoveredSlots.ToArray();
        }

        /*
         * Assigns a value to a specific slot, thus marking it as uncovered
        */
        public void AssignValue(int slot, byte number)
        {
            points[slot] = number;
            for (int x = 0; x < 9; x++)
            {
                if (points[x] == 0)
                {
                    return;
                }
            }
        }

        /*
         * Assigns a new point array, used for copying boards
        */
        private void AssignArray(byte[] values)
        {
            points = (byte[])values.Clone();
        }

        /*
         * Converts a winning ID (integer) into the winning selection as a string
        */
        public static string ConvertWinningIdToString(int winningId)
        {
            switch (winningId)
            {
                case 0: return "First Row";
                case 1: return "Second Row";
                case 2: return "Third Row";
                case 3: return "First Column";
                case 4: return "Second Column";
                case 5: return "Third Column";
                case 6: return "Top Left Diagonal";
                case 7: return "Top Right Diagonal";
            }

            throw new InvalidOperationException();
        }

        /*
         * Evaluates the score for a given winning ID
        */
        public int EvaluateScoreByWinningId(int id)
        {
            switch (id)
            {
                case 0: return scoreToPayout.GetValueOrDefault(points[0] + points[1] + points[2], 0);
                case 1: return scoreToPayout.GetValueOrDefault(points[3] + points[4] + points[5], 0);
                case 2: return scoreToPayout.GetValueOrDefault(points[6] + points[7] + points[8], 0);
                case 3: return scoreToPayout.GetValueOrDefault(points[0] + points[3] + points[6], 0);
                case 4: return scoreToPayout.GetValueOrDefault(points[1] + points[4] + points[7], 0);
                case 5: return scoreToPayout.GetValueOrDefault(points[2] + points[5] + points[8], 0);
                case 6: return scoreToPayout.GetValueOrDefault(points[0] + points[4] + points[8], 0);
                case 7: return scoreToPayout.GetValueOrDefault(points[2] + points[4] + points[6], 0);
            }

            throw new InvalidOperationException();
        }

        /*
         * Only valid for completed boards. Returns the best score available on the board.
        */
        public int EvaluateBestScoreAvailable()
        {
            int bestScore = 0;

            Action<int> FindMaxOfBestAndNew = (int newScore) =>
            {
                if (newScore > bestScore)
                {
                    bestScore = newScore;
                }
            };

            // Evaluate rows
            FindMaxOfBestAndNew(EvaluateScoreByWinningId(0));
            FindMaxOfBestAndNew(EvaluateScoreByWinningId(1));
            FindMaxOfBestAndNew(EvaluateScoreByWinningId(2));

            // Evaluate cols
            FindMaxOfBestAndNew(EvaluateScoreByWinningId(3));
            FindMaxOfBestAndNew(EvaluateScoreByWinningId(4));
            FindMaxOfBestAndNew(EvaluateScoreByWinningId(5));

            // Evaluate diagonals
            FindMaxOfBestAndNew(EvaluateScoreByWinningId(6));
            FindMaxOfBestAndNew(EvaluateScoreByWinningId(7));

            return bestScore;
        }

        /*
         * Returns true if this board is reachable by uncovering
         * more squares on the other board. Zeroes are covered slots,
         * so they are ignored. All other slots must match.
        */
        public bool IsReachable(BoardRepresentation other)
        {
            for (int x = 0; x < 9; x++)
            {
                if (other.points[x] != 0 &&
                    other.points[x] != points[x])
                {
                    return false;
                }
            }

            return true;
        }

        public override String ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"Value: {EvaluateBestScoreAvailable()}");
            for (int x = 0; x < 9; x++)
            {
                builder.Append(points[x]);

                if ((x + 1) % 3 == 0)
                {
                    builder.AppendLine();
                }
                else if (x != 8)
                {
                    builder.Append(" ");
                }
            }

            return builder.ToString();
        }

    }
}