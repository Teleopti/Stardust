using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IStandardDeviationData
	{
		IDictionary<DateOnly, double?> Data { get; }
		void Add(DateOnly day, double? d);
	}

	public class StandardDeviationData : IStandardDeviationData
	{
		public IDictionary<DateOnly, double?> Data { get; private set; }

		public StandardDeviationData()
		{
			Data = new Dictionary<DateOnly, double?>();
		}

		public void Add(DateOnly day, double? d)
		{
			Data.Add(day, d);
		}
	}
}