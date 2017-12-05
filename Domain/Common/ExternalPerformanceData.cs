using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public interface IExternalPerformanceData : IAggregateRoot
	{
	}

	public class ExternalPerformanceData : SimpleAggregateRoot, IExternalPerformanceData
	{
		public Guid ExternalPerformance { get; set; }
		public DateOnly DateFrom { get; set; }
		public Guid Person { get; set; }
		public string OriginalPersonId { get; set; }
		public int Score { get; set; }
	}
}
