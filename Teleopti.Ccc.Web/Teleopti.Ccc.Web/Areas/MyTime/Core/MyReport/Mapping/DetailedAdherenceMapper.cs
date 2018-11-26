using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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


			var viewModel = new DetailedAdherenceViewModel
			{
				ShiftDate = dataModels.First().ShiftDate.ToShortDateString(culture),
				TotalAdherence = dataModels.First().TotalAdherence.ValueAsPercent().ToString(culture),
				IntervalsPerDay = intervalsPerDay,
				DataAvailable = true,
				Intervals = dataModels.Select(model => new AdherenceIntervalViewModel
				{
					IntervalId = model.IntervalId,
					Adherence = model.Adherence,
					IntervalCounter = model.IntervalCounter,
					Deviation = model.Deviation,
					Name = model.DisplayName,
					Color = model.DisplayColor.ToHtml()
				}).ToList()
			};

			return viewModel;
		}
	}
}