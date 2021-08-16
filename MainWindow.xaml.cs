using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace cactpot_gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Dictionary<int, Tuple<Button, Uri>> _winningIdToInfo;

        Dictionary<int, Button> _slotIdToButton;

        List<BoardRepresentation> _boards = new List<BoardRepresentation>();

        // This will represent our play board that we uncover numbers on    
        BoardRepresentation _playBoard = new BoardRepresentation();

        int stage = 0;

        public MainWindow()
        {
            InitializeComponent();

            _winningIdToInfo = new Dictionary<int, Tuple<Button, Uri>> {
                { 0, new Tuple<Button, Uri>(btn_right_1, new Uri(@"graphics/edgy-16x16-arrow_right_r.png", UriKind.Relative)) },
                { 1, new Tuple<Button, Uri>(btn_right_2, new Uri(@"graphics/edgy-16x16-arrow_right_r.png", UriKind.Relative)) },
                { 2, new Tuple<Button, Uri>(btn_right_3, new Uri(@"graphics/edgy-16x16-arrow_right_r.png", UriKind.Relative)) },
                { 3, new Tuple<Button, Uri>(btn_down_1, new Uri(@"graphics/edgy-16x16-arrow_down_r.png", UriKind.Relative)) },
                { 4, new Tuple<Button, Uri>(btn_down_2, new Uri(@"graphics/edgy-16x16-arrow_down_r.png", UriKind.Relative)) },
                { 5, new Tuple<Button, Uri>(btn_down_3, new Uri(@"graphics/edgy-16x16-arrow_down_r.png", UriKind.Relative)) },
                { 6, new Tuple<Button, Uri>(btn_down_right, new Uri(@"graphics/edgy-16x16-arrow_down_right_r.png", UriKind.Relative)) },
                { 7, new Tuple<Button, Uri>(btn_down_left, new Uri(@"graphics/edgy-16x16-arrow_down_left_r.png", UriKind.Relative)) }
            };

            _slotIdToButton = new Dictionary<int, Button> {
                { 0, btn_id_0 },
                { 1, btn_id_1 },
                { 2, btn_id_2 },
                { 3, btn_id_3 },
                { 4, btn_id_4 },
                { 5, btn_id_5 },
                { 6, btn_id_6 },
                { 7, btn_id_7 },
                { 8, btn_id_8 }
            };

            InitializeGame();
        }

        private void mi_click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Update UI
                MenuItem mi = sender as MenuItem;
                ContextMenu menu = mi.Parent as ContextMenu;
                Button owningButton = menu.PlacementTarget as Button;
                string header = mi.Header.ToString();
                Grid contentGrid = owningButton.Content as Grid;
                TextBlock text = contentGrid.Children[1] as TextBlock;
                text.Text = header;
                byte value = byte.Parse(header);

                // Remove item from context menu
                mi.Visibility = Visibility.Collapsed;

                // Update model
                int slot = int.Parse(owningButton.Tag.ToString());

                // Remove button as clickable
                owningButton.IsEnabled = false;

                MakeMove(slot, value);
            }
            catch (Exception ex)
            {
                MessageBox.Show("A handled exception just occurred: " + ex.StackTrace, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /*
         * Heaps algorithm for generating permutations, which is used to generate every possible board
         * configuration
        */
        void PermuteBoards(byte[] values, int size, int n, List<BoardRepresentation> storage)
        {
            if (size == 1)
            {
                BoardRepresentation board = new BoardRepresentation(values);
                storage.Add(board);
            }

            for (byte i = 0; i < size; i++)
            {
                PermuteBoards(values, size - 1, n, storage);

                byte swapIndex = i;
                if (size % 2 == 1)
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
        int[] CalculateAverageScoreByWinningIds(List<BoardRepresentation> boards)
        {
            int[] averageScoreByWinningId = new int[8];
            int sum = 0;

            for (int winningId = 0; winningId < 8; winningId++)
            {
                foreach (var board in boards)
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
        int[] CalculateScoreImpactByCell(int[] averageScoreByWinningId)
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
        int[] FindAllIndexOf(int[] arr, int value)
        {
            List<int> indexes = new List<int>();
            for (int x = 0; x < arr.Length; x++)
            {
                if (arr[x] == value)
                {
                    indexes.Add(x);
                }
            }

            return indexes.ToArray();
        }

        void DoCellSuggestion(List<BoardRepresentation> boards)
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
            foreach (var cellId in _playBoard.UncoveredSlots())
            {
                sumOfAverageScoreByWinningIdPerCell[cellId] = 0;
            }

            int maxImpact = sumOfAverageScoreByWinningIdPerCell.Max();
            int[] cellsToChooseNext = FindAllIndexOf(sumOfAverageScoreByWinningIdPerCell, maxImpact);

            // Reset all coin buttons to yellow
            foreach (var tuple in _slotIdToButton)
            {
                Grid buttonGrid = tuple.Value.Content as Grid;
                Image buttonImage = buttonGrid.Children[0] as Image;
                buttonImage.Source = new BitmapImage(new Uri(@"graphics/coin_7.png", UriKind.Relative));
            }

            // Mark suggested moves as red coins
            foreach (var item in cellsToChooseNext)
            {
                Grid buttonGrid = _slotIdToButton[item].Content as Grid;
                Image buttonImage = buttonGrid.Children[0] as Image;
                buttonImage.Source = new BitmapImage(new Uri(@"graphics/coin_8.png", UriKind.Relative));
            }
        }

        void DoWinSuggestion()
        {
            //
            // Find which winning ID we should choose. This looks at all remaining boards
            // and chooses the winning ID (row, col, diagonal) that has the highest chance to give us
            // the biggest payout. Highest change is based on the average score of any given winning ID
            // across all remaining boards
            //
            int[] averageScoreByWinningId = CalculateAverageScoreByWinningIds(_boards);

            int maxAveragePayout = averageScoreByWinningId.Max();
            int[] winningIdsToChoose = FindAllIndexOf(averageScoreByWinningId, maxAveragePayout);

            // Reset button color to yellow
            foreach (var tuple in _slotIdToButton)
            {
                Grid buttonGrid = tuple.Value.Content as Grid;
                Image buttonImage = buttonGrid.Children[0] as Image;
                buttonImage.Source = new BitmapImage(new Uri(@"graphics/coin_7.png", UriKind.Relative));
            }

            // Disable coin buttons
            foreach (var tuple in _slotIdToButton)
            {
                tuple.Value.IsEnabled = false;
                tuple.Value.Foreground = new SolidColorBrush(Color.FromRgb(0x31, 0x5F, 0x42));
            }

            // For each winning solution, set the background image appropriately
            foreach (var winningId in winningIdsToChoose)
            {
                var tuple = _winningIdToInfo[winningId];
                Image buttonImage = tuple.Item1.Content as Image;
                buttonImage.Source = new BitmapImage(tuple.Item2);
            }
        }

        void MakeMove(int slot, byte value)
        {
            if (stage == 0)
            {
                _playBoard.AssignValue(slot, value);

                // Find the remaining _boards
                _boards.RemoveAll(item => !item.IsReachable(_playBoard));

                // Figure out which cells we should uncover next
                int maxScore = _boards.Max(item => item.EvaluateBestScoreAvailable());
                List<BoardRepresentation> bestBoards = _boards.Where(item => item.EvaluateBestScoreAvailable() == maxScore).ToList<BoardRepresentation>();
                DoCellSuggestion(bestBoards);
            }
            else if (stage < 3)
            {
                _playBoard.AssignValue(slot, value);
                Console.WriteLine(_playBoard.ToString());

                // Find the remaining boards
                _boards.RemoveAll(item => !item.IsReachable(_playBoard));

                // Figure out which cells we should uncover next
                int maxScore = _boards.Max(item => item.EvaluateBestScoreAvailable());
                List<BoardRepresentation> bestBoards = _boards.Where(item => item.EvaluateBestScoreAvailable() == maxScore).ToList<BoardRepresentation>();
                DoCellSuggestion(bestBoards);
            }
            else
            {
                _playBoard.AssignValue(slot, value);

                // Find the remaining _boards
                _boards.RemoveAll(item => !item.IsReachable(_playBoard));

                // Game is over, pick a winning ID(s) for the user to select. There may be more than one best option
                DoWinSuggestion();
            }

            stage++;
        }

        void InitializeGame()
        {
            // Create every permutation of a 1-9 array, which represent all the final board states
            PermuteBoards(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 9, 9, _boards);
        }
    }
}
