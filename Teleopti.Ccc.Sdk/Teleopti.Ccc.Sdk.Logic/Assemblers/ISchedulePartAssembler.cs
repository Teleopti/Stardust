using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
	public interface ISchedulePartAssembler : IAssembler<IScheduleDay,SchedulePartDto>
	{
		string SpecialProjection { get; set; }
		ICccTimeZoneInfo TimeZone { get; set; }
	}
}