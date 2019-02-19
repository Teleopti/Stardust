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
            new BusinessUnitRepository(currentUnitOfWork, null, null).Add(_fakeBusinessUnit);
        }
    }
}