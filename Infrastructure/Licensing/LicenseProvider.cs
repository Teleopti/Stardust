#region Imports

using System;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

#endregion

namespace Teleopti.Ccc.Infrastructure.Licensing
{
    /// <summary>
    /// Initializes the LicenseActivator using the appropriate licensing mechanism, so that 
    /// License information is available to the rest of the system
    /// </summary>
    /// <remarks>
    /// Created by: Klas
    /// Created date: 2008-12-03
    /// </remarks>
    public static class LicenseProvider
    {
        /// <summary>
        /// Initializes the license activator using the license service
        /// </summary>
        /// <param name="licenseService">The license service.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: Klas
        /// Created date: 2008-12-03
		/// </remarks>
		public static ILicenseActivator GetLicenseActivator(ILicenseService licenseService)
        {
            if (licenseService == null) throw new ArgumentNullException("licenseService");

            ILicenseActivator licenseActivator = new LicenseActivator(licenseService.CustomerName,
                                                                      licenseService.ExpirationDate,
                                                                      licenseService.MaxActiveAgents,
																	  licenseService.MaxSeats,
																	  licenseService.LicenseType,
																																			new Percent(licenseService.MaxActiveAgentsGrace), 
																																			LicenseActivator.IsThisAlmostTooManyActiveAgents,
																																			LicenseActivator.IsThisTooManyActiveAgents);

            if (licenseService.TeleoptiCccPilotCustomersBaseEnabled)
                licenseActivator.EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiCccPilotCustomersBase);
            if (licenseService.TeleoptiCccPilotCustomersForecastsEnabled)
                licenseActivator.EnabledLicenseOptionPaths.Add(
                    DefinedLicenseOptionPaths.TeleoptiCccPilotCustomersForecasts);
            if (licenseService.TeleoptiCccPilotCustomersShiftsEnabled)
                licenseActivator.EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiCccPilotCustomersShifts);
            if (licenseService.TeleoptiCccPilotCustomersPeopleEnabled)
                licenseActivator.EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiCccPilotCustomersPeople);
            if (licenseService.TeleoptiCccPilotCustomersAgentPortalEnabled)
                licenseActivator.EnabledLicenseOptionPaths.Add(
                    DefinedLicenseOptionPaths.TeleoptiCccPilotCustomersAgentPortal);
            if (licenseService.TeleoptiCccPilotCustomersOptionsEnabled)
                licenseActivator.EnabledLicenseOptionPaths.Add(
                    DefinedLicenseOptionPaths.TeleoptiCccPilotCustomersOptions);
            if (licenseService.TeleoptiCccPilotCustomersSchedulerEnabled)
                licenseActivator.EnabledLicenseOptionPaths.Add(
                    DefinedLicenseOptionPaths.TeleoptiCccPilotCustomersScheduler);
            if (licenseService.TeleoptiCccPilotCustomersIntradayEnabled)
                licenseActivator.EnabledLicenseOptionPaths.Add(
                    DefinedLicenseOptionPaths.TeleoptiCccPilotCustomersIntraday);
            if (licenseService.TeleoptiCccPilotCustomersPermissionsEnabled)
                licenseActivator.EnabledLicenseOptionPaths.Add(
                    DefinedLicenseOptionPaths.TeleoptiCccPilotCustomersPermissions);
            if (licenseService.TeleoptiCccPilotCustomersReportsEnabled)
                licenseActivator.EnabledLicenseOptionPaths.Add(
                    DefinedLicenseOptionPaths.TeleoptiCccPilotCustomersReports);


            if (licenseService.TeleoptiCccBaseEnabled)
                licenseActivator.EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiCccBase);
            if (licenseService.TeleoptiCccAgentSelfServiceEnabled)
                licenseActivator.EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiCccLifestyle);
            if (licenseService.TeleoptiCccShiftTradesEnabled)
                licenseActivator.EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiCccShiftTrader);
            if (licenseService.TeleoptiCccAgentScheduleMessengerEnabled)
                licenseActivator.EnabledLicenseOptionPaths.Add(
                    DefinedLicenseOptionPaths.TeleoptiCccAgentScheduleMessenger);
            if (licenseService.TeleoptiCccHolidayPlannerEnabled)
                licenseActivator.EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiCccHolidayPlanner);
            if (licenseService.TeleoptiCccRealTimeAdherenceEnabled)
                licenseActivator.EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiCccRealTimeAdherence);
            if (licenseService.TeleoptiCccPerformanceManagerEnabled)
                licenseActivator.EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiCccPerformanceManager);
            if (licenseService.TeleoptiCccPayrollIntegrationEnabled)
                licenseActivator.EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiCccPayrollIntegration);
			if (licenseService.TeleoptiCccSmsLinkEnabled)
				licenseActivator.EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiCccSmsLink);
			if (licenseService.TeleoptiCccCalendarLinkEnabled)
				licenseActivator.EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiCccCalendarLink);

            if (licenseService.TeleoptiCccFreemiumForecastsEnabled)
                licenseActivator.EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiCccFreemiumForecasts);

			if (licenseService.TeleoptiCccVersion8Enabled)
				licenseActivator.EnabledLicenseOptionPaths.Add(DefinedLicenseOptionPaths.TeleoptiCccVersion8);

            return licenseActivator;
        }

        public static void ProvideLicenseActivator(string dataSource, ILicenseService licenseService)
        {
            if (licenseService == null) throw new ArgumentNullException("licenseService");

            DefinedLicenseDataFactory.SetLicenseActivator(dataSource, GetLicenseActivator(licenseService));
        }
    }
}