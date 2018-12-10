using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class ReplaceLayerInSchedule : IReplaceLayerInSchedule
	{
		private const string exMessageLayerNotFound = "Layer {0} not found when doing a replace of layer!";

		public void Replace(IScheduleDay scheduleDay, ILayer<IActivity> layerToRemove, IActivity newActivity, DateTimePeriod newPeriod)
		{
			var ass = scheduleDay.PersonAssignment();
			if (ass != null)
			{
				if (tryReplaceMainLayer(ass, layerToRemove, newActivity, newPeriod) ||
					tryReplacePersonalLayer(ass, layerToRemove, newActivity, newPeriod) ||
					tryReplaceOvertimeLayer(ass, layerToRemove, newActivity, newPeriod))
				{
					return;
				}
			}
			throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, exMessageLayerNotFound, layerToRemove));
		}



		private static bool tryReplaceMainLayer(IPersonAssignment assignment, ILayer<IActivity> layerToRemove, IActivity newActivity, DateTimePeriod newPeriod)
		{
			var layerAsMain = layerToRemove as MainShiftLayer;
			if (layerAsMain != null)
			{
				var indexOfLayer = assignment.ShiftLayers.ToList().IndexOf(layerAsMain);
				if (indexOfLayer > -1)
				{
					assignment.RemoveActivity(layerAsMain);
					assignment.InsertActivity(newActivity,newPeriod,indexOfLayer);
					return true;
				}
			}
			return false;
		}

		private static bool tryReplacePersonalLayer(IPersonAssignment assignment, ILayer<IActivity> layerToRemove, IActivity newActivity, DateTimePeriod newPeriod)
		{
			var layerAsPersonal = layerToRemove as PersonalShiftLayer;
			if (layerAsPersonal != null)
			{
				var shiftLayers = assignment.ShiftLayers.ToList();
				var indexOfLayer = shiftLayers.IndexOf(layerAsPersonal);
				if (indexOfLayer > -1)
				{
					assignment.RemoveActivity(layerAsPersonal);
					assignment.InsertPersonalLayer(newActivity, newPeriod, indexOfLayer);
					return true;
				}
			}
			return false;
		}

		private static bool tryReplaceOvertimeLayer(IPersonAssignment assignment, ILayer<IActivity> layerToRemove, IActivity newActivity, DateTimePeriod newPeriod)
		{
			var layerAsOvertime = layerToRemove as OvertimeShiftLayer;
			if (layerAsOvertime != null)
			{
				var shiftLayers = assignment.ShiftLayers.ToList();
				var indexOfLayer = shiftLayers.IndexOf(layerAsOvertime);
				if (indexOfLayer > -1)
				{
					assignment.RemoveActivity(layerAsOvertime);
					assignment.InsertOvertimeLayer(newActivity, newPeriod, indexOfLayer,layerAsOvertime.DefinitionSet);
					return true;
				}
			}
			return false;
		}

		public void Replace(IScheduleDay scheduleDay, ILayer<IAbsence> layerToRemove, IAbsence newAbsence, DateTimePeriod newPeriod)
		{
			foreach (var personAbsence in scheduleDay.PersonAbsenceCollection()
																	.Where(personAbsence => personAbsence.Layer.Equals(layerToRemove)))
			{
				personAbsence.ReplaceLayer(newAbsence, newPeriod);
				return;
			}
			throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, exMessageLayerNotFound, layerToRemove));
		}
	}
}