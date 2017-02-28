﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization
{
	public interface ITeamBlockShiftCategoryLimitationValidator
	{
		bool Validate(ITeamBlockInfo teamBlockInfo1, ITeamBlockInfo teamBlockInfo2, IOptimizationPreferences optimizationPreferences);
	}

	public class TeamBlockShiftCategoryLimitationValidator : ITeamBlockShiftCategoryLimitationValidator
	{
		private readonly IShiftCategoryLimitationChecker _shiftCategoryLimitationChecker;

		public TeamBlockShiftCategoryLimitationValidator(IShiftCategoryLimitationChecker shiftCategoryLimitationChecker)
		 {
			 _shiftCategoryLimitationChecker = shiftCategoryLimitationChecker;
		 }

		public bool Validate(ITeamBlockInfo teamBlockInfo1, ITeamBlockInfo teamBlockInfo2, IOptimizationPreferences optimizationPreferences)
		{
			if (optimizationPreferences == null || !optimizationPreferences.General.UseShiftCategoryLimitations)
				return true;

			var firstBlockOk = checkTeamBlock(teamBlockInfo1);
			var secondBlockOk = checkTeamBlock(teamBlockInfo2);
			if (!(firstBlockOk && secondBlockOk))
			{
				return false;
			}

			return true;
		}

		private bool checkTeamBlock(ITeamBlockInfo teamBlock)
		{
			if (teamBlock == null) return true;

			foreach (var dateOnly in teamBlock.BlockInfo.BlockPeriod.DayCollection())
			{
				var teamInfo = teamBlock.TeamInfo;
				foreach (var groupMember in teamInfo.GroupMembers)
				{
					var matrix = teamInfo.MatrixForMemberAndDate(groupMember, dateOnly);
					if (matrix == null || !checkPerson(groupMember, dateOnly, matrix.ActiveScheduleRange))
						return false;
				}
			}

			return true;
		}

		private bool checkPerson(IPerson person, DateOnly dateOnly, IScheduleRange range)
		{
			IVirtualSchedulePeriod schedulePeriod = person.VirtualSchedulePeriod(dateOnly);
			if (!schedulePeriod.IsValid)
				return false;

			foreach (var shiftCategoryLimitation in schedulePeriod.ShiftCategoryLimitationCollection())
			{
				IList<DateOnly> datesWithCategory;
				if (shiftCategoryLimitation.Weekly)
				{
					var firstDateInPeriodLocal = DateHelper.GetFirstDateInWeek(dateOnly, person.FirstDayOfWeek);
					var dateOnlyWeek = new DateOnlyPeriod(firstDateInPeriodLocal, firstDateInPeriodLocal.AddDays(6));
					if (_shiftCategoryLimitationChecker.IsShiftCategoryOverWeekLimit(shiftCategoryLimitation,
																					 range, dateOnlyWeek,
					                                                                 out datesWithCategory))
						return false;
				}
				else
				{
					DateOnlyPeriod period = schedulePeriod.DateOnlyPeriod;
					if (_shiftCategoryLimitationChecker.IsShiftCategoryOverPeriodLimit(shiftCategoryLimitation, period, range,
														   out datesWithCategory))
						return false;
				}
			}

			return true;
		}
	}
}