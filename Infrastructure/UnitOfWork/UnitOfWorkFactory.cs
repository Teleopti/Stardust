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
				if (TeleoptiPrincipalForLegacy.CurrentPrincipal == null)
				{
					return null;
				}
				var identity = (ITeleoptiIdentity)TeleoptiPrincipalForLegacy.CurrentPrincipal.Identity;
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