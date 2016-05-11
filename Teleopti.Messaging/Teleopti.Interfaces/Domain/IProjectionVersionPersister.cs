using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IProjectionVersionPersister 
	{
		IEnumerable<ProjectionVersion> Upsert(Guid personId, IEnumerable<DateOnly> dates);
	}

	public class ProjectionVersion
	{
		public DateOnly Date { get; set; }
		public int Version { get; set; }
	}
}