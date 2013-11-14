using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddActivityCommand
	{
		public Guid ScenarioId { get; set; }
		public Guid PersonId { get; set; }
		public DateOnly Date { get; set; }
		public Guid ActivityId { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}
}