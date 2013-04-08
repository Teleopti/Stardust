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

		private TimeSpan StartTimeAsTimeSpan() { return TimeSpan.Parse(StartTime); }
		private TimeSpan EndTimeAsTimeSpan() { return TimeSpan.Parse(EndTime); }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var activityRepository = new ActivityRepository(uow);
			var activities = activityRepository.LoadAll();
			var mainActivity = activities.Single(a => a.Name == Activity);
			var lunchActivity = LunchActivity != null ? activities.Single(a => a.Name == LunchActivity) : null;

			var dateOnly = new DateOnly(Date);
			var personPeriod = user.Period(dateOnly);
			
			object[] projection;
			if (lunchActivity != null)
			{
				projection = new[]
					{
						makeLayer(mainActivity, StartTime, LunchStartTime),
						makeLayer(lunchActivity, LunchStartTime, LunchEndTime),
						makeLayer(mainActivity, LunchEndTime, EndTime)
					};
			}
			else
			{
				projection = new[]
					{
						makeLayer(mainActivity, StartTime, EndTime),
					};
			}

			// used for worktime aswell, dont know why, just not changing the behavior for now
			var contractTimeMinutes = EndTimeAsTimeSpan().Subtract(StartTimeAsTimeSpan()).TotalMinutes;

			var reposistory = new PersonScheduleDayReadModelRepository(new CurrentUnitOfWork(GlobalUnitOfWorkState.UnitOfWorkFactoryProvider));
			reposistory.SaveReadModel(new PersonScheduleDayReadModel
				{
					BusinessUnitId = mainActivity.BusinessUnit.Id.GetValueOrDefault(),
					SiteId = personPeriod.Team.Site.Id.GetValueOrDefault(),
					TeamId = personPeriod.Team.Id.GetValueOrDefault(),
					PersonId = user.Id.GetValueOrDefault(),
					Date = Date,
					ShiftStart = Date.Add(StartTimeAsTimeSpan()),
					ShiftEnd = Date.Add(EndTimeAsTimeSpan()),
					Shift = Newtonsoft.Json.JsonConvert.SerializeObject(
						new
							{
								Date,
								user.Name.FirstName,
								user.Name.LastName,
								user.EmploymentNumber,
								Id = user.Id.GetValueOrDefault().ToString(),
								ContractTimeMinutes = contractTimeMinutes,
								WorkTimeMinutes = contractTimeMinutes,
								Projection = projection
							})
				});
		}

		private object makeLayer(IActivity activity, string startTime, string endTime)
		{
			var start = Date.Add(TimeSpan.Parse(startTime));
			var end = Date.Add(TimeSpan.Parse(endTime));
			return new
				{
					Color = ColorTranslator.ToHtml(activity.DisplayColor),
					Title = activity.Name,
					Start = start,
					End = end,
					Minutes = end.Subtract(start).TotalMinutes
				};
		}
	}
}