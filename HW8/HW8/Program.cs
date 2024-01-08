using System;

class NeuralNetwork {
	static Random random = new Random();

	static double[,] InitializeWeights(int rows, int cols) {
		double[,] weights = new double[rows, cols];
		for (int i = 0; i < rows; i++) {
			for (int j = 0; j < cols; j++) {
				weights[i, j] = 2 * random.NextDouble() - 1;
			}
		}
		return weights;
	}

	static double Sigmoid(double x) {
		return 1 / (1 + Math.Exp(-x));
	}

	static double[,] Transpose(double[,] matrix) {
		int rows = matrix.GetLength(0);
		int cols = matrix.GetLength(1);
		double[,] result = new double[cols, rows];
		for (int i = 0; i < rows; i++) {
			for (int j = 0; j < cols; j++) {
				result[j, i] = matrix[i, j];
			}
		}
		return result;
	}

	static double[,] Dot(double[,] a, double[,] b) {
		int rowsA = a.GetLength(0);
		int colsA = a.GetLength(1);
		int colsB = b.GetLength(1);
		double[,] result = new double[rowsA, colsB];
		for (int i = 0; i < rowsA; i++) {
			for (int j = 0; j < colsB; j++) {
				double sum = 0;
				for (int k = 0; k < colsA; k++)
					sum += a[i, k] * b[k, j];
				result[i, j] = sum;
			}
		}
		return result;
	}

	static double[,] ApplySigmoid(double[,] matrix) {
		int rows = matrix.GetLength(0);
		int cols = matrix.GetLength(1);
		double[,] result = new double[rows, cols];
		for (int i = 0; i < rows; i++)
			for (int j = 0; j < cols; j++)
				result[i, j] = Sigmoid(matrix[i, j]);
		return result;
	}

	static double[,] ApplySigmoidDerivative(double[,] matrix) {
		int rows = matrix.GetLength(0);
		int cols = matrix.GetLength(1);
		double[,] result = new double[rows, cols];
		for (int i = 0; i < rows; i++) {
			for (int j = 0; j < cols; j++) {
				result[i, j] = matrix[i, j] * (1 - matrix[i, j]);
			}
		}
		return result;
	}

	static double[,] AddSubtract(double[,] matrix1, double[,] matrix2, bool add) {
		int rows = matrix1.GetLength(0);
		int cols = matrix1.GetLength(1);
		double[,] resultMatrix = new double[rows, cols];

		for (int i = 0; i < rows; i++) {
			for (int j = 0; j < cols; j++) {
				if(add) {
					resultMatrix[i, j] = matrix1[i, j] + matrix2[i, j];
				}
				else {
					resultMatrix[i, j] = matrix1[i, j] - matrix2[i, j];
				}
			}
		}

		return resultMatrix;
	}

	static double[,] MultiplyMatrices(double[,] matrix1, double[,] matrix2) {
		int rows1 = matrix1.GetLength(0);
		int cols1 = matrix1.GetLength(1);
		int rows2 = matrix2.GetLength(0);
		int cols2 = matrix2.GetLength(1);

		double[,] resultMatrix = new double[rows1, cols2];

		for (int i = 0; i < rows1; i++) {
			for (int j = 0; j < cols2; j++) {
				for (int k = 0; k < cols1; k++) {
					resultMatrix[i, j] +=
						matrix1[i, k] * matrix2[k, j];
				}
			}
		}

		return resultMatrix;
	}

	static void TrainSingleLayerNN(double[,] inputs, double[,] outputs, int epochs, ref double[,] weights) {
		for (int epoch = 0; epoch < epochs; epoch++) {
			double[,] output = ApplySigmoid(Dot(inputs, weights));
			double[,] error = AddSubtract(outputs, output, false);
			double[,] adjustment = Dot(Transpose(inputs), MultiplyMatrices(error, ApplySigmoidDerivative(output)));
			weights = AddSubtract(weights, adjustment, true);
		}
	}

	static void TrainMultiLayerNN(double[,] inputs, double[,] outputs, int epochs, ref double[,] weightsInputHidden, ref double[,] weightsHiddenOutput) {
		int inputSize = weightsInputHidden.GetLength(0);
		int hiddenSize = weightsInputHidden.GetLength(1);
		int outputSize = weightsHiddenOutput.GetLength(1);

		for (int epoch = 0; epoch < epochs; epoch++) {
			double[,] hiddenLayerInput = Dot(inputs, weightsInputHidden);
			double[,] hiddenLayerOutput = ApplySigmoid(hiddenLayerInput);

			double[,] outputLayerInput = Dot(hiddenLayerOutput, weightsHiddenOutput);
			double[,] output = ApplySigmoid(outputLayerInput);

			double[,] outputError = AddSubtract(outputs, output, true);
			double[,] outputDelta = MultiplyMatrices(outputError, ApplySigmoidDerivative(output));

			double[,] hiddenLayerError = Dot(outputDelta, Transpose(weightsHiddenOutput));
			double[,] hiddenLayerDelta = MultiplyMatrices(hiddenLayerError, ApplySigmoidDerivative(hiddenLayerOutput));

			weightsHiddenOutput = AddSubtract(weightsHiddenOutput, 
				Dot(Transpose(hiddenLayerOutput), outputDelta),
				true);

			weightsInputHidden = AddSubtract(weightsInputHidden, 
				Dot(Transpose(inputs), hiddenLayerDelta),
				true);
		}
	}

	static double[,] Predict(double[,] inputs, double[,] weights) {
		return ApplySigmoid(Dot(inputs, weights));
	}

	static void Print(double[,] outputs) {
		for (int i = 0; i < outputs.GetLength(0); i++) {
			for (int j = 0; j < outputs.GetLength(1); j++) {
				Console.Write(outputs[i, j] + " ");
			}
			Console.WriteLine();
		}
	}

	static void Main() {
		double[,] inputsAnd = { { 0, 0 }, { 0, 1 }, { 1, 0 }, { 1, 1 } };
		double[,] outputsAnd = { { 0 }, { 0 }, { 0 }, { 1 } };
		double[,] weightsAnd = InitializeWeights(2, 1);

		double[,] outputsOr = { { 0 }, { 1 }, { 1 }, { 1 } };
		double[,] weightsOr = InitializeWeights(2, 1);

		double[,] outputsXor = { { 0 }, { 1 }, { 1 }, { 0 } };
		double[,] weightsInputHiddenXor = InitializeWeights(2, 2);
		double[,] weightsHiddenOutputXor = InitializeWeights(2, 1);

		TrainSingleLayerNN(inputsAnd, outputsAnd, epochs: 10000, ref weightsAnd);
		TrainSingleLayerNN(inputsAnd, outputsOr, epochs: 10000, ref weightsOr);
		TrainMultiLayerNN(inputsAnd, outputsXor, epochs: 50000, ref weightsInputHiddenXor, ref weightsHiddenOutputXor);

		Console.WriteLine("AND Predictions:");
		double[,] andPrediction = Predict(inputsAnd, weightsAnd);
		Print(andPrediction);

		Console.WriteLine("OR Predictions:");
		double[,] orPrediction = Predict(inputsAnd, weightsOr);
		Print(orPrediction);
		
		Console.WriteLine("XOR Predictions:");
		double[,] xorPrediction = Predict(inputsAnd, weightsInputHiddenXor);
		Print(xorPrediction);
	}
}
