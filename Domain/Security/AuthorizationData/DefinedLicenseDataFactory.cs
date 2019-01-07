using System.Collections.Concurrent;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.LicenseOptions;

namespace Teleopti.Ccc.Domain.Security.AuthorizationData
{
	public static class DefinedLicenseDataFactory
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(DefinedLicenseDataFactory));
		private static readonly ConcurrentDictionary<string, ILicenseActivator> _licenseActivators = new ConcurrentDictionary<string, ILicenseActivator>();

		public static ILicenseActivator GetLicenseActivator(string dataSource)
		{
			ILicenseActivator activator;
			_licenseActivators.TryGetValue(dataSource, out activator);
			if (activator == null)
			{
				logger.Warn($"activator for {dataSource} is null");
			}
			return activator;
		}

		public static void ClearLicenseActivators()
		{
			_licenseActivators.Clear();
		}

		public static void ClearLicenseActivators(string tenant)
		{
			_licenseActivators.TryRemove(tenant, out var _);
		}

		public static void SetLicenseActivator(string dataSource, ILicenseActivator licenseActivator)
		{
			_licenseActivators.AddOrUpdate(dataSource, licenseActivator, (s, a) => licenseActivator);
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

		public static LicenseSchema CreateActiveLicenseSchema(string dataSource)
		{
			var activeLicenseSchema = new LicenseSchema();
			activeLicenseSchema.ActivateLicense(GetLicenseActivator(dataSource));
			return activeLicenseSchema;
		}

		public static LicenseOption[] CreateDefinedLicenseOptions()
		{
			var licenseOptions = new LicenseOption[]
			{
				new TeleoptiCccBaseLicenseOption(),
				new TeleoptiCccLifestyleLicenseOption(),
				new TeleoptiCccShiftTraderLicenseOption(),
				new TeleoptiCccVacationPlannerLicenseOption(),
				new TeleoptiCccOvertimeAvailabilityLicenseOption(),
				new TeleoptiCccNotifyLicenseOption(),
				new TeleoptiCccAgentScheduleMessengerLicenseOption(),
				new TeleoptiCccRealTimeAdherenceLicenseOption(),
				new TeleoptiCccSmsLinkLicenseOption(),
				new TeleoptiCccCalendarLinkLicenseOption(),
				new TeleoptiCccPerformanceManagerLicenseOption(),
				new TeleoptiCccPayrollIntegrationLicenseOption(),
				new TeleoptiCccMyTeamLicenseOption(),
				new TeleoptiWfmVNextPilotLicenseOption(),
				new TeleoptiWfmOutboundLicenseOption(),
				new TeleoptiWfmSeatPlannerLicenseOption(),
				new TeleoptiCccFreemiumForecastsLicenseOption(),
				new TeleoptiWfmBpoExchangeLicenseOption(),
				new TeleoptiWfmOvertimeRequestsLicenseOption(),
				new TeleoptiWfmGrantLicenseOption(),
				new TeleoptiWfmInsightsLicenseOption(),
				new AllLicenseOption()
			};
			
			return licenseOptions;
		}
	}
}
