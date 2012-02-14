using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;

namespace Teleopti.Ccc.Sdk.WcfService.QueryHandler
{
    public interface IInvokeQuery<TResult>
    {
        TResult Invoke(QueryDto query);
    }
}