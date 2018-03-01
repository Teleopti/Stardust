using System;

namespace Teleopti.Ccc.Domain.Rta.Service
{
	public class CheckForActivityChangeInputModel
	{
		public Guid PersonId { get; set; }
		public Guid BusinessUnitId { get; set; }
		public string Tenant { get; set; }
	}
}