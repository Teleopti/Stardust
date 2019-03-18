using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Util;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Wfm.Adherence.Historical.Adjustments;

namespace Teleopti.Wfm.Adherence.Historical
{
	public class AdjustmentsViewModelBuilder
	{
		private readonly IUserTimeZone _timezone;
		private readonly AdjustmentsLoader _loader;

		public AdjustmentsViewModelBuilder(IUserTimeZone timezone, AdjustmentsLoader loader)
		{
			_timezone = timezone;
			_loader = loader;
		}

		public IEnumerable<AdjustmentViewModel> Build() => _loader.Load()
			.AdjustedPeriods()
			.Select(e => new AdjustmentViewModel
				{
					StartTime = formatForUser(e.StartTime),
					EndTime = formatForUser(e.EndTime)
				}
			).ToArray();

		private string formatForUser(DateTime? time) =>
			TimeZoneInfo.ConvertTimeFromUtc(time.Value, _timezone.TimeZone()).ToString("yyyy-MM-dd HH\\:mm");
	}
}