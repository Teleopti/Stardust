using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public interface IExternalPerformanceData 
	{
		Guid ExternalPerformance { get; set; }
		DateOnly DateFrom { get; set; }
		Guid Person { get; set; }
		string OriginalPersonId { get; set; }
		int Score { get; set; }
	}

	public class ExternalPerformanceData : IEquatable<ExternalPerformanceData>, IExternalPerformanceData
	{
		public Guid ExternalPerformance { get; set; }
		public DateOnly DateFrom { get; set; }
		public Guid Person { get; set; }
		public string OriginalPersonId { get; set; }
		public int Score { get; set; }
		public bool Equals(ExternalPerformanceData other)
		{
			if (other == null) return false;
			return ExternalPerformance == other.ExternalPerformance && DateFrom == other.DateFrom &&
				   Person == other.Person && OriginalPersonId == other.OriginalPersonId &&
				   Score == other.Score;
	}
}
}
