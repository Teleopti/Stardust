using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class CurrentBusinessUnit : ICurrentBusinessUnit
	{
		private readonly ICurrentIdentity _identity;

		public static ICurrentBusinessUnit Make()
		{
			var identity = new CurrentIdentity(new CurrentTeleoptiPrincipal());
			return new CurrentBusinessUnit(identity);
		}

		public CurrentBusinessUnit(ICurrentIdentity identity)
		{
			_identity = identity;
		}

		public IBusinessUnit Current()
		{
			var identity = _identity.Current();
			return identity == null ? null : identity.BusinessUnit;
		}
	}
}