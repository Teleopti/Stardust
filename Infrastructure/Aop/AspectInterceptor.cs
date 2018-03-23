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
			var methodOnClass = invocation.Method;
			var paramsOnMethod = methodOnClass.GetParameters().Select(x => x.ParameterType).ToArray();
			var type = methodOnClass.DeclaringType;
			var attributesOnClass = methodOnClass.GetCustomAttributes<AspectAttribute>(false);
			var attributesOnClassTypes = attributesOnClass.Select(x => x.GetType());
			var attributesOnInterfaces = new List<AspectAttribute>();
			foreach (var @interface in type.GetInterfaces())
			{
				var methodOnInterface = @interface.GetMethod(methodOnClass.Name, paramsOnMethod);
				if (methodOnInterface != null)
				{
					var attributesOnInterface = methodOnInterface.GetCustomAttributes<AspectAttribute>(false);
					foreach (var attributeOnInterface in attributesOnInterface)
					{
						if (!attributesOnClassTypes.Contains(attributesOnInterface.GetType())) //not really correct but good enough for now
						{
							attributesOnInterfaces.Add(attributeOnInterface);
						}
					}
				}
			}
			
			return attributesOnClass.Union(attributesOnInterfaces);
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