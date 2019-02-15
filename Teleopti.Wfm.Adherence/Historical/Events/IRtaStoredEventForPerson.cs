using System;

namespace Teleopti.Wfm.Adherence.Historical.Events
{
	public interface IRtaStoredEventForPerson
	{
		Guid PersonId { get; set; }
		DateOnly? BelongsToDate { get; set; }
	}
}