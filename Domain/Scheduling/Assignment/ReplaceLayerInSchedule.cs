using System;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class ReplaceLayerInSchedule : IReplaceLayerInSchedule
	{
		private const string exMessageLayerNotFound = "Layer {0} not found!";

		public void Replace(IScheduleDay scheduleDay, ILayer<IActivity> layerToRemove, IActivity newActivity, DateTimePeriod newPeriod)
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
							var indexOfLayer = layer.OrderIndex;
							ms.LayerCollection.Remove(layer);
							ms.LayerCollection.Insert(indexOfLayer, new MainShiftActivityLayer(newActivity, newPeriod));
							return;
						}
					}
				}
			}
			throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, exMessageLayerNotFound, layerToRemove));
		}

		public void Replace(IScheduleDay scheduleDay, ILayer<IAbsence> layerToRemove, IAbsence newAbsence, DateTimePeriod newPeriod)
		{
			throw new System.NotImplementedException();
		}
	}
}