using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	[IsNotDeadCode("Query handlers for the SDK are resolved from container and registered using reflection.")]
    public interface IHandleQuery<TQuery, TResult>
    {
       TResult Handle(TQuery query);
    }
}
