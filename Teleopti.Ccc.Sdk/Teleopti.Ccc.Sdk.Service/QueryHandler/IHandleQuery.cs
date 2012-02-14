namespace Teleopti.Ccc.Sdk.WcfService.QueryHandler
{
    public interface IHandleQuery<TQuery, TResult>
    {
       TResult Handle(TQuery query);
    }
}
