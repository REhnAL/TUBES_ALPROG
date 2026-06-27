namespace Face_Recognition;

public class NeuralNetworkResult
{
    public double Score { get; set; }
    public string Category { get; set; } = "";
    public string Explanation { get; set; } = "";
    public string FormulaExplanation { get; set; } = "";
}

public static class NeuralNetworkService
{
    /*
        Struktur Neural Network sederhana:
        Input layer  : x1 = ketinggian air, x2 = curah hujan
        Hidden layer : 3 neuron dengan aktivasi sigmoid
        Output layer : 1 neuron berupa skor risiko 0-100

        Tujuan service ini adalah sebagai formulasi feedforward dan backpropagation
        untuk kebutuhan tugas. Pada aplikasi, hasil utama tetap Fuzzy Logic.
    */

    private static double[,] inputHiddenWeights =
    {
        { 3.20, 1.80, 4.00 },
        { 2.40, 3.00, -1.00 }
    };

    private static double[] hiddenBiases =
    {
        -2.00,
        -1.60,
        -1.20
    };

    private static double[] hiddenOutputWeights =
    {
        2.50,
        2.20,
        1.30
    };

    private static double outputBias = -2.00;

    public static NeuralNetworkResult Analyze(double waterLevel, double rainfall)
    {
        double x1 = Clamp(waterLevel / 120.0, 0, 1);
        double x2 = Clamp(rainfall / 100.0, 0, 1);

        double[] hidden = ForwardHiddenLayer(x1, x2);

        double outputNet =
            (hidden[0] * hiddenOutputWeights[0]) +
            (hidden[1] * hiddenOutputWeights[1]) +
            (hidden[2] * hiddenOutputWeights[2]) +
            outputBias;

        double output = Sigmoid(outputNet);
        double score = Math.Round(output * 100.0, 2);

        string category = GetCategory(score);

        return new NeuralNetworkResult
        {
            Score = score,
            Category = category,
            Explanation =
                $"Neural Network feedforward menghasilkan skor {score:0.00}/100 dengan kategori {category}. " +
                "Nilai input dinormalisasi dari ketinggian air dan curah hujan, lalu diproses melalui hidden layer sigmoid.",
            FormulaExplanation =
                "Feedforward: x → hidden = sigmoid(Wxh·x + bh), output = sigmoid(Who·hidden + bo). " +
                "Backpropagation: error dihitung dari target - output, lalu bobot diperbarui menggunakan learning rate."
        };
    }

    public static void TrainOneStep(
        double waterLevel,
        double rainfall,
        double targetScore,
        double learningRate = 0.05)
    {
        double x1 = Clamp(waterLevel / 120.0, 0, 1);
        double x2 = Clamp(rainfall / 100.0, 0, 1);
        double target = Clamp(targetScore / 100.0, 0, 1);

        double[] hidden = ForwardHiddenLayer(x1, x2);

        double outputNet =
            (hidden[0] * hiddenOutputWeights[0]) +
            (hidden[1] * hiddenOutputWeights[1]) +
            (hidden[2] * hiddenOutputWeights[2]) +
            outputBias;

        double output = Sigmoid(outputNet);

        double outputError = target - output;
        double outputDelta = outputError * SigmoidDerivative(output);

        double[] hiddenDeltas = new double[3];

        for (int j = 0; j < 3; j++)
        {
            hiddenDeltas[j] =
                outputDelta *
                hiddenOutputWeights[j] *
                SigmoidDerivative(hidden[j]);
        }

        for (int j = 0; j < 3; j++)
        {
            hiddenOutputWeights[j] += learningRate * outputDelta * hidden[j];
        }

        outputBias += learningRate * outputDelta;

        for (int j = 0; j < 3; j++)
        {
            inputHiddenWeights[0, j] += learningRate * hiddenDeltas[j] * x1;
            inputHiddenWeights[1, j] += learningRate * hiddenDeltas[j] * x2;
            hiddenBiases[j] += learningRate * hiddenDeltas[j];
        }
    }

    private static double[] ForwardHiddenLayer(double x1, double x2)
    {
        double[] hidden = new double[3];

        for (int j = 0; j < 3; j++)
        {
            double net =
                (x1 * inputHiddenWeights[0, j]) +
                (x2 * inputHiddenWeights[1, j]) +
                hiddenBiases[j];

            hidden[j] = Sigmoid(net);
        }

        return hidden;
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

    private static double Sigmoid(double value)
    {
        return 1.0 / (1.0 + Math.Exp(-value));
    }

    private static double SigmoidDerivative(double sigmoidOutput)
    {
        return sigmoidOutput * (1.0 - sigmoidOutput);
    }

    private static double Clamp(double value, double min, double max)
    {
        return Math.Max(min, Math.Min(max, value));
    }
}
