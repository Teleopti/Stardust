using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure
{
	// these properties are injected by reflection in ServiceLocatorModule
	// always prefer dependency injection over service locator!

	public static class ServiceLocatorForLegacy
	{
		private static INestedUnitOfWorkStrategy _nestedUnitOfWorkStrategy;

		public static INestedUnitOfWorkStrategy NestedUnitOfWorkStrategy
		{
			get { return _nestedUnitOfWorkStrategy ?? new ThrowOnNestedUnitOfWork(); }
			set { _nestedUnitOfWorkStrategy = value; }
		}
		
	}
	
}
