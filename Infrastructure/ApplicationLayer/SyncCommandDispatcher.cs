﻿using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class SyncCommandDispatcher : ICommandDispatcher
	{
		private readonly IComponentContext _componentContext;

		public SyncCommandDispatcher(IComponentContext componentContext) { _componentContext = componentContext; }

		public void Execute(object command)
		{
			var handlerType = typeof(IHandleCommand<>).MakeGenericType(new[] { command.GetType() });
			var handler = _componentContext.Resolve(handlerType);
			var method = handler.GetType().GetMethod("Handle");
			method.Invoke(handler, new[] { command });
		}
	}
}