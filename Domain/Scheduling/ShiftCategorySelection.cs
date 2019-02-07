using Teleopti.Ccc.Domain.Common.EntityBaseTypes;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class ShiftCategorySelection : AggregateRoot_Events_ChangeInfo_BusinessUnit, IShiftCategorySelection
	{
		public virtual string Model { get; set; }
	}
}