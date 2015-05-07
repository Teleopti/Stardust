using System;
using System.Linq;
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
			var aspects = (
				from IAttributeForAspect attribute in invocation.Method.GetCustomAttributes(typeof (IAttributeForAspect), false) 
				select (IAspect) _resolver.Resolve(attribute.AspectType)
				)
				.ToList();


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