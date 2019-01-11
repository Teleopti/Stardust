using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Transformer
{
	public interface IJobParameters
	{
		int DataSource { get; set; }
		IJobHelper Helper { get; set; }
		TimeZoneInfo DefaultTimeZone { get; }
		int IntervalsPerDay { get; }
		ICommonStateHolder StateHolder { get; set; }
		IJobMultipleDate JobCategoryDates { get; }
		string OlapServer { get; }
		string OlapDatabase { get; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		IList<TimeZoneInfo> TimeZonesUsedByDataSources { get; set; }
		bool IsPmInstalled { get; }
		CultureInfo CurrentCulture { get; }
		IToggleManager ToggleManager { get; }
		ITenantLogonInfoLoader TenantLogonInfoLoader { get; }
		DateTime? NowForTestPurpose { get; set; }
		bool RunIndexMaintenance { get; }
		bool InsightsEnabled { get; }
		InsightsConfiguration InsightsConfig { get; }

		void SetTenantBaseConfigValues(IBaseConfiguration baseConfiguration);
		IContainerHolder ContainerHolder { get; set; }
	}
}