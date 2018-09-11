using System;

namespace Teleopti.Ccc.Domain
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class InstancePerLifetimeScopeAttribute : Attribute
	{
	}
}