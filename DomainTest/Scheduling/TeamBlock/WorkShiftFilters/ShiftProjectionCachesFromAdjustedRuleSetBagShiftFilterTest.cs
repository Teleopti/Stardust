using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private IShiftProjectionCachesFromAdjustedRuleSetBagShiftFilter _target;
		private IRuleSetDeletedActivityChecker _ruleSetDeletedActivityChecker;
		private IRuleSetDeletedShiftCategoryChecker _rulesSetDeletedShiftCategoryChecker;
		private IRuleSetSkillActivityChecker _ruleSetSkillActivityChecker;
		private IRuleSetToShiftsGenerator _ruleSetToShiftsGenerator;
		private DateOnly _dateOnly;
		private IPerson _person;
		private IPersonPeriod _personPeriod;
		private IRuleSetBag _ruleSetBag;
		private TimeZoneInfo _timeZoneInfo;
		private IPersonalShiftMeetingTimeChecker _personalShiftMeetingTimeChecker;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_dateOnly = new DateOnly(2013, 3, 1);
			_person = _mocks.StrictMock<IPerson>();
			_personPeriod = _mocks.StrictMock<IPersonPeriod>();
			_ruleSetBag = _mocks.StrictMock<IRuleSetBag>();
			_timeZoneInfo = (TimeZoneInfo.FindSystemTimeZoneById("UTC"));
			_personalShiftMeetingTimeChecker = _mocks.StrictMock<IPersonalShiftMeetingTimeChecker>();
			_ruleSetDeletedActivityChecker = _mocks.StrictMock<IRuleSetDeletedActivityChecker>();
			_rulesSetDeletedShiftCategoryChecker = _mocks.StrictMock<IRuleSetDeletedShiftCategoryChecker>();
			_ruleSetToShiftsGenerator = _mocks.StrictMock<IRuleSetToShiftsGenerator>();
			_ruleSetSkillActivityChecker = _mocks.StrictMock<IRuleSetSkillActivityChecker>();
			_target = new ShiftProjectionCachesFromAdjustedRuleSetBagShiftFilter(_ruleSetDeletedActivityChecker,
			                                                                     _rulesSetDeletedShiftCategoryChecker,
			                                                                     _ruleSetToShiftsGenerator,
																				 _ruleSetSkillActivityChecker);
		}

		[Test]
		public void ShouldCheckParameters()
		{
			var result = _target.Filter(_dateOnly, null, false,BlockFinderType.SingleDay );
			Assert.IsNull(result);
		}

        [Test]
        public void ShouldGetShiftProjectionCachesFromAdjustedRuleSetBagForRoleModel()
        {
            var permissionInfo = new PermissionInformation(_person);
            permissionInfo.SetDefaultTimeZone(_timeZoneInfo);
            var ruleSet1 = _mocks.StrictMock<IWorkShiftRuleSet>();
            var ruleSets = new List<IWorkShiftRuleSet> { ruleSet1 };
            var shifts = getCashes();
            using (_mocks.Record())
            {
                Expect.Call(_person.Period(_dateOnly)).Return(_personPeriod);
                Expect.Call(ruleSet1.IsValidDate(_dateOnly)).Return(true);
                Expect.Call(_ruleSetDeletedActivityChecker.ContainsDeletedActivity(ruleSet1)).Return(false);
                Expect.Call(_rulesSetDeletedShiftCategoryChecker.ContainsDeletedActivity(ruleSet1)).Return(false);
                Expect.Call(_person.PermissionInformation).Return(permissionInfo);
                Expect.Call(_ruleSetToShiftsGenerator.Generate(ruleSet1)).Return(shifts);
                Expect.Call(_personPeriod.PersonSkillCollection).Return(new List<IPersonSkill>());
                Expect.Call(_ruleSetSkillActivityChecker.CheckSkillActivties(null, null)).IgnoreArguments().Return(true);
            }
            using (_mocks.Playback())
            {
                var result = _target.FilterForRoleModel(new ReadOnlyCollection<IWorkShiftRuleSet>(ruleSets), _dateOnly, _person, false, BlockFinderType.SingleDay);

                Assert.That(result.Count, Is.EqualTo(3));
            }
        }

		[Test]
		public void ShouldGetShiftProjectionCachesFromAdjustedRuleSetBag()
		{
            var permissionInfo = new PermissionInformation(_person);
            permissionInfo.SetDefaultTimeZone(_timeZoneInfo);
            var ruleSet1 = _mocks.StrictMock<IWorkShiftRuleSet>();
			var ruleSets = new List<IWorkShiftRuleSet> {ruleSet1};
			var shifts = getCashes();
			using (_mocks.Record())
			{
				Expect.Call(_person.Period(_dateOnly)).Return(_personPeriod);
				Expect.Call(_personPeriod.RuleSetBag).Return(_ruleSetBag);
				Expect.Call(ruleSet1.OnlyForRestrictions).Return(false);
				Expect.Call(ruleSet1.IsValidDate(_dateOnly)).Return(true);
				Expect.Call(_ruleSetDeletedActivityChecker.ContainsDeletedActivity(ruleSet1)).Return(false);
				Expect.Call(_rulesSetDeletedShiftCategoryChecker.ContainsDeletedActivity(ruleSet1)).Return(false);
                Expect.Call(_person.PermissionInformation).Return(permissionInfo);
				Expect.Call(_ruleSetBag.RuleSetCollection).Return(new ReadOnlyCollection<IWorkShiftRuleSet>(ruleSets));
				Expect.Call(_ruleSetToShiftsGenerator.Generate(ruleSet1)).Return(shifts);
				Expect.Call(_personPeriod.PersonSkillCollection).Return(new List<IPersonSkill>());
				Expect.Call(_ruleSetSkillActivityChecker.CheckSkillActivties(null, null)).IgnoreArguments().Return(true);
			}
			using (_mocks.Playback())
			{
                var result = _target.Filter(_dateOnly, _person, false, BlockFinderType.SingleDay);

				Assert.That(result.Count, Is.EqualTo(3));
			}
		}

		[Test]
		public void ShouldNotGenerateAnyShiftsIfRuleSetIsInvalid()
		{
            var permissionInfo = new PermissionInformation(_person);
            permissionInfo.SetDefaultTimeZone(_timeZoneInfo);
            var ruleSet1 = _mocks.StrictMock<IWorkShiftRuleSet>();
			var ruleSets = new List<IWorkShiftRuleSet> { ruleSet1 };
			using (_mocks.Record())
			{
				Expect.Call(_person.Period(_dateOnly)).Return(_personPeriod);
				Expect.Call(_personPeriod.RuleSetBag).Return(_ruleSetBag);
				Expect.Call(ruleSet1.OnlyForRestrictions).Return(false);
				Expect.Call(ruleSet1.IsValidDate(_dateOnly)).Return(false);
                Expect.Call(_person.PermissionInformation).Return(permissionInfo);
				Expect.Call(_ruleSetBag.RuleSetCollection).Return(new ReadOnlyCollection<IWorkShiftRuleSet>(ruleSets));
			}
			using (_mocks.Playback())
			{
                var result = _target.Filter(_dateOnly, _person, false, BlockFinderType.SingleDay);

				Assert.That(result.Count, Is.EqualTo(0));
			}
		}

       
		[Test]
		public void ShouldNotGenerateAnyShiftsIfRuleSetContainsDeletedActivity()
		{
            var permissionInfo = new PermissionInformation(_person);
            permissionInfo.SetDefaultTimeZone(_timeZoneInfo);
            var ruleSet1 = _mocks.StrictMock<IWorkShiftRuleSet>();
			var ruleSets = new List<IWorkShiftRuleSet> { ruleSet1 };
			using (_mocks.Record())
			{
				Expect.Call(_person.Period(_dateOnly)).Return(_personPeriod);
				Expect.Call(_personPeriod.RuleSetBag).Return(_ruleSetBag);
				Expect.Call(ruleSet1.OnlyForRestrictions).Return(false);
				Expect.Call(ruleSet1.IsValidDate(_dateOnly)).Return(true);
				Expect.Call(_ruleSetDeletedActivityChecker.ContainsDeletedActivity(ruleSet1)).Return(false);
				Expect.Call(_rulesSetDeletedShiftCategoryChecker.ContainsDeletedActivity(ruleSet1)).Return(true);
                Expect.Call(_person.PermissionInformation).Return(permissionInfo);
				Expect.Call(_ruleSetBag.RuleSetCollection).Return(new ReadOnlyCollection<IWorkShiftRuleSet>(ruleSets));
			}
			using (_mocks.Playback())
			{
                var result = _target.Filter(_dateOnly, _person, false, BlockFinderType.SingleDay);

				Assert.That(result.Count, Is.EqualTo(0));
			}
		}	
		
		[Test]
		public void ShouldNotGenerateAnyShiftsIfRuleSetShiftCategoryContainsDeletedActivity()
		{
            var permissionInfo = new PermissionInformation(_person);
            permissionInfo.SetDefaultTimeZone(_timeZoneInfo);
            var ruleSet1 = _mocks.StrictMock<IWorkShiftRuleSet>();
			var ruleSets = new List<IWorkShiftRuleSet> { ruleSet1 };
			using (_mocks.Record())
			{
				Expect.Call(_person.Period(_dateOnly)).Return(_personPeriod);
				Expect.Call(_personPeriod.RuleSetBag).Return(_ruleSetBag);
				Expect.Call(ruleSet1.OnlyForRestrictions).Return(false);
				Expect.Call(ruleSet1.IsValidDate(_dateOnly)).Return(true);
				Expect.Call(_ruleSetDeletedActivityChecker.ContainsDeletedActivity(ruleSet1)).Return(true);
                Expect.Call(_person.PermissionInformation).Return(permissionInfo);
				Expect.Call(_ruleSetBag.RuleSetCollection).Return(new ReadOnlyCollection<IWorkShiftRuleSet>(ruleSets));
			}
			using (_mocks.Playback())
			{
                var result = _target.Filter(_dateOnly, _person, false, BlockFinderType.SingleDay);

				Assert.That(result.Count, Is.EqualTo(0));
			}
		}

		[Test]
		public void ShouldNotGenerateAnyShiftsIfRuleSetContainsSkillActivityNotInPersonSkills()
		{
            var permissionInfo = new PermissionInformation(_person);
            permissionInfo.SetDefaultTimeZone(_timeZoneInfo);
            var ruleSet1 = _mocks.StrictMock<IWorkShiftRuleSet>();
			var ruleSets = new List<IWorkShiftRuleSet> { ruleSet1 };
			using (_mocks.Record())
			{
				Expect.Call(_person.Period(_dateOnly)).Return(_personPeriod);
				Expect.Call(_personPeriod.RuleSetBag).Return(_ruleSetBag);
				Expect.Call(ruleSet1.OnlyForRestrictions).Return(false);
				Expect.Call(ruleSet1.IsValidDate(_dateOnly)).Return(true);
				Expect.Call(_ruleSetDeletedActivityChecker.ContainsDeletedActivity(ruleSet1)).Return(false);
				Expect.Call(_rulesSetDeletedShiftCategoryChecker.ContainsDeletedActivity(ruleSet1)).Return(false);
				Expect.Call(_ruleSetBag.RuleSetCollection).Return(new ReadOnlyCollection<IWorkShiftRuleSet>(ruleSets));
				Expect.Call(_personPeriod.PersonSkillCollection).Return(new List<IPersonSkill>());
                Expect.Call(_person.PermissionInformation).Return(permissionInfo);
				Expect.Call(_ruleSetSkillActivityChecker.CheckSkillActivties(null, null)).IgnoreArguments().Return(false);
			}
			using (_mocks.Playback())
			{
                var result = _target.Filter(_dateOnly, _person, false, BlockFinderType.SingleDay);

				Assert.That(result.Count, Is.EqualTo(0));
			}
		}

		private IEnumerable<IShiftProjectionCache> getCashes()
		{
			var tmpList = getWorkShifts();
			var retList = new List<IShiftProjectionCache>();
			foreach (IWorkShift shift in tmpList)
			{
				var cache = new ShiftProjectionCache(shift, _personalShiftMeetingTimeChecker);
				cache.SetDate(_dateOnly, _timeZoneInfo);
				retList.Add(cache);
			}
			return retList;
		}

		private static IEnumerable<IWorkShift> getWorkShifts()
		{
			var activity = ActivityFactory.CreateActivity("sd");
			var category = ShiftCategoryFactory.CreateShiftCategory("dv");
			var workShift1 = WorkShiftFactory.CreateWorkShift(new TimeSpan(7, 0, 0), new TimeSpan(15, 0, 0),
														  activity, category);
			var workShift2 = WorkShiftFactory.CreateWorkShift(new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0),
														  activity, category);
			var workShift3 = WorkShiftFactory.CreateWorkShift(new TimeSpan(10, 0, 0), new TimeSpan(19, 0, 0),
																	  activity, category);

			return new List<IWorkShift> { workShift1, workShift2, workShift3 };
		}
	}
}
