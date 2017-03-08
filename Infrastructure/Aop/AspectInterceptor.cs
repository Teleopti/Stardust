using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Autofac;
using Castle.DynamicProxy;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.Infrastructure.Aop
{
	public class AspectInvocationException : Exception
	{
		public AspectInvocationException(string message, Exception inner) : base(message, inner)
		{
		}
	}

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
			if (!aspects.Any())
			{
				invocation.Proceed();
				return;
			}

			var invocationInfo = new InvocationInfo(invocation);
			var exceptions = new List<Exception>();

			runBefores(invocationInfo, aspects, exceptions);

			throwAny(exceptions);

			invoke(invocationInfo, exceptions);

			runAfters(invocationInfo, aspects, exceptions);

			throwAny(exceptions);
		}

		private void runBefores(InvocationInfo invocation, IEnumerable<IAspect> aspects, List<Exception> exceptions)
		{
			var completedBefores = new List<IAspect>();

			try
			{
				aspects.ForEach(a =>
				{
					a.OnBeforeInvocation(invocation);
					completedBefores.Add(a);
				});
			}
			catch (Exception e)
			{
				exceptions.Add(new AspectInvocationException($"Aspect call before {invocation.TargetType.FullName}.{invocation.Method.Name} failed", e));
				runAfters(invocation, completedBefores, exceptions);
			}
		}

		private void invoke(InvocationInfo invocation, ICollection<Exception> exceptions)
		{
			try
			{
				invocation.Proceed();
			}
			catch (Exception e)
			{
				exceptions.Add(e);
			}
		}

		private void runAfters(InvocationInfo invocation, IEnumerable<IAspect> aspects, ICollection<Exception> exceptions)
		{
			aspects
				.Reverse()
				.ForEach(a =>
				{
					try
					{
						a.OnAfterInvocation(exceptions.FirstOrDefault(), invocation);
					}
					catch (Exception e)
					{
						exceptions.Add(new AspectInvocationException($"Aspect call after {invocation.TargetType.FullName}.{invocation.Method.Name} failed", e));
					}
				});
		}

		private static void throwAny(IReadOnlyCollection<Exception> exceptions)
		{
			if (exceptions.Count == 1)
				ExceptionDispatchInfo.Capture(exceptions.First()).Throw();
			if (exceptions.Any())
				throw new AggregateException(exceptions);
		}

	}
}