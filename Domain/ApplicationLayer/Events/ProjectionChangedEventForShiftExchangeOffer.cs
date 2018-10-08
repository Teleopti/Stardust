using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	public class ProjectionChangedEventForShiftExchangeOffer : EventWithLogOnContext
	{
		public Guid PersonId { get; set; }
		public ICollection<ProjectionChangedEventForShiftExchangeOfferDateAndChecksums> Days { get; set; }
	}

	public class ProjectionChangedEventForShiftExchangeOfferDateAndChecksums
	{
		public DateTime Date { get; set; }
		public long Checksum { get; set; }
	} 
}