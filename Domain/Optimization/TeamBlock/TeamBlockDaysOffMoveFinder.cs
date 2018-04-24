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
		private readonly ISmartDayOffBackToLegalStateService _daysOffBackToLegal;
		private readonly IDayOffOptimizationDecisionMakerFactory _dayOffOptimizationDecisionMakerFactory;
		private readonly ICreatePersonalSkillDataExtractor _createPersonalSkillDataExtractor;

		public TeamBlockDaysOffMoveFinder(ISmartDayOffBackToLegalStateService daysOffBackToLegal,
		                                  IDayOffOptimizationDecisionMakerFactory dayOffOptimizationDecisionMakerFactory,
										ICreatePersonalSkillDataExtractor createPersonalSkillDataExtractor)
		{
			_daysOffBackToLegal = daysOffBackToLegal;
			_dayOffOptimizationDecisionMakerFactory = dayOffOptimizationDecisionMakerFactory;
			_createPersonalSkillDataExtractor = createPersonalSkillDataExtractor;
		}

		public ILockableBitArray TryFindMoves(IScheduleMatrixPro matrix, ILockableBitArray originalArray,
											  IOptimizationPreferences optimizationPreferences,
											  IDaysOffPreferences daysOffPreferences, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			//should use agggregated skills
			var scheduleResultDataExtractorValues =
				_createPersonalSkillDataExtractor.Create(matrix, optimizationPreferences, schedulingResultStateHolder).Values();

			// find days off to move within the common matrix period
			IEnumerable<IDayOffDecisionMaker> decisionMakers =
				_dayOffOptimizationDecisionMakerFactory.CreateDecisionMakers(originalArray, optimizationPreferences, daysOffPreferences);

			foreach (IDayOffDecisionMaker dayOffDecisionMaker in decisionMakers)
			{
				var workingBitArray = (ILockableBitArray)originalArray.Clone();
				if (!dayOffDecisionMaker.Execute(workingBitArray, scheduleResultDataExtractorValues))
				{
					if (!_daysOffBackToLegal.Execute(_daysOffBackToLegal.BuildSolverList(workingBitArray, daysOffPreferences, 100), 100))
						continue;

					if (!dayOffDecisionMaker.Execute(workingBitArray, scheduleResultDataExtractorValues))
						continue;
				}


				if(!_daysOffBackToLegal.Execute(_daysOffBackToLegal.BuildSolverList(workingBitArray, daysOffPreferences, 100), 100))
					continue;
				
				return workingBitArray;
			}

			return originalArray;
		}
	}
}