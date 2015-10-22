using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddSeatPlanCommand : ITrackableCommand
	{
		public IList<Guid> Teams { get; set; }
		public IList<Guid> Locations { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
		public List<Guid> SeatIds { get; set; }
		public List<Guid> PersonIds { get; set; }
		
	}
}