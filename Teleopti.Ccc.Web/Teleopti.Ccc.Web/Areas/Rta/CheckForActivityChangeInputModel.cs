using System;

namespace Teleopti.Ccc.Web.Areas.Rta
{
	public class CheckForActivityChangeInputModel
	{
		public Guid PersonId { get; set; }
		public Guid BusinessUnitId { get; set; }
		public DateTime Timestamp { get; set; }
	}
}