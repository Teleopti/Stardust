using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeRequestsScheduleMappingProfile : Profile
	{
		private readonly Func<IShiftTradeRequestProvider> _shiftTradeRequestProvider;
		private readonly Func<ILoggedOnUser> _loggedOnUser;
		private readonly Func<IProjectionProvider> _projectionProvider;

		public ShiftTradeRequestsScheduleMappingProfile(Func<IShiftTradeRequestProvider> shiftTradeRequestProvider, Func<ILoggedOnUser> loggedOnUser, Func<IProjectionProvider> projectionProvider)
		{
		    _shiftTradeRequestProvider = shiftTradeRequestProvider;
		    _loggedOnUser = loggedOnUser;
			_projectionProvider = projectionProvider;
		}

		protected override void Configure()
		{
		    base.Configure();


		    CreateMap<DateOnly, ShiftTradeRequestsScheduleViewModel>()
				.ForMember(d => d.MyScheduleLayers, o => o.MapFrom(s =>
				                                                   	{
				                                                   		var scheduleDay = _shiftTradeRequestProvider.Invoke().RetrieveUserScheduledDay(s);
				                                                   		return
				                                                   			CreateShiftTradeLayers(
				                                                   				_projectionProvider.Invoke().Projection(scheduleDay),
				                                                   				_loggedOnUser.Invoke().CurrentUser().PermissionInformation.
				                                                   					DefaultTimeZone());
				                                                   	}))
		    ;
		}

		private static IEnumerable<ShiftTradeScheduleLayer> CreateShiftTradeLayers(IEnumerable<IVisualLayer> layers, TimeZoneInfo timeZone)
		{
			if (!layers.Any())
				return new ShiftTradeScheduleLayer[] { };

			DateTime shiftStartTime = layers.Min(o => o.Period.StartDateTime);



			var scheduleLayers = (from visualLayer in layers
								  let startDate = TimeZoneHelper.ConvertFromUtc(visualLayer.Period.StartDateTime, timeZone)
								  let endDate = TimeZoneHelper.ConvertFromUtc(visualLayer.Period.EndDateTime, timeZone)
								  let length = visualLayer.Period.ElapsedTime().TotalMinutes
								  select new ShiftTradeScheduleLayer
								  {
									  Payload = visualLayer.DisplayDescription().Name,
									  LengthInMinutes = (int)length,
									  Color = ColorTranslator.ToHtml(visualLayer.DisplayColor()),
									  StartTimeText = startDate.ToString("HH:mm"),
									  EndTimeText = endDate.ToString("HH:mm"),
									  ElapsedMinutesSinceShiftStart = (int)startDate.Subtract(TimeZoneHelper.ConvertFromUtc(shiftStartTime, timeZone)).TotalMinutes
								  }).ToList();
			return scheduleLayers;
		}
	}
}