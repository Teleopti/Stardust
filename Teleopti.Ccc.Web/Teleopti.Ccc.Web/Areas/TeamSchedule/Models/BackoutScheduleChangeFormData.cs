using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class BackoutScheduleChangeFormData
	{
		public PersonDate[] PersonDates;			
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}
}