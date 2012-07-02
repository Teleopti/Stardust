using System;
using AutoMapper;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class DateTimePeriodFormMappingProfile : Profile
	{
		private readonly Func<IUserTimeZone> _userTimeZone;

		public DateTimePeriodFormMappingProfile(Func<IUserTimeZone> userTimeZone)
		{
			_userTimeZone = userTimeZone;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<DateTimePeriodForm, DateTimePeriod>()
				.ConvertUsing(s =>
				{
					if (s == null)
						return new DateTimePeriod();
					var fromTime = s.StartDate.Date.Add(s.StartTime.Time);
					var toTime = s.EndDate.Date.Add(s.EndTime.Time);
					var fromTimeUtc = TimeZoneHelper.ConvertToUtc(fromTime, _userTimeZone.Invoke().TimeZone());
					var toTimeUtc = TimeZoneHelper.ConvertToUtc(toTime, _userTimeZone.Invoke().TimeZone());
					return new DateTimePeriod(fromTimeUtc, toTimeUtc);
				});
		}
	}
}