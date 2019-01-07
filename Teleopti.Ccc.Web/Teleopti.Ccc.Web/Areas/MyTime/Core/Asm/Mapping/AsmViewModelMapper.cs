using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Asm;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Asm.Mapping
{
	public class AsmViewModelMapper : IAsmViewModelMapper
	{
		private readonly IProjectionProvider _projectionProvider;
		private readonly IUserTimeZone _userTimeZoneInfo;
		private readonly IUserCulture _userCulture;
		private readonly ILoggedOnUser _loggedOnUser;

		public AsmViewModelMapper(IProjectionProvider projectionProvider, IUserTimeZone userTimeZoneInfo, IUserCulture userCulture, ILoggedOnUser loggedOnUser)
		{
			_projectionProvider = projectionProvider;
			_userTimeZoneInfo = userTimeZoneInfo;
			_userCulture = userCulture;
			_loggedOnUser = loggedOnUser;
		}

		public AsmViewModel Map(DateTime asmZeroLocal, IEnumerable<IScheduleDay> scheduleDays, int unreadMessageCount)
		{
			var layers = scheduleDays.Select(scheduleDay => _projectionProvider.Projection(scheduleDay))
				.Where(proj => proj != null).SelectMany(proj => proj).ToList();
			var timeZone = _userTimeZoneInfo.TimeZone();
			var dstJudgement = new DSTJudgement();
			var culture = _userCulture.GetCulture();
			return new AsmViewModel
									{
										Hours = createHours(asmZeroLocal, timeZone, culture, dstJudgement),
										Layers = createAsmLayers(asmZeroLocal, timeZone, culture, layers, dstJudgement),
										UnreadMessageCount = unreadMessageCount,
										UserTimeZoneMinuteOffset = (asmZeroLocal - TimeZoneHelper.ConvertToUtc(asmZeroLocal, timeZone)).TotalMinutes
									};
		}
		
		private static IEnumerable<string> createHours(DateTime asmZero, TimeZoneInfo timeZone, CultureInfo culture, DSTJudgement dstJudgement)
		{
			const int numberOfHoursToShow = 24*3;
			var hoursAsInts = new List<string>();
			var asmZeroAsUtc = TimeZoneHelper.ConvertToUtc(asmZero, timeZone);
			var beginningIsDst =
				timeZone.IsDaylightSavingTime(TimeZoneInfo.ConvertTimeFromUtc(asmZeroAsUtc, timeZone));
			var endIsDst = timeZone.IsDaylightSavingTime(TimeZoneInfo.ConvertTimeFromUtc(asmZeroAsUtc.AddHours(71), timeZone));
			dstJudgement.IsContainsDSTStart = !beginningIsDst && endIsDst;
			dstJudgement.IsContainsDSTEnd = beginningIsDst && !endIsDst;

			for (var hour = 0; hour < numberOfHoursToShow; hour++)
			{
 				var localTime = TimeZoneInfo.ConvertTimeFromUtc(asmZeroAsUtc.AddHours(hour), timeZone);
				var hourString = localTime.ToString(culture.DateTimeFormat.ShortTimePattern, culture);

				if (dstJudgement.IsContainsDSTStart && DateTime.Compare(dstJudgement.DSTMarginPoint, new DateTime()) == 0)
				{
					if (timeZone.IsDaylightSavingTime(localTime))
					{
						dstJudgement.DSTMarginPoint = localTime;
					}
				}
				if (dstJudgement.IsContainsDSTEnd && DateTime.Compare(dstJudgement.DSTMarginPoint, new DateTime()) == 0)
				{
					if (!timeZone.IsDaylightSavingTime(localTime))
					{
						dstJudgement.DSTMarginPoint = localTime;
					}
				}

				const string regex = "(\\:.*\\ )";
				var output = Regex.Replace(hourString, regex, " ");
				if (output.Contains(":"))
					output = localTime.Hour.ToString();
				hoursAsInts.Add(output);
 			}
			return hoursAsInts;
		}

		private IEnumerable<AsmLayer> createAsmLayers(DateTime asmZero, TimeZoneInfo timeZone, CultureInfo culture, IEnumerable<IVisualLayer> layers, DSTJudgement dstJudgement)
		{
			var currentUser = _loggedOnUser.CurrentUser();
			var asmLayers = (from visualLayer in layers
			                 let startDate = visualLayer.Period.StartDateTimeLocal(timeZone)
			                 let endDate = visualLayer.Period.EndDateTimeLocal(timeZone)
			                 let length = visualLayer.Period.ElapsedTime().TotalMinutes
			                 select new AsmLayer
			                        	{														    
			                        		Payload = visualLayer.Payload.ConfidentialDescription(currentUser).Name,
											StartMinutesSinceAsmZero = getStartMinutesSinceAsmZero(dstJudgement, startDate,asmZero),
			                        		LengthInMinutes = length,
			                        		Color = ColorTranslator.ToHtml(visualLayer.Payload.ConfidentialDisplayColor(currentUser)),
											StartTimeText = startDate.ToString(culture.DateTimeFormat.ShortTimePattern, culture),
											EndTimeText = endDate.ToString(culture.DateTimeFormat.ShortTimePattern, culture)
							 }).ToList();
			return asmLayers;
		}

		private double getStartMinutesSinceAsmZero(DSTJudgement dstJudgement, DateTime layerStartTime, DateTime asmZero)
		{
			var compare = DateTime.Compare(dstJudgement.DSTMarginPoint, layerStartTime);
			if ((compare == -1 ||
			     compare == 0))
			{
				if (dstJudgement.IsContainsDSTStart)
					return layerStartTime.Subtract(asmZero).TotalMinutes - 60;
				if (dstJudgement.IsContainsDSTEnd)
					return layerStartTime.Subtract(asmZero).TotalMinutes + 60;
			}
			return layerStartTime.Subtract(asmZero).TotalMinutes;
		}
	}

	public class DSTJudgement
	{
		public bool IsContainsDSTStart { get; set; }
		public bool IsContainsDSTEnd { get; set; }
		public DateTime DSTMarginPoint { get; set; }
	};
}