using System.Net.Http.Json;
using System.Text.Json;

namespace Face_Recognition;

public static class NlpApiService
{
    private const string GeminiModel = "gemini-2.5-flash";

    public static string LastAnswerSource { get; private set; } = "";

    public static bool WillUseGeminiApi(string userQuestion)
    {
        if (string.IsNullOrWhiteSpace(userQuestion))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(GetLocalSiagaAnswer(userQuestion)))
        {
            return false;
        }

        if (IsQuestionTooVague(userQuestion))
        {
            return false;
        }

        if (!IsQuestionRelatedToSiaga(userQuestion))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(ApiKeys.GeminiApiKey) ||
            ApiKeys.GeminiApiKey == "ISI_API_KEY_KAMU_DI_SINI")
        {
            return false;
        }

        return true;
    }

    public static async Task<string> AskGeminiAsync(string userQuestion, string pageContext)
    {
        LastAnswerSource = "";

        if (string.IsNullOrWhiteSpace(userQuestion))
        {
            LastAnswerSource = "Lokal";
            return "";
        }

        string localAnswer = GetLocalSiagaAnswer(userQuestion);

        if (!string.IsNullOrWhiteSpace(localAnswer))
        {
            LastAnswerSource = "Lokal";
            return localAnswer;
        }

        if (IsQuestionTooVague(userQuestion))
        {
            LastAnswerSource = "Lokal";
            return "Bisa jelaskan bagian mana yang bermasalah? Misalnya login, register, kamera/wajah, database, dashboard, atau analisis risiko banjir.";
        }

        if (!IsQuestionRelatedToSiaga(userQuestion))
        {
            LastAnswerSource = "Lokal";
            return "Maaf, saya hanya dapat membantu pertanyaan terkait penggunaan aplikasi SIAGA, seperti login, registrasi, role pengguna, face recognition, database SQLite, dashboard, dan analisis risiko banjir.";
        }

        if (string.IsNullOrWhiteSpace(ApiKeys.GeminiApiKey) ||
            ApiKeys.GeminiApiKey == "ISI_API_KEY_KAMU_DI_SINI")
        {
            LastAnswerSource = "Lokal";
            return "";
        }

        try
        {
            LastAnswerSource = "Gemini";

            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(25);

            string endpoint =
                $"https://generativelanguage.googleapis.com/v1beta/models/{GeminiModel}:generateContent";

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Add("x-goog-api-key", ApiKeys.GeminiApiKey);

            var requestBody = new
            {
                systemInstruction = new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = BuildSystemInstruction()
                        }
                    }
                },
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new
                            {
                                text =
                                    $"Konteks halaman: {pageContext}\n" +
                                    $"Pertanyaan pengguna: {userQuestion}"
                            }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.10,
                    maxOutputTokens = 420
                }
            };

            request.Content = JsonContent.Create(requestBody);

            using HttpResponseMessage response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                LastAnswerSource = "Lokal";
                return "";
            }

            string json = await response.Content.ReadAsStringAsync();
            string answer = ExtractGeminiText(json);

            if (IsAnswerLikelyCut(answer))
            {
                LastAnswerSource = "Lokal";
                return GetSafeFallbackAnswer(userQuestion);
            }

            LastAnswerSource = "Gemini";
            return answer;
        }
        catch
        {
            LastAnswerSource = "Lokal";
            return "";
        }
    }

    private static string GetLocalSiagaAnswer(string userQuestion)
    {
        string text = Normalize(userQuestion);

        if (ContainsAny(text, "login", "log in", "masuk") &&
            ContainsAny(text, "gagal", "gabisa", "ga bisa", "nggak bisa", "tidak bisa", "kenapa", "error", "salah", "kok"))
        {
            return "Pastikan username, password, dan role yang dipilih sudah sama dengan data saat registrasi. Jika tetap gagal, cek apakah akun sudah berhasil tersimpan di database SQLite pada tabel User. Jika akun belum ada atau data wajah kosong, lakukan registrasi ulang.";
        }

        if (ContainsAny(text, "register", "registrasi", "daftar") &&
            ContainsAny(text, "gagal", "gabisa", "ga bisa", "nggak bisa", "tidak bisa", "kenapa", "error", "kok"))
        {
            return "Pastikan semua form wajib sudah terisi, password dan confirm password sama, serta username belum pernah digunakan. Untuk role Operator, isi kode akses OPERATOR123. Setelah itu pastikan kamera terbuka dan hanya satu wajah yang terlihat jelas.";
        }

        if (ContainsAny(text, "password", "sandi") &&
            ContainsAny(text, "sama", "beda", "tidak sama", "confirm", "konfirmasi"))
        {
            return "Registrasi akan ditolak jika Password dan Confirm Password berbeda. Samakan kedua input password, lalu coba register ulang.";
        }

        if (ContainsAny(text, "username", "user name") &&
            ContainsAny(text, "sudah", "terdaftar", "ada", "dipakai", "digunakan"))
        {
            return "Username tidak boleh digunakan dua kali. Gunakan username lain, atau cek tabel User di database SQLite untuk memastikan username tersebut memang sudah tersimpan.";
        }

        if (ContainsAny(text, "kode akses", "operator123", "kode operator"))
        {
            return "Kode akses operator pada prototype ini adalah OPERATOR123. Kode ini digunakan agar tidak semua pengguna bisa mendaftar sebagai Operator.";
        }

        if (ContainsAny(text, "kamera", "camera", "cam") &&
            ContainsAny(text, "tidak terbuka", "ga kebuka", "gak kebuka", "gabisa", "ga bisa", "nggak bisa", "gagal", "error", "kenapa", "kok"))
        {
            return "Pastikan perangkat memiliki kamera, izin kamera sudah aktif, dan kamera tidak sedang dipakai aplikasi lain. Setelah itu tutup aplikasi lalu jalankan ulang.";
        }

        if (ContainsAny(text, "wajah", "muka", "mukaku", "face", "facerecognition", "face recognition", "deteksi", "kedeteksi", "terdeteksi") &&
            ContainsAny(text, "tidak terdeteksi", "ga terdeteksi", "gak terdeteksi", "nggak terdeteksi", "ga kedeteksi", "gak kedeteksi", "nggak kedeteksi", "gabisa", "ga bisa", "nggak bisa", "gagal", "error", "kenapa", "kok"))
        {
            return "Pastikan wajah menghadap kamera, pencahayaan cukup, dan hanya ada satu wajah di foto. Hindari masker, wajah terlalu jauh, atau posisi terlalu miring. Jika masih gagal, coba ambil foto ulang dengan latar yang lebih bersih.";
        }

        if (ContainsAny(text, "database", "sqlite", "db browser", "db3", "tersimpan", "simpan", "disimpan"))
        {
            return "Data user disimpan di database SQLite bernama face_recognition_floodrisk.db3 pada FileSystem.AppDataDirectory. Foto wajah disimpan di folder FaceImages, sedangkan lokasi fotonya tersimpan di kolom FaceImagePath pada tabel User.";
        }

        if (ContainsAny(text, "foto", "gambar", "faceimage", "faceimages"))
        {
            return "Foto wajah disimpan sebagai file JPG di folder FaceImages. Format nama file adalah username_yyyyMMdd_HHmmss.jpg, dan path lengkapnya disimpan di kolom FaceImagePath.";
        }

        if (ContainsAny(text, "dashboard", "fuzzy", "banjir", "risiko", "sensor", "curah hujan", "ketinggian air"))
        {
            return "Dashboard SIAGA menampilkan risiko banjir dari data ketinggian air dan curah hujan. Metode utama yang digunakan adalah Fuzzy Logic, dengan tambahan Regression dan K-Means sebagai pembanding analisis.";
        }

        return "";
    }

    private static string GetSafeFallbackAnswer(string userQuestion)
    {
        string localAnswer = GetLocalSiagaAnswer(userQuestion);

        if (!string.IsNullOrWhiteSpace(localAnswer))
        {
            return localAnswer;
        }

        return "Pertanyaan masih terkait SIAGA, tetapi jawaban API tidak terbaca utuh. Coba tulis pertanyaan lebih spesifik, misalnya masalah login, register, kamera/wajah, database, atau dashboard.";
    }

    private static bool IsAnswerLikelyCut(string answer)
    {
        if (string.IsNullOrWhiteSpace(answer))
        {
            return false;
        }

        string trimmed = answer.Trim();

        if (trimmed.Length < 40 && !trimmed.EndsWith(".") && !trimmed.EndsWith("!") && !trimmed.EndsWith("?"))
        {
            return true;
        }

        string[] unfinishedEndings =
        {
            "mohon past",
            "pastikan",
            "jika",
            "dan",
            "atau",
            "dengan",
            "karena",
            "untuk",
            "pada"
        };

        string lower = trimmed.ToLower();

        return unfinishedEndings.Any(ending => lower.EndsWith(ending));
    }

    private static string BuildSystemInstruction()
    {
        return
            "Kamu adalah asisten bantuan internal untuk aplikasi SIAGA. " +
            "Gunakan knowledge base berikut sebagai konteks utama. " +
            "Jawab hanya pertanyaan tentang penggunaan, fitur, masalah, error, dan alur aplikasi SIAGA. " +
            "Jangan menjawab topik di luar aplikasi. " +
            "Jika pertanyaan di luar SIAGA, jawab persis: " +
            "\"Maaf, saya hanya dapat membantu pertanyaan terkait penggunaan aplikasi SIAGA.\" " +
            "Jawab dalam Bahasa Indonesia, singkat, ramah, dan mudah dipahami. " +
            "Jawab 2 sampai 4 kalimat pendek dan pastikan kalimat selesai utuh. " +
            "Jika pertanyaan tentang error, berikan langkah pengecekan paling penting saja. " +
            "Jangan menyebut bahwa kamu dilatih ulang. Jelaskan bahwa sistem menggunakan knowledge base aplikasi bila diperlukan.\n\n" +
            "KNOWLEDGE BASE SIAGA:\n" +
            NlpKnowledgeBase.GetSiagaKnowledge();
    }

    private static bool IsQuestionTooVague(string userQuestion)
    {
        string text = Normalize(userQuestion);

        string[] vagueQuestions =
        {
            "ini kenapa",
            "ini kenapa ya",
            "kok gini",
            "kok begini",
            "kenapa ya",
            "ini error",
            "ini gimana",
            "gimana ini",
            "gabisa",
            "gagal",
            "error"
        };

        string[] domainKeywords =
        {
            "login",
            "register",
            "registrasi",
            "daftar",
            "akun",
            "username",
            "password",
            "role",
            "masyarakat",
            "operator",
            "kode akses",
            "kamera",
            "camera",
            "wajah",
            "muka",
            "mukaku",
            "face",
            "deteksi",
            "kedeteksi",
            "foto",
            "gambar",
            "database",
            "sqlite",
            "db browser",
            "dashboard",
            "banjir",
            "risiko",
            "sensor",
            "fuzzy",
            "regression",
            "regresi",
            "k-means",
            "cluster"
        };

        bool containsVaguePattern = vagueQuestions.Any(pattern => text.Contains(pattern));
        bool containsDomainKeyword = domainKeywords.Any(keyword => text.Contains(keyword));

        return containsVaguePattern && !containsDomainKeyword;
    }

    private static bool IsQuestionRelatedToSiaga(string userQuestion)
    {
        string text = Normalize(userQuestion);

        string[] domainKeywords =
        {
            "siaga",
            "aplikasi",
            "login",
            "log in",
            "masuk",
            "register",
            "registrasi",
            "daftar",
            "akun",
            "username",
            "password",
            "role",
            "masyarakat",
            "operator",
            "kode akses",
            "operator123",
            "kamera",
            "camera",
            "wajah",
            "muka",
            "mukaku",
            "face",
            "faceai",
            "recognition",
            "deteksi",
            "kedeteksi",
            "terdeteksi",
            "embedding",
            "foto",
            "gambar",
            "database",
            "sqlite",
            "db browser",
            "db3",
            "filesystem",
            "appdatadirectory",
            "faceimages",
            "dashboard",
            "banjir",
            "risiko",
            "risk",
            "sensor",
            "ketinggian air",
            "curah hujan",
            "fuzzy",
            "regression",
            "regresi",
            "k-means",
            "cluster",
            "clustering"
        };

        return domainKeywords.Any(keyword => text.Contains(keyword));
    }

    private static bool ContainsAny(string text, params string[] keywords)
    {
        return keywords.Any(keyword => text.Contains(keyword));
    }

    private static string Normalize(string input)
    {
        return (input ?? "").Trim().ToLower();
    }

    private static string ExtractGeminiText(string json)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(json);

            JsonElement root = document.RootElement;

            if (!root.TryGetProperty("candidates", out JsonElement candidates) ||
                candidates.GetArrayLength() == 0)
            {
                return "";
            }

            JsonElement firstCandidate = candidates[0];

            if (!firstCandidate.TryGetProperty("content", out JsonElement content))
            {
                return "";
            }

            if (!content.TryGetProperty("parts", out JsonElement parts) ||
                parts.GetArrayLength() == 0)
            {
                return "";
            }

            if (!parts[0].TryGetProperty("text", out JsonElement textElement))
            {
                return "";
            }

            return textElement.GetString() ?? "";
        }
        catch
        {
            return "";
        }
    }
}
