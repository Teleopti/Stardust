using Autofac;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public class SetupResolver
	{
		private readonly IComponentContext _container;

		public SetupResolver(IComponentContext container)
		{
			_container = container;
		}

		public IUserDataSetup<T> ResolveUserDataSetupFor<T>()
		{
			var specType = typeof(T);
			var setupType = typeof(IUserDataSetup<>).MakeGenericType(specType);
			return _container.Resolve(setupType) as IUserDataSetup<T>;
		}
	}
}