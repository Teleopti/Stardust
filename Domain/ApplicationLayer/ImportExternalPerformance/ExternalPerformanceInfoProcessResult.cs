using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance
{
	public class PerformanceInfoExtractionResult
	{
		public string RawLine { get; set; }
		public DateOnly DateFrom { get; set; }
		public string GameType { get; set; }
		public string GameName { get; set; }
		public int GameId { get; set; }
		public string AgentId { get; set; }
		public int GameNumberScore { get; set; }
		public Percent GamePercentScore { get; set; }
		public Guid PersonId { get; set; }
	}

	public class ExternalPerformanceInfoProcessResult
	{
		public ExternalPerformanceInfoProcessResult()
		{
			ValidRecords = new List<PerformanceInfoExtractionResult>();
			InvalidRecords = new List<string>();
			ExternalPerformances = new List<IExternalPerformance>();
		}
		public bool HasError { get; set; }
		public string ErrorMessages { get; set; }
			
		public int TotalRecordCount => InvalidRecords.Count + ValidRecords.Count;
		public IList<string> InvalidRecords { get; set; }

		public List<PerformanceInfoExtractionResult> ValidRecords { get; set; }
		public List<IExternalPerformance> ExternalPerformances { get; set; }
	}
}