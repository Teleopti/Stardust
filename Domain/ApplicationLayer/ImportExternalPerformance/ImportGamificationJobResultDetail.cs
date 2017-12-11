using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance
{
	public class ImportGamificationJobResultDetail
	{
		public Guid Id { get; set; }
		public string Name { get; set; }

		public string Owner { get; set; }

		public DateTime CreateDateTime { get; set; }

		public string Status { get; set; }

		public string Category { get; set; }
	}
}
