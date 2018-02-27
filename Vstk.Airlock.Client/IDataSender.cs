using System.Threading.Tasks;

namespace Vstk.Airlock
{
    internal interface IDataSender
    {
        Task<DataSendResult> SendAsync();
    }
}