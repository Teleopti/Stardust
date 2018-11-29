using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;


namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
	[DomainTest]
	public class NewBusinessRuleCollectionTest
	{
		private INewBusinessRuleCollection _target;
		private const int totalNumberOfRules = 11;
		private ISchedulingResultStateHolder _state;

		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		private void setup()
		{
			_target = NewBusinessRuleCollection.All(new SchedulingResultStateHolder());
		}

		[Test]
		public void VerifyAll()
		{
			setup();
			//rk: kolla istället vilka IBusiness rules-typer som finns i domain... orkar inte just nu
			var rule = _target.Item(typeof(NewShiftCategoryLimitationRule));
			Assert.AreEqual(totalNumberOfRules, _target.Count);
			Assert.IsTrue(rule.HaltModify);
		}

		[Test]
		public void ShouldGetCorrectRulesFromFlag()
		{
			setup();
			const BusinessRuleFlags flag = BusinessRuleFlags.MinWeekWorkTimeRule
										   | BusinessRuleFlags.NewMaxWeekWorkTimeRule
										   | BusinessRuleFlags.DataPartOfAgentDay;
			var rules = NewBusinessRuleCollection.GetRuleDescriptionsFromFlag(flag).ToList();
			Assert.AreEqual(rules.Count, 3);
			Assert.Greater(rules.IndexOf("MinWeeklyWorkTime"), -1);
			Assert.Greater(rules.IndexOf("WeeklyWorkTime"), -1);
			Assert.Greater(rules.IndexOf("NotAllowedChange"), -1);
			Assert.AreEqual(rules.IndexOf("ShiftCategory"), -1);
		}

		[Test]
		public void ShouldGetMaximumWorkdayRuleFromFlag()
		{
			setup();
			const BusinessRuleFlags flag = BusinessRuleFlags.MaximumContinuousWorkTimeRule;

			var rules = NewBusinessRuleCollection.GetRuleDescriptionsFromFlag(flag).ToList();

			Assert.AreEqual(rules.Count, 1);
			Assert.Greater(rules.IndexOf("MaximumContinuousWorkTimeRuleName"), -1);
		}

		[Test]
		public void ShouldGetMaximumWorkdayRuleFromRules()
		{
			setup();
			const BusinessRuleFlags expectedFlag = BusinessRuleFlags.MaximumContinuousWorkTimeRule;

			var rules = new List<Type> { typeof(MaximumWorkdayRule) };

			var flag = NewBusinessRuleCollection.GetFlagFromRules(rules);
			Assert.AreEqual(flag, expectedFlag);
		}

		[Test]
		public void ShouldGetCorrectFlagFromRules()
		{
			setup();
			const BusinessRuleFlags expectedFlag = BusinessRuleFlags.MinWeekWorkTimeRule
												   | BusinessRuleFlags.NewMaxWeekWorkTimeRule
												   | BusinessRuleFlags.DataPartOfAgentDay;

			var rules = new List<Type>
			{
				typeof(NewMaxWeekWorkTimeRule),
				typeof(MinWeekWorkTimeRule),
				typeof(DataPartOfAgentDay)
			};

			var flag = NewBusinessRuleCollection.GetFlagFromRules(rules);
			Assert.AreEqual(flag, expectedFlag);
		}

		[Test, SetUICulture("en-GB")]
		public void ShouldSetCulture()
		{
			setup();
			_target.SetUICulture(CultureInfo.GetCultureInfo(1053));
			Assert.AreEqual("sv-SE", _target.UICulture.Name);
		}

		[Test, SetUICulture("en-GB")]
		public void MessageOfRespsonseShouldMatchCultureSet()
		{
			var rules = NewBusinessRuleCollection.Minimum();
			rules.Add(new DummyRule(true));
			rules.DoNotHaltModify(typeof(DataPartOfAgentDay));

			rules.SetUICulture(CultureInfo.GetCultureInfo(1053));
			var responses = rules.CheckRules(new Dictionary<IPerson, IScheduleRange>(), new List<IScheduleDay>()).First();
			
			Assert.AreEqual(responses.FriendlyName, "Icke överskrivningsbar aktivitet överskriven");
			Assert.AreEqual(responses.Message, "Det finns två överlappande skift.");
		}

		[Test]
		public void VerifyMinimum()
		{
			setup();
			var targetSmall = NewBusinessRuleCollection.Minimum();
			foreach (var rule in _target)
			{
				Assert.AreEqual(rule.IsMandatory, collectionContainsType(targetSmall, rule.GetType()));
			}
		}

		[Test]
		public void VerifyRemoveBusinessRuleResponse()
		{
			setup();
			var rule = _target.Item(typeof(NewShiftCategoryLimitationRule));
			var dateOnlyPeriod = new DateOnlyPeriod();
			_target.DoNotHaltModify(new BusinessRuleResponse(typeof(NewShiftCategoryLimitationRule), "d", false, false,
				new DateTimePeriod(2000, 1, 1, 2000, 1, 2), new Person(), dateOnlyPeriod, "tjillevippen"));
			Assert.AreEqual(totalNumberOfRules, _target.Count);
			Assert.IsFalse(rule.HaltModify);
		}

		[Test]
		public void RemovingMandatoryRuleShouldResultDoingNothing()
		{
			setup();
			_target.Add(new DummyRule(true));
			_target.DoNotHaltModify(typeof(DummyRule));
			Assert.AreEqual(totalNumberOfRules + 1, _target.Count);
		}

		[Test]
		public void VerifyClearKeepsMandatory()
		{
			setup();
			_target.Add(new DummyRule(true));
			_target.Clear();
			Assert.AreEqual(totalNumberOfRules + 1, _target.Count);
		}

		[Test]
		public void NewOverlappingAssignmentRuleHasSetPropertyForDeleteInAllRules()
		{
			setup();
			var rulesForDelete = NewBusinessRuleCollection.AllForDelete(new SchedulingResultStateHolder());
			foreach (var rule in rulesForDelete)
			{
				if (rule is DataPartOfAgentDay)
					Assert.IsTrue(rule.ForDelete);
				else
					Assert.IsFalse(rule.ForDelete);
			}
		}

		[Test]
		public void MinimumAndPersonAccountShouldContainPersonAccountRule()
		{
			setup();
			_state = new SchedulingResultStateHolder();
			var miniAndPa = NewBusinessRuleCollection.MinimumAndPersonAccount(_state, _state.AllPersonAccounts);
			Assert.That(miniAndPa.Count, Is.EqualTo(NewBusinessRuleCollection.Minimum().Count + 1));
			Assert.That(collectionContainsType(miniAndPa, typeof(NewPersonAccountRule)));
		}

		[Test]
		public void AllForSchedulingShouldConsiderUseValidation()
		{
			setup();
			_state = new SchedulingResultStateHolder
			{
				UseValidation = true
			};
			var allForScheduling = NewBusinessRuleCollection.AllForScheduling(_state);
			Assert.AreEqual(totalNumberOfRules, allForScheduling.Count);
			_state.UseValidation = false;
			allForScheduling = NewBusinessRuleCollection.AllForScheduling(_state);
			Assert.AreEqual(NewBusinessRuleCollection.Minimum().Count + 1, allForScheduling.Count);
			Assert.IsFalse(collectionContainsType(allForScheduling, typeof(MinWeekWorkTimeRule)));
		}

		[Test]
		public void ShouldConsiderUseMinWorktimePerWeek()
		{
			setup();
			_state = new SchedulingResultStateHolder
			{
				UseValidation = true
			};
			var allForScheduling = NewBusinessRuleCollection.AllForScheduling(_state);
			Assert.AreEqual(totalNumberOfRules, allForScheduling.Count);
			Assert.IsTrue(collectionContainsType(allForScheduling, typeof(MinWeekWorkTimeRule)));
		}

		[Test]
		public void ShouldConsiderUseMaximumWorkday()
		{
			setup();
			_state = new SchedulingResultStateHolder
			{
				UseMaximumWorkday = true,
				UseValidation = true
			};
			var allForScheduling = NewBusinessRuleCollection.AllForScheduling(_state);
			Assert.AreEqual(totalNumberOfRules + 1, allForScheduling.Count);
			Assert.IsTrue(collectionContainsType(allForScheduling, typeof(MaximumWorkdayRule)));
		}

		[Test]
		public void ShouldHandleAgentWithNoAssignment()
		{
			setup();
			var scenario = new Scenario("_");
			var dateOnly = new DateOnly(2016, 05, 23);

			var agent = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(new Person(), dateOnly);
			var schedulePeriod = agent.SchedulePeriod(dateOnly);
			schedulePeriod.PeriodType = SchedulePeriodType.Week;

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(dateOnly, 1),
				new[] {agent}, Enumerable.Empty<IScheduleData>(), Enumerable.Empty<ISkillDay>());

			var bussinesRuleCollection = NewBusinessRuleCollection.All(stateHolder.SchedulingResultState);
			stateHolder.Schedules.ValidateBusinessRulesOnPersons(new List<IPerson> {agent},
				bussinesRuleCollection);

			var scheduleDay = stateHolder.Schedules[agent].ScheduledDay(dateOnly);
			scheduleDay.CreateAndAddAbsence(new AbsenceLayer(new Absence(), new DateTimePeriod(2016, 05, 22, 2016, 05, 24)));
			var responses = stateHolder.Schedules.Modify(ScheduleModifier.Scheduler, scheduleDay, bussinesRuleCollection,
				new DoNothingScheduleDayChangeCallBack(), new NoScheduleTagSetter());

			responses.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldShowValidationAlertIfModifyIsCanceled()
		{
			setup();
			var scenario = new Scenario("_");
			var phoneActivity = new Activity("_") {AllowOverwrite = true};
			var lunch = new Activity("_") {AllowOverwrite = false};
			var dateOnly = new DateOnly(2016, 05, 23);
			var shiftCategory1 = new ShiftCategory("_").WithId();

			var agent = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(new Person(), dateOnly);
			var schedulePeriod = agent.SchedulePeriod(dateOnly);
			schedulePeriod.PeriodType = SchedulePeriodType.Week;

			var ass1 = new PersonAssignment(agent, scenario, dateOnly); //should not
			ass1.AddActivity(phoneActivity, new TimePeriod(8, 0, 16, 0));
			ass1.AddActivity(lunch, new TimePeriod(11, 0, 12, 0));

			ass1.SetShiftCategory(shiftCategory1);

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(dateOnly, 1),
				new[] {agent}, new[] {ass1}, Enumerable.Empty<ISkillDay>());

			var bussinesRuleCollection = NewBusinessRuleCollection.All(stateHolder.SchedulingResultState);
			stateHolder.Schedules.ValidateBusinessRulesOnPersons(new List<IPerson> {agent},
				bussinesRuleCollection);

			var scheduleDay = stateHolder.Schedules[agent].ScheduledDay(dateOnly);
			scheduleDay.BusinessRuleResponseCollection.Count.Should().Be.EqualTo(0);

			scheduleDay.PersonAssignment().AddActivity(phoneActivity, new TimePeriod(11, 30, 12, 30));
			var responses = stateHolder.Schedules.Modify(ScheduleModifier.Scheduler, scheduleDay, bussinesRuleCollection,
				new DoNothingScheduleDayChangeCallBack(), new NoScheduleTagSetter());

			responses.Count().Should().Be.EqualTo(1);
			scheduleDay = stateHolder.Schedules[agent].ScheduledDay(dateOnly);
			scheduleDay.BusinessRuleResponseCollection.Count.Should().Be.EqualTo(0);
		}

		private static bool collectionContainsType(IEnumerable<INewBusinessRule> businessRuleCollection, Type type)
		{
			return businessRuleCollection.Any(rule => rule.GetType() == type);
		}
	}
}