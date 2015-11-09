using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock
{
	[TestFixture]
	public class TeamBlockRestrictionOverLimitValidatorTest
	{
		private ITeamBlockRestrictionOverLimitValidator _target;
		private MockRepository _mocks;
		private OptimizationPreferences _optimizerPreferences;
		private IRestrictionOverLimitDecider _restrictionOverLimitDecider;
		private ITeamBlockInfo _teamBlockInfo;
		private IMaxMovedDaysOverLimitValidator _maxMovedDaysOverLimitValidator;
		private IScheduleMatrixPro _scheduleMatrixPro1;
		private IVirtualSchedulePeriod _schedulePeriod;
		private TeamInfo _teamInfo;
		private IDaysOffPreferences _daysOffPreferences;
		private IDayOffOptimizationPreferenceProvider _dayOffOptimizationPreferenceProvider;
		private IPerson _groupMember;
		private IScheduleDayPro _scheduleDayPro;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_maxMovedDaysOverLimitValidator = _mocks.StrictMock<IMaxMovedDaysOverLimitValidator>();
			_restrictionOverLimitDecider = _mocks.StrictMock<IRestrictionOverLimitDecider>();
			_optimizerPreferences = new OptimizationPreferences();
			_target = new TeamBlockRestrictionOverLimitValidator(_restrictionOverLimitDecider, _maxMovedDaysOverLimitValidator);
			_groupMember = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill>());
			Group group = new Group(new List<IPerson> { _groupMember }, "hej");
			IList<IList<IScheduleMatrixPro>> matrixes = new List<IList<IScheduleMatrixPro>>();
			_scheduleMatrixPro1 = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro1 };
			matrixes.Add(matrixList);
			_teamInfo = new TeamInfo(group, matrixes);
			_teamBlockInfo = new TeamBlockInfo(_teamInfo,
			                                   new BlockInfo(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue)));
			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_daysOffPreferences = new DaysOffPreferences();
			_dayOffOptimizationPreferenceProvider = new FixedDayOffOptimizationPreferenceProvider(_daysOffPreferences);
			_scheduleDayPro = new ScheduleDayPro(new DateOnly(2015, 1, 1),  _scheduleMatrixPro1);
		}

		[Test]
		public void ShouldReturnFalseIfOverMoveMaxDayLimit()
		{
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro1.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue));
				Expect.Call(_maxMovedDaysOverLimitValidator.ValidateMatrix(_scheduleMatrixPro1, _optimizerPreferences, _daysOffPreferences))
				      .Return(false);

				Expect.Call(_scheduleMatrixPro1.Person).Return(_groupMember);
				Expect.Call(_scheduleMatrixPro1.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
			}
			using (_mocks.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _optimizerPreferences, _dayOffOptimizationPreferenceProvider);
				Assert.IsFalse(result);
			}
		}
	
		[Test]
		public void ShouldReturnFalseIfOverMoveMaxDayLimitWithTeamInfo()
		{
			using (_mocks.Record())
			{
				Expect.Call(_maxMovedDaysOverLimitValidator.ValidateMatrix(_scheduleMatrixPro1, _optimizerPreferences, _daysOffPreferences))
				      .Return(false);

				Expect.Call(_scheduleMatrixPro1.Person).Return(_groupMember);
				Expect.Call(_scheduleMatrixPro1.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
			}
			using (_mocks.Playback())
			{
				var result = _target.Validate(_teamInfo, _optimizerPreferences, _dayOffOptimizationPreferenceProvider);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldValidateIfTeamBlockNotExceedsMoveMaxDayLimit()
		{
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro1.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue));
				Expect.Call(_maxMovedDaysOverLimitValidator.ValidateMatrix(_scheduleMatrixPro1, _optimizerPreferences, _daysOffPreferences)).Return(true);

				Expect.Call(_scheduleMatrixPro1.Person).Return(_groupMember);
				Expect.Call(_scheduleMatrixPro1.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
			}
			using (_mocks.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _optimizerPreferences, _dayOffOptimizationPreferenceProvider);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldSkipPreferenceLimit()
		{
			_optimizerPreferences.General.UsePreferences = false;
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro1.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue));
				Expect.Call(_maxMovedDaysOverLimitValidator.ValidateMatrix(_scheduleMatrixPro1, _optimizerPreferences, _daysOffPreferences)).Return(true);

				Expect.Call(_scheduleMatrixPro1.Person).Return(_groupMember);
				Expect.Call(_scheduleMatrixPro1.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
			}
			using (_mocks.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _optimizerPreferences, _dayOffOptimizationPreferenceProvider);
				Assert.IsTrue(result);
			}
		}


		[Test]
		public void ShouldValidatePreferenceLimit()
		{
			_optimizerPreferences.General.UsePreferences = true;
			_optimizerPreferences.General.PreferencesValue = 0.8;
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro1.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue));
				Expect.Call(_maxMovedDaysOverLimitValidator.ValidateMatrix(_scheduleMatrixPro1, _optimizerPreferences, _daysOffPreferences)).Return(true);
				Expect.Call(_restrictionOverLimitDecider.PreferencesOverLimit(new Percent(0.8), _scheduleMatrixPro1)).Return(new BrokenRestrictionsInfo(new List<DateOnly>(), new Percent(0.8)));

				Expect.Call(_scheduleMatrixPro1.Person).Return(_groupMember);
				Expect.Call(_scheduleMatrixPro1.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
			}
			using (_mocks.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _optimizerPreferences, _dayOffOptimizationPreferenceProvider);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldReturnFalseIfPreferenceLimitViolated()
		{
			_optimizerPreferences.General.UsePreferences = true;
			_optimizerPreferences.General.PreferencesValue = 0.8;
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro1.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue));
				Expect.Call(_maxMovedDaysOverLimitValidator.ValidateMatrix(_scheduleMatrixPro1, _optimizerPreferences, _daysOffPreferences)).Return(true);
				Expect.Call(_restrictionOverLimitDecider.PreferencesOverLimit(new Percent(0.8), _scheduleMatrixPro1))
				      .Return(new BrokenRestrictionsInfo(new List<DateOnly> {new DateOnly()}, new Percent(0.8)));

				Expect.Call(_scheduleMatrixPro1.Person).Return(_groupMember);
				Expect.Call(_scheduleMatrixPro1.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
			}
			using (_mocks.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _optimizerPreferences, _dayOffOptimizationPreferenceProvider);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldSkipMustHavesOverLimit()
		{
			_optimizerPreferences.General.UseMustHaves = false;
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro1.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue));
				Expect.Call(_maxMovedDaysOverLimitValidator.ValidateMatrix(_scheduleMatrixPro1, _optimizerPreferences, _daysOffPreferences)).Return(true);

				Expect.Call(_scheduleMatrixPro1.Person).Return(_groupMember);
				Expect.Call(_scheduleMatrixPro1.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
			}
			using (_mocks.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _optimizerPreferences, _dayOffOptimizationPreferenceProvider);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldValidateMustHavesOverLimit()
		{
			_optimizerPreferences.General.UseMustHaves = true;
			_optimizerPreferences.General.MustHavesValue = 0.8;
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro1.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue));
				Expect.Call(_maxMovedDaysOverLimitValidator.ValidateMatrix(_scheduleMatrixPro1, _optimizerPreferences, _daysOffPreferences)).Return(true);
				Expect.Call(_restrictionOverLimitDecider.MustHavesOverLimit(new Percent(0.8), _scheduleMatrixPro1)).Return(new BrokenRestrictionsInfo(new List<DateOnly>(), new Percent(0.8)));

				Expect.Call(_scheduleMatrixPro1.Person).Return(_groupMember);
				Expect.Call(_scheduleMatrixPro1.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
			}
			using (_mocks.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _optimizerPreferences, _dayOffOptimizationPreferenceProvider);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldReturnFalseIfMustHavesOverLimitViolated()
		{
			_optimizerPreferences.General.UseMustHaves = true;
			_optimizerPreferences.General.MustHavesValue = 0.8;
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro1.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue));
				Expect.Call(_maxMovedDaysOverLimitValidator.ValidateMatrix(_scheduleMatrixPro1, _optimizerPreferences, _daysOffPreferences)).Return(true);
				Expect.Call(_restrictionOverLimitDecider.MustHavesOverLimit(new Percent(0.8), _scheduleMatrixPro1))
				      .Return(new BrokenRestrictionsInfo(new List<DateOnly> {new DateOnly()}, new Percent(0.8)));

				Expect.Call(_scheduleMatrixPro1.Person).Return(_groupMember);
				Expect.Call(_scheduleMatrixPro1.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
			}
			using (_mocks.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _optimizerPreferences, _dayOffOptimizationPreferenceProvider);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldSkipRotationOverLimit()
		{
			_optimizerPreferences.General.UseRotations = false;
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro1.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue));
				Expect.Call(_maxMovedDaysOverLimitValidator.ValidateMatrix(_scheduleMatrixPro1, _optimizerPreferences, _daysOffPreferences)).Return(true);

				Expect.Call(_scheduleMatrixPro1.Person).Return(_groupMember);
				Expect.Call(_scheduleMatrixPro1.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
			}
			using (_mocks.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _optimizerPreferences, _dayOffOptimizationPreferenceProvider);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldValidateRotationOverLimit()
		{
			_optimizerPreferences.General.UseRotations = true;
			_optimizerPreferences.General.RotationsValue = 0.8;
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro1.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue));
				Expect.Call(_maxMovedDaysOverLimitValidator.ValidateMatrix(_scheduleMatrixPro1, _optimizerPreferences, _daysOffPreferences)).Return(true);
				Expect.Call(_restrictionOverLimitDecider.RotationOverLimit(new Percent(0.8), _scheduleMatrixPro1)).Return(new BrokenRestrictionsInfo(new List<DateOnly>(), new Percent(0.8)));

				Expect.Call(_scheduleMatrixPro1.Person).Return(_groupMember);
				Expect.Call(_scheduleMatrixPro1.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
			}
			using (_mocks.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _optimizerPreferences, _dayOffOptimizationPreferenceProvider);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldReturnFalseIfRotationOverLimitViolated()
		{
			_optimizerPreferences.General.UseRotations = true;
			_optimizerPreferences.General.RotationsValue = 0.8;
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro1.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue));
				Expect.Call(_maxMovedDaysOverLimitValidator.ValidateMatrix(_scheduleMatrixPro1, _optimizerPreferences, _daysOffPreferences)).Return(true);
				Expect.Call(_restrictionOverLimitDecider.RotationOverLimit(new Percent(0.8), _scheduleMatrixPro1))
				      .Return(new BrokenRestrictionsInfo(new List<DateOnly> {new DateOnly()}, new Percent(0.8)));

				Expect.Call(_scheduleMatrixPro1.Person).Return(_groupMember);
				Expect.Call(_scheduleMatrixPro1.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
			}
			using (_mocks.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _optimizerPreferences, _dayOffOptimizationPreferenceProvider);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldSkipAvailabilitiesOverLimit()
		{
			_optimizerPreferences.General.UseAvailabilities = false;
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro1.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue));
				Expect.Call(_maxMovedDaysOverLimitValidator.ValidateMatrix(_scheduleMatrixPro1, _optimizerPreferences, _daysOffPreferences)).Return(true);

				Expect.Call(_scheduleMatrixPro1.Person).Return(_groupMember);
				Expect.Call(_scheduleMatrixPro1.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
			}
			using (_mocks.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _optimizerPreferences, _dayOffOptimizationPreferenceProvider);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldValidateAvailabilitiesOverLimit()
		{
			_optimizerPreferences.General.UseAvailabilities = true;
			_optimizerPreferences.General.AvailabilitiesValue = 0.8;
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro1.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue));
				Expect.Call(_maxMovedDaysOverLimitValidator.ValidateMatrix(_scheduleMatrixPro1, _optimizerPreferences, _daysOffPreferences)).Return(true);
				Expect.Call(_restrictionOverLimitDecider.AvailabilitiesOverLimit(new Percent(0.8), _scheduleMatrixPro1)).Return(new BrokenRestrictionsInfo(new List<DateOnly>(), new Percent(0.8)));

				Expect.Call(_scheduleMatrixPro1.Person).Return(_groupMember);
				Expect.Call(_scheduleMatrixPro1.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
			}
			using (_mocks.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _optimizerPreferences, _dayOffOptimizationPreferenceProvider);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldReturnFalseIfAvailabilitiesOverLimitViolated()
		{
			_optimizerPreferences.General.UseAvailabilities= true;
			_optimizerPreferences.General.AvailabilitiesValue = 0.8;
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro1.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue));
				Expect.Call(_maxMovedDaysOverLimitValidator.ValidateMatrix(_scheduleMatrixPro1, _optimizerPreferences, _daysOffPreferences)).Return(true);
				Expect.Call(_restrictionOverLimitDecider.AvailabilitiesOverLimit(new Percent(0.8), _scheduleMatrixPro1))
				      .Return(new BrokenRestrictionsInfo(new List<DateOnly> {new DateOnly()}, new Percent(0.8)));

				Expect.Call(_scheduleMatrixPro1.Person).Return(_groupMember);
				Expect.Call(_scheduleMatrixPro1.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
			}
			using (_mocks.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _optimizerPreferences, _dayOffOptimizationPreferenceProvider);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldSkipStudentAvailabilitiesOverLimit()
		{
			_optimizerPreferences.General.UseStudentAvailabilities = false;
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro1.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue));
				Expect.Call(_maxMovedDaysOverLimitValidator.ValidateMatrix(_scheduleMatrixPro1, _optimizerPreferences, _daysOffPreferences)).Return(true);

				Expect.Call(_scheduleMatrixPro1.Person).Return(_groupMember);
				Expect.Call(_scheduleMatrixPro1.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
			}
			using (_mocks.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _optimizerPreferences, _dayOffOptimizationPreferenceProvider);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldValidateStudentAvailabilitiesOverLimit()
		{
			_optimizerPreferences.General.UseStudentAvailabilities = true;
			_optimizerPreferences.General.StudentAvailabilitiesValue = 0.8;
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro1.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue));
				Expect.Call(_maxMovedDaysOverLimitValidator.ValidateMatrix(_scheduleMatrixPro1, _optimizerPreferences, _daysOffPreferences)).Return(true);
				Expect.Call(_restrictionOverLimitDecider.StudentAvailabilitiesOverLimit(new Percent(0.8), _scheduleMatrixPro1)).Return(new BrokenRestrictionsInfo(new List<DateOnly>(), new Percent(0.8)));

				Expect.Call(_scheduleMatrixPro1.Person).Return(_groupMember);
				Expect.Call(_scheduleMatrixPro1.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> {_scheduleDayPro}));
			}
			using (_mocks.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _optimizerPreferences, _dayOffOptimizationPreferenceProvider);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldReturnFalseIfStudentAvailabilitiesOverLimitViolated()
		{
			_optimizerPreferences.General.UseStudentAvailabilities= true;
			_optimizerPreferences.General.StudentAvailabilitiesValue = 0.8;
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro1.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue));
				Expect.Call(_maxMovedDaysOverLimitValidator.ValidateMatrix(_scheduleMatrixPro1, _optimizerPreferences, _daysOffPreferences)).Return(true);
				Expect.Call(_restrictionOverLimitDecider.StudentAvailabilitiesOverLimit(new Percent(0.8), _scheduleMatrixPro1))
				      .Return(new BrokenRestrictionsInfo(new List<DateOnly> {new DateOnly()}, new Percent(0.8)));

				Expect.Call(_scheduleMatrixPro1.Person).Return(_groupMember);
				Expect.Call(_scheduleMatrixPro1.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro }));
			}
			using (_mocks.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _optimizerPreferences, _dayOffOptimizationPreferenceProvider);
				Assert.IsFalse(result);
			}
		}
	}
}
