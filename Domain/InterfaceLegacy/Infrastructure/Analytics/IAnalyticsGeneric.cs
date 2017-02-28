using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics
{
	public interface IAnalyticsGeneric
	{
		int Id { get; set; }
		Guid Code { get; set; }
	}
}