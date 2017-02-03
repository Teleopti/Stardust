using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class InputInfo
	{
		public string SourceId { get; set; }
		public string PlatformTypeId { get; set; }
		public string UserCode { get; set; }
		public string StateCode { get; set; }
		public string StateDescription { get; set; }
		public DateTime? SnapshotId { get; set; }
		public int? SnapshotDataSourceId { get; set; }

		public Guid ParsedPlatformTypeId()
		{
			return PlatformTypeId != null ? Guid.Parse(PlatformTypeId) : Guid.Empty;
		}

	}
}