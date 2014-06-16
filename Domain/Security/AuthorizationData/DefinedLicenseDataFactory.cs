using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.LicenseOptions;

namespace Teleopti.Ccc.Domain.Security.AuthorizationData
{

    /// <summary>
    /// Represents the default license schema of Raptor and makes the licence data available.
    /// </summary>
    public static class DefinedLicenseDataFactory
    {
        private static readonly ConcurrentDictionary<string, ILicenseActivator> _licenseActivators = new ConcurrentDictionary<string, ILicenseActivator>();

        public static ILicenseActivator GetLicenseActivator(string dataSource)
        {
            ILicenseActivator activator;
            _licenseActivators.TryGetValue(dataSource, out activator);
            
            return activator;
        }

	    public static void ClearLicenseActivators()
	    {
		    _licenseActivators.Clear();
	    }

        public static void SetLicenseActivator(string dataSource, ILicenseActivator licenseActivator)
        {
            _licenseActivators.AddOrUpdate(dataSource, s => licenseActivator, (s, a) => licenseActivator);
        }

        public static bool HasLicense(string dataSource)
        {
            ILicenseActivator activator;
            return _licenseActivators.TryGetValue(dataSource, out activator) && activator != null;
        }

        public static bool HasAnyLicense
        {
            get { return _licenseActivators.Values.Any(a => a != null); }
        }

        #region Interface

        /// <summary>
        /// Creates the active license schema.
        /// </summary>
        /// <param name="dataSource"></param>
        /// <returns></returns>
        public static LicenseSchema CreateActiveLicenseSchema(string dataSource)
        {
            var activeLicenseSchema = new LicenseSchema();
            activeLicenseSchema.ActivateLicense(GetLicenseActivator(dataSource));
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

            licenseOptions.Add(new TeleoptiCccLifestyleLicenseOption());
            licenseOptions.Add(new TeleoptiCccShiftTraderLicenseOption());
			licenseOptions.Add(new TeleoptiCccVacationPlannerLicenseOption());
			licenseOptions.Add(new TeleoptiCccOvertimeAvailabilityLicenseOption());

			licenseOptions.Add(new TeleoptiCccNotifyLicenseOption());
			licenseOptions.Add(new TeleoptiCccAgentScheduleMessengerLicenseOption());
            licenseOptions.Add(new TeleoptiCccRealTimeAdherenceLicenseOption());
			licenseOptions.Add(new TeleoptiCccSmsLinkLicenseOption());
			licenseOptions.Add(new TeleoptiCccCalendarLinkLicenseOption());

            licenseOptions.Add(new TeleoptiCccPerformanceManagerLicenseOption());

            licenseOptions.Add(new TeleoptiCccPayrollIntegrationLicenseOption());

            licenseOptions.Add(new TeleoptiCccMyTeamLicenseOption());

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
