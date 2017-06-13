using System;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public class RepositoriesMustNotBeUsedException : Exception
	{
		public RepositoriesMustNotBeUsedException() : base("Repositories must not be used in this context")
		{
			
		}
	}
}