using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class PersonTeamChangedEvent : IEvent, ITimestamped
	{
		public DateTime Timestamp { get; set; }
		public Guid PersonId { get; set; }
		public Guid? CurrentBusinessUnitId { get; set; }
		public Guid? CurrentSiteId { get; set; }
		public string CurrentSiteName { get; set; }
		public Guid? CurrentTeamId { get; set; }
		public string CurrentTeamName { get; set; }

		public IEnumerable<ExternalLogon> ExternalLogons { get; set; }
	
	}

	public class PersonEmploymentNumberChangedEvent : IEvent
	{
		public Guid PersonId { get; set; }
		public string EmploymentNumber { get; set; }
	}

	public class PersonNameChangedEvent : IEvent
	{
		public Guid PersonId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
	}
}