using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure
{
	// these properties are injected by reflection in ServiceLocatorModule
	// always prefer dependency injection over service locator!

	public static class ServiceLocatorForLegacy
	{
		private static INestedUnitOfWorkStrategy _nestedUnitOfWorkStrategy;
		private static IUpdatedBy _updatedBy;
		private static IChangeNHibernateConfiguration _changeNHibernateConfiguration;

		public static INestedUnitOfWorkStrategy NestedUnitOfWorkStrategy
		{
			get { return _nestedUnitOfWorkStrategy ?? new ThrowOnNestedUnitOfWork(); }
			set { _nestedUnitOfWorkStrategy = value; }
		}

		public static IUpdatedBy UpdatedBy
		{
			get { return _updatedBy ?? Domain.Security.Principal.UpdatedBy.Make(); }
			set { _updatedBy = value; }
		}

		[RemoveMeWithToggle(Toggles.ResourcePlanner_LessPersonAssignmentUpdates_42159)]
		public static IChangeNHibernateConfiguration ChangeNHibernateConfiguration
		{
			get { return _changeNHibernateConfiguration ?? new NoNHibernateConfigurationChange(); }
			set { _changeNHibernateConfiguration = value; }
		}
	}
}
