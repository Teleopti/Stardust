using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific
{
	public class ShiftCategoryPreference : BasePreference
	{
		public ShiftCategoryPreference()
		{
			ShiftCategory = TestData.ShiftCategory.Description.Name;
		}

		public string ShiftCategory { get; set; }

		protected override PreferenceRestriction ApplyRestriction(IUnitOfWork uow)
		{
			var rep = new ShiftCategoryRepository(uow);
			return new PreferenceRestriction
			{
				ShiftCategory = rep.LoadAll().FirstOrDefault(s => s.Description.Name == ShiftCategory)
			};
		}

		protected override DateTime ApplyDate(CultureInfo cultureInfo)
		{
			return DateTime.Parse(Date, SwedishCultureInfo);
		}
	}
}