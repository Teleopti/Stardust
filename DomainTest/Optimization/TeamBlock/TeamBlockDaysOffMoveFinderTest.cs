﻿using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock
{
	[TestFixture]
	public class TeamBlockDaysOffMoveFinderTest
	{
		private MockRepository _mocks;
		private ITeamBlockDaysOffMoveFinder _target;
		private IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;
		private IDayOffOptimizationDecisionMakerFactory _dayOffOptimizationDecisionMakerFactory;
		private IScheduleMatrixPro _matrix;
		private IOptimizationPreferences _optimizationPreferences;
		private IScheduleResultDataExtractor _dataExtractor;
		private ILockableBitArray _originalArray;
		private ILockableBitArray _workingArray;
		private IDayOffDecisionMaker _dayOffDecisionMaker;
		private ISmartDayOffBackToLegalStateService _daysOffBackToLegal;
		private IList<double?> _dataExtractorValues;
		private IDaysOffPreferences _daysOffPreferences;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_scheduleResultDataExtractorProvider = _mocks.StrictMock<IScheduleResultDataExtractorProvider>();
			_dayOffOptimizationDecisionMakerFactory = _mocks.StrictMock<IDayOffOptimizationDecisionMakerFactory>();
			_daysOffBackToLegal = _mocks.StrictMock<ISmartDayOffBackToLegalStateService>();
			_target = new TeamBlockDaysOffMoveFinder(_scheduleResultDataExtractorProvider, _daysOffBackToLegal, _dayOffOptimizationDecisionMakerFactory);
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_optimizationPreferences = new OptimizationPreferences();
			_dataExtractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
			_dayOffDecisionMaker = _mocks.StrictMock<IDayOffDecisionMaker>();
			_daysOffPreferences = new DaysOffPreferences();
			
			_dataExtractorValues = new List<double?>();
		}

		[Test]
		public void ShouldReturnWorkingArrayWithMovedDaysOff()
		{
			_originalArray = new LockableBitArray(2, false, false, null);
			_originalArray.Set(0, true);
			_workingArray = (ILockableBitArray)_originalArray.Clone();
			_daysOffPreferences.ConsiderWeekBefore = false;
			_daysOffPreferences.ConsiderWeekAfter = false;
			_optimizationPreferences.Extra.UseTeamBlockOption = true;

			using (_mocks.Record())
			{
				tryFindMoveMocks(false);
			}

			using (_mocks.Playback())
			{
				var result = _target.TryFindMoves(_matrix, _originalArray, _optimizationPreferences, _daysOffPreferences, null);
				Assert.IsNotNull(result);
			}
		}

		[Test]
		public void ShouldReturnOriginalArrayIfTryFailed()
		{
			_originalArray = new LockableBitArray(2, false, false, null);
			_originalArray.Set(0, true);
			_workingArray = (ILockableBitArray)_originalArray.Clone();
			_daysOffPreferences.ConsiderWeekBefore = false;
			_daysOffPreferences.ConsiderWeekAfter = false;
			_optimizationPreferences.Extra.UseTeamBlockOption = true;
			using (_mocks.Record())
			{
				tryFindMoveMocks(true);
			}

			using (_mocks.Playback())
			{
				var result = _target.TryFindMoves(_matrix, _originalArray, _optimizationPreferences, _daysOffPreferences, null);
				Assert.IsNotNull(result);
			}
		}

		private void tryFindMoveMocks(bool failOnNoMoveFound)
		{
			Expect.Call(_scheduleResultDataExtractorProvider.CreatePersonalSkillDataExtractor(_matrix, _optimizationPreferences.Advanced, null))
					  .Return(_dataExtractor);
			Expect.Call(_dayOffOptimizationDecisionMakerFactory.CreateDecisionMakers(_originalArray, _optimizationPreferences, _daysOffPreferences))
				  .Return(new List<IDayOffDecisionMaker> { _dayOffDecisionMaker });

			
			Expect.Call(_dataExtractor.Values()).Return(_dataExtractorValues);

			Expect.Call(_dayOffDecisionMaker.Execute(_workingArray, _dataExtractorValues)).IgnoreArguments().Return(false);

			List<IDayOffBackToLegalStateSolver> solverList = new List<IDayOffBackToLegalStateSolver>();
			Expect.Call(_daysOffBackToLegal.BuildSolverList(_workingArray, _daysOffPreferences)).IgnoreArguments().Return(solverList).Repeat.AtLeastOnce();
			Expect.Call(_daysOffBackToLegal.Execute(solverList, 100)).Return(true).Repeat.AtLeastOnce();

			Expect.Call(_dataExtractor.Values()).Return(_dataExtractorValues);
			Expect.Call(_dayOffDecisionMaker.Execute(_workingArray, _dataExtractorValues)).IgnoreArguments().Return(!failOnNoMoveFound);

			if (failOnNoMoveFound)
				return;

		}

	}
}