using System;

namespace Teleopti.Ccc.TestCommon.IoC
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
	public class DefaultDataAttribute : Attribute
	{
	}
	
	[AttributeUsage(AttributeTargets.Class)]
	public class NoDefaultDataAttribute : Attribute{}
}