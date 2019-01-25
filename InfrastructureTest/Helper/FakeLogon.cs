using System;
using System.Threading;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.InfrastructureTest.Helper
{
	public class FakeLogon : IDisposable
	{
		private readonly ITeleoptiPrincipal _oldPrincipal;

		public static IDisposable ToBusinessUnit(IBusinessUnit newBusinessUnit, Repository<IPerson> personRepository)
		{
			return new FakeLogon(new TeleoptiPrincipalForLegacyFactory(), personRepository, newBusinessUnit);
		}

		private FakeLogon(IPrincipalFactory factory, Repository<IPerson> persons, IBusinessUnit businessUnit)
		{
			_oldPrincipal = (ITeleoptiPrincipal)Thread.CurrentPrincipal;
			var person = persons.Get(_oldPrincipal.PersonId);
			var newPrincipal = factory.MakePrincipal(person, ((ITeleoptiIdentity)_oldPrincipal.Identity).DataSource, businessUnit, null);
			Thread.CurrentPrincipal = newPrincipal;
		}

		public void Dispose()
		{
			Thread.CurrentPrincipal = _oldPrincipal;
		}
	}
}