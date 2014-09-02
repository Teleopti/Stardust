using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class CurrentUnitOfWorkFactory : ICurrentUnitOfWorkFactory
	{
		private const string youAreNotLoggedOn ="You are not logged on.";
		private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;

		public CurrentUnitOfWorkFactory(ICurrentTeleoptiPrincipal currentTeleoptiPrincipal)
		{
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
		}

		public IUnitOfWorkFactory LoggedOnUnitOfWorkFactory()
		{
			var principal = _currentTeleoptiPrincipal.Current();
			if (principal == null)
			{
				throw new PermissionException(youAreNotLoggedOn);
			}
			var identity = ((ITeleoptiIdentity)_currentTeleoptiPrincipal.Current().Identity);
			if (!identity.IsAuthenticated)
			{
				throw new PermissionException(youAreNotLoggedOn);
			}
			return identity.DataSource.Application;
		}
	}
}