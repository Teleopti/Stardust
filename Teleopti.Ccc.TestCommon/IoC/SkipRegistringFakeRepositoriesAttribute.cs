using System;

namespace Teleopti.Ccc.TestCommon.IoC
{
	[AttributeUsage(AttributeTargets.Class)]
	public class SkipRegistringFakeRepositoriesAttribute : Attribute
	{
	}
}