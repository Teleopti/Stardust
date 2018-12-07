using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Secrets.Licensing;

namespace Teleopti.Ccc.Infrastructure.Licensing
{
	public static class LicenseProvider
	{
		private const string majorVersion = "8";

		public static ILicenseActivator GetLicenseActivator(ILicenseService licenseService)
		{
			if (licenseService == null) throw new ArgumentNullException(nameof(licenseService));

			var optionPaths = new Dictionary<string, bool>
			{
				{
					DefinedLicenseOptionPaths.TeleoptiCccBase,
					licenseService.TeleoptiCccBaseEnabled
				},
				{
					DefinedLicenseOptionPaths.TeleoptiCccLifestyle,
					licenseService.TeleoptiCccAgentSelfServiceEnabled || licenseService.TeleoptiWFMLifestyleEnabled
				},
				{
					DefinedLicenseOptionPaths.TeleoptiCccShiftTrader,
					licenseService.TeleoptiCccShiftTradesEnabled || licenseService.TeleoptiWFMShiftTraderEnabled
				},
				{
					DefinedLicenseOptionPaths.TeleoptiCccAgentScheduleMessenger,
					licenseService.TeleoptiCccAgentScheduleMessengerEnabled
				},
				{
					DefinedLicenseOptionPaths.TeleoptiCccVacationPlanner,
					licenseService.TeleoptiCccHolidayPlannerEnabled || licenseService.TeleoptiWFMVacationPlannerEnabled
				},
				{
					DefinedLicenseOptionPaths.TeleoptiCccMyTeam,
					licenseService.TeleoptiWFMMyTeamEnabled
				},
				{
					DefinedLicenseOptionPaths.TeleoptiCccNotify,
					licenseService.TeleoptiWFMNotifyEnabled
				},
				{
					DefinedLicenseOptionPaths.TeleoptiCccOvertimeAvailability,
					licenseService.TeleoptiWFMOvertimeAvailabilityEnabled
				},
				{
					DefinedLicenseOptionPaths.TeleoptiCccRealTimeAdherence,
					licenseService.TeleoptiCccRealTimeAdherenceEnabled
				},
				{
					DefinedLicenseOptionPaths.TeleoptiCccPerformanceManager,
					licenseService.TeleoptiCccPerformanceManagerEnabled
				},
				{
					DefinedLicenseOptionPaths.TeleoptiCccPayrollIntegration,
					licenseService.TeleoptiCccPayrollIntegrationEnabled
				},
				{
					DefinedLicenseOptionPaths.TeleoptiCccSmsLink,
					licenseService.TeleoptiCccSmsLinkEnabled
				},
				{
					DefinedLicenseOptionPaths.TeleoptiCccCalendarLink,
					licenseService.TeleoptiCccCalendarLinkEnabled
				},
				{
					DefinedLicenseOptionPaths.TeleoptiCccFreemiumForecasts,
					licenseService.TeleoptiCccFreemiumForecastsEnabled
				},
				{
					DefinedLicenseOptionPaths.TeleoptiWfmVNextPilot,
					licenseService.TeleoptiWFMVNextEnabled
				},
				{
					DefinedLicenseOptionPaths.TeleoptiWfmOutbound,
					licenseService.TeleoptiWFMOutboundEnabled
				},
				{
					DefinedLicenseOptionPaths.TeleoptiWfmSeatPlanner,
					licenseService.TeleoptiWFMSeatPlannerEnabled
				},
				{
					DefinedLicenseOptionPaths.TeleoptiCccPilotCustomersBpoExchange,
					licenseService.TeleoptiWFMBPOExchangeEnabled
				},{
					DefinedLicenseOptionPaths.TeleoptiWfmOvertimeRequests,
					licenseService.TeleoptiWFMOvertimeRequestsEnabled
				},{
					DefinedLicenseOptionPaths.TeleoptiWfmChatBot,
					licenseService.TeleoptiWFMChatBotEnabled
				},{
					DefinedLicenseOptionPaths.TeleoptiWfmInsights,
					licenseService.TeleoptiWFMInsightsEnabled
				}
			};

			ILicenseActivator licenseActivator = new LicenseActivator(licenseService.CustomerName,
				licenseService.ExpirationDate,
				licenseService.Perpetual,
				licenseService.MaxActiveAgents,
				licenseService.MaxSeats,
				licenseService.LicenseType,
				new Percent(licenseService.MaxActiveAgentsGrace), 
				majorVersion);

			foreach (var optionPath in optionPaths.Where(optionPath => optionPath.Value))
			{
				licenseActivator.EnabledLicenseOptionPaths.Add(optionPath.Key);
			}

			return licenseActivator;
		}

		public static void ProvideLicenseActivator(string dataSource, ILicenseService licenseService)
		{
			if (licenseService == null) throw new ArgumentNullException(nameof(licenseService));

			DefinedLicenseDataFactory.SetLicenseActivator(dataSource, GetLicenseActivator(licenseService));
		}
	}
}