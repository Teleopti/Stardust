using System;
using System.Collections.Generic;
using System.Linq;
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
		IHandleCommand<RemoveDayOffCommand>,
		IHandleCommand<RemoveShiftCommand>,
		IHandleCommand<ChangeActivityTypeCommand>
	{
		private int calledCount;
		private IList<Object> commands = new List<Object>();

		public int CalledCount => calledCount;
		public bool HasError { get; set; }

		public IList<Object> CalledCommands => commands;
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
		public void Handle(RemoveShiftCommand command)
		{
			handle(command);
		}
		public T GetSingleCommand<T>()
		{
			return (T)commands.Single();
		}

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

		public void Handle(ChangeActivityTypeCommand command)
		{
			calledCount++;
			commands.Add(command);
		}
	}
}
