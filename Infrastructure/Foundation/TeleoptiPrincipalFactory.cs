using System.Diagnostics;
using System.Security.Principal;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class TeleoptiPrincipalFactory : IPrincipalFactory
	{
		private readonly IMakeOrganisationMembershipFromPerson _makeOrganisationMembershipFromPerson;

		public TeleoptiPrincipalFactory(IMakeOrganisationMembershipFromPerson makeOrganisationMembershipFromPerson)
		{
			_makeOrganisationMembershipFromPerson = makeOrganisationMembershipFromPerson;
		}
		
		public static TeleoptiPrincipalFactory Make()
		{
			var internalsFactory = new TeleoptiPrincipalInternalsFactory();
			return new TeleoptiPrincipalFactory(internalsFactory);
		}
		
		public ITeleoptiPrincipal MakePrincipal(IPrincipalSource source, IDataSource dataSource, string tokenIdentity)
		{
			var identity = new TeleoptiIdentity(
				nameForPerson(source),
				dataSource, 
				source.PrincipalBusinessUnitId,
				source.PrincipalBusinessUnitIdName(),
				WindowsIdentity.GetCurrent(),
				tokenIdentity
				);
			var principal = new TeleoptiPrincipal(identity, source);
			principal.Organisation = _makeOrganisationMembershipFromPerson.MakeOrganisationMembership(source);
			return principal;
		}

		[DebuggerStepThrough]
		private static string nameForPerson(IPrincipalSource source)
		{
			try
			{
				return source?.PrincipalName() ?? string.Empty;
			}
			catch (NHibernate.ObjectNotFoundException exception)
			{
				throw new PersonNotFoundException("Person not found lazy loading the name", exception);
			}
		}
	}
}