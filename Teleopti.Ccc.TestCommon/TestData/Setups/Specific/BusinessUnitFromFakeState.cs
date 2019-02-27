using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Specific
{
    public class BusinessUnitFromFakeState : IDataSetup
    {
        private readonly IBusinessUnit _fakeBusinessUnit;

        public BusinessUnitFromFakeState(IBusinessUnit fakeBusinessUnit)
        {
            _fakeBusinessUnit = fakeBusinessUnit;
        }

        public void Apply(ICurrentUnitOfWork currentUnitOfWork)
        {
            BusinessUnitRepository.DONT_USE_CTOR(currentUnitOfWork, null, null).Add(_fakeBusinessUnit);
        }
    }
}