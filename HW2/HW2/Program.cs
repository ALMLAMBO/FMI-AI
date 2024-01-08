using System.Diagnostics;

namespace HW2 {
	public class Program {
		static int[]? queens;
		static int[]? rowQueens;
		static int[]? leftDiagonalQueens;
		static int[]? rightDiagonalQueens;
		static int queensCount;
		static bool hasConflicts;
		static Random random = new Random();

		static void Main(string[] args) {
			queensCount = int.Parse(Console.ReadLine()!);
			queens = new int[queensCount];
			rowQueens = new int[queensCount];
			leftDiagonalQueens = new int[2 * queensCount - 1];
			rightDiagonalQueens = new int[2 * queensCount - 1];
			hasConflicts = true;

			Stopwatch sw = Stopwatch.StartNew();
			PlaceQueens();
			FindSolution();
			sw.Stop();

			int[] transposeQueens = TransposeQueens();
			Console.WriteLine($"[{string.Join(", ", transposeQueens)}]");
			//PrintQueens(transposeQueens);

            Console.WriteLine();
            Console.WriteLine(sw.Elapsed);
		}

		static int[] TransposeQueens() {
			int[] rotatedQueens = new int[queensCount];

			for (int i = 0; i < queensCount; i++) {
				for (int j = 0; j < queensCount; j++) {
					if (queens![j] == i) {
						rotatedQueens[i] = j;
						break;
					}
				}
			}

			return rotatedQueens;
		}

		static void PrintQueens(int[] transposeQueens) {
			for (int i = 0; i < queensCount; i++) {
				for (int j = 0; j < queensCount; j++) {
					if (transposeQueens![i] == j) {
						Console.Write(" Q ");
					}
					else {
						Console.Write(" _ ");
					}
				}
				Console.WriteLine();
			}
		}

		static void PlaceQueens() {
			int column = 1;
			for (int row = 0; row < queensCount; row++) {
				queens![column] = row;
				rowQueens![row] += 1;
				leftDiagonalQueens![queensCount + column - row - 1] += 1;
				rightDiagonalQueens![column + row] += 1;

				column += 2;
				if (column >= queensCount) {
					column = 0;
				}
			}
		}

		static void FindSolution() {
			int row;
			int column;

			for (int i = 0; i < queensCount; i++) {
				column = GetColumnWithMaxConflicts();

				if (!hasConflicts) {
					break;
				}

				row = GetRowWithMinConflicts(column);
				UpdateState(row, column);
			}

			if (hasConflicts) {
				FindSolution();
			}
		}

		static int GetRowWithMinConflicts(int column) {
			int minConflicts = queensCount + 1;
			List<int> rowsWithMinConflicts = new List<int>();
			int currentConflicts = int.MaxValue;

			for (int currentRow = 0; currentRow < queensCount; currentRow++) {
				if (queens![column] == currentRow) {
					currentConflicts = rowQueens![currentRow]
						+ leftDiagonalQueens![queensCount + column - currentRow - 1]
						+ rightDiagonalQueens![currentRow + column]
						- 3;
				}
				else {
					currentConflicts = rowQueens![currentRow]
						+ leftDiagonalQueens![queensCount + column - currentRow - 1]
						+ rightDiagonalQueens![currentRow + column];
				}

				if (currentConflicts == minConflicts) {
					rowsWithMinConflicts.Add(currentRow);
				}
				else if (currentConflicts < minConflicts) {
					minConflicts = currentConflicts;
					rowsWithMinConflicts.Clear();
					rowsWithMinConflicts.Add(currentRow);
				}
			}

			int randIndex = random.Next(rowsWithMinConflicts.Count);
			return rowsWithMinConflicts[randIndex];
		}

		static int GetColumnWithMaxConflicts() {
			int maxConflicts = int.MinValue;
			int currentRow;
			int currentConflicts;
			List<int> columnsWithMaxConflicts = new List<int>();

			for (int currentColumn = 0; currentColumn < queensCount; currentColumn++) {
				currentRow = queens![currentColumn];
				currentConflicts = rowQueens![currentColumn]
					+ leftDiagonalQueens![queensCount + currentColumn - currentRow - 1]
					+ rightDiagonalQueens![currentRow + currentColumn]
					- 3;

				if (currentConflicts == maxConflicts) {
					columnsWithMaxConflicts.Add(currentColumn);
				}
				else if (currentConflicts > maxConflicts) {
					maxConflicts = currentConflicts;
					columnsWithMaxConflicts.Clear();
					columnsWithMaxConflicts.Add(currentColumn);
				}
			}

			if (maxConflicts == 0) {
				hasConflicts = false;
			}

			int randIndex = random.Next(columnsWithMaxConflicts.Count);
			return columnsWithMaxConflicts[randIndex];
		}

		static void UpdateState(int row, int column) {
			int previousRow = queens![column];
			rowQueens![previousRow] -= 1;
			leftDiagonalQueens![queensCount + column - previousRow - 1] -= 1;
			rightDiagonalQueens![previousRow + column] -= 1;

			queens![column] = row;
			rowQueens![row] += 1;
			leftDiagonalQueens[queensCount + column - row - 1] += 1;
			rightDiagonalQueens[row + column] += 1;
		}
	}
}