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
	public class AspectInterceptor : IInterceptor
	{
		private readonly Lazy<IEnumerable<IAspect>> _aspects;
		private readonly IComponentContext _resolver;

		public AspectInterceptor(Lazy<IEnumerable<IAspect>> aspects, IComponentContext resolver)
		{
			_aspects = aspects;
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
					.Select(a => a.AspectType)
					.Select(t =>
					{
						var aspect = _aspects.Value.SingleOrDefault(t.IsInstanceOfType);
						if (aspect != null)
							return aspect;
						return (IAspect) _resolver.Resolve(t);
					})
					.ToArray()
				;
			if (!aspects.Any())
			{
				invocation.Proceed();
				return;
			}

			var invocationInfo = new InvocationInfo(invocation);

			runBefores(invocationInfo, aspects);

			throwAny(invocationInfo);

			invoke(invocationInfo);

			runAfters(invocationInfo, aspects);

			throwAny(invocationInfo);
		}

		private void runBefores(InvocationInfo invocation, IEnumerable<IAspect> aspects)
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
				invocation.Exceptions.Add(e);
				runAfters(invocation, completedBefores);
			}
		}

		private static void invoke(InvocationInfo invocation)
		{
			try
			{
				invocation.Proceed();
			}
			catch (Exception e)
			{
				invocation.Exceptions.Add(e);
			}
		}

		private void runAfters(InvocationInfo invocation, IEnumerable<IAspect> aspects)
		{
			aspects
				.Reverse()
				.ForEach(a =>
				{
					try
					{
						a.OnAfterInvocation(invocation.Exceptions.FirstOrDefault(), invocation);
					}
					catch (Exception e)
					{
						invocation.Exceptions.Add(e);
					}
				});
		}

		private static void throwAny(InvocationInfo invocation)
		{
			if (invocation.Exceptions.Count == 1)
				ExceptionDispatchInfo.Capture(invocation.Exceptions.First()).Throw();
			if (invocation.Exceptions.Any())
				throw new AggregateException(invocation.Exceptions);
		}
	}
}