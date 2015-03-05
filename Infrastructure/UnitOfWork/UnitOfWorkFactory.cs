using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	/// <summary>
	/// Static class for UnitOfWork
	/// </summary>
	public static class UnitOfWorkFactory
	{
		//avoid using this one
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
			return new CurrentUnitOfWorkFactory(CurrentDataSource.Make());
		}

		public static ICurrentUnitOfWork CurrentUnitOfWork()
		{
			return new CurrentUnitOfWork(CurrentUnitOfWorkFactory());
		}
	}
}