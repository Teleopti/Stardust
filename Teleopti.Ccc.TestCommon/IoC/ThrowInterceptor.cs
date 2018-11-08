using System;
using LinFu.DynamicProxy;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public class ThrowInterceptor : IInterceptor
	{
		private readonly Type _type;

		public ThrowInterceptor(Type type)
		{
			_type = type;
		}
		
		public object Intercept(InvocationInfo info)
		{
			throw new MustNotBeUsedException(_type);
		}
	}
}