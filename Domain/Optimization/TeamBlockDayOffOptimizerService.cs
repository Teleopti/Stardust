

using System.Collections.Generic;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface ITeamBlockDayOffOptimizerService
	{
		void OptimizeDaysOff(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod,
											 IList<IPerson> selectedPersons, ISchedulingOptions schedulingOptions, IDaysOffPreferences daysOffPreferences);
	}

	public class TeamBlockDayOffOptimizerService : ITeamBlockDayOffOptimizerService
	{
		private readonly ITeamInfoFactory _teamInfoFactory;
		private readonly IList<IDayOffDecisionMaker> _decisionMakers;
		private readonly IScheduleMatrixLockableBitArrayConverter _converter;
		private readonly IScheduleResultDataExtractorProvider _scheduleResultDataExtractorProvider;
		private readonly ISmartDayOffBackToLegalStateService _smartDayOffBackToLegalStateService;

		public TeamBlockDayOffOptimizerService(ITeamInfoFactory teamInfoFactory, 
												IList<IDayOffDecisionMaker> decisionMakers,
												IScheduleMatrixLockableBitArrayConverter converter,
												IScheduleResultDataExtractorProvider scheduleResultDataExtractorProvider,
												ISmartDayOffBackToLegalStateService smartDayOffBackToLegalStateService)
		{
			_teamInfoFactory = teamInfoFactory;
			_decisionMakers = decisionMakers;
			_converter = converter;
			_scheduleResultDataExtractorProvider = scheduleResultDataExtractorProvider;
			_smartDayOffBackToLegalStateService = smartDayOffBackToLegalStateService;
		}

		public void OptimizeDaysOff(IList<IScheduleMatrixPro> allPersonMatrixList, DateOnlyPeriod selectedPeriod,
		                            IList<IPerson> selectedPersons, ISchedulingOptions schedulingOptions, IDaysOffPreferences daysOffPreferences)
		{
			var allTeamInfoListOnStartDate = new HashSet<ITeamInfo>();
			foreach (var selectedPerson in selectedPersons)
			{
				allTeamInfoListOnStartDate.Add(_teamInfoFactory.CreateTeamInfo(selectedPerson, selectedPeriod, allPersonMatrixList));
			}

			var remainingInfoList = new List<ITeamInfo>(allTeamInfoListOnStartDate);
			foreach (var teamInfo in remainingInfoList.GetRandom(remainingInfoList.Count, true))
			{
				foreach (var matrix in teamInfo.MatrixesForGroupMember(0))
				{
					ILockableBitArray originalArray = _converter.Convert(daysOffPreferences.ConsiderWeekBefore,
																   daysOffPreferences.ConsiderWeekAfter);

					IScheduleResultDataExtractor scheduleResultDataExtractor =
							_scheduleResultDataExtractorProvider.CreatePersonalSkillDataExtractor(matrix);

					ILockableBitArray resultingArray = tryFindMoves(originalArray, scheduleResultDataExtractor);

					if (resultingArray.Equals(originalArray))
						continue;


					// execute do moves
					// ev back to legal state?
					// if possible reschedule block without clearing
					// else
					//	clear involved teamblocks
					//	reschedule involved teamblocks
					// rollback id failed or not good
					// remember not to break anything in shifts or restrictions
				}
			}
		}

		private ILockableBitArray tryFindMoves(ILockableBitArray originalArray, IScheduleResultDataExtractor scheduleResultDataExtractor)
		{
			// find days off to move within the common matrix period
			foreach (var dayOffDecisionMaker in _decisionMakers)
			{
				ILockableBitArray workingBitArray = (ILockableBitArray)originalArray.Clone();

				if (!dayOffDecisionMaker.Execute(workingBitArray, scheduleResultDataExtractor.Values()))
				{
					if (!_smartDayOffBackToLegalStateService.Execute(_smartDayOffBackToLegalStateService.BuildSolverList(workingBitArray), 100))
						continue;

					if (!dayOffDecisionMaker.Execute(workingBitArray, scheduleResultDataExtractor.Values()))
						continue;
				}

				// DayOffBackToLegal if decisionMaker did something wrong
				if (!_smartDayOffBackToLegalStateService.Execute(_smartDayOffBackToLegalStateService.BuildSolverList(workingBitArray), 100))
					continue;

				return workingBitArray;
			}

			return originalArray;
		}

		// create a list of all teamInfos
		// find a random selected TeamInfo/matrix
 		
	}
}