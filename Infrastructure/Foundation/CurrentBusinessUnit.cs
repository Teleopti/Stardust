using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class CurrentBusinessUnit : ICurrentBusinessUnit
	{
		private readonly ICurrentIdentity _identity;
		private readonly ICurrentHttpContext _currentHttpContext;
		private readonly IBusinessUnitRepository _businessUnitRepository;

		public static ICurrentBusinessUnit Make()
		{
			var identity = new CurrentIdentity(new CurrentTeleoptiPrincipal());
			return new CurrentBusinessUnit(identity, null, null);
		}

		public CurrentBusinessUnit(ICurrentIdentity identity, ICurrentHttpContext currentHttpContext, IBusinessUnitRepository businessUnitRepository)
		{
			_identity = identity;
			_currentHttpContext = currentHttpContext;
			_businessUnitRepository = businessUnitRepository;
		}

		public IBusinessUnit Current()
		{
			IBusinessUnit businessUnit = null;
			if (_currentHttpContext != null)
			{
				var buid = UnitOfWorkAspect.BusinessUnitIdForRequest(_currentHttpContext);
				if (buid.HasValue) businessUnit = _businessUnitRepository.Load(buid.Value);
			}
			if (businessUnit == null)
			{
				var identity = _identity.Current();
				return identity == null ? null : identity.BusinessUnit;
			}
			return businessUnit;
		}
	}
}