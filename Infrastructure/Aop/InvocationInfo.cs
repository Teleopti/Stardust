using System;
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

		public object GetArgumentValue(int index)
		{
			return _invocation.GetArgumentValue(index);
		}

		public MethodInfo GetConcreteMethod()
		{
			return _invocation.GetConcreteMethod();
		}

		public MethodInfo GetConcreteMethodInvocationTarget()
		{
			return _invocation.GetConcreteMethodInvocationTarget();
		}

		public void Proceed()
		{
			_invocation.Proceed();
		}

		public void SetArgumentValue(int index, object value)
		{
			_invocation.SetArgumentValue(index, value);
		}

		public object[] Arguments { get { return _invocation.Arguments; } }
		public Type[] GenericArguments { get { return _invocation.GenericArguments; } }
		public object InvocationTarget { get { return _invocation.InvocationTarget; } }
		public MethodInfo Method { get { return _invocation.Method; } }
		public MethodInfo MethodInvocationTarget { get { return _invocation.MethodInvocationTarget; } }
		public object Proxy { get { return _invocation.Proxy; } }
		public object ReturnValue
		{
			get { return _invocation.ReturnValue; }
			set { _invocation.ReturnValue = value; }
		}

		public Type TargetType { get { return _invocation.TargetType; } }
	}
}