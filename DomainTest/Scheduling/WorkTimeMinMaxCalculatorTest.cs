using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Time;
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
			scheduleDay.Stub(x => x.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(DateOnly.Today, CccTimeZoneInfoFactory.StockholmTimeZoneInfo()));
			person.Stub(x => x.PersonPeriods(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today))).Return(new[] { personPeriod });
			personPeriod.Stub(x => x.RuleSetBag).Return(ruleSetBag);
			effectiveRestrictionForDisplayCreator.Stub(x => x.GetEffectiveRestrictionForDisplay(scheduleDay, new EffectiveRestrictionOptions(true, true)))
				.Return(effectiveRestriction);
			ruleSetBag.Stub(x => x.MinMaxWorkTime(workShiftWorkTime, DateOnly.Today, effectiveRestriction))
				.Return(workTimeMineMax);

			var target = new WorkTimeMinMaxCalculator(workShiftWorkTime, effectiveRestrictionForDisplayCreator);
			PreferenceType? preferenceType;
			var result = target.WorkTimeMinMax(DateOnly.Today, person, scheduleDay, out preferenceType);

			result.Should().Be.EqualTo(workTimeMineMax);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldSetPreferenceTypeCorrect()
		{
			var ruleSetBag = MockRepository.GenerateMock<IRuleSetBag>();
			var personPeriod = MockRepository.GenerateMock<IPersonPeriod>();
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var effectiveRestrictionForDisplayCreator = MockRepository.GenerateMock<IEffectiveRestrictionForDisplayCreator>();
			var effectiveRestriction = MockRepository.GenerateMock<IEffectiveRestriction>();
			effectiveRestriction.Stub(x => x.ShiftCategory).Return(new ShiftCategory("Kategori"));
			var person = MockRepository.GenerateMock<IPerson>();
			var workShiftWorkTime = MockRepository.GenerateMock<IWorkShiftWorkTime>();
			var workTimeMineMax = new WorkTimeMinMax();

			scheduleDay.Stub(x => x.Person).Return(person);
			scheduleDay.Stub(x => x.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(DateOnly.Today, CccTimeZoneInfoFactory.StockholmTimeZoneInfo()));
			person.Stub(x => x.PersonPeriods(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today))).Return(new[] { personPeriod });
			personPeriod.Stub(x => x.RuleSetBag).Return(ruleSetBag);
			effectiveRestrictionForDisplayCreator.Stub(x => x.GetEffectiveRestrictionForDisplay(scheduleDay, new EffectiveRestrictionOptions(true, true)))
				.Return(effectiveRestriction);
			ruleSetBag.Stub(x => x.MinMaxWorkTime(workShiftWorkTime, DateOnly.Today, effectiveRestriction))
				.Return(workTimeMineMax);

			var target = new WorkTimeMinMaxCalculator(workShiftWorkTime, effectiveRestrictionForDisplayCreator);
			PreferenceType? preferenceType;
			target.WorkTimeMinMax(DateOnly.Today, person, scheduleDay, out preferenceType);
			preferenceType.Should().Be.EqualTo(PreferenceType.ShiftCategory);
		}

		[Test]
		public void ShouldReturnNullIfNoPersonPeriod()
		{
			var person = MockRepository.GenerateMock<IPerson>();
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();

			scheduleDay.Stub(x => x.Person).Return(person);
			scheduleDay.Stub(x => x.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(DateOnly.Today, CccTimeZoneInfoFactory.StockholmTimeZoneInfo()));
			person.Stub(x => x.PersonPeriods(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today))).Return(new List<IPersonPeriod>());

			var target = new WorkTimeMinMaxCalculator(null, null);
			PreferenceType? preferenceType;
			var result = target.WorkTimeMinMax(DateOnly.Today, person, scheduleDay, out preferenceType);

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnNullIfNoRuleSetBag()
		{
			var person = MockRepository.GenerateMock<IPerson>();
			var personPeriod = MockRepository.GenerateMock<IPersonPeriod>();
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();

			scheduleDay.Stub(x => x.Person).Return(person);
			scheduleDay.Stub(x => x.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(DateOnly.Today, CccTimeZoneInfoFactory.StockholmTimeZoneInfo()));
			person.Stub(x => x.PersonPeriods(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today))).Return(new[] { personPeriod });
			personPeriod.Stub(x => x.RuleSetBag).Return(null);

			var target = new WorkTimeMinMaxCalculator(null, null);
			PreferenceType? preferenceType;
			var result = target.WorkTimeMinMax(DateOnly.Today, person, scheduleDay, out preferenceType);

			result.Should().Be.Null();
		}
		
		[Test]
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
			scheduleDay.Stub(x => x.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2012,01,02), CccTimeZoneInfoFactory.StockholmTimeZoneInfo()));
			effectiveRestrictionForDisplayCreator.Stub(x => x.GetEffectiveRestrictionForDisplay(scheduleDay, new EffectiveRestrictionOptions(true, true))).Return(effectiveRestriction);
			effectiveRestriction.Stub(x => x.Absence).Return(new Absence(){InContractTime = true});

			var averageWorkTime = new TimeSpan((long) (personContract.Contract.WorkTime.AvgWorkTimePerDay.Ticks*personContract.PartTimePercentage.Percentage.Value));
			var expected = new WorkTimeMinMax() {WorkTimeLimitation = new WorkTimeLimitation(averageWorkTime, averageWorkTime)};
			
			var target = new WorkTimeMinMaxCalculator(null, effectiveRestrictionForDisplayCreator);
			PreferenceType? preferenceType;
			var result = target.WorkTimeMinMax(DateOnly.Today, person, scheduleDay, out preferenceType);

			result.Should().Be.EqualTo(expected);
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
			scheduleDay.Stub(x => x.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2012, 01, 07), CccTimeZoneInfoFactory.StockholmTimeZoneInfo()));
			effectiveRestrictionForDisplayCreator.Stub(x => x.GetEffectiveRestrictionForDisplay(scheduleDay, new EffectiveRestrictionOptions(true, true))).Return(effectiveRestriction);
			effectiveRestriction.Stub(x => x.Absence).Return(new Absence() { InContractTime = true });

			var target = new WorkTimeMinMaxCalculator(null, effectiveRestrictionForDisplayCreator);
			PreferenceType? preferenceType;
			var result = target.WorkTimeMinMax(DateOnly.Today, person, scheduleDay, out preferenceType);

			result.Should().Be.Null();
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
			scheduleDay.Stub(x => x.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2012, 01, 02), CccTimeZoneInfoFactory.StockholmTimeZoneInfo()));
			effectiveRestrictionForDisplayCreator.Stub(x => x.GetEffectiveRestrictionForDisplay(scheduleDay, new EffectiveRestrictionOptions(true, true))).Return(effectiveRestriction);
			effectiveRestriction.Stub(x => x.Absence).Return(new Absence() { InContractTime = false });

			var target = new WorkTimeMinMaxCalculator(null, effectiveRestrictionForDisplayCreator);
			PreferenceType? preferenceType;
			var result = target.WorkTimeMinMax(DateOnly.Today, person, scheduleDay, out preferenceType);

			result.Should().Be.Null();
		}
	}
}
