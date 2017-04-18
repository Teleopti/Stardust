using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class CalculateMinMaxCacheDeciderTest
	{
		private MockRepository _mocks;
		private ICalculateMinMaxCacheDecider _target;
		private ISchedulerStateHolder _stateHolder;
		private SchedulingOptions _schedulingOptions;
		private IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private IDictionary<Guid, IPerson> _personDic;
		private IPerson _person1;
		private IPerson _person2;
		private IScheduleDictionary _scheduleDictionary;
		private IScheduleRange _range;
		private IScheduleDay _scheduleDay;
		

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_stateHolder = _mocks.StrictMock<ISchedulerStateHolder>();
			_target = new CalculateMinMaxCacheDecider();
			_schedulingOptions = new SchedulingOptions();
			_effectiveRestrictionCreator = _mocks.StrictMock<IEffectiveRestrictionCreator>();
			_personDic = new Dictionary<Guid, IPerson>();
			_person1 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill>());
			_person2 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill>());
			var bag1 = new RuleSetBag();
			var ruleSet1 = _mocks.StrictMock<IWorkShiftRuleSet>();
			var ruleSet2 = _mocks.StrictMock<IWorkShiftRuleSet>();
			var ruleSet3 = _mocks.StrictMock<IWorkShiftRuleSet>();
			bag1.AddRuleSet(ruleSet1);
			bag1.AddRuleSet(ruleSet2);
			var bag2 = new RuleSetBag();
			bag2.AddRuleSet(ruleSet2);
			bag2.AddRuleSet(ruleSet3);
			_person1.Period(DateOnly.MinValue).RuleSetBag = bag1;
			_person2.Period(DateOnly.MinValue).RuleSetBag = bag2;
			_personDic.Add(Guid.NewGuid(), _person1);
			_personDic.Add(Guid.NewGuid(), _person2);
			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_range = _mocks.StrictMock<IScheduleRange>();
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();

		}

		[Test]
		public void ShouldReturnTrueIfOverLimit()
		{
			using (_mocks.Record())
			{
				commonMocks();
			}

			using (_mocks.Playback())
			{
				bool result = _target.ShouldCacheBeDisabled(_stateHolder, _schedulingOptions, _effectiveRestrictionCreator, 8);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldReturnFalseIfUnderLimit()
		{
			using (_mocks.Record())
			{
				commonMocks();
			}

			using (_mocks.Playback())
			{
				bool result = _target.ShouldCacheBeDisabled(_stateHolder, _schedulingOptions, _effectiveRestrictionCreator, 10);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldReturnFalseIfAtLimit()
		{
			using (_mocks.Record())
			{
				commonMocks();
			}

			using (_mocks.Playback())
			{
				bool result = _target.ShouldCacheBeDisabled(_stateHolder, _schedulingOptions, _effectiveRestrictionCreator, 9);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldHandlePersonsWithNoPersonPeriod()
		{
			_person1.RemoveAllPersonPeriods();
			_person2.RemoveAllPersonPeriods();
			using (_mocks.Record())
			{
				commonMocks();
			}

			using (_mocks.Playback())
			{
				bool result = _target.ShouldCacheBeDisabled(_stateHolder, _schedulingOptions, _effectiveRestrictionCreator, 0);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldHandlePersonsWithNoShiftBag()
		{
			_person1.Period(DateOnly.MinValue).RuleSetBag = null;
			_person2.Period(DateOnly.MinValue).RuleSetBag = null;

			using (_mocks.Record())
			{
				commonMocks();
			}

			using (_mocks.Playback())
			{
				bool result = _target.ShouldCacheBeDisabled(_stateHolder, _schedulingOptions, _effectiveRestrictionCreator, 0);
				Assert.IsFalse(result);
			}
		}

		private void commonMocks()
		{
			Expect.Call(_stateHolder.FilteredCombinedAgentsDictionary).Return(_personDic);
			Expect.Call(_stateHolder.RequestedPeriod)
				  .Return(new DateOnlyPeriodAsDateTimePeriod(
							  new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)), TimeZoneInfo.Utc)).Repeat.AtLeastOnce();
			Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary);


			//this should get us 3 unique restrictions
			Expect.Call(_scheduleDictionary[_person1]).Return(_range);
			Expect.Call(_range.ScheduledDayCollection(new DateOnlyPeriod(DateOnly.MinValue,DateOnly.MinValue.AddDays(1)))).Return(new []{_scheduleDay,_scheduleDay});
			Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay, _schedulingOptions))
				  .Return(restriction(new ShiftCategory("Hej")));
			Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay, _schedulingOptions))
				  .Return(restriction(null));
			Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(DateOnly.MinValue, TimeZoneInfo.Utc));
			Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(DateOnly.MinValue.AddDays(1), TimeZoneInfo.Utc));

			Expect.Call(_scheduleDictionary[_person2]).Return(_range);
			Expect.Call(_range.ScheduledDayCollection(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue.AddDays(1)))).Return(new[] { _scheduleDay, _scheduleDay });
			Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay, _schedulingOptions))
				  .Return(restriction(new ShiftCategory("Hopp")));
			Expect.Call(_effectiveRestrictionCreator.GetEffectiveRestriction(_scheduleDay, _schedulingOptions))
				  .Return(restriction(null));
			Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(DateOnly.MinValue, TimeZoneInfo.Utc));
			Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(DateOnly.MinValue.AddDays(1), TimeZoneInfo.Utc));
			// unique rulesets should be 3, so the total should en up to 3*3=9
		}

		private static IEffectiveRestriction restriction(IShiftCategory shiftCategory)
		{
			IEffectiveRestriction restriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
																		 new WorkTimeLimitation(), shiftCategory, null, null,
			                                                             new List<IActivityRestriction>());
			return restriction;
		}

	}
}