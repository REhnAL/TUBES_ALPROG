using SQLite;
using System.Globalization;
using System.Linq;

namespace Face_Recognition;

public class User
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public string Username { get; set; } = "";

    public string Password { get; set; } = "";
    public string Role { get; set; } = "";

    public string FaceEmbeddingData { get; set; } = "";

    public string FaceImagePath { get; set; } = "";

    [Ignore]
    public float[] FaceEmbedding
    {
        get
        {
            return EmbeddingConverter.StringToFloatArray(FaceEmbeddingData);
        }
        set
        {
            FaceEmbeddingData = EmbeddingConverter.FloatArrayToString(value);
        }
    }

    public string FullName { get; set; } = "";
    public string Age { get; set; } = "";
    public string PhoneNumber { get; set; } = "";

    public string Address { get; set; } = "";
    public string MonitoringArea { get; set; } = "";

    public string OperatorId { get; set; } = "";
    public string Institution { get; set; } = "";
    public string DutyArea { get; set; } = "";

    public User()
    {
    }

    public User(string username, string password, string role, float[] faceEmbedding)
    {
        Username = username;
        Password = password;
        Role = role;
        FaceEmbedding = faceEmbedding;
        FaceImagePath = "";

        FullName = "";
        Age = "";
        PhoneNumber = "";
        Address = "";
        MonitoringArea = "";
        OperatorId = "";
        Institution = "";
        DutyArea = "";
    }

    public User(
        string username,
        string password,
        string role,
        float[] faceEmbedding,
        string fullName,
        string age,
        string phoneNumber,
        string address,
        string monitoringArea,
        string operatorId,
        string institution,
        string dutyArea)
    {
        Username = username;
        Password = password;
        Role = role;
        FaceEmbedding = faceEmbedding;
        FaceImagePath = "";

        FullName = fullName;
        Age = age;
        PhoneNumber = phoneNumber;
        Address = address;
        MonitoringArea = monitoringArea;

        OperatorId = operatorId;
        Institution = institution;
        DutyArea = dutyArea;
    }

    public User(
        string username,
        string password,
        string role,
        float[] faceEmbedding,
        string faceImagePath,
        string fullName,
        string age,
        string phoneNumber,
        string address,
        string monitoringArea,
        string operatorId,
        string institution,
        string dutyArea)
    {
        Username = username;
        Password = password;
        Role = role;
        FaceEmbedding = faceEmbedding;
        FaceImagePath = faceImagePath;

        FullName = fullName;
        Age = age;
        PhoneNumber = phoneNumber;
        Address = address;
        MonitoringArea = monitoringArea;

        OperatorId = operatorId;
        Institution = institution;
        DutyArea = dutyArea;
    }
}

public static class EmbeddingConverter
{
    public static string FloatArrayToString(float[]? embedding)
    {
        if (embedding == null || embedding.Length == 0)
        {
            return "";
        }

        return string.Join(
            ";",
            embedding.Select(value => value.ToString("R", CultureInfo.InvariantCulture)));
    }

    public static float[] StringToFloatArray(string? embeddingData)
    {
        if (string.IsNullOrWhiteSpace(embeddingData))
        {
            return Array.Empty<float>();
        }

        return embeddingData
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(value => float.Parse(value, CultureInfo.InvariantCulture))
            .ToArray();
    }
}
