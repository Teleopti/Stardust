using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ISeatMapLocation : IAggregateRoot, IPublishEvents
	{

		int SeatCount { get; }
		bool IncludeInSeatPlan { get; set; }
		string SeatMapJsonData { get; set; }
		string Name { get; set; }
		
		IList<ISeat> Seats { get; }
		string LocationPrefix { get; set; }
		string LocationSuffix { get; set; }

		void ClearBookingInformation();

	}
}