using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Logon
{
	public static class IPrincipalFactoryExtensions
	{
		public static ITeleoptiPrincipal MakePrincipal(this IPrincipalFactory instance, IPerson person, IDataSource dataSource, IBusinessUnit businessUnit, string tokenIdentity)
		{
			return instance.MakePrincipal(new PersonAndBusinessUnit(person, businessUnit), dataSource, tokenIdentity);
		}
	}
}