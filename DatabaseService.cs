using SQLite;

namespace Face_Recognition;

public static class DatabaseService
{
    private static SQLiteAsyncConnection? database;

    private static async Task InitAsync()
    {
        if (database != null)
        {
            return;
        }

        string databasePath = Path.Combine(
            FileSystem.AppDataDirectory,
            "face_recognition_floodrisk.db3");

        database = new SQLiteAsyncConnection(databasePath);

        await database.CreateTableAsync<User>();
        await AddMissingColumnsAsync();
    }

    private static async Task AddMissingColumnsAsync()
    {
        if (database == null)
        {
            return;
        }

        try
        {
            await database.ExecuteAsync("ALTER TABLE User ADD COLUMN FaceImagePath TEXT");
        }
        catch
        {
            // Kolom sudah ada, jadi tidak perlu melakukan apa-apa.
        }
    }

    public static async Task<string> GetDatabasePathAsync()
    {
        await InitAsync();

        return Path.Combine(
            FileSystem.AppDataDirectory,
            "face_recognition_floodrisk.db3");
    }

    public static async Task<int> SaveUserAsync(User user)
    {
        await InitAsync();

        if (database == null)
        {
            return 0;
        }

        return await database.InsertAsync(user);
    }

    public static async Task<User?> GetUserByUsernameAsync(string username)
    {
        await InitAsync();

        if (database == null)
        {
            return null;
        }

        return await database
            .Table<User>()
            .Where(user => user.Username == username)
            .FirstOrDefaultAsync();
    }

    public static async Task<User?> GetUserByLoginAsync(
        string username,
        string password,
        string role)
    {
        await InitAsync();

        if (database == null)
        {
            return null;
        }

        return await database
            .Table<User>()
            .Where(user =>
                user.Username == username &&
                user.Password == password &&
                user.Role == role)
            .FirstOrDefaultAsync();
    }

    public static async Task<List<User>> GetAllUsersAsync()
    {
        await InitAsync();

        if (database == null)
        {
            return new List<User>();
        }

        return await database.Table<User>().ToListAsync();
    }

    public static async Task<int> DeleteAllUsersAsync()
    {
        await InitAsync();

        if (database == null)
        {
            return 0;
        }

        return await database.DeleteAllAsync<User>();
    }
}
