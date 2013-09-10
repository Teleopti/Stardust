﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[TestFixture]
	public class WorkTimeMinMaxCalculatorTest
	{
		[Test]
		public void ShouldReturnMinMaxWorkTimeFromRuleSetBag()
		{
			var ruleSetBag = MockRepository.GenerateMock<IRuleSetBag>();
			var personPeriod = MockRepository.GenerateMock<IPersonPeriod>();
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var effectiveRestrictionForDisplayCreator = MockRepository.GenerateMock<IEffectiveRestrictionForDisplayCreator>();
			var effectiveRestriction = MockRepository.GenerateMock<IEffectiveRestriction>();
			var person = MockRepository.GenerateMock<IPerson>();
			var workShiftWorkTime = MockRepository.GenerateMock<IWorkShiftWorkTime>();
			var workTimeMineMax = new WorkTimeMinMax();

			scheduleDay.Stub(x => x.Person).Return(person);
			scheduleDay.Stub(x => x.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfoFactory.StockholmTimeZoneInfo()));
			person.Stub(x => x.Period(DateOnly.Today)).Return(personPeriod);
			personPeriod.Stub(x => x.RuleSetBag).Return(ruleSetBag);
			effectiveRestrictionForDisplayCreator.Stub(x => x.MakeEffectiveRestriction(scheduleDay, EffectiveRestrictionOptions.UseAll()))
				.Return(effectiveRestriction);
			ruleSetBag.Stub(x => x.MinMaxWorkTime(workShiftWorkTime, DateOnly.Today, effectiveRestriction))
				.Return(workTimeMineMax);

			var target = new WorkTimeMinMaxCalculator(workShiftWorkTime, new WorkTimeMinMaxRestrictionCreator(effectiveRestrictionForDisplayCreator));
			var result = target.WorkTimeMinMax(DateOnly.Today, person, scheduleDay);

			result.WorkTimeMinMax.Should().Be.EqualTo(workTimeMineMax);
		}

		[Test]
		public static void ShouldReturnIfRestrictionCouldNeverMatchWithAnyShifts()
		{
			var ruleSetBag = MockRepository.GenerateMock<IRuleSetBag>();
			var personPeriod = MockRepository.GenerateMock<IPersonPeriod>();
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var effectiveRestrictionForDisplayCreator = MockRepository.GenerateMock<IEffectiveRestrictionForDisplayCreator>();
			var effectiveRestriction = MockRepository.GenerateMock<IEffectiveRestriction>();
			var person = MockRepository.GenerateMock<IPerson>();
			var workShiftWorkTime = MockRepository.GenerateMock<IWorkShiftWorkTime>();
			var workTimeMineMax = new WorkTimeMinMax();

			scheduleDay.Stub(x => x.Person).Return(person);
			scheduleDay.Stub(x => x.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfoFactory.StockholmTimeZoneInfo()));
            person.Stub(x => x.Period(DateOnly.Today)).Return(personPeriod);
			personPeriod.Stub(x => x.RuleSetBag).Return(ruleSetBag);
			effectiveRestriction.Stub(x => x.ShiftCategory).Return(new ShiftCategory("Kategori"));
			effectiveRestriction.Stub(x => x.MayMatchWithShifts()).Return(false);
			effectiveRestrictionForDisplayCreator.Stub(x => x.MakeEffectiveRestriction(scheduleDay, EffectiveRestrictionOptions.UseAll()))
				.Return(effectiveRestriction);
			ruleSetBag.Stub(x => x.MinMaxWorkTime(workShiftWorkTime, DateOnly.Today, effectiveRestriction))
				.Return(workTimeMineMax);

			var target = new WorkTimeMinMaxCalculator(workShiftWorkTime, new WorkTimeMinMaxRestrictionCreator(effectiveRestrictionForDisplayCreator));
			var result = target.WorkTimeMinMax(DateOnly.Today, person, scheduleDay);
			result.RestrictionNeverHadThePossibilityToMatchWithShifts.Should().Be(true);
		}

		[Test]
		public void ShouldReturnNullIfNoPersonPeriod()
		{
			var person = MockRepository.GenerateMock<IPerson>();
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();

			scheduleDay.Stub(x => x.Person).Return(person);
			scheduleDay.Stub(x => x.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfoFactory.StockholmTimeZoneInfo()));
            person.Stub(x => x.Period(DateOnly.Today)).Return(null);

			var target = new WorkTimeMinMaxCalculator(null, null);
			var result = target.WorkTimeMinMax(DateOnly.Today, person, scheduleDay);

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnNullIfNoRuleSetBag()
		{
			var person = MockRepository.GenerateMock<IPerson>();
			var personPeriod = MockRepository.GenerateMock<IPersonPeriod>();
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();

			scheduleDay.Stub(x => x.Person).Return(person);
			scheduleDay.Stub(x => x.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(DateOnly.Today, TimeZoneInfoFactory.StockholmTimeZoneInfo()));
            person.Stub(x => x.Period(DateOnly.Today)).Return(personPeriod);
			personPeriod.Stub(x => x.RuleSetBag).Return(null);

			var target = new WorkTimeMinMaxCalculator(null, null);
			var result = target.WorkTimeMinMax(DateOnly.Today, person, scheduleDay);

			result.Should().Be.Null();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WorkDay"), Test]
		public void ShouldReturnWorkTimeFromContractForAbsencePreferenceOnWorkDay()
		{
			var person = PersonFactory.CreatePerson();
			var ruleSetBag = MockRepository.GenerateMock<IRuleSetBag>();
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var personContract = PersonContractFactory.CreateFulltimePersonContractWithWorkingWeekContractSchedule();
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2012, 01, 01), personContract, new Team());
			var effectiveRestrictionForDisplayCreator = MockRepository.GenerateMock<IEffectiveRestrictionForDisplayCreator>();
			var effectiveRestriction = MockRepository.GenerateMock<IEffectiveRestriction>();
			personPeriod.RuleSetBag = ruleSetBag;
			person.AddPersonPeriod(personPeriod);
			scheduleDay.Stub(x => x.Person).Return(person);
			scheduleDay.Stub(x => x.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2012,01,02), TimeZoneInfoFactory.StockholmTimeZoneInfo()));
			effectiveRestrictionForDisplayCreator.Stub(x => x.MakeEffectiveRestriction(scheduleDay, EffectiveRestrictionOptions.UseAll())).Return(effectiveRestriction);
			effectiveRestriction.Stub(x => x.Absence).Return(new Absence {InContractTime = true});

			var averageWorkTime = new TimeSpan((long) (personContract.Contract.WorkTime.AvgWorkTimePerDay.Ticks*personContract.PartTimePercentage.Percentage.Value));
			var expected = new WorkTimeMinMax {WorkTimeLimitation = new WorkTimeLimitation(averageWorkTime, averageWorkTime)};

			var target = new WorkTimeMinMaxCalculator(null, new WorkTimeMinMaxRestrictionCreator(effectiveRestrictionForDisplayCreator));
			var result = target.WorkTimeMinMax(DateOnly.Today, person, scheduleDay);

			result.WorkTimeMinMax.Should().Be.EqualTo(expected);
		}

        [Test]
        public void ShouldReturnWorkTimeFromSchedulePeriodForAbsencePreference()
        {
            var person = PersonFactory.CreatePerson();
            var ruleSetBag = MockRepository.GenerateMock<IRuleSetBag>();
            var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
            var personContract = PersonContractFactory.CreateFulltimePersonContractWithWorkingWeekContractSchedule();
            personContract.Contract.WorkTimeSource = WorkTimeSource.FromSchedulePeriod;
            var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2012, 01, 01), personContract, new Team());
            var effectiveRestrictionForDisplayCreator = MockRepository.GenerateMock<IEffectiveRestrictionForDisplayCreator>();
            var effectiveRestriction = MockRepository.GenerateMock<IEffectiveRestriction>();
            personPeriod.RuleSetBag = ruleSetBag;
            person.AddPersonPeriod(personPeriod);
            var schedulePeriod = SchedulePeriodFactory.CreateSchedulePeriod(new DateOnly(2012, 01, 01));
            schedulePeriod.AverageWorkTimePerDayOverride = TimeSpan.FromHours(6);
            person.AddSchedulePeriod(schedulePeriod);
            scheduleDay.Stub(x => x.Person).Return(person);
            scheduleDay.Stub(x => x.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2012, 01, 02), TimeZoneInfoFactory.StockholmTimeZoneInfo()));
            effectiveRestrictionForDisplayCreator.Stub(x => x.MakeEffectiveRestriction(scheduleDay, EffectiveRestrictionOptions.UseAll())).Return(effectiveRestriction);
            effectiveRestriction.Stub(x => x.Absence).Return(new Absence { InContractTime = true });

            var averageWorkTime = new TimeSpan((long)(TimeSpan.FromHours(6).Ticks * personContract.PartTimePercentage.Percentage.Value));
            var expected = new WorkTimeMinMax { WorkTimeLimitation = new WorkTimeLimitation(averageWorkTime, averageWorkTime) };

			var target = new WorkTimeMinMaxCalculator(null, new WorkTimeMinMaxRestrictionCreator(effectiveRestrictionForDisplayCreator));
            var result = target.WorkTimeMinMax(DateOnly.Today, person, scheduleDay);

            result.WorkTimeMinMax.Should().Be.EqualTo(expected);
        }

		[Test]
		public void ShouldReturnNullWorkTimeForAbsencePreferenceOnDayOff()
		{
			var person = PersonFactory.CreatePerson();
			var ruleSetBag = MockRepository.GenerateMock<IRuleSetBag>();
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var personContract = PersonContractFactory.CreateFulltimePersonContractWithWorkingWeekContractSchedule();
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2012, 01, 01), personContract, new Team());
			var effectiveRestrictionForDisplayCreator = MockRepository.GenerateMock<IEffectiveRestrictionForDisplayCreator>();
			var effectiveRestriction = MockRepository.GenerateMock<IEffectiveRestriction>();
			personPeriod.RuleSetBag = ruleSetBag;
			person.AddPersonPeriod(personPeriod);
			scheduleDay.Stub(x => x.Person).Return(person);
			scheduleDay.Stub(x => x.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2012, 01, 07), TimeZoneInfoFactory.StockholmTimeZoneInfo()));
			effectiveRestrictionForDisplayCreator.Stub(x => x.MakeEffectiveRestriction(scheduleDay, EffectiveRestrictionOptions.UseAll())).Return(effectiveRestriction);
			effectiveRestriction.Stub(x => x.Absence).Return(new Absence { InContractTime = true });

			var target = new WorkTimeMinMaxCalculator(null, new WorkTimeMinMaxRestrictionCreator(effectiveRestrictionForDisplayCreator));
			var result = target.WorkTimeMinMax(DateOnly.Today, person, scheduleDay);

			result.WorkTimeMinMax.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnNullWorkTimeForAbsencePreferenceNotInContractTime()
		{
			var person = PersonFactory.CreatePerson();
			var ruleSetBag = MockRepository.GenerateMock<IRuleSetBag>();
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var personContract = PersonContractFactory.CreateFulltimePersonContractWithWorkingWeekContractSchedule();
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2012, 01, 01), personContract, new Team());
			var effectiveRestrictionForDisplayCreator = MockRepository.GenerateMock<IEffectiveRestrictionForDisplayCreator>();
			var effectiveRestriction = MockRepository.GenerateMock<IEffectiveRestriction>();
			personPeriod.RuleSetBag = ruleSetBag;
			person.AddPersonPeriod(personPeriod);
			scheduleDay.Stub(x => x.Person).Return(person);
			scheduleDay.Stub(x => x.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2012, 01, 02), TimeZoneInfoFactory.StockholmTimeZoneInfo()));
			effectiveRestrictionForDisplayCreator.Stub(x => x.MakeEffectiveRestriction(scheduleDay, EffectiveRestrictionOptions.UseAll())).Return(effectiveRestriction);
			effectiveRestriction.Stub(x => x.Absence).Return(new Absence { InContractTime = false });

			var target = new WorkTimeMinMaxCalculator(null, new WorkTimeMinMaxRestrictionCreator(effectiveRestrictionForDisplayCreator));
			var result = target.WorkTimeMinMax(DateOnly.Today, person, scheduleDay);

			result.WorkTimeMinMax.Should().Be.Null();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flag"), Test]
		public static void ShouldReturnFlagIfRestrictionNeverHadThePossibilityToMatchAShift()
		{
			var workTimeMinMaxRestrictionCreator = MockRepository.GenerateMock<IWorkTimeMinMaxRestrictionCreator>();
			var workTimeMinMaxRestriction = MockRepository.GenerateMock<IWorkTimeMinMaxRestriction>();
			var scheduleDay = new StubFactory().ScheduleDayStub();
			var personPeriod = new PersonPeriod(DateOnly.Today, new PersonContract(new Contract(" "), new PartTimePercentage(" "), new ContractSchedule(" ")), new Team()) { RuleSetBag = MockRepository.GenerateMock<IRuleSetBag>() };
			var person = new Person();
			person.AddPersonPeriod(personPeriod);
			var target = new WorkTimeMinMaxCalculator(null, workTimeMinMaxRestrictionCreator);

			workTimeMinMaxRestriction.Stub(x => x.MayMatchWithShifts()).Return(false);
			workTimeMinMaxRestrictionCreator.Stub(x => x.MakeWorkTimeMinMaxRestriction(scheduleDay, EffectiveRestrictionOptions.UseAll())).Return(new WorkTimeMinMaxRestrictionCreationResult {Restriction = workTimeMinMaxRestriction});

			var result = target.WorkTimeMinMax(DateOnly.Today, person, scheduleDay);

			result.RestrictionNeverHadThePossibilityToMatchWithShifts.Should().Be.True();
		}
	}
}
