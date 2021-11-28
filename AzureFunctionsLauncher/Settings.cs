namespace Oppanda.AzureFunctions
{
    internal class Settings
    {
        public const string SettingsFileParamName = "SettingsFileUrl";
        public static string SettingsFileUrl => System.Environment.GetEnvironmentVariable(SettingsFileParamName);
    }
}