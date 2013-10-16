using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DayOffPlanning;
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
		private ISmartDayOffBackToLegalStateService _smartDayOffBackToLegalStateService;
		private IDayOffOptimizationDecisionMakerFactory _dayOffOptimizationDecisionMakerFactory;
		private IScheduleMatrixPro _matrix;
		private IOptimizationPreferences _optimizationPreferences;
		private IScheduleResultDataExtractor _dataExtractor;
		private ILockableBitArray _originalArray;
		private IDayOffDecisionMaker _dayOffDecisionMaker;
		private ISmartDayOffBackToLegalStateService _daysOffBackToLegal;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_scheduleResultDataExtractorProvider = _mocks.StrictMock<IScheduleResultDataExtractorProvider>();
			_smartDayOffBackToLegalStateService = _mocks.StrictMock<ISmartDayOffBackToLegalStateService>();
			_dayOffOptimizationDecisionMakerFactory = _mocks.StrictMock<IDayOffOptimizationDecisionMakerFactory>();
			_target = new TeamBlockDaysOffMoveFinder(_scheduleResultDataExtractorProvider, _smartDayOffBackToLegalStateService, _dayOffOptimizationDecisionMakerFactory);
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_optimizationPreferences = new OptimizationPreferences();
			_dataExtractor = _mocks.StrictMock<IScheduleResultDataExtractor>();
			_dayOffDecisionMaker = _mocks.StrictMock<IDayOffDecisionMaker>();
			_daysOffBackToLegal = _mocks.StrictMock<ISmartDayOffBackToLegalStateService>();
		}

		[Test]
		public void ShouldReturnWorkingArrayWithMovedDaysOff()
		{
			_originalArray = new LockableBitArray(2, false, false, null);
			_originalArray.Set(0, true);
			_optimizationPreferences.DaysOff.ConsiderWeekBefore = false;
			_optimizationPreferences.DaysOff.ConsiderWeekAfter = false;
			_optimizationPreferences.Extra.UseTeamBlockOption = true;
			using (_mocks.Record())
			{
				tryFindMoveMocks(false);
			}

			using (_mocks.Playback())
			{
				var result = _target.TryFindMoves(_matrix, _originalArray, _optimizationPreferences);
				Assert.That(!result.DaysOffBitArray[0]);
				Assert.That(result.DaysOffBitArray[1]);
			}
		}

		private void tryFindMoveMocks(bool failOnNoMoveFound)
		{
			Expect.Call(_scheduleResultDataExtractorProvider.CreatePersonalSkillDataExtractor(_matrix,
																								  _optimizationPreferences.Advanced))
					  .Return(_dataExtractor);
			Expect.Call(_dayOffOptimizationDecisionMakerFactory.CreateDecisionMakers(_originalArray, _optimizationPreferences))
				  .Return(new List<IDayOffDecisionMaker> { _dayOffDecisionMaker });

			IList<double?> dataExtractorValues = new List<double?>();
			Expect.Call(_dataExtractor.Values()).Return(dataExtractorValues);
			ILockableBitArray workingArray = (ILockableBitArray)_originalArray.Clone();
			Expect.Call(_dayOffDecisionMaker.Execute(workingArray, dataExtractorValues)).IgnoreArguments().Return(false);
			List<IDayOffBackToLegalStateSolver> solverList = new List<IDayOffBackToLegalStateSolver>();
			Expect.Call(_daysOffBackToLegal.BuildSolverList(workingArray)).IgnoreArguments().Return(solverList);
			//Expect.Call(_daysOffBackToLegal.Execute(solverList, 100)).Return(true);

			//Expect.Call(_dataExtractor.Values()).Return(dataExtractorValues);
			//Expect.Call(_dayOffDecisionMaker.Execute(workingArray, dataExtractorValues)).IgnoreArguments().Return(!failOnNoMoveFound);

			//if (failOnNoMoveFound)
			//	return;

			//Expect.Call(_daysOffBackToLegal.BuildSolverList(workingArray)).IgnoreArguments().Return(solverList);
			//Expect.Call(_daysOffBackToLegal.Execute(solverList, 100)).Return(true);




		}

	}
}