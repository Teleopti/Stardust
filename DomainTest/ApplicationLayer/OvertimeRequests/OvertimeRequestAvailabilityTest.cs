using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.OvertimeRequests
{
	[DomainTest]
	[TestFixture]
	public class OvertimeRequestAvailabilityTest
	{
		public IOvertimeRequestAvailability Target;
		public ICurrentDataSource CurrentDataSource;
		public FullPermission Authorization;

		[Test]
		public void ShouldGetOvertimeRequestAvailability()
		{
			var result = Target.IsEnabled();

			result.Should().Be.True();
		}

		[Test]
		public void ShouldDisabledOvertimeRequestWhenNoLicense()
		{
			var licenseActivator = LicenseProvider.GetLicenseActivator(new fakeLicenseService(DateTime.Now.AddDays(10), TimeSpan.FromDays(3), 1000));

			DefinedLicenseDataFactory.SetLicenseActivator(CurrentDataSource.CurrentName(), licenseActivator);

			var result = Target.IsLicenseEnabled();

			result.Should().Be.False();
		}

		[Test]
		public void ShouldDisabledOvertimeRequestWhenNoPermission()
		{
			Authorization.AddToBlackList(DefinedRaptorApplicationFunctionPaths.OvertimeRequestWeb);

			var result = Target.IsEnabled();

			result.Should().Be.False();
		}

		private class fakeLicenseService : ILicenseService
		{
			public fakeLicenseService(DateTime expirationDate, TimeSpan expirationGracePeriod,
																							double maxActiveAgentsGrace)
			{
				ExpirationDate = expirationDate;
				ExpirationGracePeriod = expirationGracePeriod;
				MaxActiveAgentsGrace = maxActiveAgentsGrace;

				TeleoptiCccPilotCustomersBaseEnabled = false;
				TeleoptiCccPilotCustomersForecastsEnabled = false;
				TeleoptiCccPilotCustomersShiftsEnabled = false;
				TeleoptiCccPilotCustomersPeopleEnabled = false;
				TeleoptiCccPilotCustomersAgentPortalEnabled = false;
				TeleoptiCccPilotCustomersOptionsEnabled = false;
				TeleoptiCccPilotCustomersSchedulerEnabled = false;
				TeleoptiCccPilotCustomersIntradayEnabled = false;
				TeleoptiCccPilotCustomersPermissionsEnabled = false;
				TeleoptiCccPilotCustomersReportsEnabled = false;
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
				TeleoptiWFMOvertimeAvailabilityEnabled = true;
				TeleoptiWFMShiftTraderEnabled = true;
				TeleoptiWFMVacationPlannerEnabled = true;
				TeleoptiWFMVNextEnabled = true;
				TeleoptiWFMOvertimeRequestsEnabled = false;
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
			public bool TeleoptiCccPilotCustomersBaseEnabled { get; }
			public bool TeleoptiCccPilotCustomersForecastsEnabled { get; }
			public bool TeleoptiCccPilotCustomersShiftsEnabled { get; }
			public bool TeleoptiCccPilotCustomersPeopleEnabled { get; }
			public bool TeleoptiCccPilotCustomersAgentPortalEnabled { get; }
			public bool TeleoptiCccPilotCustomersOptionsEnabled { get; }
			public bool TeleoptiCccPilotCustomersSchedulerEnabled { get; }
			public bool TeleoptiCccPilotCustomersIntradayEnabled { get; }
			public bool TeleoptiCccPilotCustomersPermissionsEnabled { get; }
			public bool TeleoptiCccPilotCustomersReportsEnabled { get; }
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
}
