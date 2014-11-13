using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class CheckForActivityChangeWebModel
	{
		public CheckForActivityChangeWebModel()
		{
			Timestamp = DateTime.Now.ToString();
		}

		public string Timestamp { get; set; }
		public string PersonId { get; set; }
		public string BusinessUnitId { get; set; }
	}
}