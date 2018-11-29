using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling
{
	public interface IGroupPagePerDate
	{
		IGroupPage GetGroupPageByDate(DateOnly dateOnly);
	}
}