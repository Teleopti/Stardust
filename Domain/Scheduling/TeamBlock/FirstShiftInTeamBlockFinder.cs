using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface IFirstShiftInTeamBlockFinder
	{
		IShiftProjectionCache FindFirst(ITeamBlockInfo teamBlockInfo, IPerson person, DateOnly dateForProjection, IScheduleDictionary scheduleDictionary);
	}

	public class FirstShiftInTeamBlockFinder : IFirstShiftInTeamBlockFinder
	{
		private readonly IShiftProjectionCacheManager _shiftProjectionCacheManager;

		public FirstShiftInTeamBlockFinder(IShiftProjectionCacheManager shiftProjectionCacheManager)
		{
			_shiftProjectionCacheManager = shiftProjectionCacheManager;
		}

		public IShiftProjectionCache FindFirst(ITeamBlockInfo teamBlockInfo, IPerson person, DateOnly dateForProjection, IScheduleDictionary scheduleDictionary)
		{
			foreach (var dateOnly in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
			{
				foreach (var groupMember in teamBlockInfo.TeamInfo.GroupMembers)
				{
					var scheduleDay = scheduleDictionary[groupMember].ScheduledDay(dateOnly);
					if (scheduleDay.SignificantPart() == SchedulePartView.MainShift)
					{
						IShiftProjectionCache foundRoleModel =
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