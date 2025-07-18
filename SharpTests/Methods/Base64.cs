namespace SharpTests.Methods;

public static partial class Global
{
    public static string Base64(string username, string password)
    {
        var credentials = $"{username}:{password}";
        var base64Credentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(credentials));
        return base64Credentials;
    }
}