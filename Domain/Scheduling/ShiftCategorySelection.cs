using Teleopti.Ccc.Domain.Common.EntityBaseTypes;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class ShiftCategorySelection : NonversionedAggregateRootWithBusinessUnit, IShiftCategorySelection
	{
		public virtual string Model { get; set; }
	}
}