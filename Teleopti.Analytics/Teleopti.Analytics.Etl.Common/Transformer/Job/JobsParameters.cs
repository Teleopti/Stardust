using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.MultipleDate;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job
{
	public class JobParameters : IJobParameters
	{
		public JobParameters(
			IJobMultipleDate jobCategoryDates, int dataSource, string timeZone,
			int intervalLengthMinutes, string cubeConnectionString,
			string pmInstall, CultureInfo currentCulture,
			IContainerHolder containerHolder, bool runIndexMaintenance
			)
		{
			DataSource = dataSource;
			CurrentCulture = currentCulture;
			DefaultTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
			IntervalsPerDay = 1440 / intervalLengthMinutes;
			StateHolder = new CommonStateHolder(this);
			JobCategoryDates = jobCategoryDates ?? new JobMultipleDate(DefaultTimeZone);
			setOlapServerAndDatabase(cubeConnectionString);
			IsPmInstalled = checkPmInstall(pmInstall);

			ToggleManager = containerHolder.ToggleManager;
			TenantLogonInfoLoader = containerHolder.TenantLogonInfoLoader;


			RunIndexMaintenance = runIndexMaintenance;
		}

		public int DataSource { get; set; }

		public void SetTenantBaseConfigValues(IBaseConfiguration baseConfiguration)
		{
			DefaultTimeZone = TimeZoneInfo.FindSystemTimeZoneById(baseConfiguration.TimeZoneCode);
			IntervalsPerDay = 1440 / baseConfiguration.IntervalLength.Value;
			RunIndexMaintenance = baseConfiguration.RunIndexMaintenance;
			CurrentCulture = CultureInfo.GetCultureInfo(baseConfiguration.CultureId.Value).FixPersianCulture();
		}
		public IJobHelper Helper { get; set; }

		public TimeZoneInfo DefaultTimeZone { get; private set; }

		public int IntervalsPerDay { get; private set; }

		public ICommonStateHolder StateHolder { get; set; }

		public IJobMultipleDate JobCategoryDates { get; private set; }

		public string OlapServer { get; private set; }

		public string OlapDatabase { get; private set; }

		public DateTime? NowForTestPurpose { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		public IList<TimeZoneInfo> TimeZonesUsedByDataSources { get; set; }

		public bool IsPmInstalled { get; private set; }

		public CultureInfo CurrentCulture { get; private set; }

		public IToggleManager ToggleManager { get; private set; }

		public ITenantLogonInfoLoader TenantLogonInfoLoader { get; private set; }

		public bool RunIndexMaintenance { get; private set; }

		private void setOlapServerAndDatabase(string cubeConnectionsString)
		{
			if (!string.IsNullOrEmpty(cubeConnectionsString))
			{
				string[] splittedString1 = cubeConnectionsString.Split(";".ToCharArray());
				foreach (string stringPart in splittedString1)
				{
					string[] splittedString2 = stringPart.Split("=".ToCharArray());
					if ("DATA SOURCE".Equals(splittedString2[0], StringComparison.InvariantCultureIgnoreCase))
					{
						OlapServer = splittedString2[1];
					}
					if ("INITIAL CATALOG".Equals(splittedString2[0], StringComparison.InvariantCultureIgnoreCase))
					{
						OlapDatabase = splittedString2[1];
					}
				}
			}
		}

		private static bool checkPmInstall(string flag)
		{
			return !string.IsNullOrEmpty(flag) && "TRUE".Equals(flag, StringComparison.InvariantCultureIgnoreCase);
		}
	}
}