using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	//throw if layer not found?
	public class MoveLayerVertical : IMoveLayerVertical
	{
		public void MoveUp(IPersonAssignment personAssignment, ILayer<IActivity> layer)
		{
			moveLayer(personAssignment, layer, -1);
		}

		public void MoveDown(IPersonAssignment personAssignment, ILayer<IActivity> layer)
		{
			moveLayer(personAssignment, layer, 1);
		}

		private static void moveLayer(IPersonAssignment personAssignment, ILayer<IActivity> layer, int positionMove)
		{
			if (layer is IMainShiftLayer)
			{
				var layers = createNewCollectionOfLayers(personAssignment.MainLayers(), layer, positionMove);
				personAssignment.ClearMainLayers();
				foreach (var newMainLayer in layers)
				{
					personAssignment.AddActivity(newMainLayer.Payload, newMainLayer.Period);
				}
				return;
			}

			if (layer is IPersonalShiftLayer)
			{
				var layers = createNewCollectionOfLayers(personAssignment.PersonalLayers(), layer, positionMove);
				personAssignment.ClearPersonalLayers();
				foreach (var newPersonalLayer in layers)
				{
					personAssignment.AddPersonalLayer(newPersonalLayer.Payload, newPersonalLayer.Period);
				}
				return;
			}

			if (layer is IOvertimeShiftLayer)
			{
				var layers = createNewCollectionOfLayers(personAssignment.OvertimeLayers(), layer, positionMove);
				personAssignment.ClearOvertimeLayers();
				foreach (IOvertimeShiftLayer newOvertimeShiftLayer in layers)
				{
					personAssignment.AddOvertimeLayer(newOvertimeShiftLayer.Payload, newOvertimeShiftLayer.Period,
					                                  newOvertimeShiftLayer.DefinitionSet);
				}
			}
		}

		private static IEnumerable<ILayer<IActivity>> createNewCollectionOfLayers(IEnumerable<ILayer<IActivity>> oldCollection,
		                                                                  ILayer<IActivity> layerToMove,
																																			int positionMove)
		{
			var oldLayers = oldCollection.ToList();
			var index = oldLayers.IndexOf(layerToMove);
			oldLayers.RemoveAt(index);
			oldLayers.Insert(index + positionMove, layerToMove);
			return oldLayers;
		}
	}
}