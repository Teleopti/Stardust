using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance
{
	public class PerformanceInfoExtractionResult
	{
		public string RawLine { get; set; }
		public string Error { get; set; }
		public DateOnly DateFrom { get; set; }
		public ExternalPerformanceDataType MeasureType { get; set; }
		public string MeasureName { get; set; }
		public int MeasureId { get; set; }
		public string AgentId { get; set; }
		public double MeasureNumberScore { get; set; }
		public Percent MeasurePercentScore { get; set; }
		public Guid PersonId { get; set; }
		public bool HasError()
		{
			return !string.IsNullOrEmpty(Error);
		}
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