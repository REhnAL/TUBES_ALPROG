namespace Face_Recognition;

public class FloodAnalysisResult
{
    public double WaterLevel { get; set; }
    public double Rainfall { get; set; }

    public string WaterStatus { get; set; } = "";
    public string RainStatus { get; set; } = "";

    public double WaterLow { get; set; }
    public double WaterMedium { get; set; }
    public double WaterHigh { get; set; }

    public double RainLight { get; set; }
    public double RainMedium { get; set; }
    public double RainHeavy { get; set; }

    public double FuzzyScore { get; set; }
    public string FuzzyCategory { get; set; } = "";
    public string FuzzyCategoryUpper { get; set; } = "";
    public string FuzzyAction { get; set; } = "";
    public string FuzzyRule { get; set; } = "";

    public double RegressionScore { get; set; }
    public string RegressionExplanation { get; set; } = "";

    public string ClusterName { get; set; } = "";
    public string ClusterExplanation { get; set; } = "";

    public double NeuralNetworkScore { get; set; }
    public string NeuralNetworkCategory { get; set; } = "";
    public string NeuralNetworkExplanation { get; set; } = "";
}

public static class FloodAnalysisService
{
    public static FloodAnalysisResult Analyze(double waterLevel, double rainfall)
    {
        double waterLow = Decreasing(waterLevel, 30, 55);
        double waterMedium = Triangle(waterLevel, 35, 65, 90);
        double waterHigh = Increasing(waterLevel, 75, 100);

        double rainLight = Decreasing(rainfall, 10, 25);
        double rainMedium = Triangle(rainfall, 15, 35, 55);
        double rainHeavy = Increasing(rainfall, 45, 70);

        double riskSafe = Math.Min(waterLow, rainLight);

        double riskLow = Math.Max(
            Math.Min(waterLow, rainMedium),
            Math.Min(waterMedium, rainLight));

        double riskMedium = Math.Max(
            Math.Min(waterMedium, rainMedium),
            Math.Max(
                Math.Min(waterHigh, rainLight),
                Math.Min(waterLow, rainHeavy)));

        double riskHigh = Math.Max(
            Math.Min(waterHigh, rainMedium),
            Math.Max(
                Math.Min(waterMedium, rainHeavy),
                Math.Max(waterHigh, rainHeavy)));

        double numerator =
            (riskSafe * 10) +
            (riskLow * 35) +
            (riskMedium * 65) +
            (riskHigh * 90);

        double denominator = riskSafe + riskLow + riskMedium + riskHigh;

        double fuzzyScore = denominator == 0 ? 0 : numerator / denominator;
        fuzzyScore = Math.Round(fuzzyScore, 2);

        string fuzzyCategory = GetCategory(fuzzyScore);
        string action = GetAction(fuzzyCategory);

        double regressionScore = CalculateRegressionScore(waterLevel, rainfall);
        string clusterName = CalculateCluster(waterLevel, rainfall);

        NeuralNetworkResult neuralNetworkResult =
            NeuralNetworkService.Analyze(waterLevel, rainfall);

        return new FloodAnalysisResult
        {
            WaterLevel = Math.Round(waterLevel, 2),
            Rainfall = Math.Round(rainfall, 2),

            WaterLow = Math.Round(waterLow, 3),
            WaterMedium = Math.Round(waterMedium, 3),
            WaterHigh = Math.Round(waterHigh, 3),

            RainLight = Math.Round(rainLight, 3),
            RainMedium = Math.Round(rainMedium, 3),
            RainHeavy = Math.Round(rainHeavy, 3),

            WaterStatus = GetWaterStatus(waterLevel),
            RainStatus = GetRainStatus(rainfall),

            FuzzyScore = fuzzyScore,
            FuzzyCategory = fuzzyCategory,
            FuzzyCategoryUpper = fuzzyCategory.ToUpper(),
            FuzzyAction = action,
            FuzzyRule = BuildFuzzyRuleExplanation(
                waterLow,
                waterMedium,
                waterHigh,
                rainLight,
                rainMedium,
                rainHeavy,
                fuzzyScore,
                fuzzyCategory),

            RegressionScore = regressionScore,
            RegressionExplanation =
                $"Regression digunakan sebagai pembanding numerik dengan rumus skor = 0,55×ketinggian air + 0,75×curah hujan. " +
                $"Untuk data saat ini, skor regression adalah {regressionScore:0.00}/100.",

            ClusterName = clusterName,
            ClusterExplanation =
                $"K-Means sederhana mengelompokkan data sensor ke centroid Aman, Waspada, dan Bahaya. " +
                $"Data saat ini paling dekat dengan cluster {clusterName}.",

            NeuralNetworkScore = neuralNetworkResult.Score,
            NeuralNetworkCategory = neuralNetworkResult.Category,
            NeuralNetworkExplanation =
                $"{neuralNetworkResult.Explanation} {neuralNetworkResult.FormulaExplanation}"
        };
    }

    private static double CalculateRegressionScore(double waterLevel, double rainfall)
    {
        double score = (0.55 * waterLevel) + (0.75 * rainfall);

        return Math.Round(Clamp(score, 0, 100), 2);
    }

    private static string CalculateCluster(double waterLevel, double rainfall)
    {
        double distanceSafe = Distance(waterLevel, rainfall, 30, 10);
        double distanceAlert = Distance(waterLevel, rainfall, 65, 35);
        double distanceDanger = Distance(waterLevel, rainfall, 95, 70);

        if (distanceSafe <= distanceAlert && distanceSafe <= distanceDanger)
        {
            return "Aman";
        }

        if (distanceAlert <= distanceSafe && distanceAlert <= distanceDanger)
        {
            return "Waspada";
        }

        return "Bahaya";
    }

    private static string BuildFuzzyRuleExplanation(
        double waterLow,
        double waterMedium,
        double waterHigh,
        double rainLight,
        double rainMedium,
        double rainHeavy,
        double fuzzyScore,
        string category)
    {
        return
            $"Membership air: rendah={waterLow:0.00}, sedang={waterMedium:0.00}, tinggi={waterHigh:0.00}. " +
            $"Membership hujan: ringan={rainLight:0.00}, sedang={rainMedium:0.00}, lebat={rainHeavy:0.00}. " +
            $"Defuzzifikasi menghasilkan skor {fuzzyScore:0.00}/100 sehingga kategori risiko adalah {category}.";
    }

    private static string GetWaterStatus(double waterLevel)
    {
        if (waterLevel < 35)
        {
            return "Air masih rendah";
        }

        if (waterLevel < 75)
        {
            return "Air mulai naik";
        }

        return "Air tinggi";
    }

    private static string GetRainStatus(double rainfall)
    {
        if (rainfall < 15)
        {
            return "Hujan ringan";
        }

        if (rainfall < 50)
        {
            return "Hujan sedang";
        }

        return "Hujan lebat";
    }

    private static string GetCategory(double score)
    {
        if (score < 25)
        {
            return "Aman";
        }

        if (score < 50)
        {
            return "Rendah";
        }

        if (score < 75)
        {
            return "Sedang";
        }

        return "Tinggi";
    }

    private static string GetAction(string category)
    {
        if (category == "Aman")
        {
            return "Kondisi relatif aman, tetap pantau informasi terbaru.";
        }

        if (category == "Rendah")
        {
            return "Tetap waspada dan pantau perubahan cuaca.";
        }

        if (category == "Sedang")
        {
            return "Siapkan langkah antisipasi dan pantau kenaikan air secara berkala.";
        }

        return "Segera lakukan peringatan dini dan siapkan tindakan evakuasi.";
    }

    private static double Triangle(double x, double a, double b, double c)
    {
        if (x <= a || x >= c)
        {
            return 0;
        }

        if (x == b)
        {
            return 1;
        }

        if (x > a && x < b)
        {
            return (x - a) / (b - a);
        }

        return (c - x) / (c - b);
    }

    private static double Increasing(double x, double a, double b)
    {
        if (x <= a)
        {
            return 0;
        }

        if (x >= b)
        {
            return 1;
        }

        return (x - a) / (b - a);
    }

    private static double Decreasing(double x, double a, double b)
    {
        if (x <= a)
        {
            return 1;
        }

        if (x >= b)
        {
            return 0;
        }

        return (b - x) / (b - a);
    }

    private static double Distance(
        double x1,
        double y1,
        double x2,
        double y2)
    {
        return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
    }

    private static double Clamp(double value, double min, double max)
    {
        return Math.Max(min, Math.Min(max, value));
    }
}
