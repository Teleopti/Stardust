using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	//one valid for mainshiftlayer and overtimeshiftlayer
	//throw if layer not found?
	public class MoveLayerVertical : IMoveLayerVertical
	{
		public void MoveUp(IPersonAssignment personAssignment, ILayer<IActivity> layer)
		{
			//no need to cast later when agentday is done
			var msLayer = layer as IMainShiftLayer;
			if (msLayer != null)
			{
				var oldLayers = new List<IMainShiftLayer>(personAssignment.MainLayers());
				var indexOfMainShiftLayer = msLayer.OrderIndex;
				oldLayers.Remove(msLayer);
				oldLayers.Insert(indexOfMainShiftLayer-1, msLayer);
				personAssignment.ClearMainLayers();
				foreach (var shiftLayer in oldLayers)
				{
					personAssignment.AddMainLayer(shiftLayer.Payload, shiftLayer.Period);
				}
				return;
			}

			var pLayer = layer as IPersonalShiftLayer;
			if (pLayer != null)
			{
				var oldLayers = new List<IPersonalShiftLayer>(personAssignment.PersonalLayers());
				var index = pLayer.OrderIndex;
				oldLayers.RemoveAt(index);
				oldLayers.Insert(index-1, pLayer);
				personAssignment.ClearPersonalLayers();
				foreach (var newPersonalLayer in oldLayers)
				{
					personAssignment.AddPersonalLayer(newPersonalLayer.Payload, newPersonalLayer.Period);
				}
				return;
			}

			//no need to cast later when agentday is done
			var overLayer = layer as IOvertimeShiftLayer;
			if (overLayer != null)
			{
				var oldLayers = personAssignment.OvertimeLayers().ToList();
				var index = overLayer.OrderIndex;
				oldLayers.RemoveAt(index);
				oldLayers.Insert(index - 1, overLayer);
				personAssignment.ClearOvertimeLayers();
				foreach (var newOvertimeShiftLayer in oldLayers)
				{
					personAssignment.AddOvertimeLayer(newOvertimeShiftLayer.Payload, newOvertimeShiftLayer.Period, newOvertimeShiftLayer.DefinitionSet);
				}
			}
		}

		public void MoveDown(IPersonAssignment personAssignment, ILayer<IActivity> layer)
		{
			//no need to cast later when agentday is done
			var msLayer = layer as IMainShiftLayer;
			if (msLayer != null)
			{
				var oldLayers = new List<IMainShiftLayer>(personAssignment.MainLayers());
				var indexOfMainShiftLayer = msLayer.OrderIndex;
				oldLayers.Remove(msLayer);
				oldLayers.Insert(indexOfMainShiftLayer + 1, msLayer);
				personAssignment.ClearMainLayers();
				foreach (var newPersonalLayer in oldLayers)
				{
					personAssignment.AddMainLayer(newPersonalLayer.Payload, newPersonalLayer.Period);
				}
				return;
			}

			var pLayer = layer as IPersonalShiftLayer;
			if (pLayer != null)
			{
				var oldLayers = new List<IPersonalShiftLayer>(personAssignment.PersonalLayers());
				var index = pLayer.OrderIndex;
				oldLayers.RemoveAt(index);
				oldLayers.Insert(index +1, pLayer);
				personAssignment.ClearPersonalLayers();
				foreach (var newPersonalLayer in oldLayers)
				{
					personAssignment.AddPersonalLayer(newPersonalLayer.Payload, newPersonalLayer.Period);
				}
				return;
			}

			//no need to cast later when agentday is done
			var overLayer = layer as IOvertimeShiftLayer;
			if (overLayer != null)
			{
				var oldLayers = personAssignment.OvertimeLayers().ToList();
				var index = overLayer.OrderIndex;
				oldLayers.RemoveAt(index);
				oldLayers.Insert(index + 1, overLayer);
				personAssignment.ClearOvertimeLayers();
				foreach (var newOvertimeShiftLayer in oldLayers)
				{
					personAssignment.AddOvertimeLayer(newOvertimeShiftLayer.Payload, newOvertimeShiftLayer.Period, newOvertimeShiftLayer.DefinitionSet);
				}
			}
		}
	}
}