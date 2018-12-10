using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
	public interface ITeamBlockDayOffDaySwapDecisionMaker : ICancellable
	{
		PossibleSwappableDays Decide(DateOnly dateOnly, ITeamBlockInfo teamBlockSenior, ITeamBlockInfo teamBlockJunior,
		                                                   IScheduleDictionary scheduleDictionary, 
															IOptimizationPreferences optimizationPreferences, 
															IList<DateOnly> dayOffsToGiveAway,
															IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider);
	}

	public class TeamBlockDayOffDaySwapDecisionMaker : ITeamBlockDayOffDaySwapDecisionMaker
	{
		private readonly ILockableBitArrayFactory _lockableBitArrayFactory;
		private readonly IDayOffRulesValidator _dayOffRulesValidator;
		private bool _cancel;

		public TeamBlockDayOffDaySwapDecisionMaker(ILockableBitArrayFactory lockableBitArrayFactory, IDayOffRulesValidator dayOffRulesValidator)
		{
			_lockableBitArrayFactory = lockableBitArrayFactory;
			_dayOffRulesValidator = dayOffRulesValidator;
		}

		public PossibleSwappableDays Decide(DateOnly dateOnly, 
											 ITeamBlockInfo teamBlockSenior, 
											 ITeamBlockInfo teamBlockJunior,
		                                     IScheduleDictionary scheduleDictionary,
		                                     IOptimizationPreferences optimizationPreferences,
		                                     IList<DateOnly> dayOffsToGiveAway,
												IDayOffOptimizationPreferenceProvider dayOffOptimizationPreferenceProvider)
		{
			var teamBlockSeniorGroupMembers = teamBlockSenior.TeamInfo.GroupMembers.ToList();
			var teamBlockJuniorGroupMembers = teamBlockJunior.TeamInfo.GroupMembers.ToList();

			for (var i = 0; i < teamBlockSeniorGroupMembers.Count(); i++)
			{
				if (_cancel) return null;
				var personSenior = teamBlockSeniorGroupMembers[i];
				var personJunior = teamBlockJuniorGroupMembers[i];
				var seniorMatrix = teamBlockSenior.TeamInfo.MatrixForMemberAndDate(personSenior, dateOnly);
				var juniorMatrix = teamBlockJunior.TeamInfo.MatrixForMemberAndDate(personJunior, dateOnly);
				var seniorScheduleRange = scheduleDictionary[personSenior];
				var juniorScheduleRange = scheduleDictionary[personJunior];
				var daySenior = seniorScheduleRange.ScheduledDay(dateOnly);
				var dayJunior = juniorScheduleRange.ScheduledDay(dateOnly);
				var dayOffOptimizePreferenceSenior = dayOffOptimizationPreferenceProvider.ForAgent(personSenior, dateOnly);
				var dayOffOptimizePreferenceJunior = dayOffOptimizationPreferenceProvider.ForAgent(personJunior, dateOnly);
				var considerWeekBeforeSenior = dayOffOptimizePreferenceSenior.ConsiderWeekBefore;
				var considerWeekAfterSenior = dayOffOptimizePreferenceSenior.ConsiderWeekAfter;
				var considerWeekBeforeJunior = dayOffOptimizePreferenceJunior.ConsiderWeekBefore;
				var considerWeekAfterJunior = dayOffOptimizePreferenceJunior.ConsiderWeekAfter;

				if (isOnlyJuniorHasDayOff(daySenior, dayJunior))
				{
					var mostValuableDayIndexOfSenior = getIndexFromMatrix(dateOnly, seniorMatrix, considerWeekBeforeSenior, considerWeekAfterSenior);
					var mostValuableDayIndexOfJunior = getIndexFromMatrix(dateOnly, juniorMatrix, considerWeekBeforeJunior, considerWeekAfterJunior);
					var workingSeniorBitArray = _lockableBitArrayFactory.ConvertFromMatrix(considerWeekBeforeSenior, considerWeekAfterSenior, seniorMatrix);
					var workingJuniorBitArray = _lockableBitArrayFactory.ConvertFromMatrix(considerWeekBeforeJunior, considerWeekAfterJunior, juniorMatrix);
					var isLocked = workingSeniorBitArray.IsLocked(mostValuableDayIndexOfSenior, true) || workingJuniorBitArray.IsLocked(mostValuableDayIndexOfJunior, true);

					if (!isLocked)
					{
						workingSeniorBitArray.Set(mostValuableDayIndexOfSenior, true);
						workingJuniorBitArray.Set(mostValuableDayIndexOfJunior, false);

						IEnumerable<DateOnly> daysOffToGiveAwayFilteredToMatrix = filterOutDaysOffOutsidePeriod(seniorMatrix.EffectivePeriodDays, dayOffsToGiveAway);
						foreach (var dayOffDay in daysOffToGiveAwayFilteredToMatrix)
						{
							if (_cancel) return null;
							var candidateDaySenior = seniorScheduleRange.ScheduledDay(dayOffDay);
							var candidateDayJunior = juniorScheduleRange.ScheduledDay(dayOffDay);

							if (isOnlySeniorHasDayOff(candidateDaySenior, candidateDayJunior))
							{
								var giveAwayDayOffSeniorBitArray = (ILockableBitArray) workingSeniorBitArray.Clone();
								var acceptDayOffJuniorBitArray = (ILockableBitArray) workingJuniorBitArray.Clone();
								var giveAwayDayOffDayIndexOfSenior = getIndexFromMatrix(dayOffDay, seniorMatrix, considerWeekBeforeSenior, considerWeekAfterSenior);
								var acceptDayOffDayIndexOfJunior = getIndexFromMatrix(dayOffDay, juniorMatrix, considerWeekBeforeJunior, considerWeekAfterJunior);
								if(acceptDayOffDayIndexOfJunior < 0) continue;	
								isLocked = giveAwayDayOffSeniorBitArray.IsLocked(giveAwayDayOffDayIndexOfSenior, true) || acceptDayOffJuniorBitArray.IsLocked(acceptDayOffDayIndexOfJunior, true);

								if (!isLocked)
								{
									giveAwayDayOffSeniorBitArray.Set(giveAwayDayOffDayIndexOfSenior, false);
									acceptDayOffJuniorBitArray.Set(acceptDayOffDayIndexOfJunior, true);

									if (_dayOffRulesValidator.Validate(giveAwayDayOffSeniorBitArray, optimizationPreferences, dayOffOptimizePreferenceSenior) &&
									    _dayOffRulesValidator.Validate(acceptDayOffJuniorBitArray, optimizationPreferences, dayOffOptimizePreferenceJunior))
									{
										return new PossibleSwappableDays{DateForSeniorDayOff = dateOnly,DateForRemovingSeniorDayOff = dayOffDay};
									}
								}
							}
						}
					}
				}
			}
			return null;
		}

		private static IEnumerable<DateOnly> filterOutDaysOffOutsidePeriod(IScheduleDayPro[] period, IEnumerable<DateOnly> dayOffsToFilter)
		{
			//reads the first and last day out of the period, perhaps there is a better way //tamasb 
			var masterPeriod = new DateOnlyPeriod(period[0].Day, period[period.Length-1].Day);
			
			return dayOffsToFilter.Where(masterPeriod.Contains).ToList();
		}

		private static bool isOnlyJuniorHasDayOff(IScheduleDay daySenior, IScheduleDay dayJunior)
		{
			var significantPartOfSeniorDay = daySenior.SignificantPart();
			var significantPartOfJuniorDay = dayJunior.SignificantPart();
			return significantPartOfSeniorDay != SchedulePartView.DayOff && significantPartOfJuniorDay == SchedulePartView.DayOff;
		}
		private static bool isOnlySeniorHasDayOff(IScheduleDay daySenior, IScheduleDay dayJunior)
		{
			var significantPartOfSeniorDay = daySenior.SignificantPart();
			var significantPartOfJuniorDay = dayJunior.SignificantPart();
			return significantPartOfSeniorDay == SchedulePartView.DayOff && significantPartOfJuniorDay != SchedulePartView.DayOff;
		}

		private int getIndexFromMatrix(DateOnly dateOnly, IScheduleMatrixPro matrix, bool considerWeekBefore, bool considerWeekAfter)
		{
			if (!considerWeekBefore && !considerWeekAfter)
				return matrix.FullWeeksPeriodDays.ToList().FindIndex(x => x.Day == dateOnly);

			if (considerWeekBefore && !considerWeekAfter)
				return matrix.WeekBeforeOuterPeriodDays.ToList().FindIndex(x => x.Day == dateOnly);

			if (!considerWeekBefore)
				return matrix.WeekAfterOuterPeriodDays.ToList().FindIndex(x => x.Day == dateOnly);

			return matrix.OuterWeeksPeriodDays.ToList().FindIndex(x => x.Day == dateOnly);
		}

		public void Cancel()
		{
			_cancel = true;
		}
	}
}
