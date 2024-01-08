namespace HW0 {
	public class Program {
		static void Main(string[] args) {
			int frogsCount = int.Parse(Console.ReadLine()!);
			SolveFrogs(frogsCount);
		}

		public static void SolveFrogs(int frogsCount) {
			string initState = new string('>', frogsCount) + "_" + new string('<', frogsCount);
			string finalState = new string('<', frogsCount) + "_" + new string('>', frogsCount);
			List<string> path = new List<string> {
				initState
			};

			DFS(initState, finalState, ref path);
			path.ForEach(Console.WriteLine);
		}

		public static bool DFS(string state, string finalState, ref List<string> path) {
			if(state == finalState) {
				return true;
			}

			bool found = false;

			for(int i = 0; i < state.Length - 2; i++) {
				string newState = CanMove(state, i);
				if(newState != "") {
					path.Add(newState);
					found = DFS(newState, finalState, ref path);
					
					if(!found) {
						path.RemoveAt(path.Count - 1);
					}
				}
			}

			return found;
		}

		public static string CanMove(string state, int index) {
			if ((state[index] == '>' && state[index + 1] == '_') 
				|| (state[index] == '_' && state[index + 1] == '<') 
				) {

				return MoveFrog(state, index);
			}

			if((state[index] == '>' && state[index + 2] == '_')
				|| (state[index] == '_' && state[index + 2] == '<')) {

				return MoveFrog(state, index + 1);
			}

			return "";
		}

		public static string MoveFrog(string state, int index) {
			char[] newStateArray = state.ToCharArray();
			newStateArray[index] = state[index + 1];
			newStateArray[index + 1] = state[index];

			return new string(newStateArray);
		}
	}
}