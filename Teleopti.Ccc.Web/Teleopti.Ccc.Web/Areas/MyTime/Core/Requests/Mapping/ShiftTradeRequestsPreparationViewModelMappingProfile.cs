using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class ShiftTradeRequestsPreparationViewModelMappingProfile : Profile
	{
		private readonly Func<IProjectionProvider> _projectionProvider;
		private readonly Func<IUserTimeZone> _userTimeZone;

		public ShiftTradeRequestsPreparationViewModelMappingProfile(Func<IProjectionProvider> projectionProvider, Func<IUserTimeZone> userTimeZone)
		{
			_projectionProvider = projectionProvider;
			_userTimeZone = userTimeZone;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<ShiftTradeRequestsPreparationDomainData, ShiftTradeRequestsPreparationViewModel>()
				.ForMember(d => d.HasWorkflowControlSet, o => o.MapFrom(s => s.WorkflowControlSet != null))
				.ForMember(d => d.MySchedulelayers, o => o.MapFrom(s =>
				                                                   	{
																		var layers = _projectionProvider.Invoke().Projection(s.MyScheduleDay).Where(proj => proj != null).ToList();
				                                                   		return CreateShiftTradeLayers(
				                                                   			_userTimeZone.Invoke().TimeZone(), layers);
				                                                   	}))
			;
		}

		private static IEnumerable<ShiftTradeScheduleLayer> CreateShiftTradeLayers(TimeZoneInfo timeZone, IEnumerable<IVisualLayer> layers)
		{
			DateTime shiftStartTime = layers.Min(o => o.Period.StartDateTime);

			

			var scheduleLayers = (from visualLayer in layers
							 let startDate = TimeZoneHelper.ConvertFromUtc(visualLayer.Period.StartDateTime, timeZone)
							 let endDate = TimeZoneHelper.ConvertFromUtc(visualLayer.Period.EndDateTime, timeZone)
							 let length = visualLayer.Period.ElapsedTime().TotalMinutes
							 select new ShiftTradeScheduleLayer
							 {
								 Payload = visualLayer.DisplayDescription().Name,
								 LengthInMinutes = (int) length,
								 Color = ColorTranslator.ToHtml(visualLayer.DisplayColor()),
								 StartTimeText = startDate.ToString("HH:mm"),
								 EndTimeText = endDate.ToString("HH:mm"),
								 ElapsedMinutesSinceShiftStart = (int) startDate.Subtract(TimeZoneHelper.ConvertFromUtc(shiftStartTime, timeZone)).TotalMinutes
							 }).ToList();
			return scheduleLayers;
		}
	}
}