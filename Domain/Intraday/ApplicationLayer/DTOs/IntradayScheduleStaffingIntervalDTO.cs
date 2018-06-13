using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teleopti.Ccc.Domain.Intraday.ApplicationLayer.DTOs
{
	public class IntradayScheduleStaffingIntervalDTO
	{
		public Guid SkillId { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public double StaffingLevel { get; set; }
	}
}
