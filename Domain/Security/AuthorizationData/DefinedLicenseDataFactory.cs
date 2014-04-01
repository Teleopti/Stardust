using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.LicenseOptions;

namespace Teleopti.Ccc.Domain.Security.AuthorizationData
{

    /// <summary>
    /// Represents the default license schema of Raptor and makes the licence data available.
    /// </summary>
    public static class DefinedLicenseDataFactory
    {
        private static ILicenseActivator _licenseActivator;

        /// <summary>
        /// Gets or sets the license activator.
        /// </summary>
        /// <value>The license activator.</value>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-10-07
		/// </remarks>
		[CLSCompliant(false)]
        public static ILicenseActivator LicenseActivator
        {
            get { return _licenseActivator; }
            set { _licenseActivator = value; }
        }

        #region Interface

        /// <summary>
        /// Creates the active license schema.
        /// </summary>
        /// <returns></returns>
        public static LicenseSchema CreateActiveLicenseSchema()
        {
            LicenseSchema activeLicenseSchema = new LicenseSchema();
            activeLicenseSchema.ActivateLicense(LicenseActivator);
            return activeLicenseSchema;
        }

        /// <summary>
        /// Creates the defined license options.
        /// </summary>
        /// <returns></returns>
        public static ReadOnlyCollection<LicenseOption> CreateDefinedLicenseOptions()
        {
            IList<LicenseOption> licenseOptions = new List<LicenseOption>();

            licenseOptions.Add(new TeleoptiCccBaseLicenseOption());
            licenseOptions.Add(new TeleoptiCccDeveloperLicenseOption());
            licenseOptions.Add(new TeleoptiCccAgentSelfServiceLicenseOption());
            licenseOptions.Add(new TeleoptiCccShiftTradesLicenseOption());
            licenseOptions.Add(new TeleoptiCccAgentScheduleMessengerLicenseOption());
            licenseOptions.Add(new TeleoptiCccHolidayPlannerLicenseOption());
            licenseOptions.Add(new TeleoptiCccRealTimeAdherenceLicenseOption());
            licenseOptions.Add(new TeleoptiCccPerformanceManagerLicenseOption());
            licenseOptions.Add(new TeleoptiCccPayrollIntegrationLicenseOption());
            licenseOptions.Add(new TeleoptiCccMyTimeWebLicenseOption());
			licenseOptions.Add(new TeleoptiCccMobileReportsLicenseOption());
			licenseOptions.Add(new TeleoptiCccSmsLinkLicenseOption());
			licenseOptions.Add(new TeleoptiCccCalendarLinkLicenseOption());
			licenseOptions.Add(new TeleoptiCccVersion8LicenseOption());
            
            licenseOptions.Add(new TeleoptiCccFreemiumForecastsLicenseOption());

            licenseOptions.Add(new TeleoptiCccPilotCustomersBaseLicenseOption());
            licenseOptions.Add(new TeleoptiCccPilotCustomersForecastsLicenseOption());
            licenseOptions.Add(new TeleoptiCccPilotCustomersShiftsLicenseOption());
            licenseOptions.Add(new TeleoptiCccPilotCustomersPeopleLicenseOption());
            licenseOptions.Add(new TeleoptiCccPilotCustomersAgentPortalLicenseOption());
            licenseOptions.Add(new TeleoptiCccPilotCustomersOptionsLicenseOption());
            licenseOptions.Add(new TeleoptiCccPilotCustomersSchedulerLicenseOption());
            licenseOptions.Add(new TeleoptiCccPilotCustomersIntradayLicenseOption());
            licenseOptions.Add(new TeleoptiCccPilotCustomersPermissionsLicenseOption());
            licenseOptions.Add(new TeleoptiCccPilotCustomersReportsLicenseOption());
            
            // TODO: this should be removed
            licenseOptions.Add(new AllLicenseOption());
            return new ReadOnlyCollection<LicenseOption>(licenseOptions);
        }

        #endregion

    }
}
