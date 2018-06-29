using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface IFirstShiftInTeamBlockFinder
	{
		ShiftProjectionCache FindFirst(ITeamBlockInfo teamBlockInfo, IPerson person, DateOnly dateForProjection, IScheduleDictionary scheduleDictionary);
	}

	public class FirstShiftInTeamBlockFinder : IFirstShiftInTeamBlockFinder
	{
		private readonly IShiftProjectionCacheManager _shiftProjectionCacheManager;

		public FirstShiftInTeamBlockFinder(IShiftProjectionCacheManager shiftProjectionCacheManager)
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