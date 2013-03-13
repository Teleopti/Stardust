using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class SyncCommandDispatcher : ICommandDispatcher
	{
		private readonly IContainer _container;

		public SyncCommandDispatcher(IContainer container) { _container = container; }

		public void Invoke(object command)
		{
			var handlerType = typeof(IHandleCommand<>).MakeGenericType(new[] { command.GetType() });
			var handler = _container.Resolve(handlerType);
			var method = handler.GetType().GetMethod("Handle");
			method.Invoke(handler, new[] { command });
		}
	}
}