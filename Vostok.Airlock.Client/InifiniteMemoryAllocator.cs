namespace Vostok.Airlock
{
    internal class InifiniteMemoryAllocator : IMemoryAllocator
    {
        public bool TryReserveBytes(int amount)
        {
            return true;
        }
    }
}