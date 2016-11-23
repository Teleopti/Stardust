using System;
using System.Linq;
using System.Reflection;
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
			try
			{
				aspects.ForEach(a => a.OnBeforeInvocation(invocationInfo));
			}
			catch (Exception e)
			{
				LogManager.GetLogger(invocation.TargetType)
					.Error($"Aspect call before {invocation.Method.Name} failed", e);
				throw;
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

				try
				{
					aspects
						.Reverse()
						.ForEach(a => a.OnAfterInvocation(exception, invocationInfo))
						;
				}
				catch (Exception e)
				{
					LogManager.GetLogger(invocation.TargetType)
					.Error($"Aspect call after {invocation.Method.Name} failed", e);
					throw;
				}

			}
		}	
	}
}