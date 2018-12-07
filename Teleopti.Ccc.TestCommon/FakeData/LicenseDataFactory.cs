using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Secrets.Licensing;


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
        public static ILicenseActivator CreateDefaultLicenseActivatorForTest()
        {
            ILicenseActivator licenseActivator = new LicenseActivator(CustomerName, ExpirationDate, false, MaxActiveAgents, 100, LicenseType.Agent, MaxActiveAgentsGrace, "8");
            licenseActivator.EnabledLicenseOptionPaths.Add(AllLicenseOption.FakeOptionPath);
            return licenseActivator;
        }

        /// <summary>
        /// Creates the base license activator.
        /// </summary>
		/// <returns></returns>
		public static ILicenseActivator CreateBaseLicenseActivatorForTest()
        {
            ILicenseActivator licenseActivator = new LicenseActivator(CustomerName, ExpirationDate, false, MaxActiveAgents, 100, LicenseType.Agent, MaxActiveAgentsGrace, "8");
			licenseActivator.EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiCccBase);
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

        /// <summary>
        /// Creates the base license schema for test.
        /// </summary>
        /// <returns></returns>
        public static LicenseSchema CreateBaseLicenseSchemaForTest()
        {
            LicenseSchema activeLicenseSchema = new LicenseSchema();
            activeLicenseSchema.ActivateLicense(CreateBaseLicenseActivatorForTest());
            return activeLicenseSchema;
        }
    }
}
