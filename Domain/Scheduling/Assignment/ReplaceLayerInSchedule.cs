﻿using System;
using System.Globalization;
using System.Linq;
using Teleopti.Interfaces.Domain;

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
			var layerAsMain = layerToRemove as IMainShiftLayer;
			if (layerAsMain != null)
			{
				var mainLayers = assignment.MainActivities().ToList();
				var indexOfLayer = mainLayers.IndexOf(layerAsMain);
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
			var layerAsPersonal = layerToRemove as IPersonalShiftLayer;
			if (layerAsPersonal != null)
			{
				var mainLayers = assignment.PersonalActivities().ToList();
				var indexOfLayer = mainLayers.IndexOf(layerAsPersonal);
				if (indexOfLayer > -1)
				{
					mainLayers.RemoveAt(indexOfLayer);
					mainLayers.Insert(indexOfLayer, new PersonalShiftLayer(newActivity, newPeriod));
					assignment.ClearPersonalActivities();
					foreach (var layer in mainLayers)
					{
						assignment.AddPersonalActivity(layer.Payload, layer.Period);
					}
					return true;
				}
			}
			return false;
		}

		private static bool tryReplaceOvertimeLayer(IPersonAssignment assignment, ILayer<IActivity> layerToRemove, IActivity newActivity, DateTimePeriod newPeriod)
		{
			var layerAsOvertime = layerToRemove as IOvertimeShiftLayer;
			if (layerAsOvertime != null)
			{
				var overtimeLayers = assignment.OvertimeActivities().ToList();
				var indexOfLayer = overtimeLayers.IndexOf(layerAsOvertime);
				if (indexOfLayer > -1)
				{
					overtimeLayers.RemoveAt(indexOfLayer);
					overtimeLayers.Insert(indexOfLayer, new OvertimeShiftLayer(newActivity, newPeriod, layerAsOvertime.DefinitionSet));
					assignment.ClearOvertimeActivities();
					foreach (var layer in overtimeLayers)
					{
						assignment.AddOvertimeActivity(layer.Payload, layer.Period, layer.DefinitionSet);
					}
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