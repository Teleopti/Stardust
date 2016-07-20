using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Core.Data
{
	public class ShiftCategoryViewModel
	{
		public Guid? Id { get; set; }
		public string ShortName { get; set; }
		public string Name { get; set; }
		public string DisplayColor { get; set; }
	}

	public interface IShiftCategoryProvider
	{
		IList<ShiftCategoryViewModel> GetAll();
	}
}