using System.Diagnostics;

namespace HW1 {
	internal class Program {
		static List<string> path = new List<string>();
		static List<int> tiles = new List<int>();
		static int targetZeroIndex = 0;
		static int currentZeroIndex = 0;
		static int boardSize;
		const int FOUND = -2;

		static void Main(string[] args) {
			int tilesCount = int.Parse(Console.ReadLine()!);
			int zeroTile = int.Parse(Console.ReadLine()!);
			boardSize = (int)Math.Sqrt(tilesCount + 1);

			if(zeroTile == -1) {
				targetZeroIndex = boardSize * boardSize - 1;
			}
			else {
				targetZeroIndex = zeroTile;
			}

			for(int i = 0; i < boardSize; i++) {
				int[] tempTiles = Console.ReadLine()!
					.Split(' ')!
					.Select(int.Parse)
					.ToArray();

				tiles.AddRange(tempTiles);
			}

			currentZeroIndex = CurrentIndexOfZero();

			//int inversions = 0;
			//for (int i = 0; i < tiles.Count - 1; i++) {
			//	if (tiles[i] > tiles[i + 1]) {
			//		inversions++;
			//	}
			//}

			//if(inversions % 2 != 0) {
   //             Console.WriteLine(-1);
			//	return;
   //         }

			Stopwatch sw = Stopwatch.StartNew();
			IdaStar();
			sw.Stop();

            Console.WriteLine($"Elapsed: {sw.Elapsed}");
        }

		static void IdaStar() {
			path.Add("start");
			int threshold = Manhattan();
			int temp;

			while(true) {
				temp = IdaStarSearch(0, threshold);
				
				if(temp == FOUND) {
					break;
				}

				threshold = temp;
			}
		}

		static int IdaStarSearch(int g, int threshold) {
			int h = g + Manhattan();

			if(h > threshold) {
				return h;
			}

			if(IsTargetState()) {
                Console.WriteLine(path.Count - 1);
				foreach (string move in path) {
					if(move != "start") {
						Console.WriteLine(move);
					}
                }

                return FOUND;
			}

			int min = int.MaxValue;
			int temp;

			string prevMove = path[path.Count - 1];
			
			if(prevMove != "up" && MoveDown()) {
				path.Add("down");
				temp = IdaStarSearch(g + 1, threshold);

				if(temp == FOUND) {
					return temp;
				}

				if(temp < min) {
					min = temp;
				}

				path.RemoveAt(path.Count - 1);
				MoveUp();
			}

			if (prevMove != "down" && MoveUp()) {
				path.Add("up");
				temp = IdaStarSearch(g + 1, threshold);

				if (temp == FOUND) {
					return temp;
				}

				if (temp < min) {
					min = temp;
				}

				path.RemoveAt(path.Count - 1);
				MoveDown();
			}

			if (prevMove != "left" && MoveRight()) {
				path.Add("right");
				temp = IdaStarSearch(g + 1, threshold);

				if (temp == FOUND) {
					return temp;
				}

				if (temp < min) {
					min = temp;
				}

				path.RemoveAt(path.Count - 1);
				MoveLeft();
			}

			if (prevMove != "right" && MoveLeft()) {
				path.Add("left");
				temp = IdaStarSearch(g + 1, threshold);

				if (temp == FOUND) {
					return temp;
				}

				if (temp < min) {
					min = temp;
				}

				path.RemoveAt(path.Count - 1);
				MoveRight();
			}

			return min;
		}

		static bool MoveUp() {
			if (currentZeroIndex / boardSize == boardSize - 1) {
				return false;
			}

			SwapTiles(currentZeroIndex, currentZeroIndex + boardSize);
			currentZeroIndex = currentZeroIndex + boardSize;
			return true;
		}

		static bool MoveDown() {
			if(currentZeroIndex / boardSize == 0) {
				return false;
			}

			SwapTiles(currentZeroIndex, currentZeroIndex - boardSize);
			currentZeroIndex = currentZeroIndex - boardSize;
			return true;
		}

		static bool MoveLeft() {
			if (currentZeroIndex % boardSize == boardSize - 1) {
				return false;
			}

			SwapTiles(currentZeroIndex, currentZeroIndex + 1);
			currentZeroIndex = currentZeroIndex + 1;
			return true;
		}

		static bool MoveRight() {
			if (currentZeroIndex % boardSize == 0) {
				return false;
			}

			SwapTiles(currentZeroIndex, currentZeroIndex - 1);
			currentZeroIndex = currentZeroIndex - 1;
			return true;
		}

		static int CurrentIndexOfZero() {
			int index = 0;

			for (int i = 0; i < tiles.Count; i++) {
				if (tiles[i] == 0) {
					index = i;
					break;
				}
			}

			return index;
		}

		static int Manhattan() {
			int sum = 0;
			int posBeforeZero = targetZeroIndex;
			int current = 0;
			int x = 0;
			for (int i = 0; i < boardSize; i++) {
				for (int j = 0; j < boardSize; j++) {
					x = j + i * boardSize;
					current = tiles[x];
					
					if(current != 0) {
						if(posBeforeZero > 0) {
							sum += Math.Abs((current - 1) / boardSize - i)
								+ Math.Abs((current - 1) % boardSize - j);

							posBeforeZero--;
						}
						else {
							sum += Math.Abs(current / boardSize - i)
									+ Math.Abs(current % boardSize - j);
						}
					}	
				}
			}

			return sum;
		}

		static bool IsTargetState() {
			return Manhattan() == 0;
		}

		static void SwapTiles(int oldPos, int newPos) {
			int temp = tiles[oldPos];
			tiles[oldPos] = tiles[newPos];
			tiles[newPos] = temp;
		}
	}
}