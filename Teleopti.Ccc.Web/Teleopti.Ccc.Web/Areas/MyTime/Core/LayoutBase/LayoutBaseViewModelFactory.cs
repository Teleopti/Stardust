using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
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
		private readonly IPermissionProvider _permissionProvider;

		public LayoutBaseViewModelFactory(ICultureSpecificViewModelFactory cultureSpecificViewModelFactory,
		                                  IDatePickerGlobalizationViewModelFactory datePickerGlobalizationViewModelFactory,
		                                  INow now, IUserTimeZone userTimeZone, IPermissionProvider permissionProvider)
		{
			_cultureSpecificViewModelFactory = cultureSpecificViewModelFactory;
			_datePickerGlobalizationViewModelFactory = datePickerGlobalizationViewModelFactory;
			_now = now;
			_userTimeZone = userTimeZone;
			_permissionProvider = permissionProvider;
		}
		
		public LayoutBaseViewModel CreateLayoutBaseViewModel(string title)
		{
			var cultureSpecificViewModel = _cultureSpecificViewModelFactory.CreateCutureSpecificViewModel();
			var datePickerGlobalizationViewModel =
				_datePickerGlobalizationViewModelFactory.CreateDatePickerGlobalizationViewModel();
			DateTime? fixedDate = null;

			if (_now is IMutateNow now && now.IsMutated())
				fixedDate = _now.UtcDateTime();

			var offset = _userTimeZone.TimeZone().BaseUtcOffset;
			var calendar = new GregorianCalendar();

			var dayLightSavingAdjustment = TimeZoneHelper.GetDaylightChanges(_userTimeZone.TimeZone(),calendar.GetYear(_now.ServerDateTime_DontUse()));

			var returnValue = new LayoutBaseViewModel
			{
				CultureSpecific = cultureSpecificViewModel,
				DatePickerGlobalization = datePickerGlobalizationViewModel,
				Footer = string.Empty,
				Title = title,
				FixedDate = fixedDate,
				UserTimezoneOffsetMinute = (int) offset.TotalMinutes,
				GrantEnabled = _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ChatBot)
			};
			
			if (dayLightSavingAdjustment != null)
			{
				returnValue.HasDayLightSaving = true;
				returnValue.DayLightSavingStart = TimeZoneHelper.ConvertToUtc(dayLightSavingAdjustment.Start,_userTimeZone.TimeZone()).ToGregorianDateTimeString();
				returnValue.DayLightSavingEnd = TimeZoneHelper.ConvertToUtc(dayLightSavingAdjustment.End.AddSeconds(-1),_userTimeZone.TimeZone()).ToGregorianDateTimeString();
				returnValue.DayLightSavingAdjustmentInMinute = (int) dayLightSavingAdjustment.Delta.TotalMinutes;
			}

			return returnValue;
		}
	}
}