namespace HW5 {
	internal class Program {
		static void Main() {
			string fileDir = Directory
				.GetParent(Directory.GetCurrentDirectory())!
				.Parent!.Parent!.ToString();

			string filePath = Directory
				.GetFiles(fileDir)
				.Where(x => x.Contains("data"))
				.FirstOrDefault()!;

			List<string[]> data = File
				.ReadLines(filePath)
				.Select(line => line.Split(','))
				.ToList();

			Random rand = new Random();
			data = data.OrderBy(x => rand.Next()).ToList();

			int folds = 10;
			int foldSize = data.Count / folds;
			double[] accuracies = new double[folds];

			for (int fold = 0; fold < folds; fold++) {
				int startIdx = fold * foldSize;
				int endIdx = (fold == folds - 1) ? data.Count : (fold + 1) * foldSize;

				List<string[]> trainingData = data
					.Take(startIdx)
					.Concat(data.Skip(endIdx))
					.ToList();

				List<string[]> testingData = data
					.Skip(startIdx)
					.Take(endIdx - startIdx)
					.ToList();

				NaiveBayes nbClassifier = new NaiveBayes();
				nbClassifier.Train(trainingData);

				int correctCount = 0;
				foreach (var instance in testingData) {
					string predictedClass = nbClassifier.Classify(instance);
					if (predictedClass == instance[0])
						correctCount++;
				}

				accuracies[fold] = (double)correctCount / (endIdx - startIdx);
				Console.WriteLine($"Fold {fold + 1}: Accuracy = {accuracies[fold]}");
			}

			double averageAccuracy = accuracies.Average();
			Console.WriteLine($"Average Accuracy: {averageAccuracy}");
		}
	}

	class NaiveBayes {
		private Dictionary<string, Dictionary<string, double>> probabilities;
		private Dictionary<string, double> classProbabilities;
		private List<string> classes;

		public NaiveBayes() {
			probabilities = new Dictionary<string, Dictionary<string, double>>();
			classProbabilities = new Dictionary<string, double>();
			classes = new List<string>();
		}

		public void Train(List<string[]> data) {
			classes = data.Select(row => row[0]).Distinct().ToList();

			foreach (var c in classes) {
				int classCount = data.Count(row => row[0] == c);
				classProbabilities[c] = (double)classCount / data.Count;
			}

			foreach (var c in classes) {
				var classData = data.Where(row => row[0] == c).ToList();
				probabilities[c] = new Dictionary<string, double>();

				for (int i = 1; i < data[0].Length; i++) {
					var featureValues = classData
						.Select(row => row[i])
						.Distinct()
						.ToList();

					foreach (var value in featureValues) {
						int countWithFeature = classData.Count(row => row[i] == value) + 1;
						int totalCount = classData.Count + featureValues.Count;

						double probability = (double)countWithFeature / totalCount;
						string featureKey = $"feature_{i}_{value}";
						probabilities[c][featureKey] = probability;
					}
				}
			}
		}

		public string Classify(string[] instance) {
			double maxProbability = double.MinValue;
			string? predictedClass = null;

			foreach (var c in classes) {
				double classProbability = Math.Log(classProbabilities[c]);

				for (int i = 1; i < instance.Length; i++) {
					string featureKey = $"feature_{i}_{instance[i]}";
					double featureProbability = Math
						.Log(probabilities[c].ContainsKey(featureKey) 
						? probabilities[c][featureKey] : LaplaceSmoothing());

					classProbability += featureProbability;
				}

				if (classProbability > maxProbability) {
					maxProbability = classProbability;
					predictedClass = c;
				}
			}

			return predictedClass!;
		}

		private double LaplaceSmoothing() {
			return 1e-5;
		}
	}
}
