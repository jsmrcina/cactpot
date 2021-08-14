using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace cactpot
{
    internal class BoardRepresentation
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
            points = (byte[]) values.Clone();
        }

        /*
         * Converts a winning ID (integer) into the winning selection as a string
        */
        public static string ConvertWinningIdToString(int winningId)
        {
            switch(winningId)
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
            switch(id)
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
            for(int x = 0; x < 9; x++)
            {
                if(other.points[x] != 0 &&
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

    class Program
    {
        /*
         * Heaps algorithm for generating permutations, which is used to generate every possible board
         * configuration
        */
        static void PermuteBoards(byte[] values, int size, int n, List<BoardRepresentation> storage)
        {
            if(size == 1)
            {
                BoardRepresentation board = new BoardRepresentation(values);
                storage.Add(board);
            }

            for(byte i = 0; i < size; i++)
            {
                PermuteBoards(values, size - 1, n, storage);

                byte swapIndex = i;
                if(size % 2 == 1)
                {
                    swapIndex = 0;
                }

                byte temp = values[swapIndex];
                values[swapIndex] = values[size - 1];
                values[size - 1] = temp;
            }
        }

        /*
         * Winning Ids are the different final selections you can make (row, col, diagonal).
         * This function returns an array containing the average score by winning ID per ID index.
        */
        static int[] CalculateAverageScoreByWinningIds(List<BoardRepresentation> boards)
        {
            int[] averageScoreByWinningId = new int[8];
            int sum = 0;

            for(int winningId = 0; winningId < 8; winningId++)
            {
                foreach(var board in boards)
                {
                    sum += board.EvaluateScoreByWinningId(winningId);
                }

                averageScoreByWinningId[winningId] = (int)(sum / boards.Count);
                sum = 0;
            }

            return averageScoreByWinningId;
        }

        /*
         * Here we calculate each cells possible impact on the winning score.
         * For example, cell 0 is affected by row 0, col 0, and top left diagonal.
         * This helps us choose which cell to uncover next.

         * Note that this intentionally weights cells that are part of more winning solutions
         * since those cells give us more information than others -- thus we don't do an average, but a sum
         */
        static int[] CalculateScoreImpactByCell(int[] averageScoreByWinningId)
        {


            int[] sumOfAverageScoreByWinningIdPerCell = new int[9];

            // Cell 0 (row0, col0, top left diagonal)
            sumOfAverageScoreByWinningIdPerCell[0] = averageScoreByWinningId[0] + averageScoreByWinningId[3] + averageScoreByWinningId[6];

            // Cell 1 (row0, col1)
            sumOfAverageScoreByWinningIdPerCell[1] = averageScoreByWinningId[0] + averageScoreByWinningId[4];

            // Cell 2 (row0, col2, top right diagonal)
            sumOfAverageScoreByWinningIdPerCell[2] = averageScoreByWinningId[0] + averageScoreByWinningId[5] + averageScoreByWinningId[7];

            // Cell 3 (row1, col0)
            sumOfAverageScoreByWinningIdPerCell[3] = averageScoreByWinningId[1] + averageScoreByWinningId[3];

            // Cell 4 (row1, col1, top left diagonal, top right diagonal)
            sumOfAverageScoreByWinningIdPerCell[4] = averageScoreByWinningId[1] + averageScoreByWinningId[4] + averageScoreByWinningId[6] + averageScoreByWinningId[7];

            // Cell 5 (row1, col2)
            sumOfAverageScoreByWinningIdPerCell[5] = averageScoreByWinningId[1] + averageScoreByWinningId[5];

            // Cell 6 (row2, col0, top right diagonal)
            sumOfAverageScoreByWinningIdPerCell[6] = averageScoreByWinningId[2] + averageScoreByWinningId[3] + averageScoreByWinningId[7];

            // Cell 7 (row2, col1)
            sumOfAverageScoreByWinningIdPerCell[7] = averageScoreByWinningId[2] + averageScoreByWinningId[4];

            // Cell 8 (row2, col2, top left diagonal)
            sumOfAverageScoreByWinningIdPerCell[8] = averageScoreByWinningId[2] + averageScoreByWinningId[5] + averageScoreByWinningId[6];

            return sumOfAverageScoreByWinningIdPerCell;

        }

        /*
         * Finds all indexes in an array with a given value
        */
        static int[] FindAllIndexOf(int[] arr, int value)
        {            
            List<int> indexes = new List<int>();
            for(int x = 0; x < arr.Length; x++)
            {
                if(arr[x] == value)
                {
                    indexes.Add(x);
                }
            }

            return indexes.ToArray();
        }

        static void Main(string[] args)
        {
            // Create every permutation of a 1-9 array, which represent all the final board states
            List<BoardRepresentation> boards = new List<BoardRepresentation>();
            PermuteBoards(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 9, 9, boards);

            // This will represent our play board that we uncover numbers on
            BoardRepresentation playBoard = new BoardRepresentation();

            Action<List<BoardRepresentation>> doCellSuggestion = (List<BoardRepresentation> boards) =>
            {
                //
                // Find which cell(s) we should choose next. Strategy employed is we should uncover
                // the cell that has the highest potential to raise our average payout. Thus, cells which
                // are part of multiple winning IDs (e.g. cell 0 which participates in row0, col0, and top left diagonal)
                // are weighted more heavily to be selected for getting more information.
                //
                int[] averageScoreByWinningId = CalculateAverageScoreByWinningIds(boards);
                int[] sumOfAverageScoreByWinningIdPerCell = CalculateScoreImpactByCell(averageScoreByWinningId);

                // Zero out any cells that are already uncovered
                foreach(var cellId in playBoard.UncoveredSlots())
                {
                    sumOfAverageScoreByWinningIdPerCell[cellId] = 0;
                }

                int maxImpact = sumOfAverageScoreByWinningIdPerCell.Max();
                int[] cellsToChooseNext = FindAllIndexOf(sumOfAverageScoreByWinningIdPerCell, maxImpact);
                Console.WriteLine("Choose one of these cells next: " + String.Join(", ", cellsToChooseNext));
            };

            Action<List<BoardRepresentation>> doWinSuggestion = (List<BoardRepresentation> boards) =>
            {
                //
                // Find which winning ID we should choose. This looks at all remaining boards
                // and chooses the winning ID (row, col, diagonal) that has the highest chance to give us
                // the biggest payout. Highest change is based on the average score of any given winning ID
                // across all remaining boards
                //
                int[] averageScoreByWinningId = CalculateAverageScoreByWinningIds(boards);

                int maxAveragePayout = averageScoreByWinningId.Max();
                int[] winningIdsToChoose = FindAllIndexOf(averageScoreByWinningId, maxAveragePayout);
                Console.WriteLine("Choose one of these to get the best outcome: ");

                foreach(var winningId in winningIdsToChoose)
                {
                    Console.WriteLine(BoardRepresentation.ConvertWinningIdToString(winningId));
                }
            };

            Console.Write("Enter initial slot that is shown: ");
            int slot = Convert.ToInt32(Console.ReadLine());

            Console.Write("Enter initial value that is shown: ");
            byte value = Convert.ToByte(Console.ReadLine());
            playBoard.AssignValue(slot, value);
            Console.WriteLine(playBoard.ToString());

            // Find the remaining boards
            boards.RemoveAll(item => !item.IsReachable(playBoard));
        
            // Figure out which cells we should uncover next
            int maxScore = boards.Max(item => item.EvaluateBestScoreAvailable());
            List<BoardRepresentation> bestBoards = boards.Where(item => item.EvaluateBestScoreAvailable() == maxScore).ToList<BoardRepresentation>();
            doCellSuggestion(bestBoards);

            // Now that we have the initial configuration, we get to uncover 3 numbers before making a final selection for a payout
            int stage = 0;
            while (stage < 3)
            {
                Console.Write("Select slot to uncover: ");
                slot = Convert.ToInt32(Console.ReadLine());

                Console.Write("Enter uncovered value: ");
                value = Convert.ToByte(Console.ReadLine());

                playBoard.AssignValue(slot, value);
                Console.WriteLine(playBoard.ToString());

                // Find the remaining boards
                boards.RemoveAll(item => !item.IsReachable(playBoard));

                if(stage != 2)
                {
                    // Figure out which cells we should uncover next
                    maxScore = boards.Max(item => item.EvaluateBestScoreAvailable());
                    bestBoards = boards.Where(item => item.EvaluateBestScoreAvailable() == maxScore).ToList<BoardRepresentation>();
                    doCellSuggestion(bestBoards);
                }
                else
                {
                    // Game is over, pick a winning ID(s) for the user to select. There may be more than one best option
                    doWinSuggestion(boards);
                }

                stage++;
            }
        }
    }
}
