using System;
using LinFu.DynamicProxy;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public class ThrowRepositoryInterceptor : IInterceptor
	{
		private readonly Type _repositoryType;

		public ThrowRepositoryInterceptor(Type repositoryType)
		{
			_repositoryType = repositoryType;
		}
		
		public object Intercept(InvocationInfo info)
		{
			throw new RepositoryMustNotBeUsedException(_repositoryType);
		}
	}
}