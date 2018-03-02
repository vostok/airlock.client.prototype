namespace Vstk.Airlock
{
    internal interface IMemoryAllocator
    {
        bool TryReserveBytes(int amount);
    }
}