using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Autofac;
using Castle.DynamicProxy;
using log4net;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.Infrastructure.Aop
{
	public class AspectInterceptor : IInterceptor
	{
		private readonly IComponentContext _resolver;

		public AspectInterceptor(IComponentContext resolver)
		{
			_resolver = resolver;
		}

		[DebuggerStepThrough]
		public void Intercept(IInvocation invocation)
		{
			var attributes = invocation.Method.GetCustomAttributes<AspectAttribute>(false);

			if (!attributes.Any())
			{
				invocation.Proceed();
				return;
			}

			var aspects = attributes
					.OrderBy(x => x.Order)
					.Select(attribute => (IAspect) _resolver.Resolve(attribute.AspectType))
					.ToArray()
				;

			var invocationInfo = new InvocationInfo(invocation);
			var completedBefores = new List<IAspect>();
			try
			{
				aspects.ForEach(a =>
				{
					a.OnBeforeInvocation(invocationInfo);
					completedBefores.Add(a);
				});
			}
			catch (Exception e)
			{
				LogManager.GetLogger(invocation.TargetType)
					.Error($"Aspect call before {invocation.Method.Name} failed", e);
				runAfters(invocation, completedBefores, invocationInfo, e);
			}

			Exception exception = null;
			try
			{
				invocation.Proceed();
			}
			catch (Exception e)
			{
				exception = e;
				throw;
			}
			finally
			{
				runAfters(invocation, aspects, invocationInfo, exception);
			}
		}

		private static void runAfters(IInvocation invocation, IEnumerable<IAspect> aspects, IInvocationInfo invocationInfo, Exception exception)
		{
			var exceptions = new List<Exception>();
			if (exception != null)
				exceptions.Add(exception);

			aspects
				.Reverse()
				.ForEach(a =>
				{
					try
					{
						a.OnAfterInvocation(exception, invocationInfo);
					}
					catch (Exception e)
					{
						LogManager.GetLogger(invocation.TargetType)
							.Error($"Aspect call after {invocation.Method.Name} failed", e);
						exceptions.Add(e);
					}
				});

			if (exceptions.Count == 1)
				ExceptionDispatchInfo.Capture(exceptions.First()).Throw();
			if (exceptions.Any())
				throw new AggregateException(exceptions);
		}
	}
}