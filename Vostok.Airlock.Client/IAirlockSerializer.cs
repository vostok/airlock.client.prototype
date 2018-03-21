namespace Vstk.Airlock
{
    public interface IAirlockSerializer<in T>
    {
        void Serialize(T item, IAirlockSink sink);
    }
}
