using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Specific
{
    public class PersonThatCreatesTestData : IDataSetup
    {
        private readonly IPerson _personThatCreatesTestData;

        public PersonThatCreatesTestData(IPerson personThatCreatesTestData)
        {
            _personThatCreatesTestData = personThatCreatesTestData;
        }

        public void Apply(ICurrentUnitOfWork currentUnitOfWork)
        {
            PersonRepository.DONT_USE_CTOR(currentUnitOfWork, null, null).Add(_personThatCreatesTestData);
        }
    }
}