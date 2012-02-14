using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.DomainTest.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.FakeData
{
    /// <summary>
    /// Simple Factory for SchedulePart
    /// </summary>
    /// <remarks>
    /// For gui tests and instead of mocking
    /// Created by: henrika
    /// Created date: 2009-09-28
    /// </remarks>
    public class FactoryForSchedulePart
    {
        public IPerson CurrentPerson { get; set; }
        public IScenario CurrentScenario { get; set; }
        public DateTimePeriod CurrentPeriod { get; set; }

        public FactoryForSchedulePart()
        {
            CurrentPerson = PersonFactory.CreatePerson("FirstName", "LastName");
            CurrentPeriod = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddHours(20));
            CurrentScenario = new Scenario("For test");
        }

        private ISchedulePart createPart()
        {
            IList<IPerson> people = new List<IPerson>() { CurrentPerson };
            DateTimePeriod schedPeriod = new DateTimePeriod(CurrentPeriod.StartDateTime.Subtract(TimeSpan.FromDays(2)),
                                                                         CurrentPeriod.EndDateTime.AddDays(2));
            IScheduleDateTimePeriod scheduleDateTimePeriod =
                new ScheduleDateTimePeriod(schedPeriod, people);
            IScheduleDictionary scheduleDictionary =
                new ScheduleDictionary(CurrentScenario, scheduleDateTimePeriod);
            ScheduleParameters parameters =
                new ScheduleParameters(CurrentScenario, CurrentPerson, CurrentPeriod);
            return new SchedulePartUsedInTest(scheduleDictionary, parameters, true, true);
        }

        public ISchedulePart CreatePartWithMainShift()
        {
            ISchedulePart part = createPart();
            DateTimePeriod first = new DateTimePeriod(CurrentPeriod.StartDateTime, CurrentPeriod.StartDateTime.AddHours(2));
            DateTimePeriod second = new DateTimePeriod(CurrentPeriod.StartDateTime.AddHours(2), CurrentPeriod.StartDateTime.AddHours(4));
            MainShift mainShift = new MainShift(ShiftCategoryFactory.CreateShiftCategory("for test"));
            ActivityLayer nowLayer = new MainShiftActivityLayer(ActivityFactory.CreateActivity("test"), first);
            ActivityLayer nextLayer = new MainShiftActivityLayer(ActivityFactory.CreateActivity("testAnother"), second);
            mainShift.LayerCollection.Add(nowLayer);
            mainShift.LayerCollection.Add(nextLayer);
            part.AddMainShift(mainShift);
            return part;
        }

        public ISchedulePart CreatePartWithMainShiftWithDifferentActivities()
        {
            ISchedulePart part = createPart();
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

        public ISchedulePart CreateSchedulePartWithMainShiftAndAbsence()
        {
            ISchedulePart part = CreatePartWithMainShiftWithDifferentActivities();
            IPersonAbsence pa = PersonAbsenceFactory.CreatePersonAbsence(part.Person, part.Scenario, part.Period);
            part.Add(pa);
            return part;
        }

        public ISchedulePart CreateSimplePart()
        {
            return createPart();
        }

    }
}


