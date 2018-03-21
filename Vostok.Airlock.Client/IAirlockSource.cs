using System.IO;
using Vstk.Commons.Binary;

namespace Vstk.Airlock
{
    public interface IAirlockSource
    {
        Stream ReadStream { get; }

        IBinaryReader Reader { get; }
    }
}