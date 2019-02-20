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
		private readonly Lazy<IBusinessUnitRepository> _businessUnitRepository;
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
			Lazy<IBusinessUnitRepository> businessUnitRepository)
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

			var identity = _identity.Current();
			var businessUnitId = _businessUnitIdForRequest?.Get() ?? identity?.BusinessUnitId;
			if (businessUnitId == null)
				return null;
			
			var hasUnitOfWork = _unitOfWork?.Current()?.HasCurrentUnitOfWork() ?? false;
			if (hasUnitOfWork && _businessUnitRepository != null)
				return _businessUnitRepository.Value.Load(businessUnitId.Value);

			var businessUnitOnlyForEntities = new BusinessUnit(identity.BusinessUnitName);
			businessUnitOnlyForEntities.SetId(businessUnitId);
			return businessUnitOnlyForEntities;
		}

		public Guid? CurrentId()
		{
			if (_threadBusinessUnit.Value != null)
				return _threadBusinessUnit.Value.Id;

			return _businessUnitIdForRequest?.Get() ?? _identity.Current()?.BusinessUnitId;
		}

		public IDisposable OnThisThreadUse(IBusinessUnit businessUnit)
		{
			_threadBusinessUnit.Value = businessUnit;
			return new GenericDisposable(() => { _threadBusinessUnit.Value = null; });
		}
	}
}