﻿using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
	[TestFixture]
	public class SeniorityTeamBlockSwapValidatorTest
	{
		private MockRepository _mocks;
		private ISeniorityTeamBlockSwapValidator _target;
		private IDayOffRulesValidator _dayOffRulesValidator;
		private ITeamBlockInfo _teamBlockInfo;
		private IOptimizationPreferences _optimizationPreferences;
		private IScheduleMatrixPro _matrix;
		private ITeamBlockSteadyStateValidator _teamBlockSteadyStateValidator;
		private IConstructTeamBlock _constructTeamBlock;
		private ISchedulingOptionsCreator _schedulingOptionsCreator;
		private IPerson _person;
		private ITeamInfo _teamInfo;
		private ISchedulingOptions _schedulingOptions;
		private IVirtualSchedulePeriod _schedulePeriod;
		private DateOnlyPeriod _period;
		private IScheduleMatrixLockableBitArrayConverterEx _matrixCoverter;
		private LockableBitArray _bitArray;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_dayOffRulesValidator = _mocks.StrictMock<IDayOffRulesValidator>();
			_teamBlockSteadyStateValidator = _mocks.StrictMock<ITeamBlockSteadyStateValidator>();
			_constructTeamBlock = _mocks.StrictMock<IConstructTeamBlock>();
			_schedulingOptionsCreator = _mocks.StrictMock<ISchedulingOptionsCreator>();
			_matrixCoverter = _mocks.StrictMock<IScheduleMatrixLockableBitArrayConverterEx>();
			_target = new SeniorityTeamBlockSwapValidator(_dayOffRulesValidator, _teamBlockSteadyStateValidator,
			                                              _constructTeamBlock, _schedulingOptionsCreator, _matrixCoverter);
			_teamBlockInfo = _mocks.StrictMock<ITeamBlockInfo>();
			_optimizationPreferences = new OptimizationPreferences();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_person = PersonFactory.CreatePerson();
			_teamInfo = _mocks.StrictMock<ITeamInfo>();
			_schedulingOptions = new SchedulingOptions();
			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_period = new DateOnlyPeriod(2014,1,31,2014,2,1);
			_bitArray = new LockableBitArray(21, true, true, null);
		}

		[Test]
		public void ShouldReturnFalseIfAnyDaysOffValidatorFail()
		{
			using (_mocks.Record())
			{
				commonMocks();
				Expect.Call(_dayOffRulesValidator.Validate(_bitArray, _optimizationPreferences)).Return(false);
			}

			using (_mocks.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _optimizationPreferences);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldReturnFalseIfTeamBlocksNotInSteadyState()
		{
			using (_mocks.Record())
			{
				commonMocks();

				Expect.Call(_dayOffRulesValidator.Validate(_bitArray, _optimizationPreferences)).Return(true);
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(_period);
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new[] {_person});
				Expect.Call(_constructTeamBlock.Construct(new List<IScheduleMatrixPro> { _matrix }, _period,
				                                          new List<IPerson> {_person},
				                                          _optimizationPreferences.Extra.BlockTypeValue ,
				                                          _optimizationPreferences.Extra.GroupPageOnTeamBlockPer))
				      .IgnoreArguments().Return(new List<ITeamBlockInfo> {_teamBlockInfo});
				Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizationPreferences)).Return(_schedulingOptions);
				Expect.Call(_teamBlockSteadyStateValidator.IsTeamBlockInSteadyState(_teamBlockInfo, _schedulingOptions))
				      .Return(false);
			}

			using (_mocks.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _optimizationPreferences);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldReturnTrueIfAllValid()
		{
			using (_mocks.Record())
			{
				commonMocks();

				Expect.Call(_dayOffRulesValidator.Validate(_bitArray, _optimizationPreferences)).Return(true);
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(_period);
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new[] { _person });
				Expect.Call(_constructTeamBlock.Construct(new List<IScheduleMatrixPro> { _matrix }, _period,
														  new List<IPerson> { _person },
														  _optimizationPreferences.Extra.BlockTypeValue ,
														  _optimizationPreferences.Extra.GroupPageOnTeamBlockPer))
					  .IgnoreArguments().Return(new List<ITeamBlockInfo> { _teamBlockInfo });
				Expect.Call(_schedulingOptionsCreator.CreateSchedulingOptions(_optimizationPreferences)).Return(_schedulingOptions);
				Expect.Call(_teamBlockSteadyStateValidator.IsTeamBlockInSteadyState(_teamBlockInfo, _schedulingOptions))
					  .Return(true);
			}

			using (_mocks.Playback())
			{
				var result = _target.Validate(_teamBlockInfo, _optimizationPreferences);
				Assert.IsTrue(result);
			}
		}

		private void commonMocks()
		{
			Expect.Call(_teamBlockInfo.MatrixesForGroupAndBlock()).Return(new[] { _matrix });
			Expect.Call(_matrixCoverter.Convert(_matrix, _optimizationPreferences.DaysOff.ConsiderWeekBefore,
			                                    _optimizationPreferences.DaysOff.ConsiderWeekAfter)).Return(_bitArray);
			
		}

	}
}