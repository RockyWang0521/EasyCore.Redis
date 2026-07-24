namespace Web.EasyCore.Cache.Services.Combo;

public interface IComboStackService
{
    Task<string> GetWithRetryAsync();

    Task<string> GetCachedAsync(string key);

    Task<string> GetStackedAsync(string key);
}
