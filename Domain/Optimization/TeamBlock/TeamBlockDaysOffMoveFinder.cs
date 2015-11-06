

using System.Collections.Generic;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public interface ITeamBlockDaysOffMoveFinder
	{
		ILockableBitArray TryFindMoves(IScheduleMatrixPro matrix, ILockableBitArray originalArray,
		                                               IOptimizationPreferences optimizationPreferences,
														IDaysOffPreferences daysOffPreferences);
	}

	public class TeamBlockDaysOffMoveFinder : ITeamBlockDaysOffMoveFinder
	{
		private readonly IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;
		private readonly ISmartDayOffBackToLegalStateService _daysOffBackToLegal;
		private readonly IDayOffOptimizationDecisionMakerFactory _dayOffOptimizationDecisionMakerFactory;

		public TeamBlockDaysOffMoveFinder(IScheduleResultDataExtractorProvider scheduleResultDataExtractorProvider,
		                                  ISmartDayOffBackToLegalStateService daysOffBackToLegal,
		                                  IDayOffOptimizationDecisionMakerFactory dayOffOptimizationDecisionMakerFactory)
		{
			_scheduleResultDataExtractorProvider = scheduleResultDataExtractorProvider;
			_daysOffBackToLegal = daysOffBackToLegal;
			_dayOffOptimizationDecisionMakerFactory = dayOffOptimizationDecisionMakerFactory;
		}

		public ILockableBitArray TryFindMoves(IScheduleMatrixPro matrix, ILockableBitArray originalArray,
											  IOptimizationPreferences optimizationPreferences,
											  IDaysOffPreferences daysOffPreferences)
		{
			//should use agggregated skills
			IScheduleResultDataExtractor scheduleResultDataExtractor =
				_scheduleResultDataExtractorProvider.CreatePersonalSkillDataExtractor(matrix, optimizationPreferences.Advanced);

			// find days off to move within the common matrix period
			IEnumerable<IDayOffDecisionMaker> decisionMakers =
				_dayOffOptimizationDecisionMakerFactory.CreateDecisionMakers(originalArray, optimizationPreferences, daysOffPreferences);

			//var workingBitArray = (ILockableBitArray)originalArray.Clone();
			//var daysOffLegalStateValidatorsFactory =  new DaysOffLegalStateValidatorsFactory();
			//var validators = daysOffLegalStateValidatorsFactory.CreateLegalStateValidators(workingBitArray, optimizationPreferences);
			//var test = new ExtendReduceDaysOffDecisionMaker(new ScheduleMatrixLockableBitArrayConverterEx());
			//var result = test.Execute2(matrix, scheduleResultDataExtractor, validators);


			//return result;

			foreach (IDayOffDecisionMaker dayOffDecisionMaker in decisionMakers)
			{
				var workingBitArray = (ILockableBitArray)originalArray.Clone();
				if (!dayOffDecisionMaker.Execute(workingBitArray, scheduleResultDataExtractor.Values()))
				{
					if (!_daysOffBackToLegal.Execute(_daysOffBackToLegal.BuildSolverList(workingBitArray, daysOffPreferences), 100))
						continue;

					if (!dayOffDecisionMaker.Execute(workingBitArray, scheduleResultDataExtractor.Values()))
						continue;
				}

				// DayOffBackToLegal if decisionMaker did something wrong
				if (!_daysOffBackToLegal.Execute(_daysOffBackToLegal.BuildSolverList(workingBitArray, daysOffPreferences), 100))
					continue;

				return workingBitArray;
			}

			return originalArray;
		}
	}
}