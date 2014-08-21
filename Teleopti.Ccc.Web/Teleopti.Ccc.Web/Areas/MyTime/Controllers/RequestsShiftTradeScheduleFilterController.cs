﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[RequestPermission]
	public class RequestsShiftTradeScheduleFilterController : Controller
    {
	    private readonly IRequestsShiftTradeScheduleFilterViewModelFactory _viewModelFactory;

	    public RequestsShiftTradeScheduleFilterController(IRequestsShiftTradeScheduleFilterViewModelFactory viewModelFactory)
	    {
		    _viewModelFactory = viewModelFactory;
	    }

		 [UnitOfWorkAction]
		 [HttpGet]
		 public JsonResult Get()
		 {
			 return Json(_viewModelFactory.ViewModel(), JsonRequestBehavior.AllowGet);
		 }

    }

	public interface IRequestsShiftTradeScheduleFilterViewModelFactory
	{
		RequestsShiftTradeScheduleFilterViewModel ViewModel();
	}

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

	public class RequestsShiftTradeScheduleFilterViewModel
	{
		public IEnumerable<string> DayOffShortNames { get; set; }
		public IEnumerable<string> HourTexts { get; set; } 
	}
}
