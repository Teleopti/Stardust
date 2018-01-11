using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;

namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule
{
	public class FakeCommandHandler :
		IHandleCommand<MoveShiftLayerCommand>,
		IHandleCommand<BackoutScheduleChangeCommand>,
		IHandleCommand<ChangeShiftCategoryCommand>,
		IHandleCommand<FixNotOverwriteLayerCommand>,
		IHandleCommand<EditScheduleNoteCommand>,
		IHandleCommand<MoveShiftCommand>,
		IHandleCommand<AddDayOffCommand>
	{
		private int calledCount;
		private IList<ITrackableCommand> commands = new List<ITrackableCommand>();
	
		public int CalledCount
		{
			get { return calledCount; }
		}

		public IList<ITrackableCommand> CalledCommands
		{
			get { return commands; }
		}
		public void ResetCalledCount()
		{
			calledCount = 0;
		}

		public void Handle(MoveShiftLayerCommand command)
		{
			calledCount++;
			commands.Add(command);
		}

		public void Handle(BackoutScheduleChangeCommand command)
		{
			calledCount++;
			commands.Add(command);
		}

		public void Handle(ChangeShiftCategoryCommand command)
		{
			calledCount++;
			commands.Add(command);
		}
		

		public void Handle(FixNotOverwriteLayerCommand command)
		{
			calledCount++;
			commands.Add(command);
		}

		public void Handle(EditScheduleNoteCommand command)
		{
			calledCount++;
			commands.Add(command);
		}

		public void Handle(MoveShiftCommand command)
		{
			calledCount++;
			commands.Add(command);
		}

		public void Handle(AddDayOffCommand command)
		{
			calledCount++;
			commands.Add(command);
		}
	}
}
