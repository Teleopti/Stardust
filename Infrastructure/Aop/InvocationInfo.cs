using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
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



		public void AddException(Exception exception, string aspectMessage)
		{
			Exceptions.Add(exception);

			//if (aspectMessage == null)
			//{
			//	Exceptions.Add(exception);
			//	return;
			//}
			//if (exception is HttpException)
			//{
			//	Exceptions.Add(exception);
			//	return;
			//}

			//Exceptions.Add(new AspectInvocationException(aspectMessage, exception));
		}

		public List<Exception> Exceptions = new List<Exception>();
		public void Proceed() => _invocation.Proceed();

	}
}