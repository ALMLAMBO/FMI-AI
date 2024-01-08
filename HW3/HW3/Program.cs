using System.Diagnostics;

namespace HW3 {
	public class Program {
		class Item {
			public int Weight { get; set; }
			public int Value { get; set; }
		}

		static int knapsackCapacity;
		static Random random = new Random();
		static List<Item> items = new List<Item>();
		static int generations = 20;
		static int generationSize = 20;
		static int itemsCount;
		static double mutationRate = 0.1;

		static void Main(string[] args) {
			string[] inputParams = Console.ReadLine()!.Split(' ');
			knapsackCapacity = int.Parse(inputParams[0]);
			itemsCount = int.Parse(inputParams[1]);

			for (int i = 0; i < itemsCount; i++) {
				string[] itemParams = Console.ReadLine()!.Split(' ');
				Item item = new Item() {
					Weight = int.Parse(itemParams[0]),
					Value = int.Parse(itemParams[1])
				};

				items.Add(item);
			}

			List<List<bool>> population = InitialPopulation();

			for (int i = 0; i < generations; i++) {
				population = CreateNewGeneration(population);
				int bestValue = BestValue(population);
				Console.WriteLine($"Best Value for generation {i + 1} is: {bestValue}");
			}
        }

		static List<bool> GenerateRandomSolution() {
			List<bool> solution = new List<bool>();

			for (int i = 0; i < itemsCount; i++) {
				solution.Add(random.Next(2) == 1);
			}

			return solution;
		}

		static bool ValidSolution(List<bool> solution) {
			int totalWeight = 0;
			for (int i = 0; i < solution.Count; i++) {
				if (solution[i]) {
					totalWeight += items[i].Weight;
				}

				if (totalWeight > knapsackCapacity) {
					return false;
				}
			}

			return true;
		}

		static bool CheckDuplicatingSolutions(List<bool> solution1, List<bool> solution2) {
			for (int i = 0; i < solution1.Count; i++) {
				if (solution1[i] != solution2[i]) {
					return false;
				}
			}

			return true;
		}

		static List<List<bool>> InitialPopulation() {
			List<List<bool>> population = new List<List<bool>>();

			int count = 0;
			while (count < generationSize) {
				List<bool> newSolution = GenerateRandomSolution();
				if (ValidSolution(newSolution)) {
					if (population.Count == 0) {
						population.Add(newSolution);
						count++;
					}
					else {
						bool skip = false;
						for (int i = 0; i < population.Count; i++) {
							if (CheckDuplicatingSolutions(newSolution, population[i])) {
								skip = true;
								continue;
							}
						}

						if (!skip) {
							population.Add(newSolution);
							count++;
						}
					}
				}
			}

			return population;
		}

		static List<bool> GetBetterSolutionValue(List<List<bool>> population) {
			int s1Index = random.Next(0, generations);
			int s2Index = random.Next(0, generations);

			int s1Value = CalculateValue(population[s1Index]);
			int s2Value = CalculateValue(population[s2Index]);

			return s1Value > s2Value ? population[s1Index] : population[s2Index];
		}

		static List<bool> Crossover(List<bool> s1, List<bool> s2) {
			int breakPoint = random.Next(0, s1.Count);
			List<bool> child = new List<bool>();

			child.AddRange(s1.Take(breakPoint));
			s2.Reverse();
			child.AddRange(s2.Take(s2.Count - breakPoint));

			if (ValidSolution(child)) {
				return child;
			}
			else {
				return Crossover(s1, s2);
			}
		}

		static List<bool> Mutation(List<bool> child) {
			List<bool> childClone = new List<bool>(child);
			int mutIndex1 = random.Next(0, child.Count);
			int mutIndex2 = random.Next(0, child.Count);

			bool temp = childClone[mutIndex1];
			childClone[mutIndex1] = childClone[mutIndex2];
			childClone[mutIndex2] = temp;

			if (ValidSolution(childClone)) {
				return childClone;
			}
			else {
				return Mutation(child);
			}
		}

		static List<List<bool>> CreateNewGeneration(List<List<bool>> population) {
			List<List<bool>> newGeneration = new List<List<bool>>();

			for (int i = 0; i < generationSize; i++) {
				List<bool> p1 = GetBetterSolutionValue(population);
				List<bool> p2 = GetBetterSolutionValue(population);

				List<bool> child = Crossover(p1, p2);
				if (random.NextDouble() < mutationRate) {
					child = Mutation(child);
				}

				newGeneration.Add(child);
			}

			return newGeneration;
		}

		static int BestValue(List<List<bool>> generation) {
			return generation
				.Select(p => CalculateValue(p))
				.Max();
		}

		static int CalculateValue(List<bool> solution) {
			int totalWeight = 0;

			for (int i = 0; i < solution.Count; i++) {
				if (solution[i]) {
					totalWeight += items[i].Value;
				}
			}

			return totalWeight;
		}
	}
}