using System;
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
			var orderedAspects = from a in invocation.Method.GetCustomAttributes(false)
			                     let orderedAspect = a as IOrderedAspect
			                     where orderedAspect != null
			                     orderby orderedAspect.Order
			                     select orderedAspect;

			var aspects = from a in orderedAspects
			              let aspect = a as IAspect
			              let resolvedAspect = a as IResolvedAspect
			              let theResolvedAspect = resolvedAspect == null ? null : (IAspect) _resolver.Resolve(resolvedAspect.AspectType)
			              let useAspect = aspect ?? theResolvedAspect
			              select useAspect;

			var array = aspects.ToArray();

			if (array.Any())
			{
				array.ForEach(a => a.OnBeforeInvocation(new InvocationInfo(invocation)));

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
					array.Reverse().ForEach(a => a.OnAfterInvocation(exception));
				}
			}
			else
			{
				invocation.Proceed();
			}
		}	
	}
}