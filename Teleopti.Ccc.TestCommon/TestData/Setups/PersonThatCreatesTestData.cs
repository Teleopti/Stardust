using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups
{
    public class PersonThatCreatesTestData : IDataSetup
    {
        private readonly IPerson _personThatCreatesTestData;

        public PersonThatCreatesTestData(IPerson personThatCreatesTestData)
        {
            _personThatCreatesTestData = personThatCreatesTestData;
        }

        public void Apply(IUnitOfWork uow)
        {
            new PersonRepository(uow).Add(_personThatCreatesTestData);
        }
    }
}