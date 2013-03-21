using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
	[TestFixture]
	public class ShiftProjectionCachesFromAdjustedRuleSetBagShiftFilterTest
	{
		private MockRepository _mocks;
		private IShiftProjectionCachesFromAdjustedRuleSetBagShiftFilter _target;
		private IRuleSetDeletedActivityChecker _ruleSetDeletedActivityChecker;
		private IRuleSetDeletedShiftCategoryChecker _rulesSetDeletedShiftCategoryChecker;
		private IRuleSetToShiftsGenerator _ruleSetToShiftsGenerator;
		private IPerson _person;
		private IPersonalShiftMeetingTimeChecker _personalShiftMeetingTimeChecker;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_ruleSetDeletedActivityChecker = _mocks.StrictMock<IRuleSetDeletedActivityChecker>();
			_rulesSetDeletedShiftCategoryChecker = _mocks.StrictMock<IRuleSetDeletedShiftCategoryChecker>();
			_ruleSetToShiftsGenerator = _mocks.StrictMock<IRuleSetToShiftsGenerator>();
			_person = _mocks.StrictMock<IPerson>();
			_personalShiftMeetingTimeChecker = _mocks.StrictMock<IPersonalShiftMeetingTimeChecker>();
			_target = new ShiftProjectionCachesFromAdjustedRuleSetBagShiftFilter(_ruleSetDeletedActivityChecker,
			                                                                     _rulesSetDeletedShiftCategoryChecker,
			                                                                     _ruleSetToShiftsGenerator);
		}

		[Test]
		public void CanAdjustWorkShiftsFromRuleSetBag()
		{
			var personPeriod = _mocks.StrictMock<IPersonPeriod>();
			var ruleSetBag = _mocks.StrictMock<IRuleSetBag>();
			var ruleSet1 = _mocks.StrictMock<IWorkShiftRuleSet>();
			var ruleSet2 = _mocks.StrictMock<IWorkShiftRuleSet>();
			var ruleSet3 = _mocks.StrictMock<IWorkShiftRuleSet>();
			var templateGenerator1 = _mocks.StrictMock<IWorkShiftTemplateGenerator>();
			var templateGenerator2 = _mocks.StrictMock<IWorkShiftTemplateGenerator>();
			var templateGenerator3 = _mocks.StrictMock<IWorkShiftTemplateGenerator>();
			var startPeriod1 = new TimePeriodWithSegment(new TimePeriod(6, 0, 7, 0), TimeSpan.FromMinutes(15));
			var endPeriod1 = new TimePeriodWithSegment(new TimePeriod(17, 0, 19, 0), TimeSpan.FromMinutes(15));
			var startPeriod2 = new TimePeriodWithSegment(new TimePeriod(9, 0, 11, 0), TimeSpan.FromMinutes(15));
			var endPeriod2 = new TimePeriodWithSegment(new TimePeriod(16, 0, 18, 0), TimeSpan.FromMinutes(15));
			var startPeriod3 = new TimePeriodWithSegment(new TimePeriod(8, 0, 10, 0), TimeSpan.FromMinutes(15));
			var endPeriod3 = new TimePeriodWithSegment(new TimePeriod(16, 0, 16, 59), TimeSpan.FromMinutes(15));

			IList<IWorkShiftRuleSet> ruleSets = new List<IWorkShiftRuleSet> { ruleSet1, ruleSet2, ruleSet3 };
			var readOnlyRuleSets = new ReadOnlyCollection<IWorkShiftRuleSet>(ruleSets);
			var dateOnly = new DateOnly(2009, 2, 2);
			TimeZoneInfo timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));
			var restriction = new EffectiveRestriction(new StartTimeLimitation(TimeSpan.FromHours(8), TimeSpan.FromHours(10)),
									  new EndTimeLimitation(TimeSpan.FromHours(17), TimeSpan.FromHours(19)),
									  new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			var permissionInfo = new PermissionInformation(_person);
			permissionInfo.SetDefaultTimeZone(timeZoneInfo);
			
			using (_mocks.Record())
			{
				Expect.Call(_person.PermissionInformation).Return(permissionInfo);
				Expect.Call(_person.Period(dateOnly)).Return(personPeriod);
				Expect.Call(personPeriod.RuleSetBag).Return(ruleSetBag);
				Expect.Call(ruleSetBag.RuleSetCollection).Return(new ReadOnlyCollection<IWorkShiftRuleSet>(ruleSets));
				expectCodeCanAdjustWorkShiftsFromRuleSetBag(startPeriod2, startPeriod1, templateGenerator3, endPeriod3,
															startPeriod3, endPeriod2, templateGenerator2, ruleSet2,
															ruleSet1, readOnlyRuleSets, templateGenerator1, dateOnly,
															ruleSet3);
			}

			using (_mocks.Playback())
			{
				var ret = _target.Filter(dateOnly,_person, false, restriction);
				Assert.That(ret.Count, Is.EqualTo(3));
			}
		}

		private void expectCodeCanAdjustWorkShiftsFromRuleSetBag(TimePeriodWithSegment startPeriod2,
																 TimePeriodWithSegment startPeriod1,
																 IWorkShiftTemplateGenerator templateGenerator3,
																 TimePeriodWithSegment endPeriod3,
																 TimePeriodWithSegment startPeriod3,
																 TimePeriodWithSegment endPeriod2,
																 IWorkShiftTemplateGenerator templateGenerator2,
																 IWorkShiftRuleSet ruleSet2, IWorkShiftRuleSet ruleSet1,
																 ReadOnlyCollection<IWorkShiftRuleSet> readOnlyRuleSets,
																 IWorkShiftTemplateGenerator templateGenerator1,
																 DateOnly dateOnly, IWorkShiftRuleSet ruleSet3)
		{
			Expect.Call(ruleSet1.OnlyForRestrictions).Return(false);
			Expect.Call(ruleSet2.OnlyForRestrictions).Return(false);
			Expect.Call(ruleSet3.OnlyForRestrictions).Return(false);
			Expect.Call(ruleSet1.IsValidDate(dateOnly)).Return(true);
			Expect.Call(ruleSet2.IsValidDate(dateOnly)).Return(true);
			Expect.Call(ruleSet3.IsValidDate(dateOnly)).Return(true);
			Expect.Call(ruleSet1.Clone()).Return(ruleSet1);
			Expect.Call(ruleSet2.Clone()).Return(ruleSet2);
			Expect.Call(ruleSet3.Clone()).Return(ruleSet3);
			Expect.Call(ruleSet1.TemplateGenerator).Return(templateGenerator1).Repeat.AtLeastOnce();
			Expect.Call(ruleSet2.TemplateGenerator).Return(templateGenerator2).Repeat.AtLeastOnce();
			Expect.Call(ruleSet3.TemplateGenerator).Return(templateGenerator3).Repeat.AtLeastOnce();
			Expect.Call(templateGenerator1.StartPeriod).Return(startPeriod1).Repeat.AtLeastOnce();
			Expect.Call(templateGenerator2.StartPeriod).Return(startPeriod2).Repeat.AtLeastOnce();
			Expect.Call(templateGenerator2.StartPeriod = new TimePeriodWithSegment(9, 0, 10, 0, 15));
			Expect.Call(templateGenerator2.EndPeriod).Return(endPeriod2).Repeat.AtLeastOnce();
			Expect.Call(templateGenerator2.EndPeriod = new TimePeriodWithSegment(17, 0, 18, 0, 15));
			Expect.Call(templateGenerator3.StartPeriod).Return(startPeriod3).Repeat.AtLeastOnce();
			Expect.Call(templateGenerator3.EndPeriod).Return(endPeriod3).Repeat.AtLeastOnce();
			Expect.Call(_ruleSetDeletedActivityChecker.ContainsDeletedActivity(ruleSet2)).Return(false);
			Expect.Call(_rulesSetDeletedShiftCategoryChecker.ContainsDeletedActivity(ruleSet2)).Return(false);
			Expect.Call(_ruleSetToShiftsGenerator.Generate(ruleSet1)).IgnoreArguments().Return(getShifts());
		}
		
		private IEnumerable<IShiftProjectionCache> getShifts()
		{
			var activity = ActivityFactory.CreateActivity("sd");
			var category = ShiftCategoryFactory.CreateShiftCategory("dv");
			var workShift1 = WorkShiftFactory.CreateWorkShift(new TimeSpan(7, 0, 0), new TimeSpan(15, 0, 0),
														   activity, category);
			var workShift2 = WorkShiftFactory.CreateWorkShift(new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0),
														 activity, category);
			var workShift3 = WorkShiftFactory.CreateWorkShift(new TimeSpan(10, 0, 0), new TimeSpan(19, 0, 0),
														   activity, category);
			return new List<IShiftProjectionCache>
				{
					new ShiftProjectionCache(workShift1, _personalShiftMeetingTimeChecker),
					new ShiftProjectionCache(workShift2, _personalShiftMeetingTimeChecker),
					new ShiftProjectionCache(workShift3, _personalShiftMeetingTimeChecker),
				};
		}
	}
}
