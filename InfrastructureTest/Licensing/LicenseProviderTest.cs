using System;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Secrets.Licensing;



namespace Teleopti.Ccc.InfrastructureTest.Licensing
{
    [TestFixture]
    [Category("BucketB")]
    public class LicenseProviderTest
    {
	    private const string customerName = "Kundnamn";
        private readonly DateTime _expirationDate = DateTime.Now.AddDays(100);
        private readonly TimeSpan _expirationGracePeriod = new TimeSpan(30, 0, 0, 0);
        private const int maxActiveAgents = 120;
        private readonly Percent _maxActiveAgentsGrace = new Percent(0.2);

        [Test]
        public void CanGetLicenseActivator()
        {
            using (
                ILicenseService licenseService = new stubStandardLicenseService(_expirationDate, _expirationGracePeriod,
                                                                                _maxActiveAgentsGrace.Value))
            {
                ILicenseActivator licenseActivator = LicenseProvider.GetLicenseActivator(licenseService);
             
				Assert.IsTrue(
                    licenseActivator.EnabledLicenseOptionPaths.Contains(DefinedLicenseOptionPaths.TeleoptiCccBase));
                Assert.IsTrue(
                    licenseActivator.EnabledLicenseOptionPaths.Contains(
                        DefinedLicenseOptionPaths.TeleoptiCccVacationPlanner));
                Assert.IsTrue(
                    licenseActivator.EnabledLicenseOptionPaths.Contains(
                        DefinedLicenseOptionPaths.TeleoptiCccShiftTrader));
                Assert.IsTrue(
                    licenseActivator.EnabledLicenseOptionPaths.Contains(
                        DefinedLicenseOptionPaths.TeleoptiCccMyTeam));
                Assert.IsTrue(
                    licenseActivator.EnabledLicenseOptionPaths.Contains(
                        DefinedLicenseOptionPaths.TeleoptiCccLifestyle));
                Assert.IsTrue(
                    licenseActivator.EnabledLicenseOptionPaths.Contains(
                        DefinedLicenseOptionPaths.TeleoptiCccNotify));
                Assert.IsTrue(
                    licenseActivator.EnabledLicenseOptionPaths.Contains(
                        DefinedLicenseOptionPaths.TeleoptiCccOvertimeAvailability));
                Assert.IsFalse(
                    licenseActivator.EnabledLicenseOptionPaths.Contains(
                        DefinedLicenseOptionPaths.TeleoptiCccFreemiumForecasts));
                Assert.AreEqual(DefinedLicenseSchemaCodes.TeleoptiWFMSchema, licenseActivator.EnabledLicenseSchemaName);
            }
        }

		  [Test]
		  public void CanGetLicenseActivatorForWFM()
		  {
			  using (ILicenseService licenseService = new stubWFMLicenseService(_expirationDate, _expirationGracePeriod, _maxActiveAgentsGrace.Value))
			  {
				  ILicenseActivator licenseActivator = LicenseProvider.GetLicenseActivator(licenseService);

				  Assert.IsTrue(licenseActivator.EnabledLicenseOptionPaths.Contains(DefinedLicenseOptionPaths.TeleoptiWfmOutbound));
				  Assert.IsTrue(licenseActivator.EnabledLicenseOptionPaths.Contains(DefinedLicenseOptionPaths.TeleoptiWfmSeatPlanner));
			  }
		  }

        [Test]
        public void CanGetLicenseActivatorForFreemium()
        {
            using (
                ILicenseService licenseService = new stubFreemiumLicenseService(_expirationDate, _expirationGracePeriod,
                                                                                _maxActiveAgentsGrace.Value))
            {
                ILicenseActivator licenseActivator = LicenseProvider.GetLicenseActivator(licenseService);
                
				Assert.IsFalse(
                    licenseActivator.EnabledLicenseOptionPaths.Contains(DefinedLicenseOptionPaths.TeleoptiCccBase));
                Assert.IsFalse(
                    licenseActivator.EnabledLicenseOptionPaths.Contains(
                        DefinedLicenseOptionPaths.TeleoptiCccVacationPlanner));
                Assert.IsTrue(
                    licenseActivator.EnabledLicenseOptionPaths.Contains(
                        DefinedLicenseOptionPaths.TeleoptiCccFreemiumForecasts));
                Assert.AreEqual(DefinedLicenseSchemaCodes.TeleoptiWFMForecastsSchema,
                                licenseActivator.EnabledLicenseSchemaName);
            }
        }
		
        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
        public void VerifyMissingArgumentsProvideLicenseActivator()
        {
	        Assert.Throws<ArgumentNullException>(() => LicenseProvider.ProvideLicenseActivator("", null));
        }

        [Test]
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public void VerifyMissingArgumentsLicense()
        {
			Assert.Throws<ArgumentNullException>(() => LicenseProvider.GetLicenseActivator(null));
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
				TeleoptiWFMInsightsEnabled = true;
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
	        public bool Perpetual { get; private set; }
	        public int MaxActiveAgents { get; private set; }
						public double MaxActiveAgentsGrace { get; private set; }
	        public double MajorVersion { get; private set; }

	        public bool IsThisTooManyActiveAgents(int activeAgents)
            {
                throw new NotImplementedException();
            }

            public bool IsThisAlmostTooManyActiveAgents(int activeAgents)
            {
                throw new NotImplementedException();
            }

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
			public bool TeleoptiWFMOvertimeRequestsEnabled { get; }
			public bool TeleoptiWFMGrantEnabled { get; }
			public bool TeleoptiWFMInsightsEnabled { get; }

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

	        public bool TeleoptiWFMVacationPlannerEnabled { get; private set; }
	        public bool TeleoptiWFMShiftTraderEnabled { get; private set; }
	        public bool TeleoptiWFMLifestyleEnabled { get; private set; }
	        public bool TeleoptiWFMOvertimeAvailabilityEnabled { get; private set; }
	        public bool TeleoptiWFMNotifyEnabled { get; private set; }
	        public bool TeleoptiWFMVNextEnabled { get; private set; }
	        public bool TeleoptiWFMMyTeamEnabled { get; private set; }
	        public bool TeleoptiWFMOutboundEnabled { get; private set; }
	        public bool TeleoptiWFMSeatPlannerEnabled { get; private set; }
	        public bool TeleoptiWFMBPOExchangeEnabled { get; private set; }

	        #endregion
        }

		  private class stubWFMLicenseService : ILicenseService
		  {
			  public stubWFMLicenseService(DateTime expirationDate, TimeSpan expirationGracePeriod,
				  double maxActiveAgentsGrace)
			  {
				  CustomerName = customerName;
				  ExpirationDate = expirationDate;
				  ExpirationGracePeriod = expirationGracePeriod;
				  MaxActiveAgents = maxActiveAgents;
				  MaxActiveAgentsGrace = maxActiveAgentsGrace;

				  TeleoptiWFMOutboundEnabled = true;
				  TeleoptiWFMSeatPlannerEnabled = true;
				  TeleoptiWFMBPOExchangeEnabled = true;
			  }

			  #region Implementation of IDisposable
			  public void Dispose()
			  {
				  GC.SuppressFinalize(this);
			  }
			  #endregion

			  #region Implementation of ILicenseService
			  public bool IsThisTooManyActiveAgents(int activeAgents)
			  {
				  throw new NotImplementedException();
			  }

			  public bool IsThisAlmostTooManyActiveAgents(int activeAgents)
			  {
				  throw new NotImplementedException();
			  }

			  public string CustomerName { get; private set; }
			  public DateTime ExpirationDate { get; private set; }
			  public TimeSpan ExpirationGracePeriod { get; private set; }
			  public bool Perpetual { get; private set; }
			  public int MaxActiveAgents { get; private set; }
			  public double MaxActiveAgentsGrace { get; private set; }
			  public double MajorVersion { get; private set; }
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
			  public bool TeleoptiWFMOvertimeRequestsEnabled { get; }
			  public bool TeleoptiWFMGrantEnabled { get; }
			  public bool TeleoptiWFMInsightsEnabled { get; }
			  public bool TeleoptiCccFreemiumForecastsEnabled { get; private set; }
			  public int MaxSeats { get; private set; }
			  public LicenseType LicenseType { get; private set; }
			  public decimal Ratio { get; private set; }
			  public bool TeleoptiWFMVacationPlannerEnabled { get; private set; }
			  public bool TeleoptiWFMShiftTraderEnabled { get; private set; }
			  public bool TeleoptiWFMLifestyleEnabled { get; private set; }
			  public bool TeleoptiWFMOvertimeAvailabilityEnabled { get; private set; }
			  public bool TeleoptiWFMNotifyEnabled { get; private set; }
			  public bool TeleoptiWFMVNextEnabled { get; private set; }
			  public bool TeleoptiWFMMyTeamEnabled { get; private set; }
			  public bool TeleoptiWFMOutboundEnabled { get; private set; }
			  public bool TeleoptiWFMSeatPlannerEnabled { get; private set; }
			  public bool TeleoptiWFMBPOExchangeEnabled { get; private set; }

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
				TeleoptiWFMVNextEnabled = false;

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
	        public bool Perpetual { get; private set; }
	        public int MaxActiveAgents { get; private set; }
            public double MaxActiveAgentsGrace { get; private set; }
	        public double MajorVersion { get; private set; }

	        public bool IsThisTooManyActiveAgents(int activeAgents)
            {
                throw new NotImplementedException();
            }

            public bool IsThisAlmostTooManyActiveAgents(int activeAgents)
            {
                throw new NotImplementedException();
            }
			
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
			public bool TeleoptiWFMOvertimeRequestsEnabled { get; }
			public bool TeleoptiWFMGrantEnabled { get; }
			public bool TeleoptiWFMInsightsEnabled { get; }

			public bool TeleoptiCccFreemiumForecastsEnabled { get; private set; }

        	public int MaxSeats { get; } = 10;

			public LicenseType LicenseType { get; } = LicenseType.Agent;

			public decimal Ratio => throw new NotImplementedException();

			public bool TeleoptiWFMVacationPlannerEnabled { get; private set; }
	        public bool TeleoptiWFMShiftTraderEnabled { get; private set; }
	        public bool TeleoptiWFMLifestyleEnabled { get; private set; }
	        public bool TeleoptiWFMOvertimeAvailabilityEnabled { get; private set; }
	        public bool TeleoptiWFMNotifyEnabled { get; private set; }
	        public bool TeleoptiWFMVNextEnabled { get; private set; }
	        public bool TeleoptiWFMMyTeamEnabled { get; private set; }
	        public bool TeleoptiWFMOutboundEnabled { get; private set; }
	        public bool TeleoptiWFMSeatPlannerEnabled { get; private set; }

	        public bool TeleoptiCccMyTimeWebEnabled { get; private set; }
	        public bool TeleoptiWFMBPOExchangeEnabled { get; private set; }

	        #endregion
        }
    }
}