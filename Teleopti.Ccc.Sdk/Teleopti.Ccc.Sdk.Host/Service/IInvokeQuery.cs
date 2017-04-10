using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;

namespace Teleopti.Ccc.Sdk.WcfHost.Service
{
    public interface IInvokeQuery<TResult>
    {
        TResult Invoke(QueryDto query);
    }
}