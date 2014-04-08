using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces;
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
		private TimeZoneInfo _timeZoneInfo;

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
    	private IPossibleStartEndCategory _possibleStartEndCategory;
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
			_personPeriod = _mocks.StrictMock<IPersonPeriod>();
			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			var zone = TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time");
			_timeZoneInfo = (zone);
             _scheduleDateOnlyPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 2, 10),_timeZoneInfo);
			_info = new PermissionInformation(_person);
			_info.SetDefaultTimeZone(_timeZoneInfo);
            _mocks.StrictMock<IShiftCategoryFairnessShiftValueCalculator>();

        	_workShiftMinMaxCalculator = _mocks.StrictMock<IWorkShiftMinMaxCalculator>();
            _fairnessAndMaxSeatCalculatorsManager = _mocks.StrictMock<IFairnessAndMaxSeatCalculatorsManager>();
            _schedulingOptions = new SchedulingOptions();
        	_shiftLengthDecider = _mocks.StrictMock<IShiftLengthDecider>();
			_target = new WorkShiftFinderService(_stateHolder, _preSchedulingStatusChecker,
               _shiftProjectionCacheFilter, _personSkillPeriodsDataHolderManager,
               _shiftProjectionCacheManager, _calculatorManager, _workShiftMinMaxCalculator, _fairnessAndMaxSeatCalculatorsManager,
			   _shiftLengthDecider);
			_possibleStartEndCategory =new PossibleStartEndCategory();
        	_personalShiftMeetingTimeChecker = _mocks.StrictMock<IPersonalShiftMeetingTimeChecker>();
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void VerifyFindBestShift()
        {
            var bag = _mocks.StrictMock<IRuleSetBag>();
			var dictionary = _mocks.StrictMock<IScheduleDictionary>();
			var range = _mocks.StrictMock<IScheduleRange>();
			var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
			var caches = getCashes();
			var dateOnly = new DateOnly(2009, 2, 2);
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
			    Expect.Call(_stateHolder.Schedules).Return(dictionary);
                Expect.Call(_part.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_part.DateOnlyAsPeriod).Return(_scheduleDateOnlyPeriod).Repeat.AtLeastOnce();
				Expect.Call(_preSchedulingStatusChecker.CheckStatus(null, null, _schedulingOptions)).Return(true).IgnoreArguments();
				Expect.Call(_person.VirtualSchedulePeriod(_scheduleDateOnly)).Return(_schedulePeriod).IgnoreArguments().Repeat.AtLeastOnce();
			    Expect.Call(_person.Period(dateOnly)).Return(_personPeriod);
				Expect.Call(_personPeriod.RuleSetBag).Return(bag);
				Expect.Call(dictionary[_person]).Return(range).Repeat.AtLeastOnce();
                Expect.Call(_person.WorkflowControlSet).Return(null).Repeat.AtLeastOnce();
                Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(dateOnly, _timeZoneInfo, bag, false, true)).Return(caches);
				Expect.Call(_shiftProjectionCacheFilter.FilterOnMainShiftOptimizeActivitiesSpecification(caches, new Domain.Specification.All<IEditableShift>())).
					IgnoreArguments().Return(caches).Repeat.AtLeastOnce();
				Expect.Call(_shiftProjectionCacheFilter.FilterOnGroupSchedulingCommonStartEnd(caches, null, _schedulingOptions, null)).
					IgnoreArguments().Return(caches).Repeat.AtLeastOnce();
                Expect.Call(_shiftProjectionCacheFilter.FilterOnGroupSchedulingCommonActivity (caches,_schedulingOptions,null,null)).
                    IgnoreArguments().Return(caches).Repeat.AtLeastOnce();
                effectiveRestriction.ShiftCategory = _category;
				Expect.Call(_shiftProjectionCacheFilter.FilterOnRestrictionAndNotAllowedShiftCategories(new DateOnly(), null, null, null, null, null)).
					IgnoreArguments().Return(caches);
				Expect.Call(_shiftProjectionCacheFilter.CheckRestrictions(_schedulingOptions, effectiveRestriction, null)).IgnoreArguments().Return(
					true);
				Expect.Call(_schedulePeriod.Person).Return(_person).Repeat.AtLeastOnce();
				Expect.Call(_workShiftMinMaxCalculator.MinMaxAllowedShiftContractTime(dateOnly, _matrix, _schedulingOptions)).Return(
					new MinMax<TimeSpan>(new TimeSpan(0, 6, 0, 0), new TimeSpan(0, 12, 0, 0)));
				Expect.Call(_shiftProjectionCacheFilter.Filter(new MinMax<TimeSpan>(), caches, _scheduleDateOnly,
															   range, null)).IgnoreArguments().Return(caches);
				Expect.Call(_personSkillPeriodsDataHolderManager.GetPersonMaxSeatSkillSkillStaffPeriods(new DateOnly(), null)).Return(
					new Dictionary<ISkill, ISkillStaffPeriodDictionary>()).IgnoreArguments();
				Expect.Call(_personSkillPeriodsDataHolderManager.GetPersonNonBlendSkillSkillStaffPeriods(new DateOnly(), null)).Return(
					new Dictionary<ISkill, ISkillStaffPeriodDictionary>()).IgnoreArguments();
				Expect.Call(_personSkillPeriodsDataHolderManager.GetPersonSkillPeriodsDataHolderDictionary(dateOnly, _schedulePeriod)).Return(dataHolders);
				Expect.Call(_calculatorManager.RunCalculators(_person, caches, null,
					new Dictionary<ISkill, ISkillStaffPeriodDictionary>(), _schedulingOptions)).Return(
						results)
					.IgnoreArguments();
				Expect.Call(_schedulePeriod.AverageWorkTimePerDay).Return(TimeSpan.FromHours(7));
				Expect.Call(_fairnessAndMaxSeatCalculatorsManager.RecalculateFoundValues(results, 2, false, _person, dateOnly, new Dictionary<ISkill, ISkillStaffPeriodDictionary>(),
					TimeSpan.FromHours(7), _schedulingOptions)).Return(results);
                
				Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
				Expect.Call(_person.PermissionInformation).Return(_info).Repeat.AtLeastOnce();
				Expect.Call(_shiftLengthDecider.FilterList(caches, _workShiftMinMaxCalculator, _matrix, _schedulingOptions)).Return(caches);
			}
            _schedulingOptions.ShiftCategory = _category;
			using (_mocks.Playback())
			{
				IWorkShiftCalculationResultHolder retShift = _target.FindBestShift(_part, _schedulingOptions, _matrix, effectiveRestriction, null);
				Assert.IsNotNull(retShift);
			}
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldReturnNullWhenAllowedMinMaxEqualsNull()
        {
            var bag = _mocks.StrictMock<IRuleSetBag>();
            var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
            IList<IShiftProjectionCache> caches = getCashes();
            var dateOnly = new DateOnly(2009, 2, 2);
            _scheduleDateOnlyPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 2, 2), _timeZoneInfo);
            using (_mocks.Record())
            {
                Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
                Expect.Call(_part.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_part.DateOnlyAsPeriod).Return(_scheduleDateOnlyPeriod).Repeat.AtLeastOnce();
				Expect.Call(_preSchedulingStatusChecker.CheckStatus(null, null, _schedulingOptions)).Return(true).IgnoreArguments();
                Expect.Call(_person.VirtualSchedulePeriod(_scheduleDateOnly)).Return(_schedulePeriod).IgnoreArguments().Repeat.AtLeastOnce();
                Expect.Call(_person.Period(dateOnly)).Return(_personPeriod);
                Expect.Call(_personPeriod.RuleSetBag).Return(bag);
                Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(dateOnly, _timeZoneInfo, bag, false, true)).Return(caches);
				Expect.Call(_shiftProjectionCacheFilter.FilterOnMainShiftOptimizeActivitiesSpecification(caches, new Domain.Specification.All<IEditableShift>())).
					IgnoreArguments().Return(caches).Repeat.AtLeastOnce();
				Expect.Call(_shiftProjectionCacheFilter.FilterOnGroupSchedulingCommonStartEnd(caches, null, _schedulingOptions, null)).
					IgnoreArguments().Return(caches).Repeat.AtLeastOnce();
                Expect.Call(_shiftProjectionCacheFilter.FilterOnGroupSchedulingCommonActivity (caches,_schedulingOptions,null,null)).
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
				IWorkShiftCalculationResultHolder retShift = _target.FindBestShift(_part, _schedulingOptions, _matrix, effectiveRestriction, null);
                Assert.That(retShift, Is.Null);
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "BlackList"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldCallShiftProjectionCachesFromRuleSetBagWithBlackListIfUsingPreferencesAndFirstAttemptFailed()
        {
            var bag = _mocks.StrictMock<IRuleSetBag>();
            var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
            IList<IShiftProjectionCache> caches = getCashes();
            var dateOnly = new DateOnly(2009, 2, 2);
            _scheduleDateOnlyPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 2, 2), _timeZoneInfo);
            using (_mocks.Record())
            {
                Expect.Call(() => _workShiftMinMaxCalculator.ResetCache());
                Expect.Call(_part.Person).Return(_person).Repeat.AtLeastOnce();
                Expect.Call(_part.DateOnlyAsPeriod).Return(_scheduleDateOnlyPeriod).Repeat.AtLeastOnce();
				Expect.Call(_preSchedulingStatusChecker.CheckStatus(null, null, _schedulingOptions)).Return(true).IgnoreArguments();
                Expect.Call(_person.VirtualSchedulePeriod(_scheduleDateOnly)).Return(_schedulePeriod).IgnoreArguments().Repeat.AtLeastOnce();
                Expect.Call(_person.Period(dateOnly)).Return(_personPeriod);
                Expect.Call(_personPeriod.RuleSetBag).Return(bag);
				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(dateOnly, _timeZoneInfo, bag, false, true)).Return(caches);
				Expect.Call(_shiftProjectionCacheFilter.FilterOnMainShiftOptimizeActivitiesSpecification(new List<IShiftProjectionCache>(), new Domain.Specification.All<IEditableShift>())).
					IgnoreArguments().Return(caches).Repeat.AtLeastOnce();
				Expect.Call(_shiftProjectionCacheFilter.FilterOnGroupSchedulingCommonStartEnd(caches, null, _schedulingOptions, null)).
					IgnoreArguments().Return(caches).Repeat.AtLeastOnce();
                Expect.Call(_shiftProjectionCacheFilter.FilterOnGroupSchedulingCommonActivity(caches,_schedulingOptions,null,null)).
                    IgnoreArguments().Return(caches).Repeat.AtLeastOnce();
                effectiveRestriction.ShiftCategory = _category;
                Expect.Call(_shiftProjectionCacheFilter.FilterOnRestrictionAndNotAllowedShiftCategories(new DateOnly(), null, null, null, null, null)).
                    IgnoreArguments().Return(caches);
				Expect.Call(_workShiftMinMaxCalculator.MinMaxAllowedShiftContractTime(dateOnly, _matrix, _schedulingOptions)).Return(null);

				Expect.Call(_shiftProjectionCacheFilter.CheckRestrictions(null, null, null)).IgnoreArguments().Return(true);
				Expect.Call(_schedulePeriod.IsValid).Return(true).Repeat.AtLeastOnce();
				Expect.Call(_person.PermissionInformation).Return(_info).Repeat.AtLeastOnce();

				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(dateOnly, _timeZoneInfo, bag, true, true)).Return(caches);
				effectiveRestriction.ShiftCategory = _category;
				Expect.Call(_shiftProjectionCacheFilter.FilterOnRestrictionAndNotAllowedShiftCategories(new DateOnly(), null, null, null, null, null)).
					IgnoreArguments().Return(caches);
				Expect.Call(_workShiftMinMaxCalculator.MinMaxAllowedShiftContractTime(dateOnly, _matrix, _schedulingOptions)).Return(null);
            }
            _schedulingOptions.ShiftCategory = _category;
            using (_mocks.Playback())
            {
				IWorkShiftCalculationResultHolder retShift = _target.FindBestShift(_part, _schedulingOptions, _matrix, effectiveRestriction, null);
                Assert.That(retShift, Is.Null);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldReturnNullWhenNoShifts()
        {
            var bag = _mocks.StrictMock<IRuleSetBag>();
            var dateOnly = new DateOnly(2009, 2, 2);
            _scheduleDateOnlyPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 2, 2), _timeZoneInfo);
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
                Expect.Call(_person.PermissionInformation).Return(_info).Repeat.AtLeastOnce();
                Expect.Call(_person.Period(dateOnly)).Return(_personPeriod);
                Expect.Call(_personPeriod.RuleSetBag).Return(bag);
				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(dateOnly, _timeZoneInfo, bag, false, true)).Return(new List<IShiftProjectionCache>());
				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(dateOnly, _timeZoneInfo, bag, true, true)).Return(new List<IShiftProjectionCache>()); 
			}
            
            using (_mocks.Playback())
            {
				IWorkShiftCalculationResultHolder retShift = _target.FindBestShift(_part, _schedulingOptions, _matrix, effectiveRestriction, null);
                Assert.That(retShift, Is.Null);
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
			Expect.Call(_person.PermissionInformation).Return(_info).Repeat.AtLeastOnce();
			Expect.Call(_person.Period(dateOnly)).Return(_personPeriod);
			Expect.Call(_personPeriod.RuleSetBag).Return(bag);
			Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(dateOnly, _timeZoneInfo, bag, false, true)).Return(new List<IShiftProjectionCache>());
		}

		[Test]
		public void ShouldNotUseBlacklistIfNoRestriction()
		{
			var bag = _mocks.StrictMock<IRuleSetBag>();
			var dateOnly = new DateOnly(2009, 2, 2);
			_scheduleDateOnlyPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 2, 2), _timeZoneInfo);
			var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
			_schedulingOptions.UseAvailability = false;
			_schedulingOptions.UsePreferences = false;
			_schedulingOptions.UseRotations = false;
			_schedulingOptions.UseStudentAvailability = false;

			using (_mocks.Record())
			{
				commonMocksForBlackListTests(bag, dateOnly);
				//Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(dateOnly, _timeZoneInfo, bag, true)).Return(new List<IShiftProjectionCache>());
			}

			using (_mocks.Playback())
			{
				IWorkShiftCalculationResultHolder retShift = _target.FindBestShift(_part, _schedulingOptions, _matrix, effectiveRestriction, _possibleStartEndCategory);
				Assert.That(retShift, Is.Null);
			}
		}

		[Test]
		public void ShouldUseBlacklistIfUseStudentAvailability()
		{
			var bag = _mocks.StrictMock<IRuleSetBag>();
			var dateOnly = new DateOnly(2009, 2, 2);
			_scheduleDateOnlyPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 2, 2), _timeZoneInfo);
			var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
			_schedulingOptions.UseAvailability = false;
			_schedulingOptions.UsePreferences = false;
			_schedulingOptions.UseRotations = false;
			_schedulingOptions.UseStudentAvailability = true;

			using (_mocks.Record())
			{
				commonMocksForBlackListTests(bag, dateOnly);
				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(dateOnly, _timeZoneInfo, bag, true, true)).Return(new List<IShiftProjectionCache>());
			}

			using (_mocks.Playback())
			{
				IWorkShiftCalculationResultHolder retShift = _target.FindBestShift(_part, _schedulingOptions, _matrix, effectiveRestriction, _possibleStartEndCategory);
				Assert.That(retShift, Is.Null);
			}
		}

		[Test]
		public void ShouldUseBlacklistIfUseRotations()
		{
			var bag = _mocks.StrictMock<IRuleSetBag>();
			var dateOnly = new DateOnly(2009, 2, 2);
			_scheduleDateOnlyPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 2, 2), _timeZoneInfo);
			var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
			_schedulingOptions.UseAvailability = false;
			_schedulingOptions.UsePreferences = false;
			_schedulingOptions.UseRotations = true;
			_schedulingOptions.UseStudentAvailability = false;

			using (_mocks.Record())
			{
				commonMocksForBlackListTests(bag, dateOnly);
				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(dateOnly, _timeZoneInfo, bag, true, true)).Return(new List<IShiftProjectionCache>());
			}

			using (_mocks.Playback())
			{
				IWorkShiftCalculationResultHolder retShift = _target.FindBestShift(_part, _schedulingOptions, _matrix, effectiveRestriction, _possibleStartEndCategory);
				Assert.That(retShift, Is.Null);
			}
		}

		[Test]
		public void ShouldUseBlacklistIfUsePreferences()
		{
			var bag = _mocks.StrictMock<IRuleSetBag>();
			var dateOnly = new DateOnly(2009, 2, 2);
			_scheduleDateOnlyPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 2, 2), _timeZoneInfo);
			var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
			_schedulingOptions.UseAvailability = false;
			_schedulingOptions.UsePreferences = true;
			_schedulingOptions.UseRotations = false;
			_schedulingOptions.UseStudentAvailability = false;

			using (_mocks.Record())
			{
				commonMocksForBlackListTests(bag, dateOnly);
				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(dateOnly, _timeZoneInfo, bag, true, true)).Return(new List<IShiftProjectionCache>());
			}

			using (_mocks.Playback())
			{
				IWorkShiftCalculationResultHolder retShift = _target.FindBestShift(_part, _schedulingOptions, _matrix, effectiveRestriction, _possibleStartEndCategory);
				Assert.That(retShift, Is.Null);
			}
		}

		[Test]
		public void ShouldUseBlacklistIfUseAvailability()
		{
			var bag = _mocks.StrictMock<IRuleSetBag>();
			var dateOnly = new DateOnly(2009, 2, 2);
			_scheduleDateOnlyPeriod = new DateOnlyAsDateTimePeriod(new DateOnly(2009, 2, 2), _timeZoneInfo);
			var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
			_schedulingOptions.UseAvailability = true;
			_schedulingOptions.UsePreferences = false;
			_schedulingOptions.UseRotations = false;
			_schedulingOptions.UseStudentAvailability = false;

			using (_mocks.Record())
			{
				commonMocksForBlackListTests(bag, dateOnly);
				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(dateOnly, _timeZoneInfo, bag, true, true)).Return(new List<IShiftProjectionCache>());
			}

			using (_mocks.Playback())
			{
				IWorkShiftCalculationResultHolder retShift = _target.FindBestShift(_part, _schedulingOptions, _matrix, effectiveRestriction, _possibleStartEndCategory);
				Assert.That(retShift, Is.Null);
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
			Expect.Call(_person.PermissionInformation).Return(_info);
            Expect.Call(_person.Period(dateOnly)).Return(_personPeriod);
			Expect.Call(_personPeriod.RuleSetBag).Return(null);
			Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(dateOnly, _timeZoneInfo, null, false, true)).Return(new List<IShiftProjectionCache>()).IgnoreArguments();
			Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSetBag(dateOnly, _timeZoneInfo, null, false, true)).Return(new List<IShiftProjectionCache>()).IgnoreArguments();
                    
			_mocks.ReplayAll();
            _schedulingOptions.ShiftCategory = _category;

            IWorkShiftCalculationResultHolder retShift = _target.FindBestShift(_part, _schedulingOptions, _matrix, effectiveRestriction, null);
			Assert.That(retShift, Is.Null);

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
			IWorkShiftCalculationResultHolder retShift = _target.FindBestShift(_part, _schedulingOptions, _matrix, effectiveRestriction, null);
			Assert.That(retShift, Is.Null);

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
			IWorkShiftCalculationResultHolder retShift = _target.FindBestShift(_part, _schedulingOptions, _matrix, effectiveRestriction, null);
			Assert.That(retShift, Is.Null);

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
			var retShift = _target.FindBestShift(_part, _schedulingOptions, _matrix, effectiveRestriction, null);
			Assert.That(retShift, Is.Null);

			_mocks.VerifyAll();
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void VerifyFindBestMainShift()
		{
			IList<IShiftProjectionCache> shiftList = new List<IShiftProjectionCache> ();
			var dataHolders = MockRepository.GenerateMock<IWorkShiftCalculatorSkillStaffPeriodData>();
			var virtualShedulePeriod = _mocks.DynamicMock<IVirtualSchedulePeriod>();
		    var nonBlendSkillPeriods = _mocks.StrictMock<IDictionary<ISkill, ISkillStaffPeriodDictionary>>();
		    var wfcs = _mocks.StrictMock<IWorkflowControlSet>();
            var results = new List<IWorkShiftCalculationResultHolder>
                              {
                                  new WorkShiftCalculationResult { Value = 1 } 
                              };

            Expect.Call(virtualShedulePeriod.Person).Return(_person).Repeat.AtLeastOnce();
            Expect.Call(_person.WorkflowControlSet).Return(wfcs).Repeat.AtLeastOnce();
		    Expect.Call(wfcs.UseShiftCategoryFairness).Return(true);
            Expect.Call(_calculatorManager.RunCalculators(_person, shiftList, dataHolders,
                                                            nonBlendSkillPeriods, _schedulingOptions)).Return(
                                                                results);
            Expect.Call(virtualShedulePeriod.AverageWorkTimePerDay).Return(TimeSpan.FromHours(7));
            Expect.Call(_fairnessAndMaxSeatCalculatorsManager.RecalculateFoundValues(results, 1, true, _person, _scheduleDateOnly, new Dictionary<ISkill, ISkillStaffPeriodDictionary>(),
                TimeSpan.FromHours(7), _schedulingOptions)).Return(results);
            _mocks.ReplayAll();
            IWorkShiftCalculationResultHolder retShift =
                _target.FindBestMainShift(_scheduleDateOnly, shiftList, dataHolders, new Dictionary<ISkill, ISkillStaffPeriodDictionary>(), nonBlendSkillPeriods, virtualShedulePeriod, _schedulingOptions);
			Assert.IsNotNull(retShift);
			_mocks.VerifyAll();
		}

		private IList<IShiftProjectionCache> getCashes()
		{
			var dateOnly = new DateOnly(2009, 2, 2);
			var tmpList = getWorkShifts();
			var retList = new List<IShiftProjectionCache>();
			foreach (IWorkShift shift in tmpList)
			{
				var cache = new ShiftProjectionCache(shift, _personalShiftMeetingTimeChecker);
				cache.SetDate(dateOnly, _timeZoneInfo);
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
			var skillstaffPeriods = MockRepository.GenerateMock<IWorkShiftCalculatorSkillStaffPeriodData>();
            var caches = new List<IShiftProjectionCache>();
            var virtualShedulePeriod = _mocks.DynamicMock<IVirtualSchedulePeriod>();
           
            Expect.Call(virtualShedulePeriod.Person).Return(_person);
            Expect.Call(_calculatorManager.RunCalculators(_person, caches, skillstaffPeriods,
															  new Dictionary<ISkill, ISkillStaffPeriodDictionary>(), _schedulingOptions)).Return(
                                                                  new List<IWorkShiftCalculationResultHolder>());
            _mocks.ReplayAll();
            _target.FindBestMainShift(_scheduleDateOnly, caches, skillstaffPeriods, new Dictionary<ISkill, ISkillStaffPeriodDictionary>(),
                new Dictionary<ISkill, ISkillStaffPeriodDictionary>(), virtualShedulePeriod, _schedulingOptions);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCalculateMaxSeatValueWhenShiftValueIsHigherThanDoubleMin()
        {
			var skillstaffPeriods = MockRepository.GenerateMock<IWorkShiftCalculatorSkillStaffPeriodData>();
            var caches = new List<IShiftProjectionCache> ();
            var virtualShedulePeriod = _mocks.DynamicMock<IVirtualSchedulePeriod>();
            var results = new List<IWorkShiftCalculationResultHolder>
                              {
                                  new WorkShiftCalculationResult { Value = -1, ShiftProjection = null } 
                              };
            _schedulingOptions.OnlyShiftsWhenUnderstaffed = true;
            Expect.Call(_person.WorkflowControlSet).Return(null).Repeat.AtLeastOnce();
            Expect.Call(_calculatorManager.RunCalculators(_person, caches, skillstaffPeriods,
															  new Dictionary<ISkill, ISkillStaffPeriodDictionary>(), _schedulingOptions)).Return(
                                                                  results);
            Expect.Call(virtualShedulePeriod.AverageWorkTimePerDay).Return(TimeSpan.FromHours(7)).Repeat.AtLeastOnce();
            Expect.Call(_fairnessAndMaxSeatCalculatorsManager.RecalculateFoundValues(results, -1, false, _person, _scheduleDateOnly, new Dictionary<ISkill, ISkillStaffPeriodDictionary>(),
                TimeSpan.FromHours(7), _schedulingOptions)).Return(results).Repeat.AtLeastOnce();

            Expect.Call(virtualShedulePeriod.Person).Return(_person);
            
            _mocks.ReplayAll();

            _target.FindBestMainShift(_scheduleDateOnly, caches, skillstaffPeriods, new Dictionary<ISkill, ISkillStaffPeriodDictionary>(),
                new Dictionary<ISkill, ISkillStaffPeriodDictionary>(), virtualShedulePeriod, _schedulingOptions);

            _mocks.VerifyAll();
        }
    }

	internal class WorkShiftFinderResultForTest : IWorkShiftFinderResult
	{
		private readonly IList<IWorkShiftFilterResult> _filterResults = new List<IWorkShiftFilterResult>();

		public IPerson Person
		{
			get { throw new NotImplementedException(); }
		}

		public string PersonDateKey
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
