using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Analytics.Etl.Interfaces.Transformer
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
		DateTime? NowForTestPurpose { get; set; }
    }
}