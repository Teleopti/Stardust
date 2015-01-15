using System;

namespace Teleopti.Interfaces.Infrastructure.Analytics
{
	public interface IAnalyticsGeneric
	{
		int Id { get; set; }
		Guid Code { get; set; }
	}
}