using System;

namespace Teleopti.Interfaces.Domain
{
	public class VersionAndId
	{
		public VersionAndId(Guid id, int version)
		{
			Id = id;
			Version = version;
		}
		public int Version { get; private set; }
		public Guid Id { get; private set; }
	}
}