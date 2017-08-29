﻿using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
            new PersonRepository(currentUnitOfWork).Add(_personThatCreatesTestData);
        }
    }
}