using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class PreferenceTemplateConfigurable : IUserDataSetup
	{
		public string Name { get; set; }
		public string ShiftCategory { get; set; }
		public DateTime? StartTimeMinimum { get; set; }

		public void Apply(IUnitOfWork uow, IPerson user, CultureInfo cultureInfo)
		{
			var restriction = new PreferenceRestrictionTemplate();
			if (ShiftCategory != null)
			{
				var shiftCategoryRepository = new ShiftCategoryRepository(uow);
				var shiftCategory = shiftCategoryRepository.LoadAll().Single(b => b.Description.Name == ShiftCategory);
				restriction.ShiftCategory = shiftCategory;
			}
			if (StartTimeMinimum != null)
			{
				restriction.StartTimeLimitation = new StartTimeLimitation(null, StartTimeMinimum.Value.TimeOfDay);
			}

			var extendedPreferenceTemplateRepository = new ExtendedPreferenceTemplateRepository(uow);
			extendedPreferenceTemplateRepository.Add(new ExtendedPreferenceTemplate(user, restriction, Name, Color.Red));
		}
	}
}