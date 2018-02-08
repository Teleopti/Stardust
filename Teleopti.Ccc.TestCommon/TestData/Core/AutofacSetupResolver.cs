using System;
using Autofac;
using Teleopti.Ccc.TestCommon.TestData.Setups.Specific;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public interface ISetupResolver
	{
		IUserDataSetup<T> ResolveUserDataSetupFor<T>();
		IUserSetup<T> ResolveUserSetupFor<T>();
	}

	public class LegacySetupResolver : ISetupResolver
	{
		public IUserDataSetup<T> ResolveUserDataSetupFor<T>()
		{
			return null;
		}

		public IUserSetup<T> ResolveUserSetupFor<T>()
		{
			if (typeof(T) == typeof(SwedishCultureSpec))
				return new SwedishCultureSetup() as IUserSetup<T>;
			if (typeof(T) == typeof(UtcTimeZoneSpec))
				return new UtcTimeZoneSetup() as IUserSetup<T>;
			return null;
		}
	}

	public class AutofacSetupResolver : ISetupResolver
	{
		private readonly IComponentContext _container;

		public AutofacSetupResolver(IComponentContext container)
		{
			_container = container;
		}

		public IUserDataSetup<T> ResolveUserDataSetupFor<T>()
		{
			var specType = typeof(T);
			var setupType = typeof(IUserDataSetup<>).MakeGenericType(specType);
			return _container.ResolveOptional(setupType) as IUserDataSetup<T>;
		}

		public IUserSetup<T> ResolveUserSetupFor<T>()
		{
			var specType = typeof(T);
			var setupType = typeof(IUserSetup<>).MakeGenericType(specType);
			return _container.ResolveOptional(setupType) as IUserSetup<T>;
		}
	}
}