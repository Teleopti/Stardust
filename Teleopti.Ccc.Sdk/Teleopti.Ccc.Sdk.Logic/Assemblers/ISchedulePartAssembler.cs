using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
	public interface ISchedulePartAssembler : IAssembler<IScheduleDay,SchedulePartDto>
	{
		string SpecialProjection { get; set; }
		TimeZoneInfo TimeZone { get; set; }
	}
}