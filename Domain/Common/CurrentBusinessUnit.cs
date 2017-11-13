using System;
using System.Threading;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Common
{
	public class CurrentBusinessUnit : ICurrentBusinessUnit, IBusinessUnitScope
	{
		private readonly ICurrentIdentity _identity;
		private readonly IBusinessUnitIdForRequest _businessUnitIdForRequest;
		private readonly ICurrentUnitOfWorkFactory _unitOfWork;
		private readonly IBusinessUnitRepository _businessUnitRepository;
		private readonly ThreadLocal<IBusinessUnit> _threadBusinessUnit = new ThreadLocal<IBusinessUnit>();

		public static ICurrentBusinessUnit Make()
		{
			var identity = new CurrentIdentity(new CurrentTeleoptiPrincipal(new ThreadPrincipalContext()));
			return new CurrentBusinessUnit(identity, null, null, null);
		}

		public CurrentBusinessUnit(
			ICurrentIdentity identity,
			IBusinessUnitIdForRequest businessUnitIdForRequest,
			ICurrentUnitOfWorkFactory unitOfWork,
			IBusinessUnitRepository businessUnitRepository)
		{
			_identity = identity;
			_businessUnitIdForRequest = businessUnitIdForRequest;
			_unitOfWork = unitOfWork;
			_businessUnitRepository = businessUnitRepository;
		}

		public IBusinessUnit Current()
		{
			if (_threadBusinessUnit.Value != null)
				return _threadBusinessUnit.Value;

			var hasUnitOfWork = _unitOfWork?.Current()?.HasCurrentUnitOfWork() ?? false;
			var id = _businessUnitIdForRequest?.Get();
			if (id.HasValue && hasUnitOfWork && _businessUnitRepository != null)
				return _businessUnitRepository.Load(id.Value);

			return _identity.Current()?.BusinessUnit;
		}

		public Guid? CurrentId()
		{
			if (_threadBusinessUnit.Value != null)
				return _threadBusinessUnit.Value.Id;

			var businessUnit = _businessUnitIdForRequest?.Get();
			if (businessUnit != null)
				return businessUnit;

			return _identity.Current()?.BusinessUnit?.Id;
		}

		public IDisposable OnThisThreadUse(IBusinessUnit businessUnit)
		{
			_threadBusinessUnit.Value = businessUnit;
			return new GenericDisposable(() => { _threadBusinessUnit.Value = null; });
		}
	}
}