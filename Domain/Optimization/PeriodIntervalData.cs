using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IPeriodIntervalData
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		IDictionary<DateOnly, double?> Data { get; }
		void Add(DateOnly day, double? standardDeviation);
	}

	public class PeriodIntervalData : IPeriodIntervalData
	{
		public IDictionary<DateOnly, double?> Data { get; private set; }

		public PeriodIntervalData()
		{
			Data = new Dictionary<DateOnly, double?>();
		}

		public void Add(DateOnly day, double? standardDeviation)
		{
			Data.Add(day, standardDeviation);
		}
	}
}