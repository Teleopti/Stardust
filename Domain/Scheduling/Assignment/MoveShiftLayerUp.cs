using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class MoveShiftLayerUp : IMoveLayerVertical
	{

		public void Move(IList<IShiftLayer> layers, IShiftLayer layer)
		{
			var t = layer.GetType();
			var currentIndex = layers.IndexOf(layer);
			var toBeReplaced = layers.Take(currentIndex).Last(l => l.GetType() == t);

			var indexToInsert = layers.IndexOf(toBeReplaced);

			layers.Remove(layer);
			layers.Insert(indexToInsert, layer);
		}
	}
}