using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.Infrastructure.Aop
{
	public class AspectInterceptor : IInterceptor
	{
		private readonly Lazy<IEnumerable<IAspect>> _aspects;

		public AspectInterceptor(Lazy<IEnumerable<IAspect>> aspects)
		{
			_aspects = aspects;
		}

		[DebuggerStepThrough]
		public void Intercept(IInvocation invocation)
		{
			var allAttributes = aspectAttributesOnMethod(invocation);

			if (!allAttributes.Any())
			{
				invocation.Proceed();
				return;
			}

			var aspects = allAttributes
					.OrderBy(x => x.Order)
					.Select(a => a.AspectType)
					.Select(t => _aspects.Value.Single(t.IsInstanceOfType))
					.ToArray();

			var invocationInfo = new InvocationInfo(invocation);

			runBefores(invocationInfo, aspects);

			throwAny(invocationInfo);

			invoke(invocationInfo);

			runAfters(invocationInfo, aspects);

			throwAny(invocationInfo);
		}

		private static IEnumerable<AspectAttribute> aspectAttributesOnMethod(IInvocation invocation)
		{
			var methodInfos = new List<MethodInfo>();
			var methodOnClass = invocation.Method;
			var paramsOnMethod = methodOnClass.GetParameters().Select(x => x.ParameterType).ToArray();
			var type = methodOnClass.DeclaringType;
			var interfaces = type.GetInterfaces();
			methodInfos.Add(methodOnClass);
			methodInfos.AddRange(interfaces.Select(@interface => @interface.GetMethod(methodOnClass.Name, paramsOnMethod))
				.Where(methodOnInterface => methodOnInterface != null));
			var allAttributes = methodInfos.SelectMany(x => x.GetCustomAttributes<AspectAttribute>(false));
			return allAttributes;
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