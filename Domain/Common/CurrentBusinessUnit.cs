using System.Threading;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class CurrentBusinessUnit : ICurrentBusinessUnit, IBusinessUnitScope
	{
		private readonly ICurrentIdentity _identity;
		private readonly IBusinessUnitForRequest _businessUnitForRequest;
		private readonly ThreadLocal<IBusinessUnit> _threadBusinessUnit = new ThreadLocal<IBusinessUnit>();

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
			if (_threadBusinessUnit.Value != null)
				return _threadBusinessUnit.Value;

			var businessUnit = _businessUnitForRequest.TryGetBusinessUnit();
			if (businessUnit != null)
				return businessUnit;

			var identity = _identity.Current();
			return identity?.BusinessUnit;
		}

		public void OnThisThreadUse(IBusinessUnit businessUnit)
		{
			_threadBusinessUnit.Value = businessUnit;
		}
	}
}