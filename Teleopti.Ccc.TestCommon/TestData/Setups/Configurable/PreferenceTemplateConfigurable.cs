using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class PreferenceTemplateConfigurable : IUserDataSetup
	{
		public string Name { get; set; }
		public string ShiftCategory { get; set; }
		public DateTime? StartTimeMinimum { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork, IPerson user, CultureInfo cultureInfo)
		{
			var restriction = new PreferenceRestrictionTemplate();
			if (ShiftCategory != null)
			{
				var shiftCategoryRepository = new ShiftCategoryRepository(currentUnitOfWork);
				var shiftCategory = shiftCategoryRepository.LoadAll().Single(b => b.Description.Name == ShiftCategory);
				restriction.ShiftCategory = shiftCategory;
			}
			if (StartTimeMinimum != null)
			{
				restriction.StartTimeLimitation = new StartTimeLimitation(null, StartTimeMinimum.Value.TimeOfDay);
			}

			var extendedPreferenceTemplateRepository = new ExtendedPreferenceTemplateRepository(currentUnitOfWork);
			extendedPreferenceTemplateRepository.Add(new ExtendedPreferenceTemplate(user, restriction, Name, Color.Red));
		}
	}
}