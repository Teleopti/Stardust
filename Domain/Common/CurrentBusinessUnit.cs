using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class CurrentBusinessUnit : ICurrentBusinessUnit
	{
		private readonly ICurrentIdentity _identity;
		private readonly IBusinessUnitForRequest _businessUnitForRequest;

		public static ICurrentBusinessUnit Make()
		{
			var identity = new CurrentIdentity(new CurrentTeleoptiPrincipal());
			return new CurrentBusinessUnit(identity, new HttpRequestFalse());
		}

		public CurrentBusinessUnit(ICurrentIdentity identity, IBusinessUnitForRequest businessUnitForRequest)
		{
			_identity = identity;
			_businessUnitForRequest = businessUnitForRequest;
		}

		public IBusinessUnit Current()
		{
			var businessUnit = _businessUnitForRequest.BusinessUnitForRequest();
			if (businessUnit == null)
			{
				var identity = _identity.Current();
				return identity == null ? null : identity.BusinessUnit;
			}
			return businessUnit;
		}
	}
}