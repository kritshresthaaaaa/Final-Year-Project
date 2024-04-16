namespace FypWeb.IService
{
    public interface ITagReaderService
    {
        Task RunContinuousRead(long inventoryTime, CancellationToken cancellationToken);
    }
}
