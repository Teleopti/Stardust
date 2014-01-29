using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class MoveShiftLayerVertical : IMoveShiftLayerVertical
	{
		private readonly IMoveLayerVertical _moveUp = new MoveShiftLayerUp();
		private readonly IMoveLayerVertical _moveDown = new MoveShiftLayerDown();

		public IMoveLayerVertical MoveUp
		{
			get { return _moveUp; }
		}

		public IMoveLayerVertical MoveDown
		{
			get { return _moveDown; }
		}
	}
}