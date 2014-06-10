using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class MoveActivityCommand
	{
		public Guid AgentId { get; set; }
		public DateOnly Date { get; set; } //todo: change to datetime
		public Guid ActivityId { get; set; }
		public TimeSpan NewStartTime { get; set; }
		public DateTime OldStartTime { get; set; } //todo: change to timespan (?)
		public TimeSpan OldProjectionLayerLength { get; set; }
	}
}