using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	//avoid using this one
	public static class UnitOfWorkFactory
	{
		public static IUnitOfWorkFactory Current
		{
			get
			{
				if (TeleoptiPrincipal.CurrentPrincipal == null)
				{
					return null;
				}
				var identity = ((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity);
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