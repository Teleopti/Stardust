using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class ChangeActivityTypeCommand : IErrorAttachedCommand, IScheduleCommand
	{
		public IPerson Person { get; set; }

		public DateOnly Date { get; set; }

		public EditingLayer Layer { get; set; }
		public IList<string> ErrorMessages { get; set; }
	}

	public class EditingLayer
	{
		public IActivity Activity { get; set; }
		public ShiftLayer ShiftLayer { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }

		public bool IsNew { get; set; }
	}



}