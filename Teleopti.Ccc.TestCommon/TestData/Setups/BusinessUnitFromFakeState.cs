﻿using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups
{
    public class BusinessUnitFromFakeState : IDataSetup
    {
        private readonly IBusinessUnit _fakeBusinessUnit;

        public BusinessUnitFromFakeState(IBusinessUnit fakeBusinessUnit)
        {
            _fakeBusinessUnit = fakeBusinessUnit;
        }

        public void Apply(IUnitOfWork uow)
        {
            new BusinessUnitRepository(uow).Add(_fakeBusinessUnit);
        }
    }
}