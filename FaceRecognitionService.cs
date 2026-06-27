using FaceAiSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Face_Recognition;

public static class FaceRecognitionService
{
    // Threshold rekomendasi awal.
    // Makin tinggi = makin ketat.
    // Kalau sering gagal padahal wajah sama, bisa turunkan ke 0.38.
    // Kalau terlalu gampang lolos, naikkan ke 0.46.
    public const double FaceMatchThreshold = 0.42;

    private static readonly Lazy<IFaceDetectorWithLandmarks> FaceDetector =
        new Lazy<IFaceDetectorWithLandmarks>(() =>
            FaceAiSharpBundleFactory.CreateFaceDetectorWithLandmarks());

    private static readonly Lazy<IFaceEmbeddingsGenerator> FaceRecognizer =
        new Lazy<IFaceEmbeddingsGenerator>(() =>
            FaceAiSharpBundleFactory.CreateFaceEmbeddingsGenerator());

    // Dipakai untuk login / verifikasi wajah.
    // File tetap disimpan, tetapi namanya pakai format umum.
    public static Task<FaceEmbeddingResult> CaptureFaceEmbeddingAsync()
    {
        return CaptureFaceEmbeddingAsync("verification");
    }

    // Dipakai untuk register, supaya nama foto menjadi username_tanggaljam.jpg
    public static async Task<FaceEmbeddingResult> CaptureFaceEmbeddingAsync(string username)
    {
        string savedImagePath = "";

        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                return FaceEmbeddingResult.Fail("Kamera tidak tersedia di perangkat ini.");
            }

            FileResult? photo = await MediaPicker.Default.CapturePhotoAsync();

            if (photo == null)
            {
                return FaceEmbeddingResult.Fail("Pengambilan foto dibatalkan.");
            }

            savedImagePath = await SaveCapturedPhotoAsync(photo, username);

            using Stream savedImageStream = File.OpenRead(savedImagePath);
            using SixLabors.ImageSharp.Image<Rgb24> image =
                SixLabors.ImageSharp.Image.Load<Rgb24>(savedImageStream);

            FaceEmbeddingResult result = ExtractFaceEmbedding(image, savedImagePath);

            if (!result.Success && File.Exists(savedImagePath))
            {
                File.Delete(savedImagePath);
            }

            return result;
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrWhiteSpace(savedImagePath) && File.Exists(savedImagePath))
            {
                File.Delete(savedImagePath);
            }

            return FaceEmbeddingResult.Fail($"Gagal memproses wajah: {ex.Message}");
        }
    }

    private static async Task<string> SaveCapturedPhotoAsync(FileResult photo, string username)
    {
        string faceImagesFolder = Path.Combine(
            FileSystem.AppDataDirectory,
            "FaceImages");

        Directory.CreateDirectory(faceImagesFolder);

        string safeUsername = SanitizeFileName(username);
        string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        string fileName = $"{safeUsername}_{timeStamp}.jpg";
        string savedImagePath = Path.Combine(faceImagesFolder, fileName);

        using Stream sourceStream = await photo.OpenReadAsync();
        using FileStream targetStream = File.Create(savedImagePath);

        await sourceStream.CopyToAsync(targetStream);

        return savedImagePath;
    }

    private static string SanitizeFileName(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return "unknown";
        }

        string cleaned = input.Trim().ToLower();

        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            cleaned = cleaned.Replace(invalidChar.ToString(), "");
        }

        cleaned = cleaned.Replace(" ", "_");

        while (cleaned.Contains("__"))
        {
            cleaned = cleaned.Replace("__", "_");
        }

        return cleaned;
    }

    private static FaceEmbeddingResult ExtractFaceEmbedding(
        Image<Rgb24> image,
        string imagePath)
    {
        var faces = FaceDetector.Value.DetectFaces(image).ToList();

        if (faces.Count == 0)
        {
            return FaceEmbeddingResult.Fail("Tidak ada wajah yang terdeteksi. Pastikan wajah terlihat jelas.");
        }

        if (faces.Count > 1)
        {
            return FaceEmbeddingResult.Fail("Terdeteksi lebih dari satu wajah. Foto harus berisi satu wajah saja.");
        }

        var face = faces[0];

        if (face.Landmarks == null)
        {
            return FaceEmbeddingResult.Fail("Titik landmark wajah tidak ditemukan. Coba foto ulang dengan pencahayaan lebih baik.");
        }

        FaceRecognizer.Value.AlignFaceUsingLandmarks(image, face.Landmarks);

        float[] embedding = FaceRecognizer.Value.GenerateEmbedding(image).ToArray();

        return FaceEmbeddingResult.Ok(embedding, imagePath);
    }

    public static double CompareFaces(float[] registeredEmbedding, float[] currentEmbedding)
    {
        if (registeredEmbedding.Length != currentEmbedding.Length)
        {
            return 0;
        }

        double dot = 0;

        for (int i = 0; i < registeredEmbedding.Length; i++)
        {
            dot += registeredEmbedding[i] * currentEmbedding[i];
        }

        return dot;
    }
}

public class FaceEmbeddingResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public float[]? Embedding { get; set; }
    public string ImagePath { get; set; } = "";

    public static FaceEmbeddingResult Ok(float[] embedding)
    {
        return new FaceEmbeddingResult
        {
            Success = true,
            Message = "Wajah berhasil diproses.",
            Embedding = embedding,
            ImagePath = ""
        };
    }

    public static FaceEmbeddingResult Ok(float[] embedding, string imagePath)
    {
        return new FaceEmbeddingResult
        {
            Success = true,
            Message = "Wajah berhasil diproses.",
            Embedding = embedding,
            ImagePath = imagePath
        };
    }

    public static FaceEmbeddingResult Fail(string message)
    {
        return new FaceEmbeddingResult
        {
            Success = false,
            Message = message,
            Embedding = null,
            ImagePath = ""
        };
    }
}
