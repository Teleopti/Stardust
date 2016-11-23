using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Castle.DynamicProxy;
using Teleopti.Ccc.Domain.Aop.Core;

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
			var orderedAspectAttributes = invocation.Method.GetCustomAttributes<AspectAttribute>(false)
				.OrderBy(x => x.Order);

			if (orderedAspectAttributes.Any())
			{
				var aspects = orderedAspectAttributes.Select(attribute => (IAspect)_resolver.Resolve(attribute.AspectType)).ToList();
				var invocationInfo = new InvocationInfo(invocation);
				aspects.ForEach(a => a.OnBeforeInvocation(invocationInfo));

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
					aspects.Reverse();
					aspects.ForEach(a => a.OnAfterInvocation(exception, invocationInfo));
				}
			}
			else
			{
				invocation.Proceed();
			}
		}	
	}
}