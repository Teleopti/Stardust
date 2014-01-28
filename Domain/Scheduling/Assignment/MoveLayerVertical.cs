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
				var layers = createNewCollectionOfLayers(personAssignment.MainActivities(), layer, positionMove);
				personAssignment.ClearMainActivities();
				foreach (var newMainLayer in layers)
				{
					personAssignment.AddActivity(newMainLayer.Payload, newMainLayer.Period);
				}
				return;
			}

			if (layer is IPersonalShiftLayer)
			{
				var layers = createNewCollectionOfLayers(personAssignment.PersonalActivities(), layer, positionMove);
				personAssignment.ClearPersonalActivities();
				foreach (var newPersonalLayer in layers)
				{
					personAssignment.AddPersonalActivity(newPersonalLayer.Payload, newPersonalLayer.Period);
				}
				return;
			}

			if (layer is IOvertimeShiftLayer)
			{
				var layers = createNewCollectionOfLayers(personAssignment.OvertimeActivities(), layer, positionMove);
				personAssignment.ClearOvertimeActivities();
				foreach (IOvertimeShiftLayer newOvertimeShiftLayer in layers)
				{
					personAssignment.AddOvertimeActivity(newOvertimeShiftLayer.Payload, newOvertimeShiftLayer.Period,
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


	public class MoveLayerUp : IMoveLayerVertical2
	{

		public void Move(IList<IShiftLayer> layers, IShiftLayer layer)
		{
			var t = layer.GetType();
			var currentIndex = layers.IndexOf(layer);
			var toBeReplaced = layers.Take(currentIndex - 1).Last(l => l.GetType() == t);

			var indexToInsert = layers.IndexOf(toBeReplaced);

			layers.Remove(layer);
			layers.Insert(indexToInsert, layer);
		}
	}

	public class MoveLayerDown : IMoveLayerVertical2
	{
		public void Move(IList<IShiftLayer> layers, IShiftLayer layer)
		{
			var t = layer.GetType();
			var currentIndex = layers.IndexOf(layer);
			var toBeReplaced = layers.Skip(currentIndex + 1).First(l => l.GetType() == t);

			layers.Remove(toBeReplaced);
			layers.Insert(currentIndex, toBeReplaced);
		}
	}
}