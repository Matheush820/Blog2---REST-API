namespace Blog2;

public static class Configuration
{
    //Token = JWT = JSON WEB TOKEN
    public static string JwtKey = "EIWUfsfLgFJpNmmo3Iz7ije0wof5wejd81p";
    public static string ApiKeyName = "api_key";
    public static string ApiKey = "curso_api_IlTevUM/z0ey3NwCV/unWG='";
    public static SmtConfiguration Smtp = new();

    public class SmtConfiguration
    {
        public string Host { get; set; }
        public int Port { get; set; } = 25;
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
