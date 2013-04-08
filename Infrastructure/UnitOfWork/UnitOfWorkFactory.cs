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
				if (TeleoptiPrincipal.Current == null)
				{
					return null;
				}
				var identity = ((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity);
				return identity.DataSource.Application;
			}
		}

		public static IUnitOfWorkFactoryProvider LoggedOnProvider()
		{
			return new UnitOfWorkFactoryProvider(new CurrentTeleoptiPrincipal());
		}

		public static ICurrentUnitOfWork CurrentUnitOfWork()
		{
			return new CurrentUnitOfWork(LoggedOnProvider());
		}
	}
}