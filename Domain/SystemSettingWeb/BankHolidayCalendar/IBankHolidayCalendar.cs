using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.SystemSettingWeb
{
	public interface IBankHolidayCalendar : IAggregateRoot, IBelongsToBusinessUnit,IDeleteTag
	{
		string Name { get; set; }
	}
}
