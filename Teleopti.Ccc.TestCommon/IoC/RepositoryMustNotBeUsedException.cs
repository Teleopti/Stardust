using System;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public class RepositoryMustNotBeUsedException : Exception
	{
		public RepositoryMustNotBeUsedException(Type repositoryType) : base($"{repositoryType} must not be used in this context")
		{
		}
	}
}