using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.Sdk.Logic.QueryHandler
{
	public static class DateOnlyDtoExtensions
	{
		public static DateOnly ToDateOnly(this DateOnlyDto dateOnlyDto)
		{
			return new DateOnly(dateOnlyDto.DateTime);
		}

		public static DateOnly? ToNullableDateOnly(this DateOnlyDto dateOnlyDto)
		{
			return dateOnlyDto != null ? (DateOnly?)dateOnlyDto.ToDateOnly() : null;
		}

		public static DateOnlyPeriod ToDateOnlyPeriod(this DateOnlyPeriodDto dateOnlyPeriodDto)
		{
			return new DateOnlyPeriod(dateOnlyPeriodDto.StartDate.ToDateOnly(), dateOnlyPeriodDto.EndDate.ToDateOnly());
		}
	}
}