using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	internal class ShiftTradePersonDayData
	{
		public string Name { get; set; }

		public IEnumerable<IVisualLayer> ScheduleLayers { get; set; }

		public string DayOffText { get; set; }

		public TimeZoneInfo PersonTimeZone { get; set; }

		public CultureInfo PersonCulture { get; set; }

		public SchedulePartView SignificantPartForDisplay { get; set; }

		public Guid PersonId { get; set; }
	}
}