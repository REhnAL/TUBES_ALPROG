namespace Face_Recognition;

public static class NlpKnowledgeBase
{
    public static string GetSiagaKnowledge()
    {
        return """
        NAMA DAN FUNGSI APLIKASI
        - Nama aplikasi: SIAGA.
        - SIAGA adalah aplikasi .NET MAUI untuk analisis risiko banjir.
        - Sistem memakai data ketinggian air dan curah hujan.
        - Metode utama analisis risiko adalah Fuzzy Logic.
        - Analisis tambahan adalah Regression dan K-Means Clustering.
        - Sistem memiliki fitur login, register, face recognition, dashboard, role user, dan database SQLite.

        ROLE PENGGUNA
        - Role Masyarakat digunakan untuk melihat informasi risiko banjir, data sensor, dan rekomendasi tindakan.
        - Role Operator digunakan untuk memantau dan mengubah data sensor simulasi di dashboard.
        - Operator memiliki akses lebih tinggi dibanding Masyarakat.
        - Kode akses operator pada prototype ini adalah OPERATOR123.
        - Pada sistem nyata, kode akses operator seharusnya hanya diberikan oleh admin atau penanggung jawab sistem.

        REGISTER
        - Register membutuhkan username, password, confirm password, dan role.
        - Jika role Masyarakat dipilih, form tambahan berisi Nama Lengkap, Usia, Nomor Telepon, Alamat/Domisili, dan Wilayah Pantauan.
        - Jika role Operator dipilih, form tambahan berisi Nama Lengkap, ID Operator, Instansi/Unit, Wilayah Tugas, Nomor Telepon, dan Kode Akses Operator.
        - Jika password dan confirm password tidak sama, registrasi ditolak.
        - Jika username sudah terdaftar, registrasi ditolak.
        - Jika kode akses operator salah, registrasi operator ditolak.
        - Setelah data valid, sistem membuka kamera untuk registrasi wajah.
        - Jika kamera dibatalkan, wajah tidak jelas, atau lebih dari satu wajah terdeteksi, registrasi wajah gagal.

        LOGIN
        - Login membutuhkan username, password, dan role yang sesuai.
        - Data login dicek dari SQLite.
        - Jika username, password, atau role salah, login gagal.
        - Jika akun ditemukan tetapi data wajah kosong, pengguna perlu registrasi ulang.
        - Setelah login data benar, pengguna diarahkan ke halaman face recognition untuk verifikasi wajah.

        FACE RECOGNITION
        - Face recognition memakai FaceAISharp.
        - Sistem menyimpan face embedding untuk proses pencocokan wajah.
        - Face embedding disimpan di SQLite pada kolom FaceEmbeddingData.
        - Foto wajah juga disimpan sebagai file gambar di folder FaceImages.
        - Path foto wajah disimpan di SQLite pada kolom FaceImagePath.
        - Format nama foto wajah adalah username_yyyyMMdd_HHmmss.jpg.
        - Foto wajah tidak langsung disimpan sebagai gambar di SQLite, hanya path-nya yang disimpan agar database tetap ringan.
        - Jika wajah tidak terdeteksi, pastikan pencahayaan cukup, wajah menghadap kamera, dan hanya ada satu wajah di frame.

        DATABASE
        - Database menggunakan SQLite.
        - File database bernama face_recognition_floodrisk.db3.
        - Database tersimpan di FileSystem.AppDataDirectory.
        - Data user tersimpan dalam tabel User.
        - Kolom utama: Id, Username, Password, Role, FaceEmbeddingData, FaceImagePath, FullName, Age, PhoneNumber, Address, MonitoringArea, OperatorId, Institution, DutyArea.
        - Folder foto wajah berada di FileSystem.AppDataDirectory/FaceImages.
        - Database dapat dibuka melalui DB Browser for SQLite dengan membuka file face_recognition_floodrisk.db3.
        - Jika data belum muncul di DB Browser, pastikan register sudah berhasil dan aplikasi sudah ditutup atau database di-refresh.

        DASHBOARD
        - Dashboard menampilkan risiko banjir berdasarkan ketinggian air dan curah hujan.
        - Masyarakat hanya melihat informasi dan rekomendasi.
        - Operator dapat mengubah input data sensor simulasi.
        - Fuzzy Logic memberi kategori risiko seperti Aman, Rendah, Sedang, atau Tinggi.
        - Regression memberi skor pembanding.
        - K-Means mengelompokkan kondisi ke cluster Aman, Waspada, atau Bahaya.

        BATASAN ASISTEN
        - Jawaban harus fokus pada aplikasi SIAGA.
        - Pertanyaan di luar aplikasi SIAGA harus ditolak dengan sopan.
        - Jawaban harus singkat, jelas, dan tidak melebar.
        - Jangan membahas topik lain seperti hiburan, resep, politik, atau hal umum yang tidak berkaitan dengan SIAGA.
        """;
    }
}
