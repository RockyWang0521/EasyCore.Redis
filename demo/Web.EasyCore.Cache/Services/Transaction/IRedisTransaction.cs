namespace Web.EasyCore.Cache.Services.Transaction
{
    public interface IRedisTransaction
    {
        Task Transaction();
    }
}
