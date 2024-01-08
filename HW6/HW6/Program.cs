class Program {
	static void Main() {
		string fileDir = Directory
				.GetParent(Directory.GetCurrentDirectory())!
				.Parent!.Parent!.ToString();

		string filePath = Directory
				.GetFiles(fileDir)
				.Where(x => x.Contains("data"))
				.FirstOrDefault()!;

		var dataset = DataReader.LoadData(filePath);
		int K = 5;

		DecisionTree tree = new DecisionTree(K);
		tree.Train(dataset.Item1, dataset.Item2);

		int folds = 5;
		double accuracy = CrossValidation(tree, dataset.Item1, dataset.Item2, folds);

		Console.WriteLine($"Accuracy: {accuracy:P2}");
	}

	static double CrossValidation(DecisionTree tree, List<double[]> inputs, List<int> outputs, int folds) {
		int foldSize = inputs.Count / folds;
		double totalAccuracy = 0;

		for (int i = 0; i < folds; i++) {
			int startIndex = i * foldSize;
			int endIndex = (i + 1) * foldSize;

			var trainingInputs = inputs.Take(startIndex).Concat(inputs.Skip(endIndex)).ToList();
			var trainingOutputs = outputs.Take(startIndex).Concat(outputs.Skip(endIndex)).ToList();
			var testingInputs = inputs.Skip(startIndex).Take(foldSize).ToList();
			var testingOutputs = outputs.Skip(startIndex).Take(foldSize).ToList();

			tree.Train(trainingInputs, trainingOutputs);

			int correct = 0;
			for (int j = 0; j < testingInputs.Count; j++) {
				int predicted = tree.Predict(testingInputs[j]);
				if (predicted == testingOutputs[j])
					correct++;
			}

			double accuracy = (double)correct / foldSize;
			totalAccuracy += accuracy;

			Console.WriteLine($"Fold {i + 1}: Accuracy = {accuracy:P2}");
		}

		double averageAccuracy = totalAccuracy / folds;
		return averageAccuracy;
	}
}

class DecisionTree {
	private int K;
	private Node? root;

	public DecisionTree(int k) {
		K = k;
	}

	public void Train(List<double[]> inputs, List<int> outputs) {
		List<string> attributeNames = new List<string> {  };
		root = BuildTree(inputs, outputs, attributeNames);
	}

	public int Predict(double[] input) {
		return Predict(root, input);
	}

	private int Predict(Node node, double[] input) {
		if (node.IsLeaf)
			return node.ClassLabel;

		int attributeIndex = node.SplitAttributeIndex;
		double splitValue = node.SplitValue;

		if (input[attributeIndex] <= splitValue)
			return Predict(node.LeftChild, input);
		else
			return Predict(node.RightChild, input);
	}

	private Node BuildTree(List<double[]> inputs, List<int> outputs, List<string> attributeNames) {
		if (inputs.Count <= K || outputs.Distinct().Count() == 1) {
			return new Node { IsLeaf = true, ClassLabel = outputs.Mode() };
		}

		int bestAttribute = -1;
		double bestSplitValue = 0;
		double bestInformationGain = double.MinValue;

		foreach (int attributeIndex in Enumerable.Range(0, inputs[0].Length)) {
			var attributeValues = inputs.Select(input => input[attributeIndex]).Distinct().OrderBy(x => x).ToList();

			foreach (double splitValue in attributeValues.Skip(1).Take(attributeValues.Count - 2)) {
				var leftOutputs = new List<int>();
				var rightOutputs = new List<int>();

				for (int i = 0; i < inputs.Count; i++) {
					if (inputs[i][attributeIndex] <= splitValue)
						leftOutputs.Add(outputs[i]);
					else
						rightOutputs.Add(outputs[i]);
				}

				double informationGain = CalculateInformationGain(outputs, leftOutputs, rightOutputs);

				if (informationGain > bestInformationGain) {
					bestInformationGain = informationGain;
					bestAttribute = attributeIndex;
					bestSplitValue = splitValue;
				}
			}
		}

		if (bestInformationGain > 0) {
			var leftInputs = new List<double[]>();
			var leftOutputs = new List<int>();
			var rightInputs = new List<double[]>();
			var rightOutputs = new List<int>();

			for (int i = 0; i < inputs.Count; i++) {
				if (inputs[i][bestAttribute] <= bestSplitValue) {
					leftInputs.Add(inputs[i]);
					leftOutputs.Add(outputs[i]);
				}
				else {
					rightInputs.Add(inputs[i]);
					rightOutputs.Add(outputs[i]);
				}
			}

			Node leftChild = BuildTree(leftInputs, leftOutputs, attributeNames);
			Node rightChild = BuildTree(rightInputs, rightOutputs, attributeNames);

			return new Node {
				IsLeaf = false,
				SplitAttributeIndex = bestAttribute,
				SplitValue = bestSplitValue,
				LeftChild = leftChild,
				RightChild = rightChild
			};
		}
		else {
			return new Node { IsLeaf = true, ClassLabel = outputs.Mode() };
		}
	}

	private double CalculateInformationGain(List<int> parentOutputs, List<int> leftOutputs, List<int> rightOutputs) {
		double parentEntropy = CalculateEntropy(parentOutputs);

		double leftWeight = (double)leftOutputs.Count / parentOutputs.Count;
		double rightWeight = (double)rightOutputs.Count / parentOutputs.Count;

		double leftEntropy = CalculateEntropy(leftOutputs);
		double rightEntropy = CalculateEntropy(rightOutputs);

		double weightedChildEntropy = leftWeight * leftEntropy + rightWeight * rightEntropy;

		return parentEntropy - weightedChildEntropy;
	}

	private double CalculateEntropy(List<int> outputs) {
		double entropy = 0;

		var distinctClasses = outputs.Distinct();
		foreach (var distinctClass in distinctClasses) {
			double probability = (double)outputs.Count(item => item == distinctClass) / outputs.Count;
			entropy -= probability * Math.Log(probability, 2);
		}

		return entropy;
	}

	private class Node {
		public bool IsLeaf { get; set; }
		public int ClassLabel { get; set; }
		public int SplitAttributeIndex { get; set; }
		public double SplitValue { get; set; }
		public Node LeftChild { get; set; }
		public Node RightChild { get; set; }
	}
}

static class ListExtensions {
	public static T Mode<T>(this List<T> list) {
		var group = list.GroupBy(x => x);
		var maxCount = group.Max(g => g.Count());
		return group.First(g => g.Count() == maxCount).Key;
	}
}

class DataReader {
	public static (List<double[]>, List<int>) LoadData(string filePath) {
		var inputs = new List<double[]>();
		var outputs = new List<int>();

		using (StreamReader reader = new StreamReader(filePath)) {
			bool dataSection = false;

			while (!reader.EndOfStream) {
				string line = reader.ReadLine().Trim();

				if (line.StartsWith("@attribute"))
					continue;

				if (line.StartsWith("@data")) {
					dataSection = true;
					continue;
				}

				if (dataSection && !string.IsNullOrEmpty(line)) {
					string[] values = line.Split(',');

					double[] input = new double[values.Length - 1];
					for (int i = 0; i < values.Length - 1; i++) {
						if (double.TryParse(values[i], out double numericValue))
							input[i] = numericValue;
					}

					int output = int.Parse(values.Last());
					inputs.Add(input);
					outputs.Add(output);
				}
			}
		}

		return (inputs, outputs);
	}
}
