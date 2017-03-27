using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
	public class WorkShiftFinderServiceTest
	{
		private MockRepository _mocks;
		private ISchedulingResultStateHolder _stateHolder;
		private WorkShiftFinderService _target;
		private readonly DateOnly _scheduleDateOnly = new DateOnly(2009, 2, 10);
		private IDateOnlyAsDateTimePeriod _scheduleDateOnlyPeriod;
		private IScheduleDay _part;
		private IPerson _person;
		private IVirtualSchedulePeriod _schedulePeriod;
		private IPersonPeriod _personPeriod;
        private IWorkShift _workShift1;
		private IWorkShift _workShift2;
		private IWorkShift _workShift3;
		private IShiftCategory _category;
		private IActivity _activity;
		private readonly TimeZoneInfo _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time");

		private IShiftProjectionCacheManager _shiftProjectionCacheManager;
		private IPersonSkillPeriodsDataHolderManager _personSkillPeriodsDataHolderManager;
		private IShiftProjectionCacheFilter _shiftProjectionCacheFilter;
		private IPreSchedulingStatusChecker _preSchedulingStatusChecker;
		private IPermissionInformation _info;
        private IWorkShiftMinMaxCalculator _workShiftMinMaxCalculator;
        private IScheduleMatrixPro _matrix;
        private IWorkShiftCalculatorsManager _calculatorManager;
        private IFairnessAndMaxSeatCalculatorsManager _fairnessAndMaxSeatCalculatorsManager;
    	private IShiftLengthDecider _shiftLengthDecider;

        private ISchedulingOptions _schedulingOptions;
    	private IPersonalShiftMeetingTimeChecker _personalShiftMeetingTimeChecker;

        [SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
            _stateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_person = _mocks.StrictMock<IPerson>();
			_part = _mocks.StrictMock<IScheduleDay>();
			_shiftProjectionCacheManager = _mocks.StrictMock<IShiftProjectionCacheManager>();
			_personSkillPeriodsDataHolderManager = _mocks.StrictMock<IPersonSkillPeriodsDataHolderManager>();
			_calculatorManager = _mocks.StrictMock<IWorkShiftCalculatorsManager>();
			_shiftProjectionCacheFilter = _mocks.StrictMock<IShiftProjectionCacheFilter>();
			_preSchedulingStatusChecker = _mocks.StrictMock<IPreSchedulingStatusChecker>();
			_personPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2001,1,1));
			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();

             _scheduleDateOnlyPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 2, 10),_timeZoneInfo);
			_info = new PermissionInformation(_person);
			_info.SetDefaultTimeZone(_timeZoneInfo);

        	_workShiftMinMaxCalculator = _mocks.StrictMock<IWorkShiftMinMaxCalculator>();
            _fairnessAndMaxSeatCalculatorsManager = _mocks.StrictMock<IFairnessAndMaxSeatCalculatorsManager>();
            _schedulingOptions = new SchedulingOptions();
        	_shiftLengthDecider = _mocks.StrictMock<IShiftLengthDecider>();
			_target = new WorkShiftFinderService(()=>_stateHolder, ()=>_preSchedulingStatusChecker,
               _shiftProjectionCacheFilter, ()=>_personSkillPeriodsDataHolderManager,
               _shiftProjectionCacheManager, _calculatorManager, ()=>_workShiftMinMaxCalculator, _fairnessAndMaxSeatCalculatorsManager,
			   _shiftLengthDecider, new PersonSkillDayCreator(new PersonalSkillsProvider()));
        	_personalShiftMeetingTimeChecker = _mocks.StrictMock<IPersonalShiftMeetingTimeChecker>();
		}

        [Test]
		public void VerifyFindBestShift()
        {
            var bag = _mocks.StrictMock<IRuleSetBag>();
			var dictionary = _mocks.StrictMock<IScheduleDictionary>();
			var range = _mocks.StrictMock<IScheduleRange>();
			var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
			var caches = getCashes();
			var dateOnly = new DateOnly(2009, 2, 2);
			_personPeriod.RuleSetBag = bag;
			_scheduleDateOnlyPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 2, 2), _timeZoneInfo);
			var dataHolders = new Dictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>> ();
            var results = new List<IWorkShiftCalculationResultHolder>
                              {
                                  new WorkShiftCalculationResult { Value = 1, ShiftProjection = caches[0] }, 
                                  new WorkShiftCalculationResult { Value = 2, ShiftProjection = caches[1] }
                              };

			using (_mocks.Record())
			{
                Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
			    Expect.Call(_stateHolder.Schedules).Return(dictionary).Repeat.Any();
                Expect.Call(_part.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_part.DateOnlyAsPeriod).Return(_scheduleDateOnlyPeriod).Repeat.AtLeastOnce();
				Expect.Call(_preSchedulingStatusChecker.CheckStatus(null, null, _schedulingOptions)).Return(true).IgnoreArguments();
				Expect.Call(_person.VirtualSchedulePeriod(_scheduleDateOnly)).Return(_schedulePeriod).IgnoreArguments().Repeat.AtLeastOnce();
			    Expect.Call(_person.Period(dateOnly)).Return(_personPeriod).Repeat.AtLeastOnce();
				Expect.Call(dictionary[_person]).Return(range).Repeat.AtLeastOnce();
                Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(_scheduleDateOnlyPeriod, bag, false, true)).Return(caches);
				Expect.Call(_shiftProjectionCacheFilter.FilterOnMainShiftOptimizeActivitiesSpecification(caches, new Domain.Specification.All<IEditableShift>())).
					IgnoreArguments().Return(caches).Repeat.AtLeastOnce();
				Expect.Call(_shiftProjectionCacheFilter.FilterOnRestrictionAndNotAllowedShiftCategories(new DateOnly(), null, null, null, null, null)).
					IgnoreArguments().Return(caches);
				Expect.Call(_shiftProjectionCacheFilter.CheckRestrictions(_schedulingOptions, effectiveRestriction, null)).IgnoreArguments().Return(
					true);
				Expect.Call(_schedulePeriod.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_workShiftMinMaxCalculator.MinMaxAllowedShiftContractTime(dateOnly, _matrix, _schedulingOptions)).Return(
					new MinMax<TimeSpan>(new TimeSpan(0, 6, 0, 0), new TimeSpan(0, 12, 0, 0)));
				Expect.Call(_shiftProjectionCacheFilter.Filter(null, new MinMax<TimeSpan>(), caches, _scheduleDateOnly,
															   range, null)).IgnoreArguments().Return(caches);
				Expect.Call(_personSkillPeriodsDataHolderManager.GetPersonMaxSeatSkillSkillStaffPeriods(new PersonSkillDay())).Return(
					new Dictionary<ISkill, ISkillStaffPeriodDictionary>()).IgnoreArguments();
				Expect.Call(_personSkillPeriodsDataHolderManager.GetPersonNonBlendSkillSkillStaffPeriods(new PersonSkillDay())).Return(
					new Dictionary<ISkill, ISkillStaffPeriodDictionary>()).IgnoreArguments();
				Expect.Call(_personSkillPeriodsDataHolderManager.GetPersonSkillPeriodsDataHolderDictionary(new PersonSkillDay())).Return(dataHolders).IgnoreArguments();
				Expect.Call(_calculatorManager.RunCalculators(_person, caches, null,
					new Dictionary<ISkill, ISkillStaffPeriodDictionary>(), _schedulingOptions)).Return(
						results)
					.IgnoreArguments();
				Expect.Call(_schedulePeriod.AverageWorkTimePerDay).Return(TimeSpan.FromHours(7));
				Expect.Call(_fairnessAndMaxSeatCalculatorsManager.RecalculateFoundValues(results, 2, _person, dateOnly, new Dictionary<ISkill, ISkillStaffPeriodDictionary>(),
					TimeSpan.FromHours(7), _schedulingOptions)).Return(results);
                
				Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
				Expect.Call(_person.PermissionInformation).Return(_info).Repeat.AtLeastOnce();
				Expect.Call(_shiftLengthDecider.FilterList(caches, _workShiftMinMaxCalculator, _matrix, _schedulingOptions)).Return(caches);
				effectiveRestriction.ShiftCategory = _category;
			}
            _schedulingOptions.ShiftCategory = _category;
			using (_mocks.Playback())
			{
				var retShift = _target.FindBestShift(_part, _schedulingOptions, _matrix, effectiveRestriction);
				Assert.IsNotNull(retShift.ResultHolder);
			}
		}

        [Test]
        public void ShouldReturnNullWhenAllowedMinMaxEqualsNull()
        {
            var bag = _mocks.StrictMock<IRuleSetBag>();
            var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
            IList<IShiftProjectionCache> caches = getCashes();
            var dateOnly = new DateOnly(2009, 2, 2);
			_personPeriod.RuleSetBag = bag;
            _scheduleDateOnlyPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 2, 2), _timeZoneInfo);
            using (_mocks.Record())
            {
                Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
                Expect.Call(_part.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_part.DateOnlyAsPeriod).Return(_scheduleDateOnlyPeriod).Repeat.AtLeastOnce();
				Expect.Call(_preSchedulingStatusChecker.CheckStatus(null, null, _schedulingOptions)).Return(true).IgnoreArguments();
                Expect.Call(_person.VirtualSchedulePeriod(_scheduleDateOnly)).Return(_schedulePeriod).IgnoreArguments().Repeat.AtLeastOnce();
                Expect.Call(_person.Period(dateOnly)).Return(_personPeriod).Repeat.AtLeastOnce();
                Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(_scheduleDateOnlyPeriod, bag, false, true)).Return(caches);
				Expect.Call(_shiftProjectionCacheFilter.FilterOnMainShiftOptimizeActivitiesSpecification(caches, new Domain.Specification.All<IEditableShift>())).
					IgnoreArguments().Return(caches).Repeat.AtLeastOnce();
                effectiveRestriction.ShiftCategory = _category;
                Expect.Call(_shiftProjectionCacheFilter.FilterOnRestrictionAndNotAllowedShiftCategories(new DateOnly(), null, null, null, null, null)).
                    IgnoreArguments().Return(new List<IShiftProjectionCache>());
				
                Expect.Call(_shiftProjectionCacheFilter.CheckRestrictions(null, null, null)).IgnoreArguments().Return(true);
                Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
                Expect.Call(_person.PermissionInformation).Return(_info).Repeat.AtLeastOnce();
            }
            _schedulingOptions.ShiftCategory = _category;
			_schedulingOptions.UseAvailability = false;
			_schedulingOptions.UsePreferences = false;
			_schedulingOptions.UseRotations = false;
			_schedulingOptions.UseStudentAvailability = false;
            using (_mocks.Playback())
            {
				var retShift = _target.FindBestShift(_part, _schedulingOptions, _matrix, effectiveRestriction);
                Assert.That(retShift.ResultHolder, Is.Null);
            }
        }

		[Test]
		public void ShouldCallShiftProjectionCachesFromRuleSetBagWithBlackListIfUsingPreferencesAndFirstAttemptFailed()
        {
            var bag = _mocks.StrictMock<IRuleSetBag>();
            var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
            IList<IShiftProjectionCache> caches = getCashes();
			var dateOnly = new DateOnly(2009, 2, 2);
			_personPeriod.RuleSetBag = bag;
            _scheduleDateOnlyPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 2, 2), _timeZoneInfo);
            using (_mocks.Record())
            {
                Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
                Expect.Call(_part.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_part.DateOnlyAsPeriod).Return(_scheduleDateOnlyPeriod).Repeat.AtLeastOnce();
				Expect.Call(_preSchedulingStatusChecker.CheckStatus(null, null, _schedulingOptions)).Return(true).IgnoreArguments();
                Expect.Call(_person.VirtualSchedulePeriod(_scheduleDateOnly)).Return(_schedulePeriod).IgnoreArguments().Repeat.AtLeastOnce();
                Expect.Call(_person.Period(dateOnly)).Return(_personPeriod).Repeat.AtLeastOnce();
				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(_scheduleDateOnlyPeriod, bag, false, true)).Return(caches);
				Expect.Call(_shiftProjectionCacheFilter.FilterOnMainShiftOptimizeActivitiesSpecification(new List<IShiftProjectionCache>(), new Domain.Specification.All<IEditableShift>())).
					IgnoreArguments().Return(caches).Repeat.AtLeastOnce();
                effectiveRestriction.ShiftCategory = _category;
                Expect.Call(_shiftProjectionCacheFilter.FilterOnRestrictionAndNotAllowedShiftCategories(new DateOnly(), null, null, null, null, null)).
                    IgnoreArguments().Return(caches);
				Expect.Call(_workShiftMinMaxCalculator.MinMaxAllowedShiftContractTime(dateOnly, _matrix, _schedulingOptions)).Return(null);

				Expect.Call(_shiftProjectionCacheFilter.CheckRestrictions(null, null, null)).IgnoreArguments().Return(true);
				Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
				Expect.Call(_person.PermissionInformation).Return(_info).Repeat.AtLeastOnce();

				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(_scheduleDateOnlyPeriod, bag, true, true)).Return(caches);
				effectiveRestriction.ShiftCategory = _category;
				Expect.Call(_shiftProjectionCacheFilter.FilterOnRestrictionAndNotAllowedShiftCategories(new DateOnly(), null, null, null, null, null)).
					IgnoreArguments().Return(caches);
				Expect.Call(_workShiftMinMaxCalculator.MinMaxAllowedShiftContractTime(dateOnly, _matrix, _schedulingOptions)).Return(null);

	            Expect.Call(effectiveRestriction.IsRestriction).Return(true);
            }
            _schedulingOptions.ShiftCategory = _category;
            using (_mocks.Playback())
            {
				var retShift = _target.FindBestShift(_part, _schedulingOptions, _matrix, effectiveRestriction);
                Assert.That(retShift.ResultHolder, Is.Null);
            }
        }

        [Test]
        public void ShouldReturnNullWhenNoShifts()
        {
            var bag = _mocks.StrictMock<IRuleSetBag>();
            var dateOnly = new DateOnly(2009, 2, 2);
            _scheduleDateOnlyPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 2, 2), _timeZoneInfo);
			_personPeriod.RuleSetBag = bag;
            var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();

            using (_mocks.Record())
            {
                Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
                Expect.Call(_part.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_part.DateOnlyAsPeriod).Return(_scheduleDateOnlyPeriod).Repeat.AtLeastOnce();
                Expect.Call(_person.VirtualSchedulePeriod(_scheduleDateOnly)).Return(_schedulePeriod).IgnoreArguments().Repeat.AtLeastOnce();
                Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
				Expect.Call(_preSchedulingStatusChecker.CheckStatus(null, null, _schedulingOptions)).Return(true).IgnoreArguments();
                Expect.Call(_shiftProjectionCacheFilter.CheckRestrictions(null, null, null)).IgnoreArguments().Return(true);
                Expect.Call(_person.Period(dateOnly)).Return(_personPeriod).Repeat.AtLeastOnce();
				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(_scheduleDateOnlyPeriod, bag, false, true)).Return(new List<IShiftProjectionCache>());
				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(_scheduleDateOnlyPeriod, bag, true, true)).Return(new List<IShiftProjectionCache>());
	            Expect.Call(effectiveRestriction.IsRestriction).Return(true);
            }
            
            using (_mocks.Playback())
            {
				var retShift = _target.FindBestShift(_part, _schedulingOptions, _matrix, effectiveRestriction);
                Assert.That(retShift.ResultHolder, Is.Null);
            }
        }

		private void commonMocksForBlackListTests(IRuleSetBag bag, DateOnly dateOnly)
		{
			Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
			Expect.Call(_part.Person).Return(_person).Repeat.AtLeastOnce();
			Expect.Call(_part.DateOnlyAsPeriod).Return(_scheduleDateOnlyPeriod).Repeat.AtLeastOnce();
			Expect.Call(_person.VirtualSchedulePeriod(_scheduleDateOnly)).Return(_schedulePeriod).IgnoreArguments().Repeat.AtLeastOnce();
			Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
			Expect.Call(_preSchedulingStatusChecker.CheckStatus(null, null, _schedulingOptions)).Return(true).IgnoreArguments();
			Expect.Call(_shiftProjectionCacheFilter.CheckRestrictions(null, null, null)).IgnoreArguments().Return(true);
			Expect.Call(_person.Period(dateOnly)).Return(_personPeriod).Repeat.AtLeastOnce();
			Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(_scheduleDateOnlyPeriod, bag, false, true)).Return(new List<IShiftProjectionCache>());
		}

		[Test]
		public void ShouldNotUseBlacklistIfNoRestriction()
		{
			var bag = _mocks.StrictMock<IRuleSetBag>();
			var dateOnly = new DateOnly(2009, 2, 2);
			_personPeriod.RuleSetBag = bag;
			_scheduleDateOnlyPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 2, 2), _timeZoneInfo);
			var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
			_schedulingOptions.UseAvailability = false;
			_schedulingOptions.UsePreferences = false;
			_schedulingOptions.UseRotations = false;
			_schedulingOptions.UseStudentAvailability = false;

			using (_mocks.Record())
			{
				commonMocksForBlackListTests(bag, dateOnly);
			}

			using (_mocks.Playback())
			{
				var retShift = _target.FindBestShift(_part, _schedulingOptions, _matrix, effectiveRestriction);
				Assert.That(retShift.ResultHolder, Is.Null);
			}
		}

		[Test]
		public void ShouldUseBlacklistIfUseStudentAvailability()
		{
			var bag = _mocks.StrictMock<IRuleSetBag>();
			var dateOnly = new DateOnly(2009, 2, 2);
			_scheduleDateOnlyPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 2, 2), _timeZoneInfo);
			_personPeriod.RuleSetBag = bag;
			var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
			_schedulingOptions.UseAvailability = false;
			_schedulingOptions.UsePreferences = false;
			_schedulingOptions.UseRotations = false;
			_schedulingOptions.UseStudentAvailability = true;

			using (_mocks.Record())
			{
				commonMocksForBlackListTests(bag, dateOnly);
				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(_scheduleDateOnlyPeriod, bag, true, true)).Return(new List<IShiftProjectionCache>());
				Expect.Call(effectiveRestriction.IsRestriction).Return(true);
			}

			using (_mocks.Playback())
			{
				var retShift = _target.FindBestShift(_part, _schedulingOptions, _matrix, effectiveRestriction);
				Assert.That(retShift.ResultHolder, Is.Null);
			}
		}

		[Test]
		public void ShouldUseBlacklistIfUseRotations()
		{
			var bag = _mocks.StrictMock<IRuleSetBag>();
			var dateOnly = new DateOnly(2009, 2, 2);
			_scheduleDateOnlyPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 2, 2), _timeZoneInfo);
			_personPeriod.RuleSetBag = bag;
			var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
			_schedulingOptions.UseAvailability = false;
			_schedulingOptions.UsePreferences = false;
			_schedulingOptions.UseRotations = true;
			_schedulingOptions.UseStudentAvailability = false;

			using (_mocks.Record())
			{
				commonMocksForBlackListTests(bag, dateOnly);
				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(_scheduleDateOnlyPeriod, bag, true, true)).Return(new List<IShiftProjectionCache>());
				Expect.Call(effectiveRestriction.IsRestriction).Return(true);
			}

			using (_mocks.Playback())
			{
				var retShift = _target.FindBestShift(_part, _schedulingOptions, _matrix, effectiveRestriction);
				Assert.That(retShift.ResultHolder, Is.Null);
			}
		}

		[Test]
		public void ShouldUseBlacklistIfUsePreferences()
		{
			var bag = _mocks.StrictMock<IRuleSetBag>();
			var dateOnly = new DateOnly(2009, 2, 2);
			_scheduleDateOnlyPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 2, 2), _timeZoneInfo);
			_personPeriod.RuleSetBag = bag;
			var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
			_schedulingOptions.UseAvailability = false;
			_schedulingOptions.UsePreferences = true;
			_schedulingOptions.UseRotations = false;
			_schedulingOptions.UseStudentAvailability = false;

			using (_mocks.Record())
			{
				commonMocksForBlackListTests(bag, dateOnly);
				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(_scheduleDateOnlyPeriod, bag, true, true)).Return(new List<IShiftProjectionCache>());
				Expect.Call(effectiveRestriction.IsRestriction).Return(true);
			}

			using (_mocks.Playback())
			{
				var retShift = _target.FindBestShift(_part, _schedulingOptions, _matrix, effectiveRestriction);
				Assert.That(retShift.ResultHolder, Is.Null);
			}
		}

		[Test]
		public void ShouldUseBlacklistIfUseAvailability()
		{
			var bag = _mocks.StrictMock<IRuleSetBag>();
			var dateOnly = new DateOnly(2009, 2, 2);
			_scheduleDateOnlyPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 2, 2), _timeZoneInfo);
			_personPeriod.RuleSetBag = bag;
			var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
			_schedulingOptions.UseAvailability = true;
			_schedulingOptions.UsePreferences = false;
			_schedulingOptions.UseRotations = false;
			_schedulingOptions.UseStudentAvailability = false;

			using (_mocks.Record())
			{
				commonMocksForBlackListTests(bag, dateOnly);
				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(_scheduleDateOnlyPeriod, bag, true, true)).Return(new List<IShiftProjectionCache>());
				Expect.Call(effectiveRestriction.IsRestriction).Return(true);
			}

			using (_mocks.Playback())
			{
				var retShift = _target.FindBestShift(_part, _schedulingOptions, _matrix, effectiveRestriction);
				Assert.That(retShift.ResultHolder, Is.Null);
			}
		}

        [Test]
    	public void ShouldReturnNullWhenNoShiftBags()
		{
			var dateOnly = new DateOnly(2009, 2, 10);
            var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();

            Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
            Expect.Call(_person.VirtualSchedulePeriod(dateOnly)).Return(_schedulePeriod);
			Expect.Call(_schedulePeriod.IsValid).Return(true);
			Expect.Call(_preSchedulingStatusChecker.CheckStatus(null, null, _schedulingOptions)).Return(true).IgnoreArguments();
			Expect.Call(_shiftProjectionCacheFilter.CheckRestrictions(null, null, null)).IgnoreArguments().Return(true);
            Expect.Call(_part.Person).Return(_person).Repeat.AtLeastOnce();
            Expect.Call(_part.DateOnlyAsPeriod).Return(_scheduleDateOnlyPeriod).Repeat.AtLeastOnce();
            Expect.Call(_person.Period(dateOnly)).Return(_personPeriod).Repeat.AtLeastOnce();
			Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(_scheduleDateOnlyPeriod, (IRuleSetBag) null, false, true)).Return(new List<IShiftProjectionCache>()).IgnoreArguments();
			Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(_scheduleDateOnlyPeriod, (IRuleSetBag) null, false, true)).Return(new List<IShiftProjectionCache>()).IgnoreArguments();
			Expect.Call(effectiveRestriction.IsRestriction).Return(true);

			_mocks.ReplayAll();
            _schedulingOptions.ShiftCategory = _category;

            var retShift = _target.FindBestShift(_part, _schedulingOptions, _matrix, effectiveRestriction);
			Assert.That(retShift.ResultHolder, Is.Null);

			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnNullWhenNotValidPeriod()
		{
			var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();

            Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
			Expect.Call(_preSchedulingStatusChecker.CheckStatus(null, null, _schedulingOptions)).Return(true).IgnoreArguments();
            Expect.Call(_part.Person).Return(_person).Repeat.AtLeastOnce();
            Expect.Call(_part.DateOnlyAsPeriod).Return(_scheduleDateOnlyPeriod).Repeat.AtLeastOnce();
            Expect.Call(_person.VirtualSchedulePeriod(_scheduleDateOnly)).Return(_schedulePeriod).Repeat.AtLeastOnce();
			Expect.Call(_schedulePeriod.IsValid).Return(false);
			_mocks.ReplayAll();

            _schedulingOptions.ShiftCategory = _category;
			var retShift = _target.FindBestShift(_part, _schedulingOptions, _matrix, effectiveRestriction);
			Assert.That(retShift.ResultHolder, Is.Null);

			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnNullWhenCheckStatusFailsButSetPersonAndDate()
		{
			var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();

		    Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
			Expect.Call(_preSchedulingStatusChecker.CheckStatus(null, null, _schedulingOptions)).Return(false).IgnoreArguments();
            Expect.Call(_part.Person).Return(_person).Repeat.AtLeastOnce();
            Expect.Call(_part.DateOnlyAsPeriod).Return(_scheduleDateOnlyPeriod).Repeat.AtLeastOnce();
			_mocks.ReplayAll();
            _schedulingOptions.ShiftCategory = _category;
			var retShift = _target.FindBestShift(_part, _schedulingOptions, _matrix, effectiveRestriction);
			Assert.That(retShift.ResultHolder, Is.Null);

			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnNullWhenCheckRestrictionFails()
		{
            var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();

            Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
            Expect.Call(_part.Person).Return(_person).Repeat.AtLeastOnce();
            Expect.Call(_part.DateOnlyAsPeriod).Return(_scheduleDateOnlyPeriod).Repeat.AtLeastOnce();
            Expect.Call(_person.VirtualSchedulePeriod(_scheduleDateOnly)).Return(_schedulePeriod).Repeat.AtLeastOnce();
            Expect.Call(_schedulePeriod.IsValid).Return(true);
			Expect.Call(_preSchedulingStatusChecker.CheckStatus(null, null, _schedulingOptions)).Return(true).IgnoreArguments();
			Expect.Call(_shiftProjectionCacheFilter.CheckRestrictions(null, null, null)).IgnoreArguments().Return(false);

			_mocks.ReplayAll();
           
            _schedulingOptions.ShiftCategory = _category;
			var retShift = _target.FindBestShift(_part, _schedulingOptions, _matrix, effectiveRestriction);
			Assert.That(retShift.ResultHolder, Is.Null);

			_mocks.VerifyAll();
		}

		private IList<IShiftProjectionCache> getCashes()
		{
			var dateOnly = new DateOnly(2009, 2, 2);
			var tmpList = getWorkShifts();
			var retList = new List<IShiftProjectionCache>();

			var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(dateOnly, _timeZoneInfo);
			foreach (IWorkShift shift in tmpList)
			{
				var cache = new ShiftProjectionCache(shift, _personalShiftMeetingTimeChecker);
				cache.SetDate(dateOnlyAsDateTimePeriod);
				retList.Add(cache);
			}
			return retList;
		}
		private IEnumerable<IWorkShift> getWorkShifts()
		{
			_activity = ActivityFactory.CreateActivity("sd");
			_category = ShiftCategoryFactory.CreateShiftCategory("dv");
			_workShift1 = WorkShiftFactory.CreateWorkShift(new TimeSpan(7, 0, 0), new TimeSpan(15, 0, 0),
														  _activity, _category);
			_workShift2 = WorkShiftFactory.CreateWorkShift(new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0),
														  _activity, _category);
			_workShift3 = WorkShiftFactory.CreateWorkShift(new TimeSpan(10, 0, 0), new TimeSpan(19, 0, 0),
																	  _activity, _category);

			return new List<IWorkShift> { _workShift1, _workShift2, _workShift3 };
		}
        
        [Test]
		public void ShouldNotCalculateMaxSeatValueWhenNoResultFromFirstCalculators()
        {
			var results = new IWorkShiftCalculationResultHolder[] { };
			var caches = new List<IShiftProjectionCache>{_mocks.DynamicMock<IShiftProjectionCache>()};
			var bag = _mocks.StrictMock<IRuleSetBag>();
			var dictionary = _mocks.StrictMock<IScheduleDictionary>();
			var range = _mocks.StrictMock<IScheduleRange>();
			var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
			var dateOnly = new DateOnly(2009, 2, 2);
			_personPeriod.RuleSetBag = bag;
			_scheduleDateOnlyPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 2, 2), _timeZoneInfo);
			var dataHolders = new Dictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>>();
			
			using (_mocks.Record())
			{
				Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
				Expect.Call(_stateHolder.Schedules).Return(dictionary).Repeat.AtLeastOnce();
				Expect.Call(_part.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_part.DateOnlyAsPeriod).Return(_scheduleDateOnlyPeriod).Repeat.AtLeastOnce();
				Expect.Call(_preSchedulingStatusChecker.CheckStatus(null, null, _schedulingOptions)).Return(true).IgnoreArguments();
				Expect.Call(_person.VirtualSchedulePeriod(_scheduleDateOnly)).Return(_schedulePeriod).IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(_person.Period(dateOnly)).Return(_personPeriod).Repeat.AtLeastOnce();
				Expect.Call(dictionary[_person]).Return(range).Repeat.AtLeastOnce();
				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(_scheduleDateOnlyPeriod, bag, false, true)).Return(caches);
				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(_scheduleDateOnlyPeriod, bag, true, true)).Return(new IShiftProjectionCache[]{});
				Expect.Call(_shiftProjectionCacheFilter.FilterOnMainShiftOptimizeActivitiesSpecification(caches, new Domain.Specification.All<IEditableShift>())).
					IgnoreArguments().Return(caches).Repeat.AtLeastOnce();
				Expect.Call(_shiftProjectionCacheFilter.FilterOnRestrictionAndNotAllowedShiftCategories(new DateOnly(), null, null, null, null, null)).
					IgnoreArguments().Return(caches).Repeat.AtLeastOnce();
				Expect.Call(_shiftProjectionCacheFilter.CheckRestrictions(_schedulingOptions, effectiveRestriction, null)).IgnoreArguments().Return(
					true);
				Expect.Call(_schedulePeriod.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_workShiftMinMaxCalculator.MinMaxAllowedShiftContractTime(dateOnly, _matrix, _schedulingOptions)).Return(
					new MinMax<TimeSpan>(new TimeSpan(0, 6, 0, 0), new TimeSpan(0, 12, 0, 0))).Repeat.AtLeastOnce();
				Expect.Call(_shiftProjectionCacheFilter.Filter(null, new MinMax<TimeSpan>(), caches, _scheduleDateOnly,
															   range, null)).IgnoreArguments().Return(caches).Repeat.AtLeastOnce();
				Expect.Call(_personSkillPeriodsDataHolderManager.GetPersonMaxSeatSkillSkillStaffPeriods(new PersonSkillDay())).Return(
					new Dictionary<ISkill, ISkillStaffPeriodDictionary>()).IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(_personSkillPeriodsDataHolderManager.GetPersonNonBlendSkillSkillStaffPeriods(new PersonSkillDay())).Return(
					new Dictionary<ISkill, ISkillStaffPeriodDictionary>()).IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(_personSkillPeriodsDataHolderManager.GetPersonSkillPeriodsDataHolderDictionary(new PersonSkillDay())).IgnoreArguments().Return(dataHolders).Repeat.AtLeastOnce();
				Expect.Call(_calculatorManager.RunCalculators(_person, caches, null,
					new Dictionary<ISkill, ISkillStaffPeriodDictionary>(), _schedulingOptions)).Return(
						new IWorkShiftCalculationResultHolder[]{})
					.IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(_fairnessAndMaxSeatCalculatorsManager.RecalculateFoundValues(results, -1, _person, dateOnly, new Dictionary<ISkill, ISkillStaffPeriodDictionary>(),
					TimeSpan.FromHours(7), _schedulingOptions)).Return(results).Repeat.Never();

				Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
				Expect.Call(_person.PermissionInformation).Return(_info).Repeat.AtLeastOnce();
				Expect.Call(_shiftLengthDecider.FilterList(caches, _workShiftMinMaxCalculator, _matrix, _schedulingOptions)).Return(caches).Repeat.AtLeastOnce();
				Expect.Call(effectiveRestriction.IsRestriction).Return(true);
			}

	        using (_mocks.Playback())
	        {
		        _target.FindBestShift(_part, _schedulingOptions, _matrix, effectiveRestriction);
	        }
        }

        [Test]
        public void ShouldCalculateMaxSeatValueWhenShiftValueIsHigherThanDoubleMin()
        {
			var results = new List<IWorkShiftCalculationResultHolder>
                              {
                                  new WorkShiftCalculationResult { Value = -1, ShiftProjection = null } 
                              };
            _schedulingOptions.OnlyShiftsWhenUnderstaffed = true;
			var caches = new[] { _mocks.DynamicMock<IShiftProjectionCache>() };
			var bag = _mocks.StrictMock<IRuleSetBag>();
			var dictionary = _mocks.StrictMock<IScheduleDictionary>();
			var range = _mocks.StrictMock<IScheduleRange>();
			var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
			var dateOnly = new DateOnly(2009, 2, 2);
			_scheduleDateOnlyPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 2, 2), _timeZoneInfo);
			var dataHolders = new Dictionary<IActivity, IDictionary<DateTime, ISkillStaffPeriodDataHolder>>();
			_personPeriod.RuleSetBag = bag;
				
			using (_mocks.Record())
			{
				Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
				Expect.Call(_stateHolder.Schedules).Return(dictionary).Repeat.AtLeastOnce();
				Expect.Call(_part.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_part.DateOnlyAsPeriod).Return(_scheduleDateOnlyPeriod).Repeat.AtLeastOnce();
				Expect.Call(_preSchedulingStatusChecker.CheckStatus(null, null, _schedulingOptions)).Return(true).IgnoreArguments();
				Expect.Call(_person.VirtualSchedulePeriod(_scheduleDateOnly)).Return(_schedulePeriod).IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(_person.Period(dateOnly)).Return(_personPeriod).Repeat.AtLeastOnce();
				Expect.Call(dictionary[_person]).Return(range).Repeat.AtLeastOnce();
				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(_scheduleDateOnlyPeriod, bag, false, true)).Return(caches);
				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(_scheduleDateOnlyPeriod, bag, true, true)).Return(caches);
				Expect.Call(_shiftProjectionCacheFilter.FilterOnMainShiftOptimizeActivitiesSpecification(caches, new Domain.Specification.All<IEditableShift>())).
					IgnoreArguments().Return(caches).Repeat.AtLeastOnce();
				Expect.Call(_shiftProjectionCacheFilter.FilterOnRestrictionAndNotAllowedShiftCategories(new DateOnly(), null, null, null, null, null)).
					IgnoreArguments().Return(caches).Repeat.AtLeastOnce();
				Expect.Call(_shiftProjectionCacheFilter.CheckRestrictions(_schedulingOptions, effectiveRestriction, null)).IgnoreArguments().Return(
					true);
				Expect.Call(_schedulePeriod.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_workShiftMinMaxCalculator.MinMaxAllowedShiftContractTime(dateOnly, _matrix, _schedulingOptions)).Return(
					new MinMax<TimeSpan>(new TimeSpan(0, 6, 0, 0), new TimeSpan(0, 12, 0, 0))).Repeat.AtLeastOnce();
				Expect.Call(_shiftProjectionCacheFilter.Filter(null, new MinMax<TimeSpan>(), caches, _scheduleDateOnly,
															   range, null)).IgnoreArguments().Return(caches).Repeat.AtLeastOnce();
				Expect.Call(_personSkillPeriodsDataHolderManager.GetPersonMaxSeatSkillSkillStaffPeriods(new PersonSkillDay())).Return(
					new Dictionary<ISkill, ISkillStaffPeriodDictionary>()).IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(_personSkillPeriodsDataHolderManager.GetPersonNonBlendSkillSkillStaffPeriods(new PersonSkillDay())).Return(
					new Dictionary<ISkill, ISkillStaffPeriodDictionary>()).IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(_personSkillPeriodsDataHolderManager.GetPersonSkillPeriodsDataHolderDictionary(new PersonSkillDay())).IgnoreArguments().Return(dataHolders).Repeat.AtLeastOnce();
				Expect.Call(_calculatorManager.RunCalculators(_person, caches, null,
					new Dictionary<ISkill, ISkillStaffPeriodDictionary>(), _schedulingOptions)).Return(
						results)
					.IgnoreArguments().Repeat.AtLeastOnce();
				Expect.Call(_schedulePeriod.AverageWorkTimePerDay).Return(TimeSpan.FromHours(7)).Repeat.AtLeastOnce();
				Expect.Call(_fairnessAndMaxSeatCalculatorsManager.RecalculateFoundValues(results, -1, _person, dateOnly, new Dictionary<ISkill, ISkillStaffPeriodDictionary>(),
					TimeSpan.FromHours(7), _schedulingOptions)).Return(results).Repeat.AtLeastOnce();

				Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
				Expect.Call(_person.PermissionInformation).Return(_info).Repeat.AtLeastOnce();
				Expect.Call(_shiftLengthDecider.FilterList(caches, _workShiftMinMaxCalculator, _matrix, _schedulingOptions)).Return(caches).Repeat.AtLeastOnce();

				Expect.Call(effectiveRestriction.IsRestriction).Return(true);
			}

			using (_mocks.Playback())
			{
				_target.FindBestShift(_part, _schedulingOptions, _matrix, effectiveRestriction);
			}
        }
    }

	internal class WorkShiftFinderResultForTest : IWorkShiftFinderResult
	{
		private readonly IList<IWorkShiftFilterResult> _filterResults = new List<IWorkShiftFilterResult>();

		public IPerson Person
		{
			get { throw new NotImplementedException(); }
		}

		public Tuple<Guid,DateOnly> PersonDateKey
		{
			get { throw new NotImplementedException(); }
		}

		public string PersonName
		{
			get { throw new NotImplementedException(); }
		}

		public DateOnly ScheduleDate
		{
			get { throw new NotImplementedException(); }
		}

		public bool Successful
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}


		public ReadOnlyCollection<IWorkShiftFilterResult> FilterResults
		{
			get { return new ReadOnlyCollection<IWorkShiftFilterResult>(_filterResults); }
		}

		public DateTime SchedulingDateTime
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public bool StoppedOnOverstaffing
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public void AddFilterResults(IWorkShiftFilterResult filterResult)
		{
			_filterResults.Add(filterResult);
		}

        
	}

}
