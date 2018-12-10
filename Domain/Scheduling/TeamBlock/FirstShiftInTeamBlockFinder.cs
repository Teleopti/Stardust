using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public class FirstShiftInTeamBlockFinder
	{
		private readonly ShiftProjectionCacheManager _shiftProjectionCacheManager;

		public FirstShiftInTeamBlockFinder(ShiftProjectionCacheManager shiftProjectionCacheManager)
		{
			_shiftProjectionCacheManager = shiftProjectionCacheManager;
		}

		public ShiftProjectionCache FindFirst(ITeamBlockInfo teamBlockInfo, IPerson person, DateOnly dateForProjection, IScheduleDictionary scheduleDictionary)
		{
			foreach (var dateOnly in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
			{
				foreach (var groupMember in teamBlockInfo.TeamInfo.GroupMembers)
				{
					var scheduleDay = scheduleDictionary[groupMember].ScheduledDay(dateOnly);
					if (scheduleDay.SignificantPart() == SchedulePartView.MainShift)
					{
						ShiftProjectionCache foundRoleModel =
							_shiftProjectionCacheManager.ShiftProjectionCacheFromShift(scheduleDay.GetEditorShift(),
								new DateOnlyAsDateTimePeriod(dateForProjection, person.PermissionInformation.DefaultTimeZone()));
						return foundRoleModel;
					}
				}
			}

			return null;
		}
	}
}