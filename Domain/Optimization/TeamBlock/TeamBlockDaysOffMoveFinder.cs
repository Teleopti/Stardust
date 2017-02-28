﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public interface ITeamBlockDaysOffMoveFinder
	{
		ILockableBitArray TryFindMoves(IScheduleMatrixPro matrix, ILockableBitArray originalArray,
		                                               IOptimizationPreferences optimizationPreferences,
														IDaysOffPreferences daysOffPreferences, ISchedulingResultStateHolder schedulingResultStateHolder);
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
											  IDaysOffPreferences daysOffPreferences, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			//should use agggregated skills
			IScheduleResultDataExtractor scheduleResultDataExtractor =
				_scheduleResultDataExtractorProvider.CreatePersonalSkillDataExtractor(matrix, optimizationPreferences.Advanced, schedulingResultStateHolder);

			// find days off to move within the common matrix period
			IEnumerable<IDayOffDecisionMaker> decisionMakers =
				_dayOffOptimizationDecisionMakerFactory.CreateDecisionMakers(originalArray, optimizationPreferences, daysOffPreferences);

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


				if(!_daysOffBackToLegal.Execute(_daysOffBackToLegal.BuildSolverList(workingBitArray, daysOffPreferences), 100))
					continue;
				
				return workingBitArray;
			}

			return originalArray;
		}
	}
}