using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;
using Teleopti.Messaging.SignalR;

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

			var reposistory = new PersonScheduleDayReadModelStorage(new CurrentUnitOfWork(GlobalUnitOfWorkState.CurrentUnitOfWorkFactory), new DoNotSend(), new SpecificDataSource(TestData.DataSource));
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

	public class ReadModelScheduleProjectionConfigurable : IUserDataSetup
	{
		public DateTime BelongsToDate { get; set; }
		public Guid PayloadId { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public int WorkTimeHours { get; set; }
		public int ContractTimeHours { get; set; }
		public string Name { get; set; }
		public string ShortName { get; set; }
		public string PayrollCode { get; set; }
		public int DisplayColor { get; set; }
		public bool IsAbsence { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{

			//fulsettning:
			PayloadId = new Guid();
			//StartDateTime = StartDateTime;
			//EndDateTime = EndDateTime;
			Name = "Maria Stein";
			ShortName = "MS";
			PayrollCode = "tjohej";
			DisplayColor = 1;
			IsAbsence = true;
			//end fulsetting


			var absenceRepo = new AbsenceRepository(uow);

			var absence = absenceRepo.LoadAll().First(a => a.Name == "Vacation");



			//var scenario = GlobalDataContext.Data().Data<CommonScenario>().Scenario;
			var scenario = GlobalDataContext.Data().Data<CommonScenario>().Scenario;

		
			//var rogersFavvoRepo = new ScheduleProjectionReadOnlyRepository(GlobalUnitOfWorkState.UnitOfWorkFactory);


			var projectionLayer = new ProjectionChangedEventLayer()
									  {
										  PayloadId = absence.Id.Value,
										  StartDateTime = StartDateTime,
										  EndDateTime = EndDateTime,
										  WorkTime = TimeSpan.FromHours(WorkTimeHours),
										  ContractTime = TimeSpan.FromHours(ContractTimeHours),
										  Name = absence.Name,
										  ShortName = ShortName,
										  PayrollCode = PayrollCode,
										  DisplayColor = DisplayColor,
										  IsAbsence = IsAbsence
									  };

// ReSharper disable PossibleInvalidOperationException
			//rogersFavvoRepo.AddProjectedLayer(new DateOnly(BelongsToDate), scenario.Id.Value, user.Id.Value, projectionLayer);
// ReSharper restore PossibleInvalidOperationException

			
		}
	}
}