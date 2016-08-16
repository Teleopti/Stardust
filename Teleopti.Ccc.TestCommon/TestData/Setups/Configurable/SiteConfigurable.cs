using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Configurable
{
	public class SiteConfigurable : IDataSetup
	{
		private bool _createSite = true;

		public string Name { get; set; }
		public string BusinessUnit { get; set; }

		public ISite Site { get; private set; }

		public bool CreateSite
		{
			get { return _createSite; }
			set { _createSite = value; }
		}

		public IDictionary<DayOfWeek, TimePeriod> OpenHours { get; set; }

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var siteRepository = new SiteRepository(currentUnitOfWork);
			if (_createSite)
			{
				if (Name == null)
					Name = RandomName.Make("site");
				Site = SiteFactory.CreateSimpleSite(Name);
				var businessUnit = new BusinessUnitRepository(currentUnitOfWork).LoadAll().Single(b => b.Name == BusinessUnit);
				if (!string.IsNullOrEmpty(BusinessUnit))
					Site.SetBusinessUnit(businessUnit);
				siteRepository.Add(Site);
			}
			else
			{
				Site = siteRepository.LoadAll().FirstOrDefault(site => site.Description.Name.Equals(Name));
			}

			if (Site == null)
			{
				return;
			}

			Site.OpenHours.Clear();
			if (OpenHours != null)
			{
				foreach (var openHour in OpenHours)
				{
					Site.OpenHours.Add(openHour);
				}
			}
		}
	}
}