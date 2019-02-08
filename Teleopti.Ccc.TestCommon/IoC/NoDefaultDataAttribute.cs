using System;

namespace Teleopti.Ccc.TestCommon.IoC
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class NoDefaultDataAttribute : Attribute{}
}