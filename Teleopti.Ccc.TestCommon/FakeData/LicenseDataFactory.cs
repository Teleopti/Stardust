using System;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeData
{
    public static class LicenseDataFactory
    {

        public const string CustomerName = "Kunden";
        public readonly static DateTime ExpirationDate = new DateTime(2020, 1, 1);
        public const int MaxActiveAgents = 100;
        public readonly static Percent MaxActiveAgentsGrace = new Percent(0.1);


        /// <summary>
        /// Creates the default license activator.
        /// </summary>
		/// <returns></returns>
		[CLSCompliant(false)]
        public static ILicenseActivator CreateDefaultLicenseActivatorForTest()
        {
            ILicenseActivator licenseActivator = new LicenseActivator(CustomerName, ExpirationDate, MaxActiveAgents, 100, LicenseType.Agent, MaxActiveAgentsGrace,
								XmlLicenseService.IsThisAlmostTooManyActiveAgents, LicenseActivator.IsThisTooManyActiveAgents);
            licenseActivator.EnabledLicenseOptionPaths.Add(AllLicenseOption.FakeOptionPath);
            return licenseActivator;
        }

        /// <summary>
        /// Creates the default active license schema for test.
        /// </summary>
        /// <returns></returns>
        public static LicenseSchema CreateDefaultActiveLicenseSchemaForTest()
        {
            LicenseSchema activeLicenseSchema = new LicenseSchema();
            activeLicenseSchema.ActivateLicense(CreateDefaultLicenseActivatorForTest());
            return activeLicenseSchema;
        }
    }
}
