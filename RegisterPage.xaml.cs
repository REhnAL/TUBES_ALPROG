using System.Threading.Tasks;

namespace Face_Recognition;

public partial class RegisterPage : ContentPage
{
    private const string OperatorAccessCode = "OPERATOR123";

    private bool isAnimating = false;
    private string currentHelpAction = "";

    public RegisterPage()
    {
        InitializeComponent();
        HideRoleDetailForms();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (isAnimating)
        {
            return;
        }

        isAnimating = true;
        _ = AnimateCircle();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        isAnimating = false;
    }

    private async Task AnimateCircle()
    {
        while (isAnimating)
        {
            await RegisterCircle.ScaleTo(1.10, 1200, Easing.SinInOut);

            if (!isAnimating)
            {
                break;
            }

            await RegisterCircle.ScaleTo(1.0, 1200, Easing.SinInOut);
        }
    }

    private void OnRegisterRoleChanged(object sender, EventArgs e)
    {
        string role = RegisterRolePicker.SelectedItem?.ToString() ?? "";

        if (role == "Masyarakat")
        {
            MasyarakatFormPanel.IsVisible = true;
            OperatorFormPanel.IsVisible = false;
        }
        else if (role == "Operator")
        {
            MasyarakatFormPanel.IsVisible = false;
            OperatorFormPanel.IsVisible = true;
        }
        else
        {
            HideRoleDetailForms();
        }
    }

    private void HideRoleDetailForms()
    {
        MasyarakatFormPanel.IsVisible = false;
        OperatorFormPanel.IsVisible = false;
    }

    private async void OnOperatorAccessInfoClicked(object sender, EventArgs e)
    {
        await DisplayAlert(
            "Informasi Kode Akses Operator",
            "Kode akses operator hanya diberikan oleh admin atau penanggung jawab sistem. Kode ini digunakan agar tidak semua pengguna dapat mendaftar sebagai operator.",
            "OK");
    }

    private async void OnMonitoringAreaInfoClicked(object sender, EventArgs e)
    {
        await DisplayAlert(
            "Informasi Wilayah Pantauan",
            "Wilayah pantauan adalah area yang ingin dipantau oleh pengguna, misalnya nama kelurahan, kecamatan, kawasan rumah, atau titik rawan banjir tertentu.",
            "OK");
    }

    private async void OnDutyAreaInfoClicked(object sender, EventArgs e)
    {
        await DisplayAlert(
            "Informasi Wilayah Tugas",
            "Wilayah tugas adalah area yang menjadi tanggung jawab operator untuk memantau data sensor dan status risiko banjir.",
            "OK");
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        string username = RegisterUsernameEntry.Text ?? "";
        string password = RegisterPasswordEntry.Text ?? "";
        string confirmPassword = ConfirmPasswordEntry.Text ?? "";
        string role = RegisterRolePicker.SelectedItem?.ToString() ?? "";

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(confirmPassword) ||
            string.IsNullOrWhiteSpace(role))
        {
            await DisplayAlert("Error", "Username, password, confirm password, dan role harus diisi.", "OK");
            return;
        }

        if (password != confirmPassword)
        {
            await DisplayAlert("Error", "Password dan Confirm Password tidak sama.", "OK");
            ShowRegisterHelpResult(SimpleNlpService.AnalyzeRegisterHelp("password tidak sama"));
            RegisterHelpPanel.IsVisible = true;
            return;
        }

        User? existingUser = await DatabaseService.GetUserByUsernameAsync(username);

        if (existingUser != null)
        {
            await DisplayAlert("Error", "Username sudah terdaftar.", "OK");
            ShowRegisterHelpResult(SimpleNlpService.AnalyzeRegisterHelp("username sudah terdaftar"));
            RegisterHelpPanel.IsVisible = true;
            return;
        }

        string fullName = "";
        string age = "";
        string phoneNumber = "";
        string address = "";
        string monitoringArea = "";
        string operatorId = "";
        string institution = "";
        string dutyArea = "";

        if (role == "Masyarakat")
        {
            fullName = MasyarakatFullNameEntry.Text ?? "";
            age = MasyarakatAgeEntry.Text ?? "";
            phoneNumber = MasyarakatPhoneEntry.Text ?? "";
            address = MasyarakatAddressEntry.Text ?? "";
            monitoringArea = MasyarakatMonitoringAreaEntry.Text ?? "";

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(age) ||
                string.IsNullOrWhiteSpace(phoneNumber) ||
                string.IsNullOrWhiteSpace(address) ||
                string.IsNullOrWhiteSpace(monitoringArea))
            {
                await DisplayAlert(
                    "Data Belum Lengkap",
                    "Untuk role Masyarakat, isi Nama Lengkap, Usia, Nomor Telepon, Alamat/Domisili, dan Wilayah Pantauan.",
                    "OK");
                return;
            }

            if (!int.TryParse(age, out int parsedAge) || parsedAge <= 0)
            {
                await DisplayAlert("Input Tidak Valid", "Usia harus berupa angka yang valid.", "OK");
                return;
            }
        }
        else if (role == "Operator")
        {
            fullName = OperatorFullNameEntry.Text ?? "";
            operatorId = OperatorIdEntry.Text ?? "";
            institution = OperatorInstitutionEntry.Text ?? "";
            dutyArea = OperatorDutyAreaEntry.Text ?? "";
            phoneNumber = OperatorPhoneEntry.Text ?? "";
            string accessCode = OperatorAccessCodeEntry.Text ?? "";

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(operatorId) ||
                string.IsNullOrWhiteSpace(institution) ||
                string.IsNullOrWhiteSpace(dutyArea) ||
                string.IsNullOrWhiteSpace(phoneNumber) ||
                string.IsNullOrWhiteSpace(accessCode))
            {
                await DisplayAlert(
                    "Data Belum Lengkap",
                    "Untuk role Operator, isi Nama Lengkap, ID Operator, Instansi/Unit, Wilayah Tugas, Nomor Telepon, dan Kode Akses Operator.",
                    "OK");
                return;
            }

            if (accessCode != OperatorAccessCode)
            {
                await DisplayAlert(
                    "Kode Akses Salah",
                    "Kode akses operator tidak sesuai. Hubungi admin atau penanggung jawab sistem untuk mendapatkan kode yang benar.",
                    "OK");
                return;
            }
        }

        await DisplayAlert(
            "Registrasi Wajah",
            "Setelah menekan OK, kamera akan terbuka. Pastikan wajah terlihat jelas dan tidak tertutup aksesoris.",
            "OK");

        FaceEmbeddingResult faceResult = await FaceRecognitionService.CaptureFaceEmbeddingAsync(username);

        if (!faceResult.Success || faceResult.Embedding == null)
        {
            await DisplayAlert("Registrasi Wajah Gagal", faceResult.Message, "OK");
            ShowRegisterHelpResult(SimpleNlpService.AnalyzeRegisterHelp("registrasi wajah gagal"));
            RegisterHelpPanel.IsVisible = true;
            return;
        }

        User userBaru = new User(
            username,
            password,
            role,
            faceResult.Embedding,
            faceResult.ImagePath,
            fullName,
            age,
            phoneNumber,
            address,
            monitoringArea,
            operatorId,
            institution,
            dutyArea);

        await DatabaseService.SaveUserAsync(userBaru);

        await DisplayAlert(
            "Berhasil",
            "Registrasi akun, data wajah, dan foto wajah berhasil disimpan ke database SQLite.",
            "OK");

        await Navigation.PopToRootAsync();
    }

    private async void OnBackToLoginClicked(object sender, EventArgs e)
    {
        await Navigation.PopToRootAsync();
    }

    private void OnRegisterHelpToggleClicked(object sender, EventArgs e)
    {
        RegisterHelpPanel.IsVisible = !RegisterHelpPanel.IsVisible;
    }

    private async void OnRegisterHelpSendClicked(object sender, EventArgs e)
    {
        string question = RegisterHelpEntry.Text ?? "";
        await ShowRegisterGeminiHelpAsync(question);
    }

    private async void OnRegisterQuickQuestionClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string question)
        {
            RegisterHelpEntry.Text = question;
            await ShowRegisterGeminiHelpAsync(question);
        }
    }

    private async Task ShowRegisterGeminiHelpAsync(string question)
    {
        question = question?.Trim() ?? "";

        currentHelpAction = "";
        RegisterHelpActionButton.IsVisible = false;

        if (string.IsNullOrWhiteSpace(question))
        {
            RegisterHelpTitleLabel.Text = "Pertanyaan Kosong";
            RegisterHelpMessageLabel.Text = "Silakan ketik pertanyaan terlebih dahulu, misalnya: kenapa saya tidak bisa register?";
            return;
        }

        bool willUseGemini = NlpApiService.WillUseGeminiApi(question);

        if (willUseGemini)
        {
            RegisterHelpTitleLabel.Text = "Memproses Gemini AI...";
            RegisterHelpMessageLabel.Text = "Pertanyaan ini diproses menggunakan Gemini AI karena membutuhkan jawaban yang lebih fleksibel.";
        }
        else
        {
            RegisterHelpTitleLabel.Text = "Memproses Bantuan Lokal SIAGA...";
            RegisterHelpMessageLabel.Text = "Pertanyaan ini diproses dari knowledge base lokal aplikasi dan tidak menggunakan quota Gemini.";
        }

        await Task.Delay(80);

        NlpHelpResult fallbackResult = SimpleNlpService.AnalyzeRegisterHelp(question);

        string answer = await NlpApiService.AskGeminiAsync(question, "Register Page");

        if (!string.IsNullOrWhiteSpace(answer))
        {
            ShowRegisterAnswer(answer, fallbackResult);
        }
        else if (!string.IsNullOrWhiteSpace(fallbackResult.Message))
        {
            ShowRegisterHelpResult(fallbackResult);
        }
        else
        {
            RegisterHelpTitleLabel.Text = "Jawaban Tidak Ditemukan";
            RegisterHelpMessageLabel.Text = "Sistem belum menemukan jawaban untuk pertanyaan ini. Coba tulis lebih spesifik, misalnya masalah login, register, kamera/wajah, database, atau dashboard.";
            RegisterHelpActionButton.IsVisible = false;
        }
    }

    private async void OnRegisterHelpActionClicked(object sender, EventArgs e)
    {
        if (currentHelpAction == "GoToLogin")
        {
            await Navigation.PopToRootAsync();
        }
    }

    private void ShowRegisterAnswer(string answer, NlpHelpResult fallbackResult)
    {
        if (NlpApiService.LastAnswerSource == "Gemini")
        {
            RegisterHelpTitleLabel.Text = "Jawaban Gemini AI";
        }
        else
        {
            RegisterHelpTitleLabel.Text = "Jawaban Lokal SIAGA";
        }

        RegisterHelpMessageLabel.Text = answer;

        currentHelpAction = fallbackResult.Action;

        if (!string.IsNullOrWhiteSpace(fallbackResult.Action))
        {
            RegisterHelpActionButton.Text = fallbackResult.ActionText;
            RegisterHelpActionButton.IsVisible = true;
        }
        else
        {
            RegisterHelpActionButton.IsVisible = false;
        }
    }

    private void ShowRegisterHelpResult(NlpHelpResult result)
    {
        RegisterHelpTitleLabel.Text = result.Title;
        RegisterHelpMessageLabel.Text = result.Message;

        currentHelpAction = result.Action;

        if (!string.IsNullOrWhiteSpace(result.Action))
        {
            RegisterHelpActionButton.Text = result.ActionText;
            RegisterHelpActionButton.IsVisible = true;
        }
        else
        {
            RegisterHelpActionButton.IsVisible = false;
        }
    }
}
