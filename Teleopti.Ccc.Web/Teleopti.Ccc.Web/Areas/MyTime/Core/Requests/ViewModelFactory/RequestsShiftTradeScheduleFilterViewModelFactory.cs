﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
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
			IList<TimeSpan> hours = new List<TimeSpan>();
			for (var i = 0; i <= 24; ++i)
			{
				hours.Add(TimeSpan.FromHours(i));
			}

			var baseDate = DateHelper.MinSmallDateTime;
			var ret = new RequestsShiftTradeScheduleFilterViewModel
			{
				DayOffShortNames = _dayOffTemplateRepository.FindAllDayOffsSortByDescription().Select(dayoff => dayoff.Description.ShortName).ToArray(),
				HourTexts = hours.Select(hour => baseDate.Add(hour).ToShortTimeString()).ToArray(),
				EmptyDayText = Resources.OptionEmptyDay,
				StartTimeSortOrders = new Dictionary<string, string> { {"Start" , Resources.StartTimeAsc}, {"Start DESC" , Resources.StartTimeDesc} },
				EndTimeSortOrders = new Dictionary<string, string> { { "End", Resources.EndTimeAsc }, { "End DESC", Resources.EndTimeDesc } }
			};

			return ret;
		}
	}
}
