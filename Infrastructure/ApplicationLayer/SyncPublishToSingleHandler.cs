﻿using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class SyncPublishToSingleHandler : IEventPublisher
	{
		private readonly object _handler;

		public SyncPublishToSingleHandler(object handler)
		{
			_handler = handler;
		}

		public void Publish(IEvent @event)
		{
			var method = _handler
				.GetType()
				.GetMethods()
				.FirstOrDefault(m => m.Name == "Handle" && m.GetParameters().Single().ParameterType == @event.GetType());
			if (method == null)
				return;
			try
			{
				method.Invoke(_handler, new[] { @event });
			}
			catch (TargetInvocationException e)
			{
				PreserveStack.ForInnerOf(e);
				throw e;
			}
		}
		
	}
}