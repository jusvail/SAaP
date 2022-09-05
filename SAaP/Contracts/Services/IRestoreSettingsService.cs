namespace SAaP.Contracts.Services;

public interface IRestoreSettingsService
{
    Task<string> RestoreLastQueryStringFromDb();
}