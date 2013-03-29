using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
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
		private IScheduleDayEquator _scheduleDayEquator;
		private ISafeRollbackAndResourceCalculation _safeRollbackAndResourceCalculation;
		private OptimizationPreferences _optimizerPreferences;
		private SchedulingOptions _schedulingOptions;
		private ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private IScheduleMatrixOriginalStateContainerFactory _scheduleMatrixOriginalStateContainerFactory;
		private IOptimizationOverLimitByRestrictionDeciderFactory _optimizationOverLimitByRestrictionDeciderFactory;
		private IRestrictionOverLimitDecider _restrictionOverLimitDecider;
		private ITeamBlockInfo _teamBlockInfo;
		private IMaxMovedDaysOverLimitValidator _maxMovedDaysOverLimitValidator;
		private IScheduleMatrixPro _scheduleMatrixPro1;
		private IVirtualSchedulePeriod _schedulePeriod;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_maxMovedDaysOverLimitValidator = _mocks.StrictMock<IMaxMovedDaysOverLimitValidator>();
			_scheduleDayEquator = _mocks.StrictMock<IScheduleDayEquator>();
			_safeRollbackAndResourceCalculation = _mocks.StrictMock<ISafeRollbackAndResourceCalculation>();
			_optimizerPreferences = new OptimizationPreferences();
			_schedulingOptions = new SchedulingOptions();
			_schedulePartModifyAndRollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_scheduleMatrixOriginalStateContainerFactory = _mocks.StrictMock<IScheduleMatrixOriginalStateContainerFactory>();
			_optimizationOverLimitByRestrictionDeciderFactory = _mocks.StrictMock<IOptimizationOverLimitByRestrictionDeciderFactory>();
			_restrictionOverLimitDecider = _mocks.StrictMock<IRestrictionOverLimitDecider>();
			_target = new TeamBlockRestrictionOverLimitValidator(_restrictionOverLimitDecider, _maxMovedDaysOverLimitValidator);
			IPerson groupMember = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill>());
			IGroupPerson groupPerson = new GroupPerson(new List<IPerson> { groupMember }, DateOnly.MinValue, "hej", null);
			IList<IList<IScheduleMatrixPro>> matrixes = new List<IList<IScheduleMatrixPro>>();
			_scheduleMatrixPro1 = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro1 };
			matrixes.Add(matrixList);
			_teamBlockInfo = new TeamBlockInfo(new TeamInfo(groupPerson, matrixes),
			                                   new BlockInfo(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue)));
			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
		}

		[Test]
		public void ShouldReturnFalseIfOverMoveMaxDayLimit()
		{
			using (_mocks.Record())
			{
				Expect.Call(_scheduleMatrixPro1.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue));
				Expect.Call(_maxMovedDaysOverLimitValidator.ValidateMatrix(_scheduleMatrixPro1, _optimizerPreferences))
				      .Return(false);
			}
			using (_mocks.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _optimizerPreferences);
				Assert.IsFalse(result);
			}
		}

		//[Test]
		//public void ShouldValidateIfTeamBlockNotExceedsMoveMaxDayLimit()
		//{
		//	var scheduleMatrixPro1 = _mocks.StrictMock<IScheduleMatrixPro>();
		//	var matrixList = new List<IScheduleMatrixPro> { scheduleMatrixPro1 };
		//	var originalStateContainer = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();
		//	var optimizationOverLimitByRestrictionDecider = _mocks.StrictMock<IOptimizationOverLimitByRestrictionDecider>();
		//	var restrictionChecker = new RestrictionChecker();

		//	using (_mocks.Record())
		//	{
				
		//	}
		//	using (_mocks.Playback())
		//	{
		//		var result = _target.Validate(_teamBlockInfo, _optimizerPreferences);
		//		Assert.IsTrue(result);
		//	}
		//}

		//[Test]
		//public void ShouldRollbackIfTeamBlockOverLimit()
		//{
		//	var dateOnly = new DateOnly();
		//	var scheduleMatrixPro1 = _mocks.StrictMock<IScheduleMatrixPro>();
		//	var schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
		//	var matrixList = new List<IScheduleMatrixPro> { scheduleMatrixPro1 };

		//	var originalStateContainer = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();
		//	var optimizationOverLimitByRestrictionDecider = _mocks.StrictMock<IOptimizationOverLimitByRestrictionDecider>();
		//	var restrictionChecker = new RestrictionChecker();

		//	using (_mocks.Record())
		//	{
				
		//	}
		//	using (_mocks.Playback())
		//	{
		//		var result = _target.Validate(_teamBlockInfo, _optimizerPreferences);
		//		Assert.IsFalse(result);
		//	}
		//}
		
		//[Test]
		//public void ShouldValidateIfTeamBlockNoOverLimit()
		//{
		//	var scheduleMatrixPro1 = _mocks.StrictMock<IScheduleMatrixPro>();
		//	var matrixList = new List<IScheduleMatrixPro> { scheduleMatrixPro1 };

		//	var originalStateContainer = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();
		//	var optimizationOverLimitByRestrictionDecider = _mocks.StrictMock<IOptimizationOverLimitByRestrictionDecider>();
		//	var restrictionChecker = new RestrictionChecker();

		//	using (_mocks.Record())
		//	{
				
		//	}
		//	using (_mocks.Playback())
		//	{
		//		var result = _target.Validate(_teamBlockInfo, _optimizerPreferences);
		//		Assert.True(result);
		//	}
		//}
	}
}
