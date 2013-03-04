using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class ReadModelShiftConfigurable : IUserDataSetup
	{
		public DateTime Date { get; set; }
		public string StartTime { get; set; }
		public string EndTime { get; set; }
		public string Activity { get; set; }
		public string LunchStartTime { get; set; }
		public string LunchEndTime { get; set; }
		public string LunchActivity { get; set; }

		private static TimeSpan AsTimeSpan(string value)
		{
			TimeSpan result;
			TimeSpan.TryParse(value, out result);
			return result;
		}

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var activityRepository = new ActivityRepository(uow);
			var activities = activityRepository.LoadAll();
			var mainActivity = activities.Single(a => a.Name == Activity);
			var lunchActivity = LunchActivity != null ? activities.Single(a => a.Name == LunchActivity) : null;

			var dateOnly = new DateOnly(Date);
			var personPeriod = user.Period(dateOnly);
			var timeZone = user.PermissionInformation.DefaultTimeZone();
			
			object[] projection;
			if (lunchActivity != null)
			{
				projection = new[]
					{
						makeLayer(mainActivity, timeZone, StartTime, LunchStartTime),
						makeLayer(lunchActivity, timeZone, LunchStartTime, LunchEndTime),
						makeLayer(mainActivity, timeZone, LunchEndTime, EndTime)
					};
			}
			else
			{
				projection = new[]
					{
						makeLayer(mainActivity, timeZone, StartTime, EndTime),
					};
			}
			
			var reposistory = new PersonScheduleDayReadModelRepository(new CurrentUnitOfWork(GlobalUnitOfWorkState.UnitOfWorkFactory));
			reposistory.SaveReadModel(new PersonScheduleDayReadModel
				{
					BusinessUnitId = mainActivity.BusinessUnit.Id.GetValueOrDefault(),
					SiteId = personPeriod.Team.Site.Id.GetValueOrDefault(),
					TeamId = personPeriod.Team.Id.GetValueOrDefault(),
					PersonId = user.Id.GetValueOrDefault(),
					Date = Date,
					ShiftStart = TimeZoneInfo.ConvertTimeToUtc(Date.Add(AsTimeSpan(StartTime)), timeZone),
					ShiftEnd = TimeZoneInfo.ConvertTimeToUtc(Date.Add(AsTimeSpan(EndTime)), timeZone),
					Shift = Newtonsoft.Json.JsonConvert.SerializeObject(
						new
							{
								Date,
								user.Name.FirstName,
								user.Name.LastName,
								user.EmploymentNumber,
								Id = user.Id.GetValueOrDefault().ToString(),
								ContractTimeMinutes = AsTimeSpan(EndTime).Subtract(AsTimeSpan(StartTime)).
								                                          Subtract(AsTimeSpan(LunchEndTime)).Add(
									                                          AsTimeSpan(LunchStartTime)).TotalMinutes,
								WorkTimeMinutes = AsTimeSpan(EndTime).Subtract(AsTimeSpan(StartTime)).
								                                      Subtract(AsTimeSpan(LunchEndTime)).Add(
									                                      AsTimeSpan(LunchStartTime)).TotalMinutes,
								Projection = projection
							})
				});
		}

		private object makeLayer(IActivity activity, TimeZoneInfo timeZone, string startTime, string endTime)
		{
			return new
				{
					Color = ColorTranslator.ToHtml(activity.DisplayColor),
					Title = activity.Name,
					Start = TimeZoneInfo.ConvertTimeToUtc(Date.Add(AsTimeSpan(startTime)), timeZone),
					End = TimeZoneInfo.ConvertTimeToUtc(Date.Add(AsTimeSpan(endTime)), timeZone),
					Minutes = AsTimeSpan(endTime).Subtract(AsTimeSpan(startTime)).TotalMinutes
				};
		}
	}
}