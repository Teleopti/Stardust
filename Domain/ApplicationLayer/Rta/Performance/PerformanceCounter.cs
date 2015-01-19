using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Performance
{
	public interface IPerformanceCounter
	{
		bool IsEnabled { get; }
		int Limit { get; set; }
		Guid BusinessUnitId { get; set; }
		string DataSource { get; set; }
		void Count();
	}
}