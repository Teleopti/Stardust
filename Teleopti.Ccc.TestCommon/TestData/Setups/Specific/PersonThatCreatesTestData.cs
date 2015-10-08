using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Specific
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
            new PersonRepository(new ThisUnitOfWork(uow)).Add(_personThatCreatesTestData);
        }
    }
}