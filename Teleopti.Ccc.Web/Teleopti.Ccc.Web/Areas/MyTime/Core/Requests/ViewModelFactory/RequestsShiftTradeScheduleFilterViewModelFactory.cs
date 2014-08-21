using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory
{
	public class RequestsShiftTradeScheduleFilterViewModelFactory : IRequestsShiftTradeScheduleFilterViewModelFactory
	{
		private readonly IDayOffTemplateRepository _dayOffTemplateRepository;
		private readonly ICreateHourText _createHourText;
		private readonly IUserTimeZone _userTimeZone;

		public RequestsShiftTradeScheduleFilterViewModelFactory(IDayOffTemplateRepository dayOffTemplateRepository,
																			ICreateHourText createHourText, IUserTimeZone timeZone)
		{
			_dayOffTemplateRepository = dayOffTemplateRepository;
			_createHourText = createHourText;
			_userTimeZone = timeZone;
		}

		public RequestsShiftTradeScheduleFilterViewModel ViewModel()
		{
			IList<DateTime> hours = new List<DateTime>();
			for (var i = 0; i <= 24; ++i)
			{
				hours.Add(TimeZoneHelper.ConvertToUtc(DateTime.Today, _userTimeZone.TimeZone()).AddHours(i));
			}
			var ret = new RequestsShiftTradeScheduleFilterViewModel
			{
				DayOffShortNames =
					from dayoff in _dayOffTemplateRepository.FindAllDayOffsSortByDescription()
					select dayoff.Description.ShortName,
				HourTexts = from hour in hours
								select _createHourText.CreateText(hour)
			};

			return ret;
		}
	}
}
