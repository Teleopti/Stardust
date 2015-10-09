using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
            new BusinessUnitRepository(currentUnitOfWork).Add(_fakeBusinessUnit);
        }
    }
}