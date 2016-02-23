using System.IO;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Specific
{
    public class LicenseFromFile : IDataSetup
    {
        public void Apply(ICurrentUnitOfWork currentUnitOfWork)
        {
            var license = new License { XmlString = File.ReadAllText("TestLicense.xml") };
            var licenseRepository = new LicenseRepository(currentUnitOfWork);
            licenseRepository.Add(license);
        }
    }
}