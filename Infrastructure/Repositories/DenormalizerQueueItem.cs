using System;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class DenormalizerQueueItem
	{
		public Guid Id { get; set; }
		public string Message { get; set; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
		public string Type { get; set; }
	}
}