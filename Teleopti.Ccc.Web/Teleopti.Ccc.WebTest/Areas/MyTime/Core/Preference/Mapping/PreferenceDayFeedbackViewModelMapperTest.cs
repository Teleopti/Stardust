using System;
using System.Globalization;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.WebTest.Core.IoC;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.Mapping
{
	[TestFixture]
	[MyTimeWebTest]
	[SetCulture("sv-SE")]
	public class PreferenceDayFeedbackViewModelMapperTest
	{
		public PreferenceDayFeedbackViewModelMapper Target;
		public FakeRuleSetBagRepository RuleSetBagRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public FakePreferenceDayRepository PreferenceDayRepository;

		[Test]
		public void ShouldMapDate()
		{
			var result = Target.Map(DateOnly.Today);
			result.Date.Should().Be.EqualTo(DateOnly.Today.ToFixedClientDateOnlyFormat());
		}

		[Test]
		public void ShouldMapPossibleContractTimeMinutesLower()
		{
			var shiftCategory = new ShiftCategory("test").WithId();
			var activity = new Activity("test");
			var ruleSetBag = RuleSetBagRepository.Has(
				new RuleSetBag(new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
					new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory)))
				{
					Description = new Description("_")
				}.WithId());

			var team = TeamFactory.CreateTeamWithId("test");
			var personPeriod = new PersonPeriod(DateOnly.Today.AddDays(-1)
				, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team)
			{
				RuleSetBag = ruleSetBag
			};
			LoggedOnUser.CurrentUser().AddPersonPeriod(personPeriod);

			var result = Target.Map(DateOnly.Today);

			result.PossibleContractTimeMinutesLower.Should()
				.Be(TimeSpan.FromHours(8).TotalMinutes.ToString(CultureInfo.InvariantCulture));
		}

		[Test]
		public void ShouldMapPossibleContractTimeMinutesUpper()
		{
			var shiftCategory = new ShiftCategory("test").WithId();
			var activity = new Activity("test");
			var ruleSetBag = RuleSetBagRepository.Has(
				new RuleSetBag(new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
					new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 20, 0, 15), shiftCategory)))
				{
					Description = new Description("_")
				}.WithId());

			var team = TeamFactory.CreateTeamWithId("test");
			var personPeriod = new PersonPeriod(DateOnly.Today.AddDays(-1)
				, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team)
			{
				RuleSetBag = ruleSetBag
			};
			LoggedOnUser.CurrentUser().AddPersonPeriod(personPeriod);

			var result = Target.Map(DateOnly.Today);

			result.PossibleContractTimeMinutesUpper.Should()
				.Be(TimeSpan.FromHours(12).TotalMinutes.ToString(CultureInfo.InvariantCulture));
		}

		[Test]
		public void ShouldMapPossibleEndTimes()
		{
			var shiftCategory = new ShiftCategory("test").WithId();
			var activity = new Activity("test");
			var ruleSetBag = RuleSetBagRepository.Has(
				new RuleSetBag(new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
					new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 20, 0, 15), shiftCategory)))
				{
					Description = new Description("_")
				}.WithId());

			var team = TeamFactory.CreateTeamWithId("test");
			var personPeriod = new PersonPeriod(DateOnly.Today.AddDays(-1)
				, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team)
			{
				RuleSetBag = ruleSetBag
			};
			LoggedOnUser.CurrentUser().AddPersonPeriod(personPeriod);

			var result = Target.Map(DateOnly.Today);
			result.PossibleEndTimes.Should()
				.Be("16:00-20:00");
		}

		[Test]
		public void ShouldMapPossibleStartTimes()
		{
			var shiftCategory = new ShiftCategory("test").WithId();
			var activity = new Activity("test");
			var ruleSetBag = RuleSetBagRepository.Has(
				new RuleSetBag(new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
					new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 20, 0, 15), shiftCategory)))
				{
					Description = new Description("_")
				}.WithId());

			var team = TeamFactory.CreateTeamWithId("test");
			var personPeriod = new PersonPeriod(DateOnly.Today.AddDays(-1)
				, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team)
			{
				RuleSetBag = ruleSetBag
			};
			LoggedOnUser.CurrentUser().AddPersonPeriod(personPeriod);

			var result = Target.Map(DateOnly.Today);
			result.PossibleStartTimes.Should().Be("08:00-08:00");
		}

		[Test]
		public void ShouldMapValidationErrors()
		{
			var result = Target.Map(DateOnly.Today);
			result.FeedbackError.Should().Be(Resources.NoAvailableShifts);
		}

		[Test]
		public void ShouldValidateNightlyRestBasedOnPossibleShifts()
		{
			var shiftCategory = new ShiftCategory("test").WithId();
			var activity = new Activity("test");
			var ruleSetBag = RuleSetBagRepository.Has(
				new RuleSetBag(new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
					new TimePeriodWithSegment(15, 0, 15, 0, 15), new TimePeriodWithSegment(22, 0, 22, 0, 15), shiftCategory)))
				{
					Description = new Description("_")
				}.WithId());

			var team = TeamFactory.CreateTeamWithId("test");
			var personPeriod = new PersonPeriod(new DateOnly(2010, 7, 30)
				, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team)
			{
				RuleSetBag = ruleSetBag
			};
			LoggedOnUser.CurrentUser().AddPersonPeriod(personPeriod);

			var preferenceDayOne = new PreferenceDay(LoggedOnUser.CurrentUser(), new DateOnly(2018, 7, 31),
				new PreferenceRestriction {
					StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(13),null),
					EndTimeLimitation = new EndTimeLimitation(null,TimeSpan.FromHours(22)),
					WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(1), TimeSpan.FromHours(8)) });
			PreferenceDayRepository.Add(preferenceDayOne);

			var preferenceDayTwo = new PreferenceDay(LoggedOnUser.CurrentUser(), new DateOnly(2018, 8, 1),
				new PreferenceRestriction
				{
					StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(7), TimeSpan.FromHours(7)),
					EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(16), TimeSpan.FromHours(16)),
					WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8))
				});
			PreferenceDayRepository.Add(preferenceDayTwo);

			var result = Target.Map(new DateOnly(2018, 7, 31));

			result.HasNightRestViolationToNextDay.Should().Be(true);
		}

		[Test]
		public void ShouldValidateNightlyRestBasedOnInputTimeWhenThereIsNoPossibleShift()
		{
			
			var team = TeamFactory.CreateTeamWithId("test");
			var personPeriod = new PersonPeriod(new DateOnly(2010, 7, 30)
				, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), team)
			{
				RuleSetBag = null
			};
			LoggedOnUser.CurrentUser().AddPersonPeriod(personPeriod);

			var preferenceDayOne = new PreferenceDay(LoggedOnUser.CurrentUser(), new DateOnly(2018, 7, 31),
				new PreferenceRestriction
				{
					StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(13), null),
					EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(22), TimeSpan.FromHours(22)),
					WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(1), TimeSpan.FromHours(8))
				});
			PreferenceDayRepository.Add(preferenceDayOne);

			var preferenceDayTwo = new PreferenceDay(LoggedOnUser.CurrentUser(), new DateOnly(2018, 8, 1),
				new PreferenceRestriction
				{
					StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(7), TimeSpan.FromHours(7)),
					EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(16), TimeSpan.FromHours(16)),
					WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(8))
				});
			PreferenceDayRepository.Add(preferenceDayTwo);

			var result = Target.Map(new DateOnly(2018, 8, 1));

			result.HasNightRestViolationToPreviousDay.Should().Be(true);
		}

	}
}