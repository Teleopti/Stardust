using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific
{
	public class ShiftCategoryPreference : BasePreference
	{
		public string ShiftCategory { get; set; }

		protected override PreferenceRestriction ApplyRestriction(IUnitOfWork uow)
		{
			IShiftCategory shiftCategory;
			if (ShiftCategory != null)
			{
				shiftCategory = new ShiftCategoryRepository(uow).LoadAll().Single(sCat => sCat.Description.Name.Equals(ShiftCategory));
			}
			else
			{
				shiftCategory = ShiftCategoryFactory.CreateShiftCategory(RandomName.Make(), "Purple");
				new ShiftCategoryRepository(uow).Add(shiftCategory);
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