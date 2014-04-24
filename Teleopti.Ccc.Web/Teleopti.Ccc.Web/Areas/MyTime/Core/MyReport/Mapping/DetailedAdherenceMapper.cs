using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Web.UI.WebControls.Expressions;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.WebReports;
using Teleopti.Ccc.Web.Areas.MyTime.Models.MyReport;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.Mapping
{
	public class DetailedAdherenceMapper : IDetailedAdherenceMapper
	{
		private readonly IUserCulture _userCulture;

		public DetailedAdherenceMapper(IUserCulture userCulture)
		{
			_userCulture = userCulture;
		}

		public DetailedAdherenceViewModel Map(ICollection<DetailedAdherenceForDayResult> dataModels)
		{
			var culture = _userCulture == null ? CultureInfo.InvariantCulture : _userCulture.GetCulture();

			if (dataModels == null || dataModels.IsEmpty())
			{
				return new DetailedAdherenceViewModel {DataAvailable = false};
			}
			var intervalsPerDay = dataModels.First().IntervalsPerDay;

			var intervals = setWholeHoursInStartAndEnd(dataModels, intervalsPerDay);

			var viewModel = new DetailedAdherenceViewModel
			{
				ShiftDate = dataModels.First().ShiftDate.ToShortDateString(culture),
				TotalAdherence = dataModels.First().TotalAdherence.ValueAsPercent().ToString(culture),
				IntervalsPerDay = intervalsPerDay,
				DataAvailable = true,
				Intervals = intervals
			};

			return viewModel;
		}

		private List<AdherenceIntervalViewModel> setWholeHoursInStartAndEnd(
			ICollection<DetailedAdherenceForDayResult> dataModels, int intervalsPerDay)
		{
			var startIdForSchedule = dataModels.First().IntervalId;
			var intervalsPerHour = intervalsPerDay/24;
			var startId = startIdForSchedule - startIdForSchedule%intervalsPerHour;
			var endIdForSchedule = dataModels.Last().IntervalId;
			var endId = endIdForSchedule + intervalsPerHour - endIdForSchedule%intervalsPerHour;

			var result = dataModels.Select(model => new AdherenceIntervalViewModel
			{
				IntervalId = model.IntervalId,
				Adherence = model.Adherence,
				IntervalCounter = model.IntervalCounter,
				Deviation = model.Deviation,
				Name = model.DisplayName,
				Color = model.DisplayColor.ToHtml()
			}).ToList();
			if (startId != startIdForSchedule)
				result.Insert(0, new AdherenceIntervalViewModel
				{
					IntervalId = startId
				});
			if (endId != endIdForSchedule)
				result.Add(new AdherenceIntervalViewModel
				{
					IntervalId = endId
				});
			return result;
		}
	}
}