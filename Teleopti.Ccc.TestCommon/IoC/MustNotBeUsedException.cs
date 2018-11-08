using System;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public class MustNotBeUsedException : Exception
	{
		public MustNotBeUsedException(Type type) : base($"{type} must not be used in this context")
		{
		}
	}
}