using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
    /// <summary>
    /// Static class for UnitOfWork
    /// </summary>
    public static class UnitOfWorkFactory
    {
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
    }
}