using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IProjectionVersionPersister 
	{
		IEnumerable<ProjectionVersion> LockAndGetVersions(Guid personId, DateOnly from, DateOnly to);
	}

	public class ProjectionVersion
	{
		public DateOnly Date { get; set; }
		public int Version { get; set; }
	}
}