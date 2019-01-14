using System;
using System.Reflection;

namespace Teleopti.Ccc.Domain.Aop.Core
{
	public interface IInvocationInfo
	{
		object[] Arguments { get; }
		MethodInfo Method { get; }
		object ReturnValue { get; }
		Type TargetType { get; }
	}
}