namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
    public interface IHandleQuery<TQuery, TResult>
    {
       TResult Handle(TQuery query);
    }
}
