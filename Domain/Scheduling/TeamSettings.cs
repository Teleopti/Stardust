using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class TeamSettings
	{
		public TeamSameType TeamSameType { get; set; }
		public GroupPageType GroupPageType { get; set; }
	}
	    
	public enum TeamSameType
	{
		ShiftCategory,
		StartTime,
		EndTime
	}
}