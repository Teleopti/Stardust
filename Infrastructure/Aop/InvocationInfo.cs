using System;
using System.Collections.Generic;
using System.Reflection;
using Castle.DynamicProxy;
using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Infrastructure.Aop
{
	public class InvocationInfo : IInvocationInfo
	{
		private readonly IInvocation _invocation;

		public InvocationInfo(IInvocation invocation)
		{
			_invocation = invocation;
		}

		public object[] Arguments => _invocation.Arguments;
		public MethodInfo Method => _invocation.Method;
		public object ReturnValue => _invocation.ReturnValue;
		public Type TargetType => _invocation.TargetType;

		public object Proxy => _invocation.Proxy;


		public List<Exception> Exceptions = new List<Exception>();
		public void Proceed() => _invocation.Proceed();
	}
}