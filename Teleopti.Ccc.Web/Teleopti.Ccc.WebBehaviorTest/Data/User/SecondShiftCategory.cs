using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class SecondShiftCategory : BaseShiftCategory
	{
		public SecondShiftCategory()
		{
			ShiftCategory = new ShiftCategory(CategoryName());
		}
		
		protected override string CategoryName()
		{
			return "2nd";
		}
	}
}
