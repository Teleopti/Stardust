using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddPersonalActivityCommand : IErrorAttachedCommand
	{
		public Guid PersonId { get; set; }
		public DateOnly Date { get; set; }
		public Guid PersonalActivityId { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
		public IList<string> ErrorMessages { get; set; }
	}
}