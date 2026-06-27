namespace Face_Recognition;

public partial class FaceRecognitionPage : ContentPage
{
    private bool isAnimating = false;

    public FaceRecognitionPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (isAnimating)
            return;

        isAnimating = true;
        _ = AnimateVerifyCircle();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        isAnimating = false;
    }

    private async Task AnimateVerifyCircle()
    {
        while (isAnimating)
        {
            await VerifyCircle.ScaleTo(1.12, 1200, Easing.SinInOut);

            if (!isAnimating)
                break;

            await VerifyCircle.ScaleTo(1.0, 1200, Easing.SinInOut);
        }
    }

    private async void OnVerifyFaceClicked(object sender, EventArgs e)
    {
        User? currentUser = UserSession.CurrentUser;

        if (currentUser == null)
        {
            await DisplayAlert("Error", "Session login tidak ditemukan. Silakan login ulang.", "OK");
            await Navigation.PopToRootAsync();
            return;
        }

        StatusLabel.Text = "Membuka kamera untuk verifikasi...";

        await DisplayAlert(
            "Verifikasi Wajah",
            "Setelah menekan OK, kamera akan terbuka. Pastikan wajah terlihat jelas.",
            "OK");

        FaceEmbeddingResult faceResult = await FaceRecognitionService.CaptureFaceEmbeddingAsync();

        if (!faceResult.Success || faceResult.Embedding == null)
        {
            await GagalVerifikasi(faceResult.Message);
            return;
        }

        double similarity = FaceRecognitionService.CompareFaces(
            currentUser.FaceEmbedding,
            faceResult.Embedding);

        if (similarity >= FaceRecognitionService.FaceMatchThreshold)
        {
            StatusLabel.Text = $"Verifikasi berhasil. Skor: {similarity:0.00}";

            await DisplayAlert(
                "Verifikasi Berhasil",
                $"Wajah cocok dengan data pendaftar.\nSkor kecocokan: {similarity:0.00}",
                "OK");

            await Navigation.PushAsync(new DashboardPage());
        }
        else
        {
            await GagalVerifikasi(
                $"Wajah tidak cocok dengan data pendaftar.\nSkor kecocokan: {similarity:0.00}");
        }
    }

    private async Task GagalVerifikasi(string message)
    {
        StatusLabel.Text = "Akses ditolak.";

        await DisplayAlert(
            "Verifikasi Wajah Gagal",
            message,
            "OK");

        UserSession.CurrentUser = null;

        await Navigation.PopToRootAsync();
    }
}