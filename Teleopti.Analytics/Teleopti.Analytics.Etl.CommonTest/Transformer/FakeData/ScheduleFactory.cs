using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData
{
	internal class ScheduleFactory
	{
		private readonly IList<IActivity> _activityCollection;
		private readonly IList<IPerson> _personCollection;
		private readonly IList<IScenario> _scenarioCollection;
		private readonly IList<IShiftCategory> _shiftCategoryCollection;

		internal ScheduleFactory()
		{
			_personCollection = PersonFactory.CreatePersonGraphCollection();
			_scenarioCollection = ScenarioFactory.CreateScenarioCollection();
			_shiftCategoryCollection = ShiftCategoryFactory.CreateShiftCategoryCollection();
			_activityCollection = ActivityFactory.CreateActivityCollection();
		}

		public IList<IActivity> ActivityCollection
		{
			get { return _activityCollection; }
		}

		internal IList<IScheduleDay> CreateShiftCollection()
		{
			var period =
				 new DateTimePeriod(new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc),
										  new DateTime(2007, 1, 3, 0, 0, 0, DateTimeKind.Utc));
			ScheduleRange schedule = getSheduleRange(period, _scenarioCollection[0], _personCollection[0]);

			schedule.AddRange(createPersonAssignmentlList());
			schedule.AddRange(createPersonAbsenceList());
			IList<IScheduleDay> scheduleList = new List<IScheduleDay>();
			scheduleList.Add(schedule.ScheduledDay(new DateOnly(2000, 1, 1)));

			return scheduleList;
		}

		private static ScheduleRange getSheduleRange(DateTimePeriod period, IScenario scenario, IPerson person)
		{
			IScheduleParameters scheduleParams = new ScheduleParameters(scenario, person, period);
			var currentAuthorization = new FullPermission();
			var dataPermissionChecker = new PersistableScheduleDataPermissionChecker(currentAuthorization);
			var dic = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(period), dataPermissionChecker, currentAuthorization);
			return new ScheduleRange(dic, scheduleParams, dataPermissionChecker, currentAuthorization);
		}


		private IEnumerable<IPersonAssignment> createPersonAssignmentlList()
		{
			var personAssignment = new PersonAssignment(_personCollection[0], _scenarioCollection[0], new DateOnly(2007, 1, 1));

			// CreateProjection activity periods
			var phone1 = new DateTimePeriod(new DateTime(2007, 1, 1, 8, 0, 0, DateTimeKind.Utc),
																	 new DateTime(2007, 1, 1, 10, 5, 0, DateTimeKind.Utc)); //9
			var shortBreak1 = new DateTimePeriod(new DateTime(2007, 1, 1, 10, 5, 0, DateTimeKind.Utc),
																			new DateTime(2007, 1, 1, 10, 10, 0, DateTimeKind.Utc)); //1
			var phone2 = new DateTimePeriod(new DateTime(2007, 1, 1, 10, 10, 0, DateTimeKind.Utc),
																	 new DateTime(2007, 1, 1, 11, 20, 0, DateTimeKind.Utc)); //6
			var lunchBreak1 = new DateTimePeriod(new DateTime(2007, 1, 1, 11, 20, 0, DateTimeKind.Utc),
																			new DateTime(2007, 1, 1, 12, 5, 0, DateTimeKind.Utc)); //4
			var phone3 = new DateTimePeriod(new DateTime(2007, 1, 1, 12, 05, 0, DateTimeKind.Utc),
																	 new DateTime(2007, 1, 1, 14, 50, 0, DateTimeKind.Utc)); //12
			var shortbreak2 = new DateTimePeriod(new DateTime(2007, 1, 1, 14, 50, 0, DateTimeKind.Utc),
																			new DateTime(2007, 1, 1, 15, 5, 0, DateTimeKind.Utc)); //2
			var phone4 = new DateTimePeriod(new DateTime(2007, 1, 1, 15, 5, 0, DateTimeKind.Utc),
																	 new DateTime(2007, 1, 2, 1, 40, 0, DateTimeKind.Utc));
			//43 - SUM = 77
			// Add activity periods to a Layer
			personAssignment.AddActivity(ActivityCollection[0], phone1);
			personAssignment.AddActivity(ActivityCollection[2], shortBreak1);
			personAssignment.AddActivity(ActivityCollection[0], phone2);
			personAssignment.AddActivity(ActivityCollection[1], lunchBreak1);
			personAssignment.AddActivity(ActivityCollection[0], phone3);
			personAssignment.AddActivity(ActivityCollection[2], shortbreak2);
			personAssignment.AddActivity(ActivityCollection[2], phone4);
			personAssignment.SetShiftCategory(_shiftCategoryCollection[0]);

			RaptorTransformerHelper.SetUpdatedOn(personAssignment, DateTime.Now);

			return new List<IPersonAssignment> { personAssignment };
		}

		private IEnumerable<IPersonAbsence> createPersonAbsenceList()
		{
			IList<IPersonAbsence> personAbsenceList = new List<IPersonAbsence>();
			IAbsence absence = new Absence();
			absence.Description = new Description("Sick Leave");
			absence.SetId(Guid.NewGuid());
			var absencePeriod = new DateTimePeriod(new DateTime(2007, 1, 1, 15, 0, 0, DateTimeKind.Utc),
																			  new DateTime(2007, 1, 2, 1, 40, 0, DateTimeKind.Utc));
			IAbsenceLayer absenceLayer = new AbsenceLayer(absence, absencePeriod);
			IPersonAbsence personAbsence = new PersonAbsence(_personCollection[0], _scenarioCollection[0], absenceLayer);

			RaptorTransformerHelper.SetUpdatedOn(personAbsence, DateTime.Now);
			personAbsenceList.Add(personAbsence);

			return personAbsenceList;
		}
	}
}