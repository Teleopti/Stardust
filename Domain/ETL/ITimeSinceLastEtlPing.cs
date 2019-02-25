using System;

namespace Teleopti.Ccc.Domain.ETL
{
	public interface ITimeSinceLastEtlPing
	{
		TimeSpan Fetch();
	}
}