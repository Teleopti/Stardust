using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class MoveShiftLayerDown : IMoveLayerVertical
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