using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction
{
	public class SameActivityRestriction : IScheduleRestrictionStrategy
	{
		private readonly IActivity _activity;

		public SameActivityRestriction(IActivity activity)
		{
			_activity = activity;
		}

		public IEffectiveRestriction ExtractRestriction(IList<DateOnly> dateOnlyList, IList<IScheduleMatrixPro> matrixList)
		{
			var restriction = new EffectiveRestriction(new StartTimeLimitation(),
													   new EndTimeLimitation(),
													   new WorkTimeLimitation(), null, null, null,
													   new List<IActivityRestriction>());
			foreach (IScheduleMatrixPro matrix in matrixList)
			{
				foreach (DateOnly dateOnly in dateOnlyList)
				{
					IScheduleDayPro schedule = matrix.GetScheduleDayByKey(dateOnly);

					IScheduleDay schedulePart = schedule?.DaySchedulePart();
					if (schedulePart?.SignificantPart() == SchedulePartView.MainShift)
					{
						IPersonAssignment assignment = schedulePart.PersonAssignment();
						if (assignment == null) continue;
						var commonActivityLayers = assignment.ShiftLayers.Where(x => x.Payload.Id == _activity.Id).ToList();
						if (commonActivityLayers.IsNullOrEmpty())
							continue;
						var commonActivityPeriods = new List<DateTimePeriod>();
						foreach (var commonActivityLayer in commonActivityLayers)
						{
							if (!commonActivityPeriods.Contains(commonActivityLayer.Period))
								commonActivityPeriods.Add(commonActivityLayer.Period);
						}
						if (restriction.CommonActivity == null)
						{
							restriction.CommonActivity = new CommonActivity {Activity = _activity, Periods = commonActivityPeriods};
						}
						else
						{
							if (!restriction.CommonActivity.Periods.NonSequenceEquals(commonActivityPeriods))
								return null;
						}
					}
				}
			}
			return restriction;
		}
	}
}
