using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IMoveLayerVertical
	{
		void MoveUp(IPersonAssignment personAssignment, ILayer<IActivity> layer);
		void MoveDown(IPersonAssignment personAssignment, ILayer<IActivity> layer);
	}

	public interface IMoveLayerVertical2
	{
		void Move(IList<IShiftLayer> layers, IShiftLayer layer);
	}
}