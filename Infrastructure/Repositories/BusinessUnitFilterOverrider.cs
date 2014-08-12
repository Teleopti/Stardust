using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class BusinessUnitFilterOverrider : IBusinessUnitFilterOverrider
    {
        private readonly ICurrentUnitOfWork _unitOfWork;
        private readonly ICurrentIdentity _identity;

        public BusinessUnitFilterOverrider(ICurrentUnitOfWork unitOfWork, ICurrentIdentity identity)
        {
            _unitOfWork = unitOfWork;
            _identity = identity;
        }

        public IDisposable OverrideWith(Guid buId)
        {
            _unitOfWork.Current().DisableFilter(QueryFilter.BusinessUnit);
            _unitOfWork.Session().EnableFilter("businessUnitFilter").SetParameter("businessUnitParameter", buId);
            return new GenericDisposable(() =>
            {
                _unitOfWork.Current().DisableFilter(QueryFilter.BusinessUnit);
                _unitOfWork.Session().EnableFilter("businessUnitFilter").SetParameter("businessUnitParameter", _identity.Current().BusinessUnit.Id.Value);
            });
        }
    }

    public interface IBusinessUnitFilterOverrider

    {
        IDisposable OverrideWith(Guid buId);
    }


    public class GenericDisposable : IDisposable
    {
        private Action _disposeAction;

        public GenericDisposable(Action disposeAction)
        {
            _disposeAction = disposeAction;
        }

        public void Dispose()
        {
            _disposeAction.Invoke();
            _disposeAction = null;
        }
    }
}