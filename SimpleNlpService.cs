namespace Face_Recognition;

public class NlpHelpResult
{
    public bool IsRecognized { get; set; }
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string Action { get; set; } = "";
    public string ActionText { get; set; } = "";
}

public static class SimpleNlpService
{
    public static NlpHelpResult AnalyzeLoginHelp(string input)
    {
        string text = Normalize(input);

        if (string.IsNullOrWhiteSpace(text))
        {
            return new NlpHelpResult
            {
                IsRecognized = false,
                Title = "Perintah Kosong",
                Message = "Silakan ketik pertanyaan atau pilih salah satu pertanyaan berwarna biru."
            };
        }

        if (ContainsAny(text, "wajah tidak terdeteksi", "wajah gagal", "face recognition", "verifikasi wajah", "wajah tidak dikenali", "tidak terdeteksi"))
        {
            return new NlpHelpResult
            {
                IsRecognized = true,
                Title = "Wajah Tidak Terdeteksi",
                Message =
                    "1. Pastikan kamu sudah melakukan registrasi akun dan registrasi wajah terlebih dahulu.\n" +
                    "2. Pastikan wajah berada di tengah kamera dan menghadap lurus ke depan.\n" +
                    "3. Gunakan pencahayaan yang cukup, jangan terlalu gelap atau membelakangi cahaya.\n" +
                    "4. Jangan gunakan aksesoris yang menutupi wajah seperti masker, kacamata hitam, topi, atau penutup wajah lainnya.\n" +
                    "5. Pastikan hanya ada satu wajah yang terlihat di kamera.\n" +
                    "6. Jika wajah tetap gagal dikenali, lakukan registrasi ulang agar data wajah tersimpan lebih jelas.",
                Action = "GoToRegister",
                ActionText = "Registrasi di sini"
            };
        }

        if (ContainsAny(text, "tidak bisa login", "login gagal", "gagal login", "username salah", "password salah", "role salah", "tidak bisa masuk"))
        {
            return new NlpHelpResult
            {
                IsRecognized = true,
                Title = "Tidak Bisa Login",
                Message =
                    "1. Pastikan username, password, dan role sudah diisi dengan benar.\n" +
                    "2. Pastikan role yang dipilih sama dengan role saat registrasi.\n" +
                    "3. Jika belum memiliki akun, lakukan registrasi terlebih dahulu.\n" +
                    "4. Setelah registrasi berhasil, login kembali lalu lanjutkan ke proses verifikasi wajah.",
                Action = "GoToRegister",
                ActionText = "Registrasi di sini"
            };
        }

        if (ContainsAny(text, "belum punya akun", "belum daftar", "cara register", "cara registrasi", "daftar akun", "buat akun", "registrasi"))
        {
            return new NlpHelpResult
            {
                IsRecognized = true,
                Title = "Belum Punya Akun",
                Message =
                    "Untuk menggunakan aplikasi, kamu perlu membuat akun terlebih dahulu.\n\n" +
                    "1. Tekan tombol Registrasi di bawah.\n" +
                    "2. Isi username, password, confirm password, dan role.\n" +
                    "3. Setelah itu sistem akan membuka kamera untuk registrasi wajah.\n" +
                    "4. Jika registrasi berhasil, kamu bisa kembali login.",
                Action = "GoToRegister",
                ActionText = "Registrasi di sini"
            };
        }

        if (ContainsAny(text, "kamera tidak terbuka", "kamera gagal", "kamera error", "tidak bisa kamera", "izin kamera", "permission kamera"))
        {
            return new NlpHelpResult
            {
                IsRecognized = true,
                Title = "Kamera Tidak Terbuka",
                Message =
                    "1. Pastikan aplikasi sudah memiliki izin akses kamera.\n" +
                    "2. Tutup aplikasi lain yang sedang menggunakan kamera.\n" +
                    "3. Coba restart aplikasi lalu buka kembali.\n" +
                    "4. Jika masih gagal, periksa pengaturan permission aplikasi di perangkat."
            };
        }

        if (ContainsAny(text, "lupa password", "password lupa", "akun lupa", "tidak ingat password"))
        {
            return new NlpHelpResult
            {
                IsRecognized = true,
                Title = "Lupa Password",
                Message =
                    "Pada versi aplikasi ini belum tersedia fitur reset password otomatis.\n\n" +
                    "Solusinya:\n" +
                    "1. Coba ingat kembali username, password, dan role yang digunakan saat registrasi.\n" +
                    "2. Jika tetap tidak bisa, buat akun baru melalui halaman registrasi.\n" +
                    "3. Saat membuat akun baru, pastikan data wajah direkam dengan jelas.",
                Action = "GoToRegister",
                ActionText = "Buat akun baru"
            };
        }

        if (ContainsAny(text, "bantuan", "help", "ada masalah", "pertanyaan", "apa saja"))
        {
            return new NlpHelpResult
            {
                IsRecognized = true,
                Title = "Bantuan Login",
                Message =
                    "Kamu bisa bertanya seputar:\n" +
                    "1. Wajah tidak terdeteksi.\n" +
                    "2. Tidak bisa login.\n" +
                    "3. Belum punya akun.\n" +
                    "4. Kamera tidak terbuka.\n" +
                    "5. Lupa password."
            };
        }

        return new NlpHelpResult
        {
            IsRecognized = false,
            Title = "Pertanyaan Tidak Dikenali",
            Message =
                "Maaf, sistem belum memahami pertanyaan tersebut.\n\n" +
                "Coba gunakan pertanyaan seperti:\n" +
                "1. Wajah tidak terdeteksi?\n" +
                "2. Tidak bisa login?\n" +
                "3. Belum punya akun?\n" +
                "4. Kamera tidak terbuka?"
        };
    }

    public static NlpHelpResult AnalyzeRegisterHelp(string input)
    {
        string text = Normalize(input);

        if (string.IsNullOrWhiteSpace(text))
        {
            return new NlpHelpResult
            {
                IsRecognized = false,
                Title = "Perintah Kosong",
                Message = "Silakan ketik pertanyaan atau pilih salah satu pertanyaan berwarna biru."
            };
        }

        if (ContainsAny(text, "cara register", "cara registrasi", "cara daftar", "daftar akun", "buat akun", "registrasi"))
        {
            return new NlpHelpResult
            {
                IsRecognized = true,
                Title = "Cara Registrasi",
                Message =
                    "1. Isi username yang ingin digunakan.\n" +
                    "2. Isi password dan confirm password dengan nilai yang sama.\n" +
                    "3. Pilih role sebagai Masyarakat atau Operator.\n" +
                    "4. Tekan tombol Register.\n" +
                    "5. Setelah itu kamera akan terbuka untuk merekam data wajah.\n" +
                    "6. Jika wajah berhasil direkam, akun siap digunakan untuk login."
            };
        }

        if (ContainsAny(text, "wajah tidak terdeteksi", "wajah gagal", "wajah tidak dikenali", "face recognition", "registrasi wajah", "kamera tidak mendeteksi"))
        {
            return new NlpHelpResult
            {
                IsRecognized = true,
                Title = "Registrasi Wajah Gagal",
                Message =
                    "1. Pastikan wajah terlihat jelas di kamera.\n" +
                    "2. Jangan menggunakan masker, kacamata hitam, topi, atau aksesoris yang menutupi wajah.\n" +
                    "3. Gunakan pencahayaan yang cukup.\n" +
                    "4. Jangan terlalu jauh atau terlalu dekat dari kamera.\n" +
                    "5. Pastikan hanya satu wajah yang masuk ke kamera.\n" +
                    "6. Ulangi proses registrasi jika wajah belum berhasil tersimpan."
            };
        }

        if (ContainsAny(text, "password tidak sama", "confirm password", "konfirmasi password", "password beda", "password berbeda"))
        {
            return new NlpHelpResult
            {
                IsRecognized = true,
                Title = "Password Tidak Sama",
                Message =
                    "Password dan Confirm Password harus sama.\n\n" +
                    "Contoh:\n" +
                    "Password: 12345\n" +
                    "Confirm Password: 12345\n\n" +
                    "Jika berbeda satu huruf atau angka saja, registrasi akan ditolak."
            };
        }

        if (ContainsAny(text, "username sudah terdaftar", "username ada", "username dipakai", "akun sudah ada"))
        {
            return new NlpHelpResult
            {
                IsRecognized = true,
                Title = "Username Sudah Terdaftar",
                Message =
                    "Username yang sama tidak bisa digunakan dua kali.\n\n" +
                    "Solusinya:\n" +
                    "1. Gunakan username lain.\n" +
                    "2. Tambahkan angka atau identitas unik pada username.\n" +
                    "3. Jika kamu sudah pernah registrasi, kembali ke halaman login.",
                Action = "GoToLogin",
                ActionText = "Kembali ke Login"
            };
        }

        if (ContainsAny(text, "kamera tidak terbuka", "kamera gagal", "kamera error", "izin kamera", "permission kamera"))
        {
            return new NlpHelpResult
            {
                IsRecognized = true,
                Title = "Kamera Tidak Terbuka",
                Message =
                    "1. Pastikan aplikasi memiliki izin kamera.\n" +
                    "2. Tutup aplikasi lain yang sedang menggunakan kamera.\n" +
                    "3. Restart aplikasi jika kamera masih belum terbuka.\n" +
                    "4. Periksa pengaturan permission aplikasi pada perangkat."
            };
        }

        if (ContainsAny(text, "sudah registrasi", "sudah daftar", "mau login", "ke login", "kembali login"))
        {
            return new NlpHelpResult
            {
                IsRecognized = true,
                Title = "Sudah Registrasi",
                Message =
                    "Jika akun dan wajah sudah berhasil diregistrasikan, kamu bisa kembali ke halaman login.\n\n" +
                    "Setelah login, sistem akan meminta verifikasi wajah untuk memastikan identitas pengguna.",
                Action = "GoToLogin",
                ActionText = "Kembali ke Login"
            };
        }

        if (ContainsAny(text, "bantuan", "help", "ada masalah", "pertanyaan", "apa saja"))
        {
            return new NlpHelpResult
            {
                IsRecognized = true,
                Title = "Bantuan Registrasi",
                Message =
                    "Kamu bisa bertanya seputar:\n" +
                    "1. Cara registrasi.\n" +
                    "2. Registrasi wajah gagal.\n" +
                    "3. Password tidak sama.\n" +
                    "4. Username sudah terdaftar.\n" +
                    "5. Kamera tidak terbuka."
            };
        }

        return new NlpHelpResult
        {
            IsRecognized = false,
            Title = "Pertanyaan Tidak Dikenali",
            Message =
                "Maaf, sistem belum memahami pertanyaan tersebut.\n\n" +
                "Coba gunakan pertanyaan seperti:\n" +
                "1. Cara registrasi?\n" +
                "2. Registrasi wajah gagal?\n" +
                "3. Password tidak sama?\n" +
                "4. Username sudah terdaftar?"
        };
    }

    private static string Normalize(string input)
    {
        return (input ?? "").ToLower().Trim();
    }

    private static bool ContainsAny(string text, params string[] keywords)
    {
        foreach (string keyword in keywords)
        {
            if (text.Contains(keyword))
            {
                return true;
            }
        }

        return false;
    }
} 