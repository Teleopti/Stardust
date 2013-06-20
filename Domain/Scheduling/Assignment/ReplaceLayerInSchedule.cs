using System;
using System.Collections.Generic;
using System.Globalization;
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
			foreach (var ass in scheduleDay.PersonAssignmentCollection())
			{
				foreach (var layer in ass.MainLayers)
				{
					if (layer.Equals(layerToRemove))
					{
						var indexOfLayer = layer.OrderIndex;
						var newLayers = new List<IMainShiftLayer>(ass.MainLayers);
						newLayers.Remove(layer);
						newLayers.Insert(indexOfLayer, new MainShiftLayer(newActivity, newPeriod));
						ass.SetMainShiftLayers(newLayers, ass.ShiftCategory);
						return;
					}
				}

				foreach (var personalShift in ass.PersonalShiftCollection)
				{
					foreach (var layer in personalShift.LayerCollection)
					{
						if (layer.Equals(layerToRemove))
						{
							//MICKE! DENNA FIXAR JAG/ROGER!
							var indexOfLayer = layer.OrderIndex;
							personalShift.LayerCollection.Remove(layer);
							personalShift.LayerCollection.Insert(indexOfLayer, new PersonalShiftActivityLayer(newActivity, newPeriod));
							return;
						}
					}
				}

				foreach (var overtimeShift in ass.OvertimeShiftCollection)
				{
					foreach (IOvertimeShiftActivityLayer layer in overtimeShift.LayerCollection)
					{
						if (layer.Equals(layerToRemove))
						{
							var indexOfLayer = layer.OrderIndex;
							overtimeShift.LayerCollection.Remove(layer);
							overtimeShift.LayerCollection.Insert(indexOfLayer, new OvertimeShiftActivityLayer(newActivity, newPeriod, layer.DefinitionSet));
							return;
						}
					}
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