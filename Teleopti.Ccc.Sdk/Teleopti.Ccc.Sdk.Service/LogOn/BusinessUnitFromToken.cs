using System.Globalization;
using System.Linq;
using System.ServiceModel;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.WcfExtensions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.WcfService.LogOn
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
                    new BusinessUnitRepository(uow).Get(_tokenWithBusinessUnit.BusinessUnit);
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