﻿using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific
{
	public class FirstShiftCategory : BaseShiftCategory
	{
		public FirstShiftCategory()
		{
			ShiftCategory = new ShiftCategory(CategoryName());
		}
		
		protected override sealed string CategoryName()
		{
			return "1st";
		}
	}
}
