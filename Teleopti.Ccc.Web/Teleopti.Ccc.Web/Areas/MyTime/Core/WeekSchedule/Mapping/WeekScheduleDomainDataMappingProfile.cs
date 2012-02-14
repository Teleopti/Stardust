using System;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping
{
	public class WeekScheduleDomainDataMappingProfile : Profile
	{
		private readonly Func<IMappingEngine> _mapper;
		private readonly Func<IScheduleProvider> _scheduleProvider;
		private readonly Func<IProjectionProvider> _projectionProvider;
		private readonly Func<IPersonRequestProvider> _personRequestProvider;
		private readonly Func<IUserTimeZone> _userTimeZone; 

		public WeekScheduleDomainDataMappingProfile(Func<IMappingEngine> mapper, Func<IScheduleProvider> scheduleProvider, Func<IProjectionProvider> projectionProvider, Func<IPersonRequestProvider> personRequestProvider, Func<IUserTimeZone> userTimeZone)
		{
			_mapper = mapper;
			_scheduleProvider = scheduleProvider;
			_projectionProvider = projectionProvider;
			_personRequestProvider = personRequestProvider;
			_userTimeZone = userTimeZone;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<DateOnly, WeekScheduleDomainData>()
				.ConvertUsing(s =>
								{
									var date = s;
									var firstDayOfWeek = new DateOnly(DateHelper.GetFirstDateInWeek(s, CultureInfo.CurrentCulture));
									var week = new DateOnlyPeriod(firstDayOfWeek, firstDayOfWeek.AddDays(6));

									var scheduleDays = _scheduleProvider.Invoke().GetScheduleForPeriod(week) ?? new IScheduleDay[] { };
									var personRequests = _personRequestProvider.Invoke().RetrieveRequests(week);

									var days = (from day in firstDayOfWeek.Date.DateRange(7)
												let scheduleDay = scheduleDays.SingleOrDefault(d => d.DateOnlyAsPeriod.DateOnly == day)
												let projection = scheduleDay == null ? null : _projectionProvider.Invoke().Projection(scheduleDay)
												let personRequestsForDay = personRequests == null ? null : (from i in personRequests where _userTimeZone.Invoke().TimeZone().ConvertTimeFromUtc(i.Request.Period.StartDateTime).Date == day select i).ToArray()
												select new WeekScheduleDayDomainData
												       	{
															Date = new DateOnly(day),
															PersonRequests = personRequestsForDay,
															Projection = projection,
															ScheduleDay = scheduleDay,
															}
											   ).ToArray();

									return new WeekScheduleDomainData
											{
												Date = date,
												Days = days,
											};
								});

		}
	}
}