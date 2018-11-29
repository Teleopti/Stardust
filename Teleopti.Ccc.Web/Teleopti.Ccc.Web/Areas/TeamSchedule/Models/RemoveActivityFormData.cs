using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class ShiftLayerDate
	{
		public Guid ShiftLayerId { get; set; }
		public DateOnly Date { get; set; }
        public bool IsOvertime { get; set; }
	}

	public class PersonActivityInfo
	{
		public Guid PersonId { get; set; }
		public IList<ShiftLayerDate> ShiftLayers { get; set; } 
	}

	public class RemoveActivityFormData
	{
		public IList<PersonActivityInfo> PersonActivities { get; set; } 		
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}
}