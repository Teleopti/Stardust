using System;
using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Aspects
{
	public class NoDataSourceFromAuthenticationKeyAspect : IDataSourceFromAuthenticationKeyAspect
	{
		public void OnBeforeInvocation(IInvocationInfo invocation)
		{
		}

		public void OnAfterInvocation(Exception exception, IInvocationInfo invocation)
		{
		}
	}
}