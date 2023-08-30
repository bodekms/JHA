namespace MyTracker.Interfaces
{
    public interface IConfigurationService
    {
        T GetSetting<T>(string key, T defaultValue);
    }
}
