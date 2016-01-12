using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Castle.DynamicProxy;
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
			var orderedAspectAttributes = invocation.Method.GetCustomAttributes(typeof (IAttributeForAspect), false)
				.Cast<IAttributeForAspect>()
				.OrderBy(x => x.Order);
			var aspects = orderedAspectAttributes.Select(attribute => (IAspect) _resolver.Resolve(attribute.AspectType)).ToList();

			if (aspects.Any())
			{
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