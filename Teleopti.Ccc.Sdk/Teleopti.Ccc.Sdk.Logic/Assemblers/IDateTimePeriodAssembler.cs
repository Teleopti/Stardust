using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;


namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
	public interface IDateTimePeriodAssembler : IAssembler<DateTimePeriod,DateTimePeriodDto>
	{
		TimeZoneInfo TimeZone { get; set; }
	}
}