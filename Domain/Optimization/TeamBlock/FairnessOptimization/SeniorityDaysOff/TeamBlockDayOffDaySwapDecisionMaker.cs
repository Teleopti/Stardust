using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
	public interface ITeamBlockDayOffDaySwapDecisionMaker : ICancellable
	{
		IPossibleSwappableDays Decide(DateOnly dateOnly, ITeamBlockInfo teamBlockSenior, ITeamBlockInfo teamBlockJunior,
		                                                   IScheduleDictionary scheduleDictionary, IOptimizationPreferences optimizationPreferences, IList<DateOnly> dayOffsToGiveAway);
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

		public IPossibleSwappableDays Decide(DateOnly dateOnly, ITeamBlockInfo teamBlockSenior, ITeamBlockInfo teamBlockJunior,
		                                     IScheduleDictionary scheduleDictionary,
		                                     IOptimizationPreferences optimizationPreferences,
		                                     IList<DateOnly> dayOffsToGiveAway)
		{
			var teamBlockSeniorGroupMembers = teamBlockSenior.TeamInfo.GroupMembers.ToList();
			var teamBlockJuniorGroupMembers = teamBlockJunior.TeamInfo.GroupMembers.ToList();
			bool considerWeekBefore = optimizationPreferences.DaysOff.ConsiderWeekBefore;
			bool considerWeekAfter = optimizationPreferences.DaysOff.ConsiderWeekAfter;
			for (int i = 0; i < teamBlockSeniorGroupMembers.Count(); i++)
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

				if (isOnlyJuniorHasDayOff(daySenior, dayJunior))
				{
					var mostValuableDayIndexOfSenior = getIndexFromMatrix(dateOnly, seniorMatrix, considerWeekBefore, considerWeekAfter);
					var mostValuableDayIndexOfJunior = getIndexFromMatrix(dateOnly, juniorMatrix, considerWeekBefore, considerWeekAfter);
					var workingSeniorBitArray = _lockableBitArrayFactory.ConvertFromMatrix(considerWeekBefore, considerWeekAfter,
					                                                                       seniorMatrix);
					var workingJuniorBitArray = _lockableBitArrayFactory.ConvertFromMatrix(considerWeekBefore, considerWeekAfter,
					                                                                       juniorMatrix);
					workingSeniorBitArray.Set(mostValuableDayIndexOfSenior, true);
					workingJuniorBitArray.Set(mostValuableDayIndexOfJunior, false);

					foreach (var dayOffDay in dayOffsToGiveAway)
					{
						if (_cancel) return null;
						var candidateDaySenior = seniorScheduleRange.ScheduledDay(dayOffDay);
						var candidateDayJunior = juniorScheduleRange.ScheduledDay(dayOffDay);

						if (isOnlySeniorHasDayOff(candidateDaySenior, candidateDayJunior))
						{
							var giveAwayDayOffSeniorBitArray = (ILockableBitArray) workingSeniorBitArray.Clone();
							var acceptDayOffJuniorBitArray = (ILockableBitArray) workingJuniorBitArray.Clone();
							var giveAwayDayOffDayIndexOfSenior = getIndexFromMatrix(dayOffDay, seniorMatrix, considerWeekBefore,
							                                                        considerWeekAfter);
							var acceptDayOffDayIndexOfJunior = getIndexFromMatrix(dayOffDay, juniorMatrix, considerWeekBefore,
							                                                      considerWeekAfter);
							giveAwayDayOffSeniorBitArray.Set(giveAwayDayOffDayIndexOfSenior, false);
							acceptDayOffJuniorBitArray.Set(acceptDayOffDayIndexOfJunior, true);

							if (_dayOffRulesValidator.Validate(giveAwayDayOffSeniorBitArray, optimizationPreferences) &&
							    _dayOffRulesValidator.Validate(acceptDayOffJuniorBitArray, optimizationPreferences))
							{
								return new PossibleSwappableDays
									{
										DateForSeniorDayOff = dateOnly,
										DateForRemovingSeniorDayOff = dayOffDay
									};
							}
						}
					}
				}
			}
			return null;
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
