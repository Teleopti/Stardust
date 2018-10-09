﻿using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.ApplicationLayer;

namespace Teleopti.Ccc.Web.Areas.People.Core.Aspects
{
	public class AuditTrailAspect : IAspect
	{
		private readonly IResolve _resolve;

		public AuditTrailAspect(IResolve resolve)
		{
			_resolve = resolve;
		}

		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
			//using (var resolve = _resolve.NewScope())
			//{
				var handlerType = typeof(IHandleContext<>).MakeGenericType(invocation.Arguments[0].GetType());
				var handler = _resolve.Resolve(handlerType);
				var method = handler.GetType().GetMethod("Handle", new[] { invocation.Arguments[0].GetType() });
				method.Invoke(handler, new[] { invocation.Arguments[0] });
			//}
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
		}
	}
}