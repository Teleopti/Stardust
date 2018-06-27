using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class MultipleChangeScheduleCommand : IErrorAttachedCommand
	{
		public MultipleChangeScheduleCommand()
		{
			Commands = new List<IScheduleCommand>();
		}
		public IList<IScheduleCommand> Commands { get; set; }
		public IPerson Person { get; set; }
		public DateOnly Date { get; set; }
		public IScheduleDictionary ScheduleDictionary { get; set; }
		public IList<string> ErrorMessages { get; set; }
	}

	public class ChangeActivityTypeCommand : IScheduleCommand
	{
		public IActivity Activity { get; set; }
		public ShiftLayer ShiftLayer { get; set; }
	}


}