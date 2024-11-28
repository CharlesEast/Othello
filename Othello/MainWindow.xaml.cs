using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Othello2D
{
    public partial class MainWindow : Window
    {
        private int[,] board; // Game board
        private Button[,] buttons; // UI buttons
        private int currentPlayer = 1; // 1: Player, 2: AI
        private List<int[]> directions = new List<int[]>
        {
            new int[]{-1, -1}, new int[]{-1, 0}, new int[]{-1, 1},
            new int[]{0, -1},                new int[]{0, 1},
            new int[]{1, -1}, new int[]{1, 0}, new int[]{1, 1}
        };
        private int roundCounter = 1;
        private string difficulty = "Easy";
        private int boardSize = 8; // Default board size

        public MainWindow()
        {
            InitializeComponent();
            // Disable the game grid until the game starts
            GameGrid.IsEnabled = false;
        }

        /// <summary>
        /// Starts the game after selecting difficulty and board size.
        /// </summary>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // Get selected difficulty
            ComboBoxItem selectedDifficulty = (ComboBoxItem)DifficultyComboBox.SelectedItem;
            difficulty = selectedDifficulty.Content.ToString();

            // Get selected board size
            ComboBoxItem selectedBoardSize = (ComboBoxItem)BoardSizeComboBox.SelectedItem;
            boardSize = int.Parse(selectedBoardSize.Content.ToString());

            // Initialize the game board
            InitializeBoard();

            // Disable the selection controls and start button
            DifficultyComboBox.IsEnabled = false;
            BoardSizeComboBox.IsEnabled = false;
            StartButton.IsEnabled = false;

            // Enable the game grid
            GameGrid.IsEnabled = true;

            UpdateBoard();
            UpdateUI();
        }

        /// <summary>
        /// Initializes the game board and UI elements.
        /// </summary>
        private void InitializeBoard()
        {
            board = new int[boardSize, boardSize];
            buttons = new Button[boardSize, boardSize];
            currentPlayer = 1;
            roundCounter = 1;

            GameGrid.Children.Clear();
            GameGrid.RowDefinitions.Clear();
            GameGrid.ColumnDefinitions.Clear();

            // Create rows and columns
            for (int i = 0; i < boardSize; i++)
            {
                GameGrid.RowDefinitions.Add(new RowDefinition());
                GameGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            // Create buttons
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    Button button = new Button
                    {
                        Background = Brushes.Green,
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1),
                    };
                    button.Click += Button_Click;
                    Grid.SetRow(button, x);
                    Grid.SetColumn(button, y);
                    GameGrid.Children.Add(button);
                    buttons[x, y] = button;
                }
            }

            // Set starting positions
            int mid = boardSize / 2;
            board[mid - 1, mid - 1] = 2;
            board[mid, mid] = 2;
            board[mid - 1, mid] = 1;
            board[mid, mid - 1] = 1;
        }

        /// <summary>
        /// Updates the Round Counter and Turn Indicator UI elements.
        /// </summary>
        private void UpdateUI()
        {
            RoundCounterText.Text = $"Round: {roundCounter}";
            TurnIndicatorText.Text = currentPlayer == 1 ? "Your Turn" : "AI's Turn";
        }

        /// <summary>
        /// Handles button clicks on the game board.
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (currentPlayer != 1)
                return; // Not player's turn

            Button clickedButton = sender as Button;
            int x = Grid.GetRow(clickedButton);
            int y = Grid.GetColumn(clickedButton);

            HandlePlayerMove(x, y);
        }

        /// <summary>
        /// Handles the player's move.
        /// </summary>
        private void HandlePlayerMove(int x, int y)
        {
            if (currentPlayer != 1)
                return; // Not player's turn

            if (IsValidMove(x, y, currentPlayer))
            {
                ApplyMove(x, y, currentPlayer);
                UpdateBoard();
                roundCounter++;
                currentPlayer = 2; // Switch to AI
                UpdateUI();

                // Delay AI's turn
                AITurnAsync();
            }
            else
            {
                MessageBox.Show("Invalid Move! Please select a valid position.", "Invalid Move", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Executes the AI's turn asynchronously with a delay.
        /// </summary>
        private async void AITurnAsync()
        {
            await Task.Delay(1000); // Delay for 1 second

            var moves = GetValidMoves(currentPlayer);
            if (moves.Count > 0)
            {
                Tuple<int, int> move = null;

                switch (difficulty)
                {
                    case "Easy":
                        // Easy: Random Move
                        var rnd = new Random();
                        move = moves[rnd.Next(moves.Count)];
                        break;

                    case "Medium":
                        // Medium: Maximize Flipped Pieces
                        int maxFlipped = -1;
                        foreach (var m in moves)
                        {
                            int flipped = CountFlippablePieces(m.Item1, m.Item2, currentPlayer);
                            if (flipped > maxFlipped)
                            {
                                maxFlipped = flipped;
                                move = m;
                            }
                        }
                        break;

                    case "Hard":
                        // Hard: Simple Minimax Algorithm (depth 3)
                        move = GetBestMove(currentPlayer, 3).Item1;
                        break;
                }

                ApplyMove(move.Item1, move.Item2, currentPlayer);
                UpdateBoard();
                roundCounter++;
            }
            else
            {
                MessageBox.Show("AI has no valid moves!", "No Moves", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            currentPlayer = 1; // Switch back to player
            UpdateUI();

            CheckForGameEnd();
        }

        /// <summary>
        /// Updates the visual representation of the board.
        /// </summary>
        private void UpdateBoard()
        {
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    Button button = buttons[x, y];
                    if (board[x, y] == 1)
                    {
                        button.Content = CreateDisc(Brushes.Black);
                    }
                    else if (board[x, y] == 2)
                    {
                        button.Content = CreateDisc(Brushes.White);
                    }
                    else
                    {
                        button.Content = null;
                    }
                }
            }
        }

        /// <summary>
        /// Creates a disc (Ellipse) with the specified color.
        /// </summary>
        private UIElement CreateDisc(Brush color)
        {
            return new System.Windows.Shapes.Ellipse
            {
                Fill = color,
                Width = 40,
                Height = 40,
                Margin = new Thickness(5)
            };
        }

        /// <summary>
        /// Checks if a move is valid for the given player.
        /// </summary>
        private bool IsValidMove(int x, int y, int player)
        {
            if (board[x, y] != 0)
                return false;

            int opponent = (player == 1) ? 2 : 1;
            bool valid = false;

            foreach (var dir in directions)
            {
                int dx = dir[0], dy = dir[1];
                int nx = x + dx, ny = y + dy;
                bool hasOpponentBetween = false;

                while (nx >= 0 && nx < boardSize && ny >= 0 && ny < boardSize && board[nx, ny] == opponent)
                {
                    nx += dx;
                    ny += dy;
                    hasOpponentBetween = true;
                }

                if (hasOpponentBetween && nx >= 0 && nx < boardSize && ny >= 0 && ny < boardSize && board[nx, ny] == player)
                {
                    valid = true;
                    break;
                }
            }

            return valid;
        }

        /// <summary>
        /// Applies a move to the board and flips the necessary pieces.
        /// </summary>
        private void ApplyMove(int x, int y, int player)
        {
            board[x, y] = player;
            FlipPieces(x, y, player);
        }

        /// <summary>
        /// Flips the opponent's pieces based on the current move.
        /// </summary>
        private void FlipPieces(int x, int y, int player)
        {
            int opponent = (player == 1) ? 2 : 1;

            foreach (var dir in directions)
            {
                int dx = dir[0], dy = dir[1];
                int nx = x + dx, ny = y + dy;
                List<Tuple<int, int>> piecesToFlip = new List<Tuple<int, int>>();

                while (nx >= 0 && nx < boardSize && ny >= 0 && ny < boardSize && board[nx, ny] == opponent)
                {
                    piecesToFlip.Add(new Tuple<int, int>(nx, ny));
                    nx += dx;
                    ny += dy;
                }

                if (piecesToFlip.Count > 0 && nx >= 0 && nx < boardSize && ny >= 0 && ny < boardSize && board[nx, ny] == player)
                {
                    foreach (var pos in piecesToFlip)
                    {
                        board[pos.Item1, pos.Item2] = player;
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves a list of valid moves for the given player.
        /// </summary>
        private List<Tuple<int, int>> GetValidMoves(int player)
        {
            List<Tuple<int, int>> moves = new List<Tuple<int, int>>();

            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    if (IsValidMove(i, j, player))
                    {
                        moves.Add(new Tuple<int, int>(i, j));
                    }
                }
            }

            return moves;
        }

        /// <summary>
        /// Counts the number of pieces that would be flipped for a move.
        /// Used in Medium difficulty AI.
        /// </summary>
        private int CountFlippablePieces(int x, int y, int player)
        {
            int totalFlipped = 0;
            int opponent = (player == 1) ? 2 : 1;

            foreach (var dir in directions)
            {
                int dx = dir[0], dy = dir[1];
                int nx = x + dx, ny = y + dy;
                int flipped = 0;

                while (nx >= 0 && nx < boardSize && ny >= 0 && ny < boardSize && board[nx, ny] == opponent)
                {
                    flipped++;
                    nx += dx;
                    ny += dy;
                }

                if (flipped > 0 && nx >= 0 && nx < boardSize && ny >= 0 && ny < boardSize && board[nx, ny] == player)
                {
                    totalFlipped += flipped;
                }
            }

            return totalFlipped;
        }

        /// <summary>
        /// Minimax algorithm to find the best move.
        /// Used in Hard difficulty AI.
        /// </summary>
        private Tuple<Tuple<int, int>, int> GetBestMove(int player, int depth)
        {
            int bestScore = (player == 2) ? int.MinValue : int.MaxValue;
            Tuple<int, int> bestMove = null;

            var moves = GetValidMoves(player);
            if (moves.Count == 0 || depth == 0)
            {
                int score = EvaluateBoard();
                return new Tuple<Tuple<int, int>, int>(null, score);
            }

            foreach (var move in moves)
            {
                int[,] tempBoard = (int[,])board.Clone();
                ApplyMove(move.Item1, move.Item2, player);

                int score = GetBestMove(3 - player, depth - 1).Item2;

                board = tempBoard; // Undo move

                if (player == 2) // Maximizing for AI
                {
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = move;
                    }
                }
                else // Minimizing for player
                {
                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestMove = move;
                    }
                }
            }

            return new Tuple<Tuple<int, int>, int>(bestMove, bestScore);
        }

        /// <summary>
        /// Evaluates the board and returns a score.
        /// Positive score favors AI, negative favors player.
        /// </summary>
        private int EvaluateBoard()
        {
            int score = 0;
            int aiCount = 0, playerCount = 0;

            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    if (board[i, j] == 2)
                        aiCount++;
                    else if (board[i, j] == 1)
                        playerCount++;
                }
            }

            score = aiCount - playerCount;
            return score;
        }

        /// <summary>
        /// Checks if the game has ended and declares the winner.
        /// </summary>
        private void CheckForGameEnd()
        {
            var playerMoves = GetValidMoves(1);
            var aiMoves = GetValidMoves(2);

            if (playerMoves.Count == 0 && aiMoves.Count == 0)
            {
                // Game over
                int playerScore = 0;
                int aiScore = 0;

                for (int i = 0; i < boardSize; i++)
                {
                    for (int j = 0; j < boardSize; j++)
                    {
                        if (board[i, j] == 1)
                            playerScore++;
                        else if (board[i, j] == 2)
                            aiScore++;
                    }
                }

                string message;
                if (playerScore > aiScore)
                {
                    message = $"You Win!\n\nFinal Score:\nYou: {playerScore}\nAI: {aiScore}";
                }
                else if (aiScore > playerScore)
                {
                    message = $"AI Wins!\n\nFinal Score:\nYou: {playerScore}\nAI: {aiScore}";
                }
                else
                {
                    message = $"It's a Tie!\n\nFinal Score:\nYou: {playerScore}\nAI: {aiScore}";
                }

                MessageBox.Show(message, "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);

                // Optionally, reset the game
                ResetGame();
            }
            else if (currentPlayer == 1 && playerMoves.Count == 0)
            {
                MessageBox.Show("You have no valid moves. AI's turn.", "No Moves", MessageBoxButton.OK, MessageBoxImage.Information);
                currentPlayer = 2;
                UpdateUI();
                AITurnAsync();
            }
            else if (currentPlayer == 2 && aiMoves.Count == 0)
            {
                MessageBox.Show("AI has no valid moves. Your turn.", "No Moves", MessageBoxButton.OK, MessageBoxImage.Information);
                currentPlayer = 1;
                UpdateUI();
            }
        }

        /// <summary>
        /// Resets the game to allow playing again.
        /// </summary>
        private void ResetGame()
        {
            // Enable the selection controls and start button
            DifficultyComboBox.IsEnabled = true;
            BoardSizeComboBox.IsEnabled = true;
            StartButton.IsEnabled = true;

            // Disable the game grid
            GameGrid.IsEnabled = false;

            // Clear the game grid
            GameGrid.Children.Clear();
            GameGrid.RowDefinitions.Clear();
            GameGrid.ColumnDefinitions.Clear();

            // Reset UI elements
            RoundCounterText.Text = "";
            TurnIndicatorText.Text = "";
        }
    }
}
