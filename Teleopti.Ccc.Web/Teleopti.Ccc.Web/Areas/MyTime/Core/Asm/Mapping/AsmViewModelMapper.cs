using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Asm;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.Mapping
{
	public class AsmViewModelMapper : IAsmViewModelMapper
	{
		private readonly IProjectionProvider _projectionProvider;
		private readonly IUserTimeZone _userTimeZoneInfo;
		private readonly ILoggedOnUser _loggedOnUser;

		public AsmViewModelMapper(IProjectionProvider projectionProvider, IUserTimeZone userTimeZoneInfo, ILoggedOnUser loggedOnUser)
		{
			_projectionProvider = projectionProvider;
			_userTimeZoneInfo = userTimeZoneInfo;
			_loggedOnUser = loggedOnUser;
		}

		public AsmViewModel Map(DateTime asmZeroLocal, IEnumerable<IScheduleDay> scheduleDays, int unreadMessageCount)
		{
			var layers = new List<IVisualLayer>();
			foreach (var proj in scheduleDays.Select(scheduleDay => _projectionProvider.Projection(scheduleDay)).Where(proj => proj != null))
			{
				layers.AddRange(proj);
			}
			var timeZone = _userTimeZoneInfo.TimeZone();
			var culture = _loggedOnUser.CurrentUser().PermissionInformation.Culture();
			return new AsmViewModel
			          	{
			          		Layers = createAsmLayers(asmZeroLocal, timeZone, culture, layers),
								Hours = createHours(asmZeroLocal, timeZone, culture),
								UnreadMessageCount = unreadMessageCount
			          	};
		}
		
		private static IEnumerable<string> createHours(DateTime asmZero, TimeZoneInfo timeZone, CultureInfo culture)
		{
			const int numberOfHoursToShow = 24*3;
			var hoursAsInts = new List<string>();
			var asmZeroAsUtc = TimeZoneHelper.ConvertToUtc(asmZero, timeZone);
			
			for (var hour = 0; hour < numberOfHoursToShow; hour++)
			{
 				var localTime = TimeZoneInfo.ConvertTimeFromUtc(asmZeroAsUtc.AddHours(hour), timeZone);
				var hourString = string.Format(culture, localTime.ToShortTimeString());
 
				const string regex = "(\\:.*\\ )";
				var output = Regex.Replace(hourString, regex, " ");
				if (output.Contains(":"))
					output = localTime.Hour.ToString();
				hoursAsInts.Add(output);
 			}
			return hoursAsInts;
		}

		private IEnumerable<AsmLayer> createAsmLayers(DateTime asmZero, TimeZoneInfo timeZone, CultureInfo culture, IEnumerable<IVisualLayer> layers)
		{
			var asmLayers = (from visualLayer in layers
			                 let startDate = TimeZoneHelper.ConvertFromUtc(visualLayer.Period.StartDateTime, timeZone)
			                 let endDate = TimeZoneHelper.ConvertFromUtc(visualLayer.Period.EndDateTime, timeZone)
			                 let length = visualLayer.Period.ElapsedTime().TotalMinutes
			                 select new AsmLayer
			                        	{
			                        		Payload = visualLayer.DisplayDescription().Name,
													StartMinutesSinceAsmZero = startDate.Subtract(asmZero).TotalMinutes,
			                        		LengthInMinutes = length,
			                        		Color = ColorTranslator.ToHtml(visualLayer.DisplayColor()),
			                        		StartTimeText = string.Format(culture, startDate.ToShortTimeString()),
													EndTimeText = string.Format(culture, endDate.ToShortTimeString())
 			                        	}).ToList();
			return asmLayers;
		}
	}
}