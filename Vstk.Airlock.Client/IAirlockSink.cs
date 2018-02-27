using System.IO;
using Vstk.Commons.Binary;

namespace Vstk.Airlock
{
    public interface IAirlockSink
    {
        Stream WriteStream { get; }

        IBinaryWriter Writer { get; }
    }
}
