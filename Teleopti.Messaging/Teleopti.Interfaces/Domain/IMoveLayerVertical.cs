using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	
	public interface IMoveLayerVertical
	{
		void Move(IList<IShiftLayer> layers, IShiftLayer layer);
	}

	public interface IMoveShiftLayerVertical
	{
		IMoveLayerVertical MoveUp { get; }
		IMoveLayerVertical MoveDown { get; }
	}
}