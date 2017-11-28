using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance
{
	public class ImportExternalPerformanceInfo
	{
		public DateTime DateFrom { get; set; }
		public string AgentId { get; set; }
		public string GameName { get; set; }
		public string GameId { get; set; }
		public int GameType { get; set; }
		public int GameScore { get; set; }
	}
}
