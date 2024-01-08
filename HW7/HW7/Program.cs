using System.Drawing;
using System.Text.RegularExpressions;
using ScottPlot;

class Program {
	static void Main() {
		string fileDir = Directory
						 .GetParent(Directory.GetCurrentDirectory())!
						 .Parent!
						 .Parent!
						 .ToString();

		string normalFileName = "normal.txt";
		string unbalanceFileName = "unbalance.txt";

		List<PointF> normalData = LoadData(Path.Combine(fileDir, normalFileName));
		List<PointF> unbalanceData = LoadData(Path.Combine(fileDir, unbalanceFileName));

		List<List<PointF>> normalClusters = KMeansWithRandomRestart(normalData, 5, 10);
		List<List<PointF>> unbalanceClusters = KMeansWithRandomRestart(unbalanceData, 10, 10);

		VisualizeClusters(normalData, normalClusters, "Normal Clusters");
		VisualizeClusters(unbalanceData, unbalanceClusters, "Unbalance Clusters");
	}

	static List<PointF> LoadData(string fileName) {
		string[] lines = File.ReadAllLines(fileName);

		return lines.Select(line => {
								string[] parts = Regex.Replace(line, "\\s+", " ")
													  .Replace('.', ',')
													  .Split(' ');
								return new PointF(float.Parse(parts[0]), float.Parse(parts[1]));
							}).ToList();
	}

	static List<List<PointF>> KMeansWithRandomRestart(List<PointF> data, int numClusters, int numRestarts) {
		List<List<PointF>> bestClusters = null;
		double bestCost = double.MaxValue;

		for (int restart = 0; restart < numRestarts; restart++) {
			List<List<PointF>> clusters = KMeans(data, numClusters);
			double cost = CalculateCost(clusters);

			if (cost < bestCost) {
				bestCost = cost;
				bestClusters = clusters;
			}
		}

		return bestClusters;
	}

	static double CalculateCost(List<List<PointF>> clusters) {
		double intraClusterDistance = 0;
		double interClusterDistance = 0;

		foreach (var cluster in clusters) {
			intraClusterDistance += CalculateIntraClusterDistance(cluster);
		}

		interClusterDistance = CalculateInterClusterDistance(clusters);

		return intraClusterDistance + interClusterDistance;
	}

	static double CalculateIntraClusterDistance(List<PointF> cluster) {
		double distance = 0;

		for (int i = 0; i < cluster.Count; i++) {
			for (int j = i + 1; j < cluster.Count; j++) {
				distance += Distance(cluster[i], cluster[j]);
			}
		}

		return distance;
	}

	static double CalculateInterClusterDistance(List<List<PointF>> clusters) {
		double distance = 0;

		for (int i = 0; i < clusters.Count; i++) {
			for (int j = i + 1; j < clusters.Count; j++) {
				distance += CalculateCentroidsDistance(clusters[i], clusters[j]);
			}
		}

		return distance;
	}

	static double CalculateCentroidsDistance(List<PointF> cluster1, List<PointF> cluster2) {
		if (cluster1.Count == 0 || cluster2.Count == 0) {
			return 0;
		}

		PointF centroid1 = new PointF(cluster1.Average(p => p.X), cluster1.Average(p => p.Y));
		PointF centroid2 = new PointF(cluster2.Average(p => p.X), cluster2.Average(p => p.Y));

		return Distance(centroid1, centroid2);
	}

	static List<List<PointF>> KMeans(List<PointF> data, int numClusters) {
		Random random = new Random();
		List<PointF> centroids = Enumerable.Range(0, numClusters)
										   .Select(_ => data[random.Next(data.Count)])
										   .ToList();

		List<List<PointF>> clusters;

		bool converged = true;
		do {
			clusters = AssignToClusters(data, centroids);
			List<PointF> newCentroids = CalculateCentroids(clusters);
			converged = CentroidsConverged(centroids, newCentroids);
			centroids = newCentroids;
		} while (!converged);

		return clusters;
	}

	static List<List<PointF>> AssignToClusters(List<PointF> data, List<PointF> centroids) {
		List<List<PointF>> clusters = centroids
									  .Select(_ => new List<PointF>())
									  .ToList();

		foreach (PointF point in data) {
			int closestCentroidIndex = ClosestCentroidIndex(point, centroids);
			clusters[closestCentroidIndex].Add(point);
		}

		return clusters;
	}

	static List<PointF> CalculateCentroids(List<List<PointF>> clusters) {
		return clusters
			   .Select(cluster => {
						   if (cluster.Count == 0) {
							   return new PointF(0, 0);
						   }

						   return new PointF(
											 cluster.Average(point => point.X),
											 cluster.Average(point => point.Y)
											);
					   })
			   .ToList();
	}

	static int ClosestCentroidIndex(PointF point, List<PointF> centroids) {
		float minDistance = float.MaxValue;
		int closestIndex = -1;

		for (int i = 0; i < centroids.Count; i++) {
			float distance = Distance(point, centroids[i]);
			if (distance < minDistance) {
				minDistance = distance;
				closestIndex = i;
			}
		}

		return closestIndex;
	}

	static float Distance(PointF p1, PointF p2) {
		float dx = p1.X - p2.X;
		float dy = p1.Y - p2.Y;
		return (float)Math.Sqrt(dx * dx + dy * dy);
	}

	static bool CentroidsConverged(List<PointF> oldCentroids, List<PointF> newCentroids) {
		float epsilon = 0.001f;

		return oldCentroids
			   .Zip(newCentroids, (oldCentroid, newCentroid) => Distance(oldCentroid, newCentroid))
			   .All(distance => distance < epsilon);
	}

	static void VisualizeClusters(List<PointF> data, List<List<PointF>> clusters, string title) {
		ScottPlot.Plot plot = new Plot();
		double[] xCoordinates = data.Select(x => (double)x.X).ToArray();
		double[] yCoordinates = data.Select(x => (double)x.Y).ToArray();

		foreach (PointF point in data) {
			plot.AddPoint(point.X, point.Y, null, default, default);
		}

		Random random = new Random();

		for (int i = 0; i < clusters.Count; i++) {
			List<PointF> clusterPoints = clusters[i];
			Color color = Color.FromArgb(random.Next(200, 256), random.Next(256),
										 random.Next(256), random.Next(256));

			clusterPoints
				.ForEach(x => plot.AddPoint(x.X, x.Y, color));
		}

		foreach (List<PointF> cluster in clusters) {
			PointF center = CalculateCentroids(new List<List<PointF>> { cluster }).First();
			plot.AddPoint(center.X, center.Y, Color.Black, 10.0f, MarkerShape.cross);
		}

		plot.SaveFig($"../../../{title.ToLower().Replace(' ', '-')}.png");
	}
}