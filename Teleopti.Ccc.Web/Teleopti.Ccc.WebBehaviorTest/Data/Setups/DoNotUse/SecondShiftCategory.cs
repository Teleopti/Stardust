using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class SecondShiftCategory : BaseShiftCategory
	{
		public SecondShiftCategory()
		{
			ShiftCategory = new ShiftCategory(CategoryName());
		}
		
		protected override sealed string CategoryName()
		{
			return "2nd";
		}
	}
}
