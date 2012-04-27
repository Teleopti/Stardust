using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;

namespace Teleopti.Ccc.Sdk.WcfService
{
    public interface IInvokeQuery<TResult>
    {
        TResult Invoke(QueryDto query);
    }
}