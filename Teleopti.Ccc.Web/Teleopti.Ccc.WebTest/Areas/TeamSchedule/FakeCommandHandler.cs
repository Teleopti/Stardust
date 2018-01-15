using System;
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
		IHandleCommand<AddDayOffCommand>,
		IHandleCommand<AddActivityCommand>,
		IHandleCommand<RemoveActivityCommand>,
		IHandleCommand<MoveActivityCommand>,
		IHandleCommand<AddOvertimeActivityCommand>,
		IHandleCommand<AddPersonalActivityCommand>,
		IHandleCommand<RemoveDayOffCommand>
	{
		private int calledCount;
		private IList<ITrackableCommand> commands = new List<ITrackableCommand>();

		public int CalledCount => calledCount;
		public bool HasError { get; set; }

		public IList<ITrackableCommand> CalledCommands => commands;
		public void ResetCalledCount() => calledCount = 0;
		public void Handle(MoveShiftLayerCommand command) { handle(command); }
		public void Handle(BackoutScheduleChangeCommand command) { handle(command); }
		public void Handle(ChangeShiftCategoryCommand command) { handle(command); }
		public void Handle(FixNotOverwriteLayerCommand command) { handle(command); }
		public void Handle(EditScheduleNoteCommand command) { handle(command); }
		public void Handle(MoveShiftCommand command) { handle(command); }
		public void Handle(AddDayOffCommand command) { handle(command); }
		public void Handle(AddActivityCommand command) { handle(command); }
		public void Handle(RemoveActivityCommand command) { handle(command); }
		public void Handle(MoveActivityCommand command) { handle(command); }
		public void Handle(AddOvertimeActivityCommand command) { handle(command); }
		public void Handle(AddPersonalActivityCommand command) { handle(command); }
		public void Handle(RemoveDayOffCommand command) { handle(command); }



		private void handle(ITrackableCommand command)
		{
			calledCount++;
			if (command is IErrorAttachedCommand && HasError)
			{
				var msgs = (command as IErrorAttachedCommand).ErrorMessages ?? new List<string>();
				msgs.Add("Execute command failed.");
				typeof(IErrorAttachedCommand).GetProperty(nameof(IErrorAttachedCommand.ErrorMessages)).SetValue(command, msgs);
			}
			commands.Add(command);
		}
	}
}
