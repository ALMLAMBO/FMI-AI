namespace HW4 {
	public class Program {
		static char[,] board = {
			{'.', '.', '.'},
			{'.', '.', '.'},
			{'.', '.', '.'}
		};

		static void Main(string[] args) {
			bool startsPlayer = false;
			Console.Write("Player starts first(y/n): ");

			if (Console.ReadLine()!.ToLower() == "y") {
				startsPlayer = true;
			}

			Console.WriteLine();
			Console.WriteLine("Start of the game");
			Console.WriteLine("-----------------");
			PrintBoard();

			for (int turn = 0; turn < 9; turn++) {
				if ((turn % 2 == 0 && startsPlayer) ||
					(turn % 2 == 1 && !startsPlayer)) {

					Console.WriteLine("Enter move: ");
					string[] move = Console.ReadLine()!.Split(' ');

					int row = int.Parse(move[0]) - 1;
					int column = int.Parse(move[1]) - 1;

					if (board[row, column] != '.' ||
						row > board.GetLength(0) || column > board.GetLength(1)) {

						Console.WriteLine("Invalid move!");
						turn--;
						continue;
					}

					board[row, column] = 'X';
				}
				else {
					Tuple<int, int> move = BestMove();
					Console.WriteLine($"Computer's move: {move.Item1 + 1}, {move.Item2 + 1}");
					board[move.Item1, move.Item2] = 'O';
				}

				PrintBoard();

				if(Winner('X')) {
                    Console.WriteLine("You win");
					break;
				}
				else if(Winner('O')) {
                    Console.WriteLine("Computer wins");
					break;
                }
				else if(BoardFull()) {
                    Console.WriteLine("Draw");
					break;
                }
			}

            Console.WriteLine("End of Game");
        }

		static void PrintBoard() {
			for (int i = 0; i < board.GetLength(0); i++) {
				for (int j = 0; j < board.GetLength(1); j++) {
					Console.Write(board[i, j] + " ");
				}
				Console.WriteLine();
			}
			Console.WriteLine();
		}

		static bool Winner(char player) {
			for (int i = 0; i < board.GetLength(0); i++) {
				if (board[0, i] == player && board[1, i] == player && board[2, i] == player) {
					return true;
				}

				if (board[i, 0] == player && board[i, 1] == player && board[i, 2] == player) {
					return true;
				}
			}

			if (board[0, 0] == player && board[1, 1] == player && board[2, 2] == player) {
				return true;
			}

			if (board[2, 0] == player && board[1, 1] == player && board[0, 2] == player) {
				return true;
			}

			return false;
		}

		static bool BoardFull() {
			for (int i = 0; i < board.GetLength(0); i++) {
				for (int j = 0; j < board.GetLength(1); j++) {
					if (board[i, j] == '.') {
						return false;
					}
				}
			}

			return true;
		}

		static int Evaluate() {
			if (Winner('O')) {
				return 1;
			}

			if (Winner('X')) {
				return -1;
			}

			if (BoardFull()) {
				return 0;
			}

			return int.MinValue;
		}

		static Tuple<int, int> BestMove() {
			int bestValue = int.MinValue;

			Tuple<int, int> bestMove = Tuple.Create(-1, -1);
			for (int i = 0; i < board.GetLength(0); i++) {
				for (int j = 0; j < board.GetLength(1); j++) {
					if (board[i, j] == '.') {
						board[i, j] = 'O';

						int moveValue = MinMax(0, false, int.MinValue, int.MaxValue);
						board[i, j] = '.';

						if (moveValue > bestValue) {
							bestMove = Tuple.Create(i, j);
							bestValue = moveValue;
						}
					}
				}
			}

			return bestMove;
		}

		static int MinMax(int depth, bool maximising, int alpha, int beta) {
			int score = Evaluate();

			if (score != int.MinValue) {
				return score;
			}

			if (maximising) {
				int maxValue = int.MinValue;

				for (int i = 0; i < board.GetLength(0); i++) {
					for (int j = 0; j < board.GetLength(1); j++) {
						if (board[i, j] == '.') {
							board[i, j] = 'O';
							maxValue = Math.Max(maxValue, MinMax(depth + 1, false, alpha, beta));
							board[i, j] = '.';
							alpha = Math.Max(alpha, maxValue);

							if (beta <= alpha) {
								break;
							}
						}
					}
				}

				return maxValue;
			}
			else {
				int minValue = int.MaxValue;

				for (int i = 0; i < board.GetLength(0); i++) {
					for (int j = 0; j < board.GetLength(1); j++) {
						if (board[i, j] == '.') {
							board[i, j] = 'X';
							minValue = Math.Min(minValue, MinMax(depth + 1, true, alpha, beta));
							board[i, j] = '.';
							beta = Math.Max(beta, minValue);

							if (beta <= alpha) {
								break;
							}
						}
					}
				}

				return minValue;
			}
		}
	}
}