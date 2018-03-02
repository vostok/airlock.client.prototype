namespace Vostok.Airlock
{
    internal interface IMemoryManager : IMemoryAllocator
    {
        bool TryCreateBuffer(out byte[] buffer);
    }
}