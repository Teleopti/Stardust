using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;


namespace Teleopti.Ccc.TestCommon.FakeData
{
    /// <summary>
    /// Simple Factory for SchedulePart
    /// </summary>
    /// <remarks>
    /// For gui tests and instead of mocking
    /// Created by: henrika
    /// Created date: 2009-09-28
    /// </remarks>
    public class SchedulePartFactoryForDomain
    {
        public IPerson CurrentPerson { get; set; }
        public IScenario CurrentScenario { get; set; }
        public DateTimePeriod CurrentPeriod { get; set; }
        public ISkill CurrentSkill { get; set; }
        private readonly IScheduleDay _part;

		public SchedulePartFactoryForDomain(IPerson person, IScenario scenario, DateTimePeriod period, ISkill skill)
			: this(person, scenario, period, skill, TimeZoneInfo.Utc)
        {
        }

		public SchedulePartFactoryForDomain(IPerson person, IScenario scenario, DateTimePeriod period, ISkill skill,TimeZoneInfo timeZone)
		{
			CurrentScenario = scenario;
			CurrentPeriod = period;
			CurrentSkill = skill;
			CurrentPerson = PersonFactory.CreatePersonWithPersonPeriod(person, new DateOnly(CurrentPeriod.StartDateTime), new List<ISkill> { CurrentSkill }, new Contract("ctr"), new PartTimePercentage("ptc"));
			CurrentPerson.PermissionInformation.SetDefaultTimeZone(timeZone);
			_part = createPart(new DateOnly(CurrentPeriod.StartDateTime));
		}

		public SchedulePartFactoryForDomain(IPerson person, DateTimePeriod period)
			: this(person, ScenarioFactory.CreateScenarioAggregate("For test", true), period, SkillFactory.CreateSkill("Skill"), person.PermissionInformation.DefaultTimeZone())
		{	
		}

        public SchedulePartFactoryForDomain()
            : this(PersonFactory.CreatePerson(), ScenarioFactory.CreateScenarioAggregate("For test",true), new DateTimePeriod(2001, 1, 1, 2001, 1, 3), SkillFactory.CreateSkill("Skill")) {}


        private IScheduleDay createPart(DateOnly dateOnly)
        {
            IList<IPerson> people = new List<IPerson> { CurrentPerson };
            var schedPeriod = new DateTimePeriod(CurrentPeriod.StartDateTime.Subtract(TimeSpan.FromDays(2)),
                                                                         CurrentPeriod.EndDateTime.AddDays(2));
            IScheduleDateTimePeriod scheduleDateTimePeriod =
                new ScheduleDateTimePeriod(schedPeriod, people);
            IScheduleDictionary scheduleDictionary =
                new ScheduleDictionaryForTest(CurrentScenario, scheduleDateTimePeriod,new Dictionary<IPerson, IScheduleRange>());
            var parameters = new ScheduleParameters(CurrentScenario, CurrentPerson, CurrentPeriod);
            var ret = ExtractedSchedule.CreateScheduleDay(scheduleDictionary, parameters.Person, dateOnly, new FullPermission());
            return ret;
        }

        public IScheduleDay CreatePartWithMainShift()
        {
            IScheduleDay part = createPart(new DateOnly(CurrentPeriod.StartDateTime));
            DateTimePeriod first = new DateTimePeriod(CurrentPeriod.StartDateTime, CurrentPeriod.StartDateTime.AddHours(2));
            DateTimePeriod second = new DateTimePeriod(CurrentPeriod.StartDateTime.AddHours(2), CurrentPeriod.StartDateTime.AddHours(4));
            var mainShift = new EditableShift(ShiftCategoryFactory.CreateShiftCategory("for test"));
            var firstLayer = new EditableShiftLayer(ActivityFactory.CreateActivity("first"), first);
			var secondLayer = new EditableShiftLayer(ActivityFactory.CreateActivity("second"), second);
            mainShift.LayerCollection.Add(firstLayer);
            mainShift.LayerCollection.Add(secondLayer);
            part.AddMainShift(mainShift);
            return part;
        }

         public IScheduleDay CreatePartWithoutMainShift()
         {
             return  createPart(new DateOnly(CurrentPeriod.StartDateTime));
         }

        public IScheduleDay CreatePartWithMainShiftWithDifferentActivities()
        {
            IScheduleDay part = createPart(new DateOnly(CurrentPeriod.StartDateTime));
            DateTimePeriod first = new DateTimePeriod(CurrentPeriod.StartDateTime.AddHours(5), CurrentPeriod.StartDateTime.AddHours(6));
            DateTimePeriod second = new DateTimePeriod(CurrentPeriod.StartDateTime.AddHours(4), CurrentPeriod.StartDateTime.AddHours(8));

            var mainShift = new EditableShift(ShiftCategoryFactory.CreateShiftCategory("for test"));
            var layer1 = new EditableShiftLayer(ActivityFactory.CreateActivity("ActivityOne", System.Drawing.Color.DodgerBlue), first.MovePeriod(TimeSpan.FromHours(1)));
			var layer2 = new EditableShiftLayer(ActivityFactory.CreateActivity("ActivityTwo", System.Drawing.Color.Yellow), first.MovePeriod(TimeSpan.FromHours(2)));
			var layer3 = new EditableShiftLayer(ActivityFactory.CreateActivity("ActivityThree", System.Drawing.Color.DodgerBlue), first.MovePeriod(TimeSpan.FromHours(4)));
			var layer4 = new EditableShiftLayer(ActivityFactory.CreateActivity("ActivityFour", System.Drawing.Color.GreenYellow), first.MovePeriod(TimeSpan.FromMinutes(21)));
			var layer5 = new EditableShiftLayer(ActivityFactory.CreateActivity("ActivityFive"), second);
            mainShift.LayerCollection.Add(layer1);
            mainShift.LayerCollection.Add(layer2);
            mainShift.LayerCollection.Add(layer3);
            mainShift.LayerCollection.Add(layer4);
            mainShift.LayerCollection.Add(layer5);
            part.AddMainShift(mainShift);

            return part;
        }

        public IScheduleDay CreateSchedulePartWithMainShiftAndAbsence()
        {
            IScheduleDay part = CreatePartWithMainShiftWithDifferentActivities();
            IPersonAbsence pa = PersonAbsenceFactory.CreatePersonAbsence(part.Person, part.Scenario, part.Period);
            part.Add(pa);
            return part;
        }

        public IScheduleDay CreatePart()
        {
            return _part;
        }

        public IScheduleDay CreatePartWithDifferentPeriod(int days)
        {
            return createPart(new DateOnly(CurrentPeriod.StartDateTime.Add(TimeSpan.FromDays(days))));
        }

        public SchedulePartFactoryForDomain AddAbsence()
        {
            _part.CreateAndAddAbsence(new AbsenceLayer(AbsenceFactory.CreateAbsence("absence"),CurrentPeriod));
            return this;
        }

        public SchedulePartFactoryForDomain AddMeeting()
        {
            Meeting meeting = new Meeting(CurrentPerson, 
                new List<IMeetingPerson>(), 
                "subject", 
                "location", 
                "description", 
                ActivityFactory.CreateActivity("meeting"), 
                CurrentScenario);

            _part.Add(new PersonMeeting(meeting, new MeetingPerson(CurrentPerson, true), CurrentPeriod));
            return this;
        }

		public SchedulePartFactoryForDomain AddOvertime()
		{
			return AddOvertime("overtime");
		}

		public SchedulePartFactoryForDomain AddPersonalLayer()
		{
			return AddPersonalLayer("PersonalActivity");
		}

		public SchedulePartFactoryForDomain AddMainShiftLayer()
		{
			return AddMainShiftLayer("main");
		}

        public SchedulePartFactoryForDomain AddOvertime(string activityName)
        {
            var newMultiplicatorSet = new MultiplicatorDefinitionSet("multplic", MultiplicatorType.Overtime);
           IPersonPeriod period = _part.Person.Period(new DateOnly(CurrentPeriod.StartDateTime));
           period.PersonContract.Contract.AddMultiplicatorDefinitionSetCollection(newMultiplicatorSet);
		   _part.CreateAndAddOvertime(ActivityFactory.CreateActivity(activityName), CurrentPeriod, newMultiplicatorSet);
            return this;
        }

		public SchedulePartFactoryForDomain AddPersonalLayer(string activityName)
		{
			_part.CreateAndAddPersonalActivity(ActivityFactory.CreateActivity(activityName), CurrentPeriod);
			return this;
		}

		public SchedulePartFactoryForDomain AddPersonalLayer(IScheduleDay scheduleDay)
		{
			scheduleDay.CreateAndAddPersonalActivity(ActivityFactory.CreateActivity("PersonActivity"), CurrentPeriod);
			return this;
		}

		public SchedulePartFactoryForDomain AddMainShiftLayer(string activityName)
		{
			_part.CreateAndAddActivity(ActivityFactory.CreateActivity(activityName), CurrentPeriod, ShiftCategoryFactory.CreateShiftCategory("Shiftcategory"));
			return this;
		}

		public SchedulePartFactoryForDomain AddMainShiftLayerBetween(TimeSpan startTime, TimeSpan endTime)
	    {
		    var start = new DateTime(CurrentPeriod.StartDateTime.Year, CurrentPeriod.StartDateTime.Month,
		                             CurrentPeriod.StartDateTime.Day, startTime.Hours, startTime.Minutes, startTime.Seconds,
		                             DateTimeKind.Utc);
			var end = new DateTime(CurrentPeriod.StartDateTime.Year, CurrentPeriod.StartDateTime.Month,
									CurrentPeriod.StartDateTime.Day, endTime.Hours, endTime.Minutes, endTime.Seconds,
									DateTimeKind.Utc);
			_part.CreateAndAddActivity(ActivityFactory.CreateActivity("Main"), new DateTimePeriod(start,end), ShiftCategoryFactory.CreateShiftCategory("Shiftcategory"));
			return this;
	    }
    }
}


