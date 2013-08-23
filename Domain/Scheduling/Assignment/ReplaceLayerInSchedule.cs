using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class ReplaceLayerInSchedule : IReplaceLayerInSchedule
	{
		private const string exMessageLayerNotFound = "Layer {0} not found when doing a replace of layer!";


		//this can be done MUCH simpler when we have one list of layers and no shifts....
		//should work against a PersonAssignment (aka AgentDay) and not IScheduleDay
		public void Replace(IScheduleDay scheduleDay, ILayer<IActivity> layerToRemove, IActivity newActivity, DateTimePeriod newPeriod)
		{
			foreach (var ass in scheduleDay.PersonAssignmentCollectionDoNotUse())
			{
				foreach (var layer in ass.MainLayers())
				{
					if (layer.Equals(layerToRemove))
					{
						var indexOfLayer = layer.OrderIndex;
						var newLayers = new List<IMainShiftLayer>(ass.MainLayers());
						newLayers.Remove(layer);
						newLayers.Insert(indexOfLayer, new MainShiftLayer(newActivity, newPeriod));
						ass.SetMainShiftLayers(newLayers, ass.ShiftCategory);
						return;
					}
				}

				var layerAsPersonal = layerToRemove as IPersonalShiftLayer;
				if (layerAsPersonal != null)
				{
					var personalLayers = ass.PersonalLayers().ToList();
					var indexOfLayer = personalLayers.IndexOf(layerAsPersonal);
					personalLayers.RemoveAt(indexOfLayer);
					personalLayers.Insert(indexOfLayer, new PersonalShiftLayer(newActivity, newPeriod));
					ass.ClearPersonalLayers();
					foreach (var personalLayer in personalLayers)
					{
						ass.AddPersonalLayer(personalLayer.Payload, personalLayer.Period);
					}
					return;
				}

				var layerAsOvertime = layerToRemove as IOvertimeShiftLayer;
				if (layerAsOvertime != null)
				{
					var overtimeLayers = ass.OvertimeLayers().ToList();
					var indexOfLayer = layerAsOvertime.OrderIndex;
					overtimeLayers.RemoveAt(indexOfLayer);
					overtimeLayers.Insert(indexOfLayer, new OvertimeShiftLayer(newActivity, newPeriod, layerAsOvertime.DefinitionSet));
					ass.ClearOvertimeLayers();
					foreach (var overtimeLayer in overtimeLayers)
					{
						ass.AddOvertimeLayer(overtimeLayer.Payload, overtimeLayer.Period, overtimeLayer.DefinitionSet);
					}
					return;
				}
			}
			throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, exMessageLayerNotFound, layerToRemove));
		}

		public void Replace(IScheduleDay scheduleDay, ILayer<IAbsence> layerToRemove, IAbsence newAbsence, DateTimePeriod newPeriod)
		{
			foreach (var personAbsence in scheduleDay.PersonAbsenceCollection())
			{
				if (personAbsence.Layer.Equals(layerToRemove))
				{
					//behövs nån form av orderindex på personabsence kommas ihåg?
					scheduleDay.Remove(personAbsence);
					scheduleDay.Add(new PersonAbsence(personAbsence.Person, personAbsence.Scenario,
																						new AbsenceLayer(newAbsence, newPeriod)));
					return;
				}
			}
			throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, exMessageLayerNotFound, layerToRemove));
		}
	}
}