using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsBridgeTimeZoneRepository
	{
		IList<AnalyticsBridgeTimeZone> GetBridges(int timeZoneId);
		void Save(List<AnalyticsBridgeTimeZone> toBeAdded);
	}

	public class AnalyticsBridgeTimeZone
	{
		public int DateId { get; set; }
		public int IntervalId { get; set; }
		public int TimeZoneId { get; set; }
		public int LocalDateId { get; set; }
		public int LocalIntervalId { get; set; }
	}
}