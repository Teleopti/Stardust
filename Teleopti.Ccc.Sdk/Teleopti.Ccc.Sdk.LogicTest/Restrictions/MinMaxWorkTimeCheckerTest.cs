using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Sdk.Logic.Restrictions;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.Sdk.LogicTest.Restrictions
{
    [TestFixture]
    public class MinMaxWorkTimeCheckerTest
    {
        [Test]
        public void ShouldThrowIfRuleSetProjectionServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new MinMaxWorkTimeChecker(null));
        }

        [Test]
        public void ShouldThrowIfScheduleDayIsNull()
        {
			var workShiftWorkTime = new WorkShiftWorkTime(new RuleSetProjectionService(new ShiftCreatorService(new CreateWorkShiftsFromTemplate())));
			var target = new MinMaxWorkTimeChecker(workShiftWorkTime);
			Assert.Throws<ArgumentNullException>(() => target.MinMaxWorkTime(null, new RuleSetBag(), new EffectiveRestriction(), false));
        }

        [Test]
        public void ShouldThrowIfRuleSetBagIsNull()
		{
			var scheduleDay = new SchedulePartFactoryForDomain().CreatePart();
			var workShiftWorkTime = new WorkShiftWorkTime(new RuleSetProjectionService(new ShiftCreatorService(new CreateWorkShiftsFromTemplate())));
			var target = new MinMaxWorkTimeChecker(workShiftWorkTime);

			Assert.Throws<ArgumentNullException>(() => target.MinMaxWorkTime(scheduleDay, null, new EffectiveRestriction(), false));
        }

        [Test]
        public void GetWorkTimeShouldThrowIfScheduleDayIsNull()
		{
            Assert.Throws<ArgumentNullException>(() => MinMaxWorkTimeChecker.GetWorkTime(null));
        }

        [Test]
        public void ShouldReturnEmptyIfDayOff()
		{
			var scheduleDay = new SchedulePartFactoryForDomain().CreatePart();
			scheduleDay.PersonAssignment(true).SetDayOff(new DayOffTemplate());

			var ruleSetBag =
				new RuleSetBag(
					new WorkShiftRuleSet(new WorkShiftTemplateGenerator(new Activity("Phone"),
						new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(17, 0, 17, 0, 15), new ShiftCategory("AM"))));
			var workShiftWorkTime = new WorkShiftWorkTime(new RuleSetProjectionService(new ShiftCreatorService(new CreateWorkShiftsFromTemplate())));
			var target = new MinMaxWorkTimeChecker(workShiftWorkTime);
			
            var result = target.MinMaxWorkTime(scheduleDay, ruleSetBag, new EffectiveRestriction(), false);
            Assert.That(result.EndTimeLimitation.StartTime, Is.Null);
            Assert.That(result.EndTimeLimitation.EndTime, Is.Null);
            Assert.That(result.StartTimeLimitation.StartTime, Is.Null);
            Assert.That(result.StartTimeLimitation.EndTime, Is.Null);
            Assert.That(result.WorkTimeLimitation.StartTime, Is.Null);
            Assert.That(result.WorkTimeLimitation.EndTime, Is.Null);
        }
		
        [Test]
        public void ShouldGetMinMaxWorkTimeFromScheduleIfScheduled()
		{
			var payload1 = ActivityFactory.CreateActivity("Phone").WithId();
			payload1.InWorkTime = true;
			payload1.InContractTime = true;
			var payload2 = ActivityFactory.CreateActivity("Extra").WithId();
			payload2.InWorkTime = false;
			payload2.InContractTime = true;

			var shiftCategory = new ShiftCategory("DY").WithId();
			var dateTime = new DateTime(2010, 12, 16, 8, 0, 0, DateTimeKind.Utc);

	        var scheduleDay =
		        new SchedulePartFactoryForDomain(
			        PersonFactory.CreatePerson(new Name("Ashley", "Andeen"),
				        TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")), new DateTimePeriod(2010, 12, 16, 2010, 12, 17))
			        .CreatePart();
	        
			var personAssignment = scheduleDay.PersonAssignment(true).WithId();
	        personAssignment.SetShiftCategory(shiftCategory);
	        personAssignment.AddActivity(payload1, new DateTimePeriod(dateTime, dateTime.AddHours(9)));
	        personAssignment.AddActivity(payload2, new DateTimePeriod(dateTime.AddHours(9), dateTime.AddHours(10)));

	        var ruleSetBag =
				new RuleSetBag(
					new WorkShiftRuleSet(new WorkShiftTemplateGenerator(payload1,
						new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(17, 0, 17, 0, 15), shiftCategory)));
			var workShiftWorkTime = new WorkShiftWorkTime(new RuleSetProjectionService(new ShiftCreatorService(new CreateWorkShiftsFromTemplate())));
			var target = new MinMaxWorkTimeChecker(workShiftWorkTime);
			
            var result = target.MinMaxWorkTime(scheduleDay, ruleSetBag, new EffectiveRestriction(), false);
            Assert.That(result, Is.Not.Null);
	        result.EndTimeLimitation.StartTime.Should().Be.EqualTo(TimeSpan.FromHours(18));
	        result.EndTimeLimitation.EndTime.Should().Be.EqualTo(TimeSpan.FromHours(18));
			result.StartTimeLimitation.StartTime.Should().Be.EqualTo(TimeSpan.FromHours(9));
	        result.StartTimeLimitation.EndTime.Should().Be.EqualTo(TimeSpan.FromHours(9));
			result.WorkTimeLimitation.StartTime.Should().Be.EqualTo(TimeSpan.FromHours(9));
			result.WorkTimeLimitation.EndTime.Should().Be.EqualTo(TimeSpan.FromHours(9));
        }
		
		[Test]
		public void ShouldGetMinMaxWorkTimeFromContractTimeIfUseConctractTimeOnMainShift()
		{
			var payload1 = ActivityFactory.CreateActivity("Phone").WithId();
			payload1.InWorkTime = true;
			payload1.InContractTime = true;
			var payload2 = ActivityFactory.CreateActivity("Extra").WithId();
			payload2.InWorkTime = false;
			payload2.InContractTime = true;

			var shiftCategory = new ShiftCategory("DY").WithId();
			var dateTime = new DateTime(2010, 12, 16, 8, 0, 0, DateTimeKind.Utc);

	        var scheduleDay =
		        new SchedulePartFactoryForDomain(
			        PersonFactory.CreatePerson(new Name("Ashley", "Andeen"),
				        TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")), new DateTimePeriod(2010, 12, 16, 2010, 12, 17))
			        .CreatePart();
	        
			var personAssignment = scheduleDay.PersonAssignment(true).WithId();
	        personAssignment.SetShiftCategory(shiftCategory);
	        personAssignment.AddActivity(payload1, new DateTimePeriod(dateTime, dateTime.AddHours(9)));
	        personAssignment.AddActivity(payload2, new DateTimePeriod(dateTime.AddHours(9), dateTime.AddHours(10)));

	        var ruleSetBag =
				new RuleSetBag(
					new WorkShiftRuleSet(new WorkShiftTemplateGenerator(payload1,
						new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(17, 0, 17, 0, 15), shiftCategory)));
			var workShiftWorkTime = new WorkShiftWorkTime(new RuleSetProjectionService(new ShiftCreatorService(new CreateWorkShiftsFromTemplate())));
			var target = new MinMaxWorkTimeChecker(workShiftWorkTime);
			
            var result = target.MinMaxWorkTime(scheduleDay, ruleSetBag, new EffectiveRestriction(), true);
            Assert.That(result, Is.Not.Null);
			result.EndTimeLimitation.StartTime.Should().Be.EqualTo(TimeSpan.FromHours(18));
			result.EndTimeLimitation.EndTime.Should().Be.EqualTo(TimeSpan.FromHours(18));
			result.StartTimeLimitation.StartTime.Should().Be.EqualTo(TimeSpan.FromHours(9));
			result.StartTimeLimitation.EndTime.Should().Be.EqualTo(TimeSpan.FromHours(9));
			result.WorkTimeLimitation.StartTime.Should().Be.EqualTo(TimeSpan.FromHours(10));
			result.WorkTimeLimitation.EndTime.Should().Be.EqualTo(TimeSpan.FromHours(10));
		}

	    [Test]
	    public void ShouldGetMinMaxWorkTimeFromAverageWorkTimeOnNotAvailableDay()
	    {
		    var scheduleDay = new SchedulePartFactoryForDomain().CreatePart();
		    var ruleSetBag =
			    new RuleSetBag(
				    new WorkShiftRuleSet(new WorkShiftTemplateGenerator(new Activity("Phone"),
					    new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(17, 0, 17, 0, 15), new ShiftCategory("AM"))));
		    var workShiftWorkTime =
			    new WorkShiftWorkTime(new RuleSetProjectionService(new ShiftCreatorService(new CreateWorkShiftsFromTemplate())));
		    var target = new MinMaxWorkTimeChecker(workShiftWorkTime);
			
		    var result = target.MinMaxWorkTime(scheduleDay, ruleSetBag, new EffectiveRestriction {NotAvailable = true},
			    false);
		    Assert.That(result.WorkTimeLimitation.StartTime, Is.Null);
	    }

	    [Test]
	    public void ShouldGetMinMaxWorkTimeFromAverageWorkTimeOnAbsencePreferenceOnWorkdays()
	    {
		    var scheduleDay = new SchedulePartFactoryForDomain().CreatePart();
		    var ruleSetBag =
			    new RuleSetBag(
				    new WorkShiftRuleSet(new WorkShiftTemplateGenerator(new Activity("Phone"),
					    new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(17, 0, 17, 0, 15), new ShiftCategory("AM"))));
		    var workShiftWorkTime =
			    new WorkShiftWorkTime(new RuleSetProjectionService(new ShiftCreatorService(new CreateWorkShiftsFromTemplate())));
		    var target = new MinMaxWorkTimeChecker(workShiftWorkTime);

		    var absence = AbsenceFactory.CreateAbsence("Holiday");
		    absence.InContractTime = true;

		    var effectiveRestriction = new EffectiveRestriction {Absence = absence};

		    var result = target.MinMaxWorkTime(scheduleDay, ruleSetBag, effectiveRestriction, false);
		    Assert.That(result.WorkTimeLimitation.StartTime, Is.EqualTo(TimeSpan.FromHours(8)));
	    }

	    [Test]
	    public void ShouldGetMinMaxWorkTimeZeroOnAbsenceNotInContractTime()
	    {
		    var scheduleDay = new SchedulePartFactoryForDomain().CreatePart();
		    var ruleSetBag =
			    new RuleSetBag(
				    new WorkShiftRuleSet(new WorkShiftTemplateGenerator(new Activity("Phone"),
					    new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(17, 0, 17, 0, 15), new ShiftCategory("AM"))));
		    var workShiftWorkTime =
			    new WorkShiftWorkTime(new RuleSetProjectionService(new ShiftCreatorService(new CreateWorkShiftsFromTemplate())));
		    var target = new MinMaxWorkTimeChecker(workShiftWorkTime);

		    var absence = AbsenceFactory.CreateAbsence("Holiday");
		    absence.InContractTime = false;

		    var effectiveRestriction = new EffectiveRestriction {Absence = absence};

		    var result = target.MinMaxWorkTime(scheduleDay, ruleSetBag, effectiveRestriction, false);
		    Assert.That(result.WorkTimeLimitation.StartTime, Is.EqualTo(TimeSpan.Zero));
	    }

        [Test]
        public void ShouldGetMinMaxWorkTimeFromContractTimeOnAbsenceNotInWorkTime()
		{
			var scheduleDay = new SchedulePartFactoryForDomain().CreatePart();
			var ruleSetBag =
				new RuleSetBag(
					new WorkShiftRuleSet(new WorkShiftTemplateGenerator(new Activity("Phone"),
						new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(17, 0, 17, 0, 15), new ShiftCategory("AM"))));
			var workShiftWorkTime =
				new WorkShiftWorkTime(new RuleSetProjectionService(new ShiftCreatorService(new CreateWorkShiftsFromTemplate())));
			var target = new MinMaxWorkTimeChecker(workShiftWorkTime);

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			absence.InContractTime = true;
	        absence.InWorkTime = false;

			var effectiveRestriction = new EffectiveRestriction { Absence = absence };

			var result = target.MinMaxWorkTime(scheduleDay, ruleSetBag, effectiveRestriction, false);
			Assert.That(result.WorkTimeLimitation.StartTime, Is.EqualTo(TimeSpan.FromHours(8)));
		}

        [Test]
        public void ShouldGetMinMaxWorkTimeFromRuleSetBagIfNoSchedule()
		{
			var scheduleDay = new SchedulePartFactoryForDomain().CreatePart();
			var ruleSetBag =
				new RuleSetBag(
					new WorkShiftRuleSet(new WorkShiftTemplateGenerator(new Activity("Phone").WithId(),
						new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(17, 0, 17, 0, 15), new ShiftCategory("AM").WithId())));
			var workShiftWorkTime = new WorkShiftWorkTime(new RuleSetProjectionService(new ShiftCreatorService(new CreateWorkShiftsFromTemplate())));
			var target = new MinMaxWorkTimeChecker(workShiftWorkTime);
			
            var result = target.MinMaxWorkTime(scheduleDay, ruleSetBag, new EffectiveRestriction(), false);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void ShouldConsiderPartTimePercentageIfRestrictionIsAbsence()
		{
			var scheduleDay = new SchedulePartFactoryForDomain().CreatePart();
			
            IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
                                                                                  new EndTimeLimitation(),
                                                                                  new WorkTimeLimitation(), null, null, null,
                                                                                  new List<IActivityRestriction>());
            effectiveRestriction.Absence = AbsenceFactory.CreateRequestableAbsence("hej", "hh", Color.Black);
            effectiveRestriction.Absence.InContractTime = true;

			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly());
			scheduleDay.Person.RemoveAllPersonPeriods();
			scheduleDay.Person.AddPersonPeriod(personPeriod);
        	
            IPersonContract personContract = PersonContractFactory.CreatePersonContract();
            IContract contract = ContractFactory.CreateContract("Hej");
            contract.WorkTime= new WorkTime(TimeSpan.FromHours(8));
            IPartTimePercentage partTimePercentage = new PartTimePercentage("Hej");
            partTimePercentage.Percentage = new Percent(0.5);
            personContract.Contract = contract;
            personContract.PartTimePercentage = partTimePercentage;
            personPeriod.PersonContract = personContract;
            
            IWorkTimeMinMax result = MinMaxWorkTimeChecker.GetWorkTimeAbsencePreference(scheduleDay, effectiveRestriction);
            
            Assert.AreEqual(TimeSpan.FromHours(4), result.WorkTimeLimitation.StartTime);
            Assert.AreEqual(TimeSpan.FromHours(4), result.WorkTimeLimitation.EndTime);
        }
	}
}