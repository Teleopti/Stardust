using System;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;


namespace Teleopti.Ccc.InfrastructureTest.Licensing
{
    [TestFixture]
    [Category("LongRunning")]
    public class LicenseProviderTest
    {
        private MockRepository _mocks;

        private const string customerName = "Kundnamn";
        private readonly DateTime _expirationDate = DateTime.Now.AddDays(100);
        private readonly TimeSpan _expirationGracePeriod = new TimeSpan(30, 0, 0, 0);
        private const int maxActiveAgents = 120;
        private readonly Percent _maxActiveAgentsGrace = new Percent(0.2);

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
        }

        [Test]
        public void CanGetLicenseActivator()
        {
            using (
                ILicenseService licenseService = new stubStandardLicenseService(_expirationDate, _expirationGracePeriod,
                                                                                _maxActiveAgentsGrace.Value))
            {
                _mocks.ReplayAll();
                ILicenseActivator licenseActivator = LicenseProvider.GetLicenseActivator(licenseService);
                _mocks.VerifyAll();
                Assert.IsTrue(
                    licenseActivator.EnabledLicenseOptionPaths.Contains(DefinedLicenseOptionPaths.TeleoptiCccBase));
                Assert.IsTrue(
                    licenseActivator.EnabledLicenseOptionPaths.Contains(
                        DefinedLicenseOptionPaths.TeleoptiCccHolidayPlanner));
                Assert.IsFalse(
                    licenseActivator.EnabledLicenseOptionPaths.Contains(
                        DefinedLicenseOptionPaths.TeleoptiCccPilotCustomersBase));
                Assert.IsFalse(
                    licenseActivator.EnabledLicenseOptionPaths.Contains(
                        DefinedLicenseOptionPaths.TeleoptiCccFreemiumForecasts));
                Assert.AreEqual(DefinedLicenseSchemaCodes.TeleoptiCccSchema, licenseActivator.EnabledLicenseSchemaName);
            }
        }

        [Test]
        public void CanGetLicenseActivatorForFreemium()
        {
            using (
                ILicenseService licenseService = new stubFreemiumLicenseService(_expirationDate, _expirationGracePeriod,
                                                                                _maxActiveAgentsGrace.Value))
            {
                _mocks.ReplayAll();
                ILicenseActivator licenseActivator = LicenseProvider.GetLicenseActivator(licenseService);
                _mocks.VerifyAll();
                Assert.IsFalse(
                    licenseActivator.EnabledLicenseOptionPaths.Contains(DefinedLicenseOptionPaths.TeleoptiCccBase));
                Assert.IsFalse(
                    licenseActivator.EnabledLicenseOptionPaths.Contains(
                        DefinedLicenseOptionPaths.TeleoptiCccHolidayPlanner));
                Assert.IsFalse(
                    licenseActivator.EnabledLicenseOptionPaths.Contains(
                        DefinedLicenseOptionPaths.TeleoptiCccPilotCustomersBase));
                Assert.IsTrue(
                    licenseActivator.EnabledLicenseOptionPaths.Contains(
                        DefinedLicenseOptionPaths.TeleoptiCccFreemiumForecasts));
                Assert.AreEqual(DefinedLicenseSchemaCodes.TeleoptiCccForecastsSchema,
                                licenseActivator.EnabledLicenseSchemaName);
            }
        }

        [Test]
        public void CanGetLicenseActivatorForPilotCustomer()
        {
            using (
                ILicenseService licenseService = new stubPilotCustomersLicenseService(_expirationDate,
                                                                                      _expirationGracePeriod,
                                                                                      _maxActiveAgentsGrace.Value))
            {
                _mocks.ReplayAll();
                ILicenseActivator licenseActivator = LicenseProvider.GetLicenseActivator(licenseService);
                _mocks.VerifyAll();
                Assert.IsFalse(
                    licenseActivator.EnabledLicenseOptionPaths.Contains(DefinedLicenseOptionPaths.TeleoptiCccBase));
                Assert.IsFalse(
                    licenseActivator.EnabledLicenseOptionPaths.Contains(
                        DefinedLicenseOptionPaths.TeleoptiCccHolidayPlanner));
                Assert.IsTrue(
                    licenseActivator.EnabledLicenseOptionPaths.Contains(
                        DefinedLicenseOptionPaths.TeleoptiCccPilotCustomersBase));
                Assert.IsFalse(
                    licenseActivator.EnabledLicenseOptionPaths.Contains(
                        DefinedLicenseOptionPaths.TeleoptiCccFreemiumForecasts));
                Assert.AreEqual(DefinedLicenseSchemaCodes.TeleoptiCccPilotCustomersSchema,
                                licenseActivator.EnabledLicenseSchemaName);
            }
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
        public void VerifyMissingArgumentsProvideLicenseActivator()
        {
            LicenseProvider.ProvideLicenseActivator("",null);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void VerifyMissingArgumentsLicense()
        {
            LicenseProvider.GetLicenseActivator(null);
        }

        private class stubStandardLicenseService : ILicenseService
        {
            public stubStandardLicenseService(DateTime expirationDate, TimeSpan expirationGracePeriod,
																							double maxActiveAgentsGrace)
            {
                CustomerName = customerName;
                ExpirationDate = expirationDate;
                ExpirationGracePeriod = expirationGracePeriod;
                MaxActiveAgents = maxActiveAgents;
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
				TeleoptiCccVersion8Enabled = true;
            }

            #region Implementation of IDisposable

            public void Dispose()
            {
                GC.SuppressFinalize(this);
            }

            #endregion

            #region Implementation of ILicenseService

            public string CustomerName { get; private set; }
            public DateTime ExpirationDate { get; private set; }
            public TimeSpan ExpirationGracePeriod { get; private set; }
            public int MaxActiveAgents { get; private set; }
						public double MaxActiveAgentsGrace { get; private set; }

            public bool IsThisTooManyActiveAgents(int activeAgents)
            {
                throw new NotImplementedException();
            }

            public bool IsThisAlmostTooManyActiveAgents(int activeAgents)
            {
                throw new NotImplementedException();
            }

            public bool TeleoptiCccPilotCustomersBaseEnabled { get; private set; }
            public bool TeleoptiCccPilotCustomersForecastsEnabled { get; private set; }
            public bool TeleoptiCccPilotCustomersShiftsEnabled { get; private set; }
            public bool TeleoptiCccPilotCustomersPeopleEnabled { get; private set; }
            public bool TeleoptiCccPilotCustomersAgentPortalEnabled { get; private set; }
            public bool TeleoptiCccPilotCustomersOptionsEnabled { get; private set; }
            public bool TeleoptiCccPilotCustomersSchedulerEnabled { get; private set; }
            public bool TeleoptiCccPilotCustomersIntradayEnabled { get; private set; }
            public bool TeleoptiCccPilotCustomersPermissionsEnabled { get; private set; }
            public bool TeleoptiCccPilotCustomersReportsEnabled { get; private set; }
            public bool TeleoptiCccBaseEnabled { get; private set; }
            public bool TeleoptiCccDeveloperEnabled { get; private set; }
            public bool TeleoptiCccAgentSelfServiceEnabled { get; private set; }
            public bool TeleoptiCccShiftTradesEnabled { get; private set; }
            public bool TeleoptiCccAgentScheduleMessengerEnabled { get; private set; }
            public bool TeleoptiCccHolidayPlannerEnabled { get; private set; }
            public bool TeleoptiCccRealTimeAdherenceEnabled { get; private set; }
            public bool TeleoptiCccPerformanceManagerEnabled { get; private set; }
            public bool TeleoptiCccPayrollIntegrationEnabled { get; private set; }
            public bool TeleoptiCccMyTimeWebEnabled { get; private set; }
			public bool TeleoptiCccSmsLinkEnabled { get; private set; }
	        public bool TeleoptiCccCalendarLinkEnabled { get; private set; }
	        public bool TeleoptiCccVersion8Enabled { get; private set; }

        	public bool TeleoptiCccFreemiumForecastsEnabled { get; private set; }

        	public int MaxSeats
        	{
				get { return 10; }
        	}

        	public LicenseType LicenseType
        	{
        		get {return LicenseType.Agent; }
        	}

        	public decimal Ratio
        	{
        		get { throw new NotImplementedException(); }
        	}

        	#endregion
        }

        private class stubFreemiumLicenseService : ILicenseService
        {
            public stubFreemiumLicenseService(DateTime expirationDate, TimeSpan expirationGracePeriod,
																							double maxActiveAgentsGrace)
            {
                CustomerName = customerName;
                ExpirationDate = expirationDate;
                ExpirationGracePeriod = expirationGracePeriod;
                MaxActiveAgents = maxActiveAgents;
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

                TeleoptiCccBaseEnabled = false;
                TeleoptiCccDeveloperEnabled = false;
                TeleoptiCccAgentSelfServiceEnabled = false;
                TeleoptiCccShiftTradesEnabled = false;
                TeleoptiCccAgentScheduleMessengerEnabled = false;
                TeleoptiCccHolidayPlannerEnabled = false;
                TeleoptiCccRealTimeAdherenceEnabled = false;
                TeleoptiCccPerformanceManagerEnabled = false;
                TeleoptiCccPayrollIntegrationEnabled = false;
                TeleoptiCccMyTimeWebEnabled = false;
				TeleoptiCccSmsLinkEnabled = false;
	            TeleoptiCccCalendarLinkEnabled = false;
				TeleoptiCccVersion8Enabled = false;

                TeleoptiCccFreemiumForecastsEnabled = true;
            }

            #region Implementation of IDisposable

            public void Dispose()
            {
                GC.SuppressFinalize(this);
            }

            #endregion

            #region Implementation of ILicenseService

            public string CustomerName { get; private set; }
            public DateTime ExpirationDate { get; private set; }
            public TimeSpan ExpirationGracePeriod { get; private set; }
            public int MaxActiveAgents { get; private set; }
            public double MaxActiveAgentsGrace { get; private set; }

            public bool IsThisTooManyActiveAgents(int activeAgents)
            {
                throw new NotImplementedException();
            }

            public bool IsThisAlmostTooManyActiveAgents(int activeAgents)
            {
                throw new NotImplementedException();
            }

            public bool TeleoptiCccPilotCustomersBaseEnabled { get; private set; }
            public bool TeleoptiCccPilotCustomersForecastsEnabled { get; private set; }
            public bool TeleoptiCccPilotCustomersShiftsEnabled { get; private set; }
            public bool TeleoptiCccPilotCustomersPeopleEnabled { get; private set; }
            public bool TeleoptiCccPilotCustomersAgentPortalEnabled { get; private set; }
            public bool TeleoptiCccPilotCustomersOptionsEnabled { get; private set; }
            public bool TeleoptiCccPilotCustomersSchedulerEnabled { get; private set; }
            public bool TeleoptiCccPilotCustomersIntradayEnabled { get; private set; }
            public bool TeleoptiCccPilotCustomersPermissionsEnabled { get; private set; }
            public bool TeleoptiCccPilotCustomersReportsEnabled { get; private set; }
            public bool TeleoptiCccBaseEnabled { get; private set; }
            public bool TeleoptiCccDeveloperEnabled { get; private set; }
            public bool TeleoptiCccAgentSelfServiceEnabled { get; private set; }
            public bool TeleoptiCccShiftTradesEnabled { get; private set; }
            public bool TeleoptiCccAgentScheduleMessengerEnabled { get; private set; }
            public bool TeleoptiCccHolidayPlannerEnabled { get; private set; }
            public bool TeleoptiCccRealTimeAdherenceEnabled { get; private set; }
            public bool TeleoptiCccPerformanceManagerEnabled { get; private set; }
            public bool TeleoptiCccPayrollIntegrationEnabled { get; private set; }
			public bool TeleoptiCccSmsLinkEnabled { get; private set; }
	        public bool TeleoptiCccCalendarLinkEnabled { get; private set; }
	        public bool TeleoptiCccVersion8Enabled { get; private set; }

        	public bool TeleoptiCccFreemiumForecastsEnabled { get; private set; }

        	public int MaxSeats
        	{
        		get { return 10; }
        	}

        	public LicenseType LicenseType
        	{
        		get { return LicenseType.Agent; }
        	}

        	public decimal Ratio
        	{
        		get { throw new NotImplementedException(); }
        	}

        	public bool TeleoptiCccMyTimeWebEnabled { get; private set; }

            #endregion
        }

        private class stubPilotCustomersLicenseService : ILicenseService
        {
            public stubPilotCustomersLicenseService(DateTime expirationDate, TimeSpan expirationGracePeriod,
																										double maxActiveAgentsGrace)
            {
                CustomerName = customerName;
                ExpirationDate = expirationDate;
                ExpirationGracePeriod = expirationGracePeriod;
                MaxActiveAgents = maxActiveAgents;
                MaxActiveAgentsGrace = maxActiveAgentsGrace;

                TeleoptiCccPilotCustomersBaseEnabled = true;
                TeleoptiCccPilotCustomersForecastsEnabled = true;
                TeleoptiCccPilotCustomersShiftsEnabled = true;
                TeleoptiCccPilotCustomersPeopleEnabled = true;
                TeleoptiCccPilotCustomersAgentPortalEnabled = true;
                TeleoptiCccPilotCustomersOptionsEnabled = true;
                TeleoptiCccPilotCustomersSchedulerEnabled = true;
                TeleoptiCccPilotCustomersIntradayEnabled = true;
                TeleoptiCccPilotCustomersPermissionsEnabled = true;
                TeleoptiCccPilotCustomersReportsEnabled = true;

                TeleoptiCccBaseEnabled = false;
                TeleoptiCccDeveloperEnabled = false;
                TeleoptiCccAgentSelfServiceEnabled = false;
                TeleoptiCccShiftTradesEnabled = false;
                TeleoptiCccAgentScheduleMessengerEnabled = false;
                TeleoptiCccHolidayPlannerEnabled = false;
                TeleoptiCccRealTimeAdherenceEnabled = false;
                TeleoptiCccPerformanceManagerEnabled = false;
                TeleoptiCccPayrollIntegrationEnabled = false;
                TeleoptiCccMyTimeWebEnabled = false;
            	TeleoptiCccSmsLinkEnabled = false;
	            TeleoptiCccCalendarLinkEnabled = false;
            	TeleoptiCccVersion8Enabled = false;

                TeleoptiCccFreemiumForecastsEnabled = false;
            }

            #region Implementation of IDisposable

            public void Dispose()
            {
                GC.SuppressFinalize(this);
            }

            #endregion

            #region Implementation of ILicenseService

            public string CustomerName { get; private set; }
            public DateTime ExpirationDate { get; private set; }
            public TimeSpan ExpirationGracePeriod { get; private set; }
            public int MaxActiveAgents { get; private set; }
						public double MaxActiveAgentsGrace { get; private set; }

            public bool IsThisTooManyActiveAgents(int activeAgents)
            {
                throw new NotImplementedException();
            }

            public bool IsThisAlmostTooManyActiveAgents(int activeAgents)
            {
                throw new NotImplementedException();
            }

            public bool TeleoptiCccPilotCustomersBaseEnabled { get; private set; }
            public bool TeleoptiCccPilotCustomersForecastsEnabled { get; private set; }
            public bool TeleoptiCccPilotCustomersShiftsEnabled { get; private set; }
            public bool TeleoptiCccPilotCustomersPeopleEnabled { get; private set; }
            public bool TeleoptiCccPilotCustomersAgentPortalEnabled { get; private set; }
            public bool TeleoptiCccPilotCustomersOptionsEnabled { get; private set; }
            public bool TeleoptiCccPilotCustomersSchedulerEnabled { get; private set; }
            public bool TeleoptiCccPilotCustomersIntradayEnabled { get; private set; }
            public bool TeleoptiCccPilotCustomersPermissionsEnabled { get; private set; }
            public bool TeleoptiCccPilotCustomersReportsEnabled { get; private set; }
            public bool TeleoptiCccBaseEnabled { get; private set; }
            public bool TeleoptiCccDeveloperEnabled { get; private set; }
            public bool TeleoptiCccAgentSelfServiceEnabled { get; private set; }
            public bool TeleoptiCccShiftTradesEnabled { get; private set; }
            public bool TeleoptiCccAgentScheduleMessengerEnabled { get; private set; }
            public bool TeleoptiCccHolidayPlannerEnabled { get; private set; }
            public bool TeleoptiCccRealTimeAdherenceEnabled { get; private set; }
            public bool TeleoptiCccPerformanceManagerEnabled { get; private set; }
            public bool TeleoptiCccPayrollIntegrationEnabled { get; private set; }
			public bool TeleoptiCccSmsLinkEnabled { get; private set; }
	        public bool TeleoptiCccCalendarLinkEnabled { get; private set; }
	        public bool TeleoptiCccVersion8Enabled { get; private set; }

        	public bool TeleoptiCccFreemiumForecastsEnabled { get; private set; }

        	public int MaxSeats
        	{
        		get { return 10; }
        	}

        	public LicenseType LicenseType
        	{
				get { return LicenseType.Agent; }
        	}

        	public decimal Ratio
        	{
        		get { throw new NotImplementedException(); }
        	}

        	public bool TeleoptiCccMyTimeWebEnabled { get; private set; }

            #endregion
        }
    }
}