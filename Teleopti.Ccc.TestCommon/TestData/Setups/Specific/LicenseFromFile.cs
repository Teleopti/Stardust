﻿using System.IO;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Specific
{
    public class LicenseFromFile : IDataSetup
    {
        public void Apply(IUnitOfWork uow)
        {
            var license = new License { XmlString = File.ReadAllText("License.xml") };
            var licenseRepository = new LicenseRepository(uow);
            licenseRepository.Add(license);
        }
    }
}