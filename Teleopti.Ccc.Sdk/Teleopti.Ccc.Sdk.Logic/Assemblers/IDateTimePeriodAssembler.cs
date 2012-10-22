using System;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
	public interface IDateTimePeriodAssembler : IAssembler<DateTimePeriod,DateTimePeriodDto>
	{
		TimeZoneInfo TimeZone { get; set; }
	}
}