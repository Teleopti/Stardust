using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	//avoid using this one
	public static class UnitOfWorkFactory
	{
		public static IUnitOfWorkFactory Current
		{
			get
			{
				if (TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal == null)
				{
					return null;
				}
				var identity = (ITeleoptiIdentity)TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Identity;
				return identity.DataSource.Application;
			}
		}

		public static ICurrentUnitOfWorkFactory CurrentUnitOfWorkFactory()
		{
			return UnitOfWork.CurrentUnitOfWorkFactory.Make();
		}

		public static ICurrentUnitOfWork CurrentUnitOfWork()
		{
			return UnitOfWork.CurrentUnitOfWork.Make();
		}
	}
}