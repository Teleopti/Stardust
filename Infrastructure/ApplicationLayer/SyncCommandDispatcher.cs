using Teleopti.Ccc.Domain.ApplicationLayer;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class SyncCommandDispatcher : ICommandDispatcher
	{
		private readonly IResolve _resolver;

		public SyncCommandDispatcher(IResolve resolver)
		{
			_resolver = resolver;
		}

		public void Execute(object command)
		{
			var handlerType = typeof(IHandleCommand<>).MakeGenericType(new[] { command.GetType() });
			var handler = _resolver.Resolve(handlerType);
			var method = handler.GetType().GetMethod("Handle", new [] { command.GetType() } );
			method.Invoke(handler, new[] { command });
		}
	}
}