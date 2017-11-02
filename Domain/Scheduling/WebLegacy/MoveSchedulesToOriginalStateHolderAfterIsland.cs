using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WebLegacy
{
	public class MoveSchedulesToOriginalStateHolderAfterIsland : ISynchronizeSchedulesAfterIsland
	{
		private readonly DesktopContext _desktopContext;

		public MoveSchedulesToOriginalStateHolderAfterIsland(DesktopContext desktopContext)
		{
			_desktopContext = desktopContext;
		}

		public void Synchronize(IScheduleDictionary modifiedScheduleDictionary, DateOnlyPeriod period)
		{
			var schedulerScheduleDictionary = _desktopContext.CurrentContext().SchedulerStateHolderFrom.Schedules;
			foreach (var diff in modifiedScheduleDictionary.DifferenceSinceSnapshot()
				.Where(x => x.CurrentItem is IPersonAssignment || x.CurrentItem is IPersonAbsence))
			{
				var dateOnly = new DateOnly(diff.CurrentItem.Period.StartDateTimeLocal(diff.CurrentItem.Person.PermissionInformation.DefaultTimeZone()).Date);
				var toScheduleDay = schedulerScheduleDictionary[diff.CurrentItem.Person].ScheduledDay(dateOnly);
				var fromScheduleDay = modifiedScheduleDictionary[diff.CurrentItem.Person].ScheduledDay(dateOnly);
				toScheduleDay.Replace(diff.CurrentItem);
				schedulerScheduleDictionary.Modify(
					ScheduleModifier.Scheduler, 
					toScheduleDay, 
					NewBusinessRuleCollection.Minimum(),
					new DoNothingScheduleDayChangeCallBack(), 
					new ScheduleTagSetter(fromScheduleDay.ScheduleTag()));
			}
		}
	}
}