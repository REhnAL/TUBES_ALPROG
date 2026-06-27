using System.Threading.Tasks;

namespace Face_Recognition;

public partial class LoginPage : ContentPage
{
    private bool isAnimating = false;
    private string currentHelpAction = "";

    public LoginPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (isAnimating)
        {
            return;
        }

        isAnimating = true;
        _ = AnimateBackground();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        isAnimating = false;
    }

    private async Task AnimateBackground()
    {
        while (isAnimating)
        {
            await CircleOne.TranslateTo(35, 25, 1200, Easing.SinInOut);

            if (!isAnimating)
            {
                break;
            }

            await CircleOne.TranslateTo(0, 0, 1200, Easing.SinInOut);

            if (!isAnimating)
            {
                break;
            }

            await CircleTwo.ScaleTo(1.12, 1200, Easing.SinInOut);

            if (!isAnimating)
            {
                break;
            }

            await CircleTwo.ScaleTo(1.0, 1200, Easing.SinInOut);
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string username = LoginUsernameEntry.Text ?? "";
        string password = LoginPasswordEntry.Text ?? "";
        string role = LoginRolePicker.SelectedItem?.ToString() ?? "";

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(role))
        {
            await DisplayAlert("Error", "Username, password, dan role harus diisi.", "OK");
            return;
        }

        User? userDitemukan = await DatabaseService.GetUserByLoginAsync(username, password, role);

        if (userDitemukan == null)
        {
            await DisplayAlert(
                "Login Gagal",
                "Username, password, atau role salah. Jika belum memiliki akun, silakan lakukan registrasi terlebih dahulu.",
                "OK");

            ShowLoginHelpResult(SimpleNlpService.AnalyzeLoginHelp("tidak bisa login"));
            LoginHelpPanel.IsVisible = true;
            return;
        }

        if (userDitemukan.FaceEmbedding.Length == 0)
        {
            await DisplayAlert(
                "Data Wajah Tidak Ditemukan",
                "Akun ditemukan, tetapi data wajah belum tersimpan dengan benar. Silakan registrasi ulang.",
                "OK");

            return;
        }

        UserSession.CurrentUser = userDitemukan;

        await Navigation.PushAsync(new FaceRecognitionPage());
    }

    private async void OnGoToRegisterClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage());
    }

    private void OnLoginHelpToggleClicked(object sender, EventArgs e)
    {
        LoginHelpPanel.IsVisible = !LoginHelpPanel.IsVisible;
    }

    private async void OnLoginHelpSendClicked(object sender, EventArgs e)
    {
        string question = LoginHelpEntry.Text ?? "";
        await ShowLoginGeminiHelpAsync(question);
    }

    private async void OnLoginQuickQuestionClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string question)
        {
            LoginHelpEntry.Text = question;
            await ShowLoginGeminiHelpAsync(question);
        }
    }

    private async Task ShowLoginGeminiHelpAsync(string question)
    {
        question = question?.Trim() ?? "";

        currentHelpAction = "";
        LoginHelpActionButton.IsVisible = false;

        if (string.IsNullOrWhiteSpace(question))
        {
            LoginHelpTitleLabel.Text = "Pertanyaan Kosong";
            LoginHelpMessageLabel.Text = "Silakan ketik pertanyaan terlebih dahulu, misalnya: kenapa saya tidak bisa login?";
            return;
        }

        bool willUseGemini = NlpApiService.WillUseGeminiApi(question);

        if (willUseGemini)
        {
            LoginHelpTitleLabel.Text = "Memproses Gemini AI...";
            LoginHelpMessageLabel.Text = "Pertanyaan ini diproses menggunakan Gemini AI karena membutuhkan jawaban yang lebih fleksibel.";
        }
        else
        {
            LoginHelpTitleLabel.Text = "Memproses Bantuan Lokal SIAGA...";
            LoginHelpMessageLabel.Text = "Pertanyaan ini diproses dari knowledge base lokal aplikasi dan tidak menggunakan quota Gemini.";
        }

        await Task.Delay(80);

        NlpHelpResult fallbackResult = SimpleNlpService.AnalyzeLoginHelp(question);

        string answer = await NlpApiService.AskGeminiAsync(question, "Login Page");

        if (!string.IsNullOrWhiteSpace(answer))
        {
            ShowLoginAnswer(answer, fallbackResult);
        }
        else if (!string.IsNullOrWhiteSpace(fallbackResult.Message))
        {
            ShowLoginHelpResult(fallbackResult);
        }
        else
        {
            LoginHelpTitleLabel.Text = "Jawaban Tidak Ditemukan";
            LoginHelpMessageLabel.Text = "Sistem belum menemukan jawaban untuk pertanyaan ini. Coba tulis lebih spesifik, misalnya masalah login, register, kamera/wajah, database, atau dashboard.";
            LoginHelpActionButton.IsVisible = false;
        }
    }

    private async void OnLoginHelpActionClicked(object sender, EventArgs e)
    {
        if (currentHelpAction == "GoToRegister")
        {
            await Navigation.PushAsync(new RegisterPage());
        }
    }

    private void ShowLoginAnswer(string answer, NlpHelpResult fallbackResult)
    {
        if (NlpApiService.LastAnswerSource == "Gemini")
        {
            LoginHelpTitleLabel.Text = "Jawaban Gemini AI";
        }
        else
        {
            LoginHelpTitleLabel.Text = "Jawaban Lokal SIAGA";
        }

        LoginHelpMessageLabel.Text = answer;

        currentHelpAction = fallbackResult.Action;

        if (!string.IsNullOrWhiteSpace(fallbackResult.Action))
        {
            LoginHelpActionButton.Text = fallbackResult.ActionText;
            LoginHelpActionButton.IsVisible = true;
        }
        else
        {
            LoginHelpActionButton.IsVisible = false;
        }
    }

    private void ShowLoginHelpResult(NlpHelpResult result)
    {
        LoginHelpTitleLabel.Text = result.Title;
        LoginHelpMessageLabel.Text = result.Message;

        currentHelpAction = result.Action;

        if (!string.IsNullOrWhiteSpace(result.Action))
        {
            LoginHelpActionButton.Text = result.ActionText;
            LoginHelpActionButton.IsVisible = true;
        }
        else
        {
            LoginHelpActionButton.IsVisible = false;
        }
    }
}
