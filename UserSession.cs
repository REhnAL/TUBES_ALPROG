using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Face_Recognition;

public static class UserSession
{
    public static User? CurrentUser { get; set; }

    // List ini tetap dipertahankan agar kode lama tidak error.
    // Penyimpanan utama sekarang menggunakan SQLite melalui DatabaseService.
    public static List<User> Users { get; } = new();
}
