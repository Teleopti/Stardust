using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure
{
	// these properties are injected by reflection in ServiceLocatorModule
	// always prefer dependency injection over service locator!

	public static class ServiceLocatorForLegacy
	{
		private static IUpdatedBy _updatedBy;

		public static IUpdatedBy UpdatedBy
		{
			get { return _updatedBy ?? Domain.Security.Principal.UpdatedBy.Make(); }
			set { _updatedBy = value; }
		}
	}
}