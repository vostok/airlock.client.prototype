using System;
using System.Threading.Tasks;

namespace Vstk.Airlock
{
    public interface IRequestSender
    {
        Task<RequestSendResult> SendAsync(ArraySegment<byte> serializedMessage);
    }
}