using System;
using System.Threading;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.InfrastructureTest.Helper
{
	public class FakeLogon : IDisposable
	{
		private readonly TeleoptiPrincipal _oldPrincipal;

		public static IDisposable ToBusinessUnit(IBusinessUnit newBusinessUnit)
		{
			return new FakeLogon(new TeleoptiPrincipalFactory(), newBusinessUnit);
		}

		private FakeLogon(IPrincipalFactory factory, IBusinessUnit businessUnit)
		{
			_oldPrincipal = (TeleoptiPrincipal)Thread.CurrentPrincipal;
			var newPrincipal = factory.MakePrincipal(((IUnsafePerson)_oldPrincipal).Person, ((TeleoptiIdentity)_oldPrincipal.Identity).DataSource, businessUnit, null);
			Thread.CurrentPrincipal = newPrincipal;
		}

		public void Dispose()
		{
			Thread.CurrentPrincipal = _oldPrincipal;
		}
	}
}