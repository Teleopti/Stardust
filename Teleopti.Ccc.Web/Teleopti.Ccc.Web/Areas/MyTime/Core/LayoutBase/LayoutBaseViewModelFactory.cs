﻿using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Web.Areas.MyTime.Models.LayoutBase;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.LayoutBase
{
	public class LayoutBaseViewModelFactory : ILayoutBaseViewModelFactory
	{
		private readonly ICultureSpecificViewModelFactory _cultureSpecificViewModelFactory;
		private readonly IDatePickerGlobalizationViewModelFactory _datePickerGlobalizationViewModelFactory;
		private readonly INow _now;
		private readonly IUserTimeZone _userTimeZone;

		public LayoutBaseViewModelFactory(ICultureSpecificViewModelFactory cultureSpecificViewModelFactory,
		                                  IDatePickerGlobalizationViewModelFactory datePickerGlobalizationViewModelFactory,
		                                  INow now, IUserTimeZone userTimeZone)
		{
			_cultureSpecificViewModelFactory = cultureSpecificViewModelFactory;
			_datePickerGlobalizationViewModelFactory = datePickerGlobalizationViewModelFactory;
			_now = now;
			_userTimeZone = userTimeZone;
		}

		// TODO: JonasN, Texts
		public LayoutBaseViewModel CreateLayoutBaseViewModel(string title)
		{
			var cultureSpecificViewModel = _cultureSpecificViewModelFactory.CreateCutureSpecificViewModel();
			var datePickerGlobalizationViewModel =
				_datePickerGlobalizationViewModelFactory.CreateDatePickerGlobalizationViewModel();
			DateTime? fixedDate = null;

			if (_now is IMutateNow && (_now as IMutateNow).IsMutated())
				fixedDate = _now.UtcDateTime();

			var offset = _userTimeZone.TimeZone().BaseUtcOffset;
			var dayLightSavingAdjustment = TimeZoneHelper.GetDaylightChanges(_userTimeZone.TimeZone(), _now.LocalDateTime().Year);

			var returnValue = new LayoutBaseViewModel
			{
				CultureSpecific = cultureSpecificViewModel,
				DatePickerGlobalization = datePickerGlobalizationViewModel,
				Footer = string.Empty,
				Title = title,
				FixedDate = fixedDate,
				UserTimezoneOffsetMinute = (int) offset.TotalMinutes

			};

			if (dayLightSavingAdjustment != null)
			{
				returnValue.HasDayLightSaving = true;
				returnValue.DayLightSavingStart = string.Format("{0:yyyy-MM-dd HH:mm:ss}", dayLightSavingAdjustment.Start);
				returnValue.DayLightSavingEnd = string.Format("{0:yyyy-MM-dd HH:mm:ss}", dayLightSavingAdjustment.End);
				returnValue.DayLightSavingAdjustmentInMinute = (int) dayLightSavingAdjustment.Delta.TotalMinutes;
			}

			return returnValue;
		}
	}
}