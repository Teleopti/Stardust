using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface IFirstShiftInTeamBlockFinder
	{
		IShiftProjectionCache FindFirst(ITeamBlockInfo teamBlockInfo, IPerson person, DateOnly dateForProjection, ISchedulingResultStateHolder schedulingResultStateHolder);
	}

	public class FirstShiftInTeamBlockFinder : IFirstShiftInTeamBlockFinder
	{
		private readonly IShiftProjectionCacheManager _shiftProjectionCacheManager;

		public FirstShiftInTeamBlockFinder(IShiftProjectionCacheManager shiftProjectionCacheManager)
		{
			_shiftProjectionCacheManager = shiftProjectionCacheManager;
		}

		public IShiftProjectionCache FindFirst(ITeamBlockInfo teamBlockInfo, IPerson person, DateOnly dateForProjection, ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			foreach (var dateOnly in teamBlockInfo.BlockInfo.BlockPeriod.DayCollection())
			{
				foreach (var groupMember in teamBlockInfo.TeamInfo.GroupMembers)
				{
					var scheduleDay = schedulingResultStateHolder.Schedules[groupMember].ScheduledDay(dateOnly);
					if (scheduleDay.SignificantPart() == SchedulePartView.MainShift)
					{
						IShiftProjectionCache foundRoleModel =
							_shiftProjectionCacheManager.ShiftProjectionCacheFromShift(scheduleDay.GetEditorShift(), dateForProjection,
								person.PermissionInformation.DefaultTimeZone());
						return foundRoleModel;
					}
				}
			}

			return null;
		}
	}
}