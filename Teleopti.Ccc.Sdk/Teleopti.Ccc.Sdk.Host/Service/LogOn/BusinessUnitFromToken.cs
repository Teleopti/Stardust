using System.Globalization;
using System.ServiceModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.LogOn
{
    public class BusinessUnitFromToken
    {
        private IBusinessUnit _businessUnit;
        private ITokenWithBusinessUnitAndDataSource _tokenWithBusinessUnit;
        private IDataSource _dataSource;
        private readonly BusinessUnitCache _businessUnitCache = new BusinessUnitCache();

        public IBusinessUnit BusinessUnit
        {
            get { return _businessUnit; }
        }

        public void SetBusinessUnitFromToken(ITokenWithBusinessUnitAndDataSource tokenWithBusinessUnit, IDataSource dataSource)
        {
            _tokenWithBusinessUnit = tokenWithBusinessUnit;
            _dataSource = dataSource;
            if (TryGetBusinessUnitFromCache())
            {
                return;
            }
            if (TryGetBusinessUnitFromStore())
            {
                _businessUnitCache.Add(_businessUnit);
                return;
            }
            throw new FaultException(string.Format(CultureInfo.InvariantCulture, "The business unit with id {0} could not be found.", tokenWithBusinessUnit.BusinessUnit));
        }

        private bool TryGetBusinessUnitFromStore()
        {
            using (var uow = _dataSource.Application.CreateAndOpenUnitOfWork())
            {
                _businessUnit =
                    BusinessUnitRepository.DONT_USE_CTOR(uow).Get(_tokenWithBusinessUnit.BusinessUnit);
                return _businessUnit != null;
            }
        }

        private bool TryGetBusinessUnitFromCache()
        {
            _businessUnit = _businessUnitCache.Get(_tokenWithBusinessUnit.BusinessUnit);
            return _businessUnit != null;
        }
    }
}