using System.Collections.Generic;
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
				var oldLayers = new List<IMainShiftLayer>(personAssignment.MainShiftActivityLayers);
				var indexOfMainShiftLayer = oldLayers.IndexOf(msLayer);
				oldLayers.Remove(msLayer);
				oldLayers.Insert(indexOfMainShiftLayer-1, msLayer);
				personAssignment.SetMainShiftLayers(oldLayers, personAssignment.ShiftCategory);
			}

			//no need to cast later when agentday is done
			var overLayer = layer as IOvertimeShiftActivityLayer;
			if (overLayer != null)
			{
				var shift = (IOvertimeShift) overLayer.Parent;
				shift.LayerCollection.MoveUpLayer(layer);
			}
		}

		public void MoveDown(IPersonAssignment personAssignment, ILayer<IActivity> layer)
		{
			//no need to cast later when agentday is done
			var msLayer = layer as IMainShiftLayer;
			if (msLayer != null)
			{
				var oldLayers = new List<IMainShiftLayer>(personAssignment.MainShiftActivityLayers);
				var indexOfMainShiftLayer = oldLayers.IndexOf(msLayer);
				oldLayers.Remove(msLayer);
				oldLayers.Insert(indexOfMainShiftLayer + 1, msLayer);
				personAssignment.SetMainShiftLayers(oldLayers, personAssignment.ShiftCategory);
			}

			//no need to cast later when agentday is done
			var overLayer = layer as IOvertimeShiftActivityLayer;
			if (overLayer != null)
			{
				var shift = (IOvertimeShift)overLayer.Parent;
				shift.LayerCollection.MoveDownLayer(layer);
			}
		}
	}
}