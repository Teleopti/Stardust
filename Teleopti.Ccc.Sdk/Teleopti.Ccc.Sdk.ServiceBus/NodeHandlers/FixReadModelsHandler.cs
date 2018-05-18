﻿using System;
using System.Collections.Generic;
using System.Threading;
using Autofac;
using Stardust.Node.Interfaces;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ReadModelValidator;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Sdk.ServiceBus.NodeHandlers
{
	public class FixReadModelsHandler : IHandle<FixReadModelsEvent>
	{
		private readonly IComponentContext _componentContext;

		public FixReadModelsHandler(IComponentContext componentContext)
		{
			_componentContext = componentContext;
		}

		[AsSystem]
		public virtual void Handle(FixReadModelsEvent parameters,
			CancellationTokenSource cancellationTokenSource,
			Action<string> sendProgress,
			ref IEnumerable<object> returnObjects)
		{
			var theRealOne = _componentContext.Resolve<IHandleEvent<FixReadModelsEvent>>();
			theRealOne.Handle(parameters);
		}
	}
}
