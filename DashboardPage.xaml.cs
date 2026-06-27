using Microsoft.Maui.Controls.Shapes;
using System.Globalization;

namespace Face_Recognition;

public partial class DashboardPage : ContentPage
{
    private double waterLevel = 72;
    private double rainfall = 38;

    public DashboardPage()
    {
        InitializeComponent();
        LoadDashboard();
    }

    private void LoadDashboard()
    {
        User? user = UserSession.CurrentUser;

        if (user == null)
        {
            return;
        }

        WelcomeLabel.Text = $"Selamat Datang, {user.Username}";
        RoleLabel.Text = user.Role;

        SensorInputPanel.IsVisible = user.Role == "Operator";

        if (user.Role == "Operator")
        {
            WaterInputEntry.Text = waterLevel.ToString(CultureInfo.InvariantCulture);
            RainInputEntry.Text = rainfall.ToString(CultureInfo.InvariantCulture);
        }

        FloodAnalysisResult result = FloodAnalysisService.Analyze(waterLevel, rainfall);

        ApplyAnalysisResult(result);

        MenuContainer.Clear();

        if (user.Role == "Masyarakat")
        {
            LoadMasyarakatDashboard(result);
        }
        else if (user.Role == "Operator")
        {
            LoadOperatorDashboard(result);
        }
        else
        {
            LoadMasyarakatDashboard(result);
        }
    }

    private void ApplyAnalysisResult(FloodAnalysisResult result)
    {
        WaterLevelLabel.Text = $"{result.WaterLevel} cm";
        RainfallLabel.Text = $"{result.Rainfall} mm/jam";
        RiskStatusLabel.Text = result.FuzzyCategory;

        WaterStatusLabel.Text = result.WaterStatus;
        RainStatusLabel.Text = result.RainStatus;
        RiskStatusDescriptionLabel.Text = result.FuzzyAction;

        RiskCategoryBigLabel.Text = result.FuzzyCategoryUpper;
        RiskActionShortLabel.Text = result.FuzzyAction;

        FuzzyScoreLabel.Text = $"{result.FuzzyScore:0.00}/100";
        RegressionScoreLabel.Text = $"{result.RegressionScore:0.00}/100";
        ClusterLabel.Text = result.ClusterName;

        NeuralNetworkScoreLabel.Text = $"{result.NeuralNetworkScore:0.00}/100";
        NeuralNetworkCategoryLabel.Text = result.NeuralNetworkCategory;
        NeuralNetworkDescriptionLabel.Text = result.NeuralNetworkExplanation;

        DataSourceLabel.Text = "Sumber data: simulasi sensor. Nilai ketinggian air dan curah hujan masih dapat diubah oleh operator melalui dashboard.";
    }

    private void LoadMasyarakatDashboard(FloodAnalysisResult result)
    {
        SidebarDescriptionLabel.Text = "Dashboard masyarakat berisi informasi risiko banjir dan rekomendasi tindakan.";

        MainMethodLabel.Text = "Fuzzy Logic + NN";

        DecisionTitleLabel.Text = "Informasi Risiko Banjir Wilayah";
        DecisionDescriptionLabel.Text = "Fuzzy Logic digunakan sebagai metode utama untuk menentukan tingkat risiko banjir berdasarkan ketinggian air dan curah hujan.";
        DecisionRuleLabel.Text = result.FuzzyRule;

        RegressionDescriptionLabel.Text = result.RegressionExplanation;
        ClusterDescriptionLabel.Text = result.ClusterExplanation;

        FeatureTitleLabel.Text = "Fitur untuk Masyarakat";

        AddMenuCard("Status Wilayah", "Melihat kategori risiko banjir di wilayah sekitar berdasarkan hasil Fuzzy Logic.");
        AddMenuCard("Peringatan Dini", "Menampilkan peringatan ketika risiko banjir mulai meningkat.");
        AddMenuCard("Data Sensor", "Melihat nilai ketinggian air dan curah hujan yang menjadi dasar analisis.");
        AddMenuCard("Panduan Evakuasi", "Memberikan arahan tindakan ketika risiko banjir berada pada kategori sedang atau tinggi.");
        AddMenuCard("Rekomendasi Tindakan", "Saran untuk mengamankan barang, memantau air, dan menyiapkan jalur evakuasi.");
        AddMenuCard("Riwayat Peringatan", "Menampilkan riwayat status risiko banjir yang pernah muncul sebelumnya.");
    }

    private void LoadOperatorDashboard(FloodAnalysisResult result)
    {
        SidebarDescriptionLabel.Text = "Dashboard operator berisi input sensor, hasil Fuzzy Logic, Regression, K-Means, dan Neural Network.";

        MainMethodLabel.Text = "Fuzzy Logic + NN";

        DecisionTitleLabel.Text = "Monitoring Analisis Risiko Banjir";
        DecisionDescriptionLabel.Text = "Operator dapat mengubah data ketinggian air dan curah hujan, lalu sistem memprosesnya menggunakan Fuzzy Logic, Regression, dan K-Means.";
        DecisionRuleLabel.Text = result.FuzzyRule;

        RegressionDescriptionLabel.Text = result.RegressionExplanation;
        ClusterDescriptionLabel.Text = result.ClusterExplanation;

        FeatureTitleLabel.Text = "Fitur untuk Operator";

        AddMenuCard("Input Sensor", "Mengubah nilai simulasi ketinggian air dan curah hujan.");
        AddMenuCard("Fuzzy Logic", "Melihat hasil penentuan risiko berdasarkan membership function dan rule fuzzy.");
        AddMenuCard("Regression", "Melihat estimasi skor risiko numerik berdasarkan data sensor.");
        AddMenuCard("K-Means Cluster", "Melihat pengelompokan pola kondisi sensor ke cluster aman, waspada, atau bahaya.");
        AddMenuCard("Neural Network", "Melihat skor risiko dari formulasi feedforward dan backpropagation sederhana.");
        AddMenuCard("Kelola Peringatan", "Mengatur status peringatan yang akan ditampilkan kepada masyarakat.");
        AddMenuCard("Kalibrasi Sensor", "Mencatat kebutuhan pengecekan sensor jika data tidak stabil atau tidak normal.");
    }

    private void AddMenuCard(string title, string description)
    {
        Border card = new Border
        {
            BackgroundColor = Color.FromArgb("#FFFFFF"),
            Stroke = Color.FromArgb("#DCEDEA"),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(22)
            },
            Padding = 22,
            WidthRequest = 285,
            HeightRequest = 205,
            Margin = new Thickness(0, 0, 20, 20)
        };

        VerticalStackLayout layout = new VerticalStackLayout
        {
            Spacing = 12
        };

        layout.Children.Add(new Label
        {
            Text = title,
            FontSize = 21,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#173B2F"),
            LineBreakMode = LineBreakMode.WordWrap
        });

        layout.Children.Add(new Label
        {
            Text = description,
            FontSize = 14,
            TextColor = Color.FromArgb("#6D8B82"),
            LineBreakMode = LineBreakMode.WordWrap
        });

        card.Content = layout;
        MenuContainer.Add(card);
    }

    private async void OnUpdateSensorClicked(object sender, EventArgs e)
    {
        User? user = UserSession.CurrentUser;

        if (user == null || user.Role != "Operator")
        {
            await DisplayAlert(
                "Akses Ditolak",
                "Hanya operator yang dapat mengubah data sensor.",
                "OK");

            return;
        }

        string waterText = (WaterInputEntry.Text ?? "").Replace(",", ".");
        string rainText = (RainInputEntry.Text ?? "").Replace(",", ".");

        bool waterValid = double.TryParse(
            waterText,
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out double newWaterLevel);

        bool rainValid = double.TryParse(
            rainText,
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out double newRainfall);

        if (!waterValid || !rainValid)
        {
            await DisplayAlert(
                "Input Tidak Valid",
                "Masukkan angka yang benar untuk ketinggian air dan curah hujan.",
                "OK");

            return;
        }

        if (newWaterLevel < 0 || newRainfall < 0)
        {
            await DisplayAlert(
                "Input Tidak Valid",
                "Nilai ketinggian air dan curah hujan tidak boleh negatif.",
                "OK");

            return;
        }

        waterLevel = newWaterLevel;
        rainfall = newRainfall;

        LoadDashboard();

        await DisplayAlert(
            "Berhasil",
            "Data sensor berhasil diperbarui dan dianalisis ulang.",
            "OK");
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        UserSession.CurrentUser = null;
        await Navigation.PopToRootAsync();
    }
}