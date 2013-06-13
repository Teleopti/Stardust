using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class ReplaceLayerInSchedule : IReplaceLayerInSchedule
	{
		public bool Replace(IScheduleDay scheduleDay, ILayer<IActivity> layerToRemove, IActivity newActivity, DateTimePeriod newPeriod)
		{
			foreach (var ass in scheduleDay.PersonAssignmentCollection())
			{
#pragma warning disable 612,618
				var ms = ass.ToMainShift();
#pragma warning restore 612,618
				if (ms != null)
				{
					foreach (var layer in ms.LayerCollection)
					{
						if (layer.Equals(layerToRemove))
						{
							ms.LayerCollection.Remove(layer);
							ms.LayerCollection.Insert(layer.OrderIndex, new MainShiftActivityLayer(newActivity, newPeriod));
							return true;
						}
					}
				}
			}
			return false;
		}

		public bool Replace(IScheduleDay scheduleDay, ILayer<IAbsence> layerToRemove, IAbsence newAbsence, DateTimePeriod newPeriod)
		{
			throw new System.NotImplementedException();
		}
	}
}