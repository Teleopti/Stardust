using System;
using Teleopti.Ccc.Secrets.Licensing;

namespace Teleopti.Ccc.TestCommon
{
	public class OvertimeFakeLicenseService : ILicenseService
	{
		public OvertimeFakeLicenseService(bool overtimeAvailabilityEnabled, bool overtimeRequestsEnabled)
		{
			ExpirationDate = DateTime.MaxValue;
			ExpirationGracePeriod = TimeSpan.FromDays(10);
			MaxActiveAgentsGrace = 1000;
			
			TeleoptiCccBaseEnabled = true;
			TeleoptiCccDeveloperEnabled = true;
			TeleoptiCccAgentSelfServiceEnabled = true;
			TeleoptiCccShiftTradesEnabled = true;
			TeleoptiCccAgentScheduleMessengerEnabled = true;
			TeleoptiCccHolidayPlannerEnabled = true;
			TeleoptiCccRealTimeAdherenceEnabled = true;
			TeleoptiCccPerformanceManagerEnabled = true;
			TeleoptiCccPayrollIntegrationEnabled = true;
			TeleoptiCccFreemiumForecastsEnabled = false;
			TeleoptiCccMyTimeWebEnabled = true;
			TeleoptiCccSmsLinkEnabled = true;
			TeleoptiCccCalendarLinkEnabled = true;
			TeleoptiWFMLifestyleEnabled = true;
			TeleoptiWFMMyTeamEnabled = true;
			TeleoptiWFMNotifyEnabled = true;
			TeleoptiWFMOvertimeAvailabilityEnabled = overtimeAvailabilityEnabled;
			TeleoptiWFMShiftTraderEnabled = true;
			TeleoptiWFMVacationPlannerEnabled = true;
			TeleoptiWFMVNextEnabled = true;
			TeleoptiWFMOvertimeRequestsEnabled = overtimeRequestsEnabled;
			TeleoptiWFMGrantEnabled = true;
			TeleoptiWFMInsightsEnabled = true;
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		public string CustomerName { get; private set; }
		public DateTime ExpirationDate { get; }
		public TimeSpan ExpirationGracePeriod { get; }
		public bool Perpetual { get; private set; }
		public int MaxActiveAgents { get; private set; }
		public double MaxActiveAgentsGrace { get; }
		public double MajorVersion { get; private set; }

		public bool IsThisTooManyActiveAgents(int activeAgents)
		{
			throw new NotImplementedException();
		}
		public bool IsThisAlmostTooManyActiveAgents(int activeAgents)
		{
			throw new NotImplementedException();
		}
		public bool TeleoptiCccBaseEnabled { get; }
		public bool TeleoptiCccDeveloperEnabled { get; }
		public bool TeleoptiCccAgentSelfServiceEnabled { get; }
		public bool TeleoptiCccShiftTradesEnabled { get; }
		public bool TeleoptiCccAgentScheduleMessengerEnabled { get; }
		public bool TeleoptiCccHolidayPlannerEnabled { get; }
		public bool TeleoptiCccRealTimeAdherenceEnabled { get; }
		public bool TeleoptiCccPerformanceManagerEnabled { get; }
		public bool TeleoptiCccPayrollIntegrationEnabled { get; }
		public bool TeleoptiCccMyTimeWebEnabled { get; }
		public bool TeleoptiCccSmsLinkEnabled { get; }
		public bool TeleoptiCccCalendarLinkEnabled { get; }
		public bool TeleoptiWFMOvertimeRequestsEnabled { get; }
		public bool TeleoptiWFMGrantEnabled { get; }
		public bool TeleoptiWFMInsightsEnabled { get; }
		public bool TeleoptiCccFreemiumForecastsEnabled { get; }
		public int MaxSeats => 10;
		public LicenseType LicenseType => LicenseType.Agent;
		public decimal Ratio => throw new NotImplementedException();
		public bool TeleoptiWFMVacationPlannerEnabled { get; }
		public bool TeleoptiWFMShiftTraderEnabled { get; }
		public bool TeleoptiWFMLifestyleEnabled { get; }
		public bool TeleoptiWFMOvertimeAvailabilityEnabled { get; }
		public bool TeleoptiWFMNotifyEnabled { get; }
		public bool TeleoptiWFMVNextEnabled { get; }
		public bool TeleoptiWFMMyTeamEnabled { get; }
		public bool TeleoptiWFMOutboundEnabled { get; private set; }
		public bool TeleoptiWFMSeatPlannerEnabled { get; private set; }
		public bool TeleoptiWFMBPOExchangeEnabled { get; private set; }
	}
}
