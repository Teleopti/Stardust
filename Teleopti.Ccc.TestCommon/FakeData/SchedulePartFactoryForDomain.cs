﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Meetings;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

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

        public SchedulePartFactoryForDomain(IPerson person,IScenario scenario,DateTimePeriod period,ISkill skill)
        {
            CurrentScenario = scenario;
            CurrentPeriod = period;
            CurrentSkill = skill;
            CurrentPerson = PersonFactory.CreatePersonWithPersonPeriod(person,new DateOnly(CurrentPeriod.StartDateTime), new List<ISkill> { CurrentSkill });
            CurrentPerson.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
            _part = createPart(new DateOnly(CurrentPeriod.StartDateTime));

        }

        public SchedulePartFactoryForDomain()
            : this(PersonFactory.CreatePerson(), ScenarioFactory.CreateScenarioAggregate("For test",true,false), new DateTimePeriod(2001, 1, 1, 2001, 1, 3), SkillFactory.CreateSkill("Skill")) {}


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
            var ret = ExtractedSchedule.CreateScheduleDay(scheduleDictionary, parameters.Person, dateOnly);
            return ret;
        }

        public IScheduleDay CreatePartWithMainShift()
        {
            IScheduleDay part = createPart(new DateOnly(CurrentPeriod.StartDateTime));
            DateTimePeriod first = new DateTimePeriod(CurrentPeriod.StartDateTime, CurrentPeriod.StartDateTime.AddHours(2));
            DateTimePeriod second = new DateTimePeriod(CurrentPeriod.StartDateTime.AddHours(2), CurrentPeriod.StartDateTime.AddHours(4));
            MainShift mainShift = new MainShift(ShiftCategoryFactory.CreateShiftCategory("for test"));
            ActivityLayer firstLayer = new MainShiftActivityLayer(ActivityFactory.CreateActivity("first"), first);
            ActivityLayer secondLayer = new MainShiftActivityLayer(ActivityFactory.CreateActivity("second"), second);
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

            MainShift mainShift = new MainShift(ShiftCategoryFactory.CreateShiftCategory("for test"));
            ActivityLayer layer1 = new MainShiftActivityLayer(ActivityFactory.CreateActivity("ActivityOne", System.Drawing.Color.DodgerBlue), first.MovePeriod(TimeSpan.FromHours(1)));
            ActivityLayer layer2 = new MainShiftActivityLayer(ActivityFactory.CreateActivity("ActivityTwo", System.Drawing.Color.Yellow), first.MovePeriod(TimeSpan.FromHours(2)));
            ActivityLayer layer3 = new MainShiftActivityLayer(ActivityFactory.CreateActivity("ActivityThree", System.Drawing.Color.DodgerBlue), first.MovePeriod(TimeSpan.FromHours(4)));
            ActivityLayer layer4 = new MainShiftActivityLayer(ActivityFactory.CreateActivity("ActivityFour", System.Drawing.Color.GreenYellow), first.MovePeriod(TimeSpan.FromMinutes(21)));
            ActivityLayer layer5 = new MainShiftActivityLayer(ActivityFactory.CreateActivity("ActivityFive"), second);
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
            var newMultiplicatorSet = new MultiplicatorDefinitionSet("multplic", MultiplicatorType.Overtime);
           IPersonPeriod period = _part.Person.Period(new DateOnly(CurrentPeriod.StartDateTime));
           period.PersonContract.Contract.AddMultiplicatorDefinitionSetCollection(newMultiplicatorSet);
           _part.CreateAndAddOvertime(new OvertimeShiftActivityLayer(ActivityFactory.CreateActivity("overtime"), CurrentPeriod, newMultiplicatorSet));
            return this;
        }

        public SchedulePartFactoryForDomain AddPersonalLayer()
        {
            _part.CreateAndAddPersonalActivity(new PersonalShiftActivityLayer(ActivityFactory.CreateActivity("PersonActivity"),CurrentPeriod));
            return this;
        }


		public SchedulePartFactoryForDomain AddPersonalLayer(IScheduleDay scheduleDay)
		{
			scheduleDay.CreateAndAddPersonalActivity(new PersonalShiftActivityLayer(ActivityFactory.CreateActivity("PersonActivity"), CurrentPeriod));
			return this;
		}

        public SchedulePartFactoryForDomain AddMainShiftLayer()
        {
            _part.CreateAndAddActivity(new MainShiftActivityLayer(ActivityFactory.CreateActivity("Main"),CurrentPeriod),ShiftCategoryFactory.CreateShiftCategory("Shiftcategory"));
            return this;
        }
    }
}


