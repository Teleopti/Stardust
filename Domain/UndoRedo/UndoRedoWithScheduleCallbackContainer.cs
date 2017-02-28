using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.UndoRedo
{
	public class UndoRedoWithScheduleCallbackContainer : UndoRedoContainer
	{
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;

		public UndoRedoWithScheduleCallbackContainer(IScheduleDayChangeCallback scheduleDayChangeCallback)
		{
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
		}

		public override bool Undo()
		{
			using (UndoRedoState.Create(_scheduleDayChangeCallback))
			{
				return base.Undo();
			}
		}

		public override bool Redo()
		{
			using (UndoRedoState.Create(_scheduleDayChangeCallback))
			{
				return base.Redo();
			}
		}
	}
}