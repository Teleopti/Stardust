using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse
{
	public class ShiftCategoryPreference : BasePreference
	{
		public string ShiftCategory { get; set; }

		protected override PreferenceRestriction ApplyRestriction(ICurrentUnitOfWork currentUnitOfWork)
		{
			IShiftCategory shiftCategory;
			if (ShiftCategory != null)
			{
				shiftCategory = new ShiftCategoryRepository(currentUnitOfWork).LoadAll().Single(sCat => sCat.Description.Name.Equals(ShiftCategory));
			}
			else
			{
				shiftCategory = ShiftCategoryFactory.CreateShiftCategory(RandomName.Make(), "Purple");
				new ShiftCategoryRepository(currentUnitOfWork).Add(shiftCategory);
			}

			return new PreferenceRestriction
			{
				ShiftCategory = shiftCategory
			};
		}

		protected override DateTime ApplyDate(CultureInfo cultureInfo)
		{
			return DateTime.Parse(Date, SwedishCultureInfo);
		}
	}
}