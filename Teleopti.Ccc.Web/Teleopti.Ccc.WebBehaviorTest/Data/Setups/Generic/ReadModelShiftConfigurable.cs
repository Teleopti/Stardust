using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Repositories;
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
			var lunchActivity = activities.Single(a => a.Name == LunchActivity);

			var dateOnly = new DateOnly(Date);
			var personPeriod = user.Period(dateOnly);
			var timeZone = user.PermissionInformation.DefaultTimeZone();

			var reposistory = new PersonScheduleDayReadModelRepository(GlobalUnitOfWorkState.UnitOfWorkFactory);
			reposistory.SaveReadModel(new PersonScheduleDayReadModel
			                           			{
			                           				BusinessUnitId = mainActivity.BusinessUnit.Id.GetValueOrDefault(),
			                           				SiteId = personPeriod.Team.Site.Id.GetValueOrDefault(),
			                           				TeamId = personPeriod.Team.Id.GetValueOrDefault(),
													PersonId = user.Id.GetValueOrDefault(),
			                           				Date = Date,
			                           				ShiftStart = TimeZoneInfo.ConvertTimeToUtc(Date.Add(AsTimeSpan(StartTime)), timeZone),
			                           				ShiftEnd = TimeZoneInfo.ConvertTimeToUtc(Date.Add(AsTimeSpan(EndTime)), timeZone),
			                           				Shift = Newtonsoft.Json.JsonConvert.SerializeObject(new
			                           				                                                    	{
			                           				                                                    		Date,
																												user.Name.FirstName,
																												user.Name.LastName,
																												user.EmploymentNumber,
			                           				                                                    		Id =
			                           				                                                    	user.Id.GetValueOrDefault().
			                           				                                                    	ToString(),
			                           				                                                    		ContractTimeMinutes =
			                           				                                                    	AsTimeSpan(EndTime).Subtract(AsTimeSpan(StartTime)).
			                           				                                                    	Subtract(AsTimeSpan(LunchEndTime)).Add(
			                           				                                                    		AsTimeSpan(LunchStartTime)).TotalMinutes,
			                           				                                                    		WorkTimeMinutes =
			                           				                                                    	AsTimeSpan(EndTime).Subtract(AsTimeSpan(StartTime)).
			                           				                                                    	Subtract(AsTimeSpan(LunchEndTime)).Add(
			                           				                                                    		AsTimeSpan(LunchStartTime)).TotalMinutes,
			                           				                                                    		Projection = new[]
			                           				                                                    		             	{
			                           				                                                    		             		new
			                           				                                                    		             			{
			                           				                                                    		             				Color = ColorTranslator.ToHtml(mainActivity.DisplayColor),
			                           				                                                    		             				Title = mainActivity.Name,
																																			Start = TimeZoneInfo.ConvertTimeToUtc(Date.Add(AsTimeSpan(StartTime)),timeZone),
																																			End = TimeZoneInfo.ConvertTimeToUtc(Date.Add(AsTimeSpan(LunchStartTime)),timeZone),
			                           				                                                    		             				Minutes = AsTimeSpan(LunchStartTime).Subtract(AsTimeSpan(StartTime)).TotalMinutes
			                           				                                                    		             			},
																																		new
			                           				                                                    		             			{
			                           				                                                    		             				Color = ColorTranslator.ToHtml(lunchActivity.DisplayColor),
			                           				                                                    		             				Title = lunchActivity.Name,
																																			Start = TimeZoneInfo.ConvertTimeToUtc(Date.Add(AsTimeSpan(LunchStartTime)),timeZone),
																																			End = TimeZoneInfo.ConvertTimeToUtc(Date.Add(AsTimeSpan(LunchEndTime)),timeZone),
			                           				                                                    		             				Minutes = AsTimeSpan(LunchEndTime).Subtract(AsTimeSpan(LunchStartTime)).TotalMinutes
			                           				                                                    		             			},
																																		new
			                           				                                                    		             			{
			                           				                                                    		             				Color = ColorTranslator.ToHtml(mainActivity.DisplayColor),
			                           				                                                    		             				Title = mainActivity.Name,
																																			Start = TimeZoneInfo.ConvertTimeToUtc(Date.Add(AsTimeSpan(LunchEndTime)),timeZone),
																																			End = TimeZoneInfo.ConvertTimeToUtc(Date.Add(AsTimeSpan(EndTime)),timeZone),
			                           				                                                    		             				Minutes = AsTimeSpan(EndTime).Subtract(AsTimeSpan(LunchEndTime)).TotalMinutes
			                           				                                                    		             			}
			                           				                                                    		             	}
			                           				                                                    	})
			                           			});
		}

	}
}