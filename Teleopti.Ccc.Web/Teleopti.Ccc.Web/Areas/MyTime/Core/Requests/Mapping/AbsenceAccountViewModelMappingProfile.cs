using System;
using System.Globalization;
using AutoMapper;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;
namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping
{
	public class PersonAccountViewModelMappingProfile : Profile
	{
		private readonly Func<IUserTimeZone> _userTimeZone;

		public PersonAccountViewModelMappingProfile(Func<IUserTimeZone> timeZone)
		{
			_userTimeZone = timeZone;
		}

		protected override void Configure()
		{
			base.Configure();

			CreateMap<IAccount, AbsenceAccountViewModel>()
				.ForMember(d => d.AbsenceName, o => o.MapFrom(m => m.Owner.Absence.Name))
				.ForMember(d => d.TrackerType, o => o.ResolveUsing(s =>
					{
						var trackerType = "Others";
						var classTypeOfTracker = s.Owner.Absence.Tracker.GetType();
						if (classTypeOfTracker == Tracker.CreateDayTracker().GetType())
						{
							trackerType = "Days";
						}
						else if (classTypeOfTracker == Tracker.CreateTimeTracker().GetType())
						{
							trackerType = "Hours";
						}
						return trackerType;
					}))
				.ForMember(d => d.PeriodStartUtc, o => o.MapFrom(m => TimeZoneInfo.ConvertTimeFromUtc(m.StartDate, _userTimeZone.Invoke().TimeZone())))
				.ForMember(d => d.PeriodEndUtc, o => o.MapFrom(m => TimeZoneInfo.ConvertTimeFromUtc(m.Period().EndDate, _userTimeZone.Invoke().TimeZone())))
				.ForMember(d => d.Accrued, o => o.MapFrom(m => convertTimeSpanToString(m.Accrued, m.Owner.Absence.Tracker)))
				.ForMember(d => d.Used, o => o.MapFrom(m =>  convertTimeSpanToString(m.LatestCalculatedBalance, m.Owner.Absence.Tracker)))
				.ForMember(d => d.Remaining, o => o.MapFrom(m => convertTimeSpanToString(m.Remaining, m.Owner.Absence.Tracker)));
		}

		private static string convertTimeSpanToString(TimeSpan ts, ITracker tracker)
		{
			var result = string.Empty;

			var classTypeOfTracker = tracker.GetType();
			if (classTypeOfTracker == Tracker.CreateDayTracker().GetType())
			{
				result = ts.TotalDays.ToString(CultureInfo.InvariantCulture);
			}
			else if (classTypeOfTracker == Tracker.CreateTimeTracker().GetType())
			{
				var totalMinutes = ts.TotalMinutes;

				var hours = (int)(totalMinutes / 60);
				var minutes = (int)(totalMinutes % 60);

				result = string.Format("{0}:{1:00}", hours, minutes);
			}

			return result;
		}
	}
}