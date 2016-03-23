using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class CurrentBusinessUnit : ICurrentBusinessUnit
	{
		private readonly ICurrentIdentity _identity;
		private readonly IBusinessUnitForRequest _businessUnitForRequest;

		public static ICurrentBusinessUnit Make()
		{
			var identity = new CurrentIdentity(new CurrentTeleoptiPrincipal(new ThreadPrincipalContext()));
			return new CurrentBusinessUnit(identity, new NoBusinessUnitForRequest());
		}

		public CurrentBusinessUnit(ICurrentIdentity identity, IBusinessUnitForRequest businessUnitForRequest)
		{
			_identity = identity;
			_businessUnitForRequest = businessUnitForRequest;
		}

		public IBusinessUnit Current()
		{
			var businessUnit = _businessUnitForRequest.TryGetBusinessUnit();
			if (businessUnit == null)
			{
				var identity = _identity.Current();
				return identity == null ? null : identity.BusinessUnit;
			}
			return businessUnit;
		}
	}
}