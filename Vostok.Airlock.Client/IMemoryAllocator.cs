namespace Vostok.Airlock
{
    internal interface IMemoryAllocator
    {
        bool TryReserveBytes(int amount);
    }
}