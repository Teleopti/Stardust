using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.DomainTest.SchedulingScenarios;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
	[DomainTest]
	public class NewBusinessRuleCollectionTest
	{
		private INewBusinessRuleCollection _target;
		private const int totalNumberOfRules = 10;
		private ISchedulingResultStateHolder _state;

		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		[SetUp]
		public void Setup()
		{
			_target = NewBusinessRuleCollection.All(new SchedulingResultStateHolder());
		}

		[Test]
		public void VerifyAll()
		{
			//rk: kolla istället vilka IBusiness rules-typer som finns i domain... orkar inte just nu
			INewBusinessRule rule = _target.Item(typeof(NewShiftCategoryLimitationRule));
			Assert.AreEqual(totalNumberOfRules, _target.Count);
			Assert.IsTrue(rule.HaltModify);
		}

		[Test]
		public void ShouldGetCorrectRulesFromFlag()
		{
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
		public void ShouldGetCorrectFlagFromRules()
		{
			const BusinessRuleFlags expectedFlag = BusinessRuleFlags.MinWeekWorkTimeRule
												   | BusinessRuleFlags.NewMaxWeekWorkTimeRule
												   | BusinessRuleFlags.DataPartOfAgentDay;

			var rules = new List<Type>
			{
				typeof (NewMaxWeekWorkTimeRule),
				typeof (MinWeekWorkTimeRule),
				typeof (DataPartOfAgentDay)
			};

			var flag = NewBusinessRuleCollection.GetFlagFromRules(rules);
			Assert.AreEqual(flag, expectedFlag);
		}

		[Test, SetUICulture("en-GB")]
		public void ShouldSetCulture()
		{
			_target.SetUICulture(CultureInfo.GetCultureInfo(1053));
			Assert.AreEqual("sv-SE",_target.UICulture.Name);
		}

		[Test]
		public void VerifyMinimum()
		{
			INewBusinessRuleCollection targetSmall = NewBusinessRuleCollection.Minimum();
			foreach (var rule in _target)
			{
				Assert.AreEqual(rule.IsMandatory, collectionContainsType(targetSmall, rule.GetType()));
			}
		}
		
		[Test]
		public void VerifyRemoveBusinessRuleResponse()
		{
			INewBusinessRule rule = _target.Item(typeof(NewShiftCategoryLimitationRule));
			var dateOnlyPeriod = new DateOnlyPeriod();
			_target.Remove(new BusinessRuleResponse(typeof(NewShiftCategoryLimitationRule), "d", false, false, new DateTimePeriod(2000, 1, 1, 2000, 1, 2), new Person(), dateOnlyPeriod, "tjillevippen"));
			Assert.AreEqual(totalNumberOfRules, _target.Count);
			Assert.IsFalse(rule.HaltModify);
		}

		[Test]
		public void RemovingMandatoryRuleShouldResultDoingNothing()
		{
			_target.Add(new dummyRule(true));
			_target.Remove(typeof(dummyRule));
			Assert.AreEqual(totalNumberOfRules + 1, _target.Count);
		}

		[Test]
		public void VerifyClearKeepsMandatory()
		{
			_target.Add(new dummyRule(true));
			_target.Clear();
			Assert.AreEqual(totalNumberOfRules + 1, _target.Count);
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void NewOverlappingAssignmentRuleHasSetPropertyForDeleteInAllRules()
		{
			var rulesForDelete = NewBusinessRuleCollection.AllForDelete(new SchedulingResultStateHolder());
			foreach (INewBusinessRule rule in rulesForDelete)
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
			_state = new SchedulingResultStateHolder();
			var miniAndPa = NewBusinessRuleCollection.MinimumAndPersonAccount(_state);
			Assert.That(miniAndPa.Count, Is.EqualTo(NewBusinessRuleCollection.Minimum().Count + 1));
			Assert.That(collectionContainsType(miniAndPa, typeof(NewPersonAccountRule)));
		}

		[Test]
		public void AllForSchedulingShouldConsiderUseValidation()
		{
			_state = new SchedulingResultStateHolder();
			_state.UseValidation = true;
			var allForScheduling = NewBusinessRuleCollection.AllForScheduling(_state);
			Assert.AreEqual(totalNumberOfRules, allForScheduling.Count);
			_state.UseValidation = false;
			allForScheduling = NewBusinessRuleCollection.AllForScheduling(_state);
			Assert.AreEqual(NewBusinessRuleCollection.Minimum().Count + 1, allForScheduling.Count);
			Assert.IsFalse((collectionContainsType(allForScheduling, typeof(MinWeekWorkTimeRule))));
		}

		[Test]
		public void ShouldConsiderUseMinWorktimePerWeek()
		{
			_state = new SchedulingResultStateHolder();
			_state.UseMinWeekWorkTime = true;
			_state.UseValidation = true;
			var allForScheduling = NewBusinessRuleCollection.AllForScheduling(_state);
			Assert.AreEqual(totalNumberOfRules + 1, allForScheduling.Count);
			Assert.IsTrue(collectionContainsType(allForScheduling, typeof(MinWeekWorkTimeRule)));
		}

		[Test]
		public void ShouldHandleAgentWithNoAssignment()
		{
			var scenario = new Scenario("_");
			var dateOnly = new DateOnly(2016, 05, 23);

			var agent = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(new Person(), dateOnly);
			var schedulePeriod = agent.SchedulePeriod(dateOnly);
			schedulePeriod.PeriodType = SchedulePeriodType.Week;

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly.AddWeeks(1)),
				new[] { agent }, Enumerable.Empty<IScheduleData>(), Enumerable.Empty<ISkillDay>());

			var bussinesRuleCollection = NewBusinessRuleCollection.All(stateHolder.SchedulingResultState);
			stateHolder.Schedules.ValidateBusinessRulesOnPersons(new List<IPerson> { agent }, CultureInfo.InvariantCulture, bussinesRuleCollection);

			var scheduleDay = stateHolder.Schedules[agent].ScheduledDay(dateOnly);
			scheduleDay.CreateAndAddAbsence(new AbsenceLayer(new Absence(), new DateTimePeriod(2016, 05, 22, 2016, 05, 24)));
			var responses = stateHolder.Schedules.Modify(ScheduleModifier.Scheduler, scheduleDay, bussinesRuleCollection,
				new DoNothingScheduleDayChangeCallBack(), new NoScheduleTagSetter());

			responses.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldShowValidationAlertIfModifyIsCanceled()
		{
			var scenario = new Scenario("_");
			var phoneActivity = new Activity("_") { AllowOverwrite = true };
			var lunch = new Activity("_") { AllowOverwrite = false };
			var dateOnly = new DateOnly(2016, 05, 23);
			var shiftCategory1 = new ShiftCategory("_").WithId();

			var agent = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(new Person(), dateOnly);
			var schedulePeriod = agent.SchedulePeriod(dateOnly);
			schedulePeriod.PeriodType = SchedulePeriodType.Week;

			var ass1 = new PersonAssignment(agent, scenario, dateOnly); //should not
			ass1.AddActivity(phoneActivity, new TimePeriod(8, 0, 16, 0));
			ass1.AddActivity(lunch, new TimePeriod(11, 0, 12, 0));

			ass1.SetShiftCategory(shiftCategory1);

			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly.AddWeeks(1)),
				new[] { agent }, new[] { ass1 }, Enumerable.Empty<ISkillDay>());

			var bussinesRuleCollection = NewBusinessRuleCollection.All(stateHolder.SchedulingResultState);
			stateHolder.Schedules.ValidateBusinessRulesOnPersons(new List<IPerson> { agent }, CultureInfo.InvariantCulture, bussinesRuleCollection);

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
			foreach (var rule in businessRuleCollection)
			{
				if (rule.GetType().Equals(type))
					return true;
			}
			return false;
		}
	
		private class dummyRule : INewBusinessRule
		{
			private readonly bool _mandatory;

			public dummyRule(bool mandatory)
			{
				_mandatory = mandatory;
				FriendlyName = string.Empty;
			}

			public string ErrorMessage
			{
				get { return string.Empty; }
			}

			public bool IsMandatory
			{
				get { return _mandatory; }
			}

			public bool HaltModify { get; set; }

			public bool ForDelete
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}

			public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays)
			{
				throw new NotImplementedException();
			}

			public string FriendlyName { get; }
		}
	}
}