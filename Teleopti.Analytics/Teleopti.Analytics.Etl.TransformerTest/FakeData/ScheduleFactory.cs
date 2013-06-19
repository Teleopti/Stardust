using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Transformer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest.FakeData
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
            DateTimePeriod period =
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
            ScheduleDictionary dic = new ScheduleDictionary(scenario, new ScheduleDateTimePeriod(period));
            return new ScheduleRange(dic, scheduleParams);
        }


        private IList<IPersonAssignment> createPersonAssignmentlList()
        {
					PersonAssignment personAssignment = new PersonAssignment(_personCollection[0], _scenarioCollection[0], new DateOnly(2007, 1, 1));

            // CreateProjection activity periods
            DateTimePeriod phone1 = new DateTimePeriod(new DateTime(2007, 1, 1, 8, 0, 0, DateTimeKind.Utc),
                                                       new DateTime(2007, 1, 1, 10, 5, 0, DateTimeKind.Utc)); //9
            DateTimePeriod shortBreak1 = new DateTimePeriod(new DateTime(2007, 1, 1, 10, 5, 0, DateTimeKind.Utc),
                                                            new DateTime(2007, 1, 1, 10, 10, 0, DateTimeKind.Utc)); //1
            DateTimePeriod phone2 = new DateTimePeriod(new DateTime(2007, 1, 1, 10, 10, 0, DateTimeKind.Utc),
                                                       new DateTime(2007, 1, 1, 11, 20, 0, DateTimeKind.Utc)); //6
            DateTimePeriod lunchBreak1 = new DateTimePeriod(new DateTime(2007, 1, 1, 11, 20, 0, DateTimeKind.Utc),
                                                            new DateTime(2007, 1, 1, 12, 5, 0, DateTimeKind.Utc)); //4
            DateTimePeriod phone3 = new DateTimePeriod(new DateTime(2007, 1, 1, 12, 05, 0, DateTimeKind.Utc),
                                                       new DateTime(2007, 1, 1, 14, 50, 0, DateTimeKind.Utc)); //12
            DateTimePeriod shortbreak2 = new DateTimePeriod(new DateTime(2007, 1, 1, 14, 50, 0, DateTimeKind.Utc),
                                                            new DateTime(2007, 1, 1, 15, 5, 0, DateTimeKind.Utc)); //2
            DateTimePeriod phone4 = new DateTimePeriod(new DateTime(2007, 1, 1, 15, 5, 0, DateTimeKind.Utc),
                                                       new DateTime(2007, 1, 2, 1, 40, 0, DateTimeKind.Utc));
            //43 - SUM = 77
            // Add activity periods to a Layer
	        var layers = new[]
		        {
							new MainShiftLayer(ActivityCollection[0], phone1),
							new MainShiftLayer(ActivityCollection[2], shortBreak1),
							new MainShiftLayer(ActivityCollection[0], phone2),
							new MainShiftLayer(ActivityCollection[1], lunchBreak1),
							new MainShiftLayer(ActivityCollection[0], phone3),
							new MainShiftLayer(ActivityCollection[2], shortbreak2),
							new MainShiftLayer(ActivityCollection[0], phone4)
		        };

						personAssignment.SetMainShiftLayers(layers, _shiftCategoryCollection[0]);
            RaptorTransformerHelper.SetCreatedOn(personAssignment, DateTime.Now);

            return new List<IPersonAssignment> {personAssignment};
        }

        private IList<IPersonAbsence> createPersonAbsenceList()
        {
            IList<IPersonAbsence> personAbsenceList = new List<IPersonAbsence>();
            IAbsence absence = new Absence();
            absence.Description = new Description("Sick Leave");
            absence.SetId(Guid.NewGuid());
            DateTimePeriod absencePeriod = new DateTimePeriod(new DateTime(2007, 1, 1, 15, 0, 0, DateTimeKind.Utc),
                                                              new DateTime(2007, 1, 2, 1, 40, 0, DateTimeKind.Utc));
            IAbsenceLayer absenceLayer = new AbsenceLayer(absence, absencePeriod);
            IPersonAbsence personAbsence = new PersonAbsence(_personCollection[0], _scenarioCollection[0], absenceLayer);

            RaptorTransformerHelper.SetCreatedOn(personAbsence, DateTime.Now);
            personAbsenceList.Add(personAbsence);

            return personAbsenceList;
        }
    }
}