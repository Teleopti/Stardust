using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData.Core;


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
			var siteRepository = SiteRepository.DONT_USE_CTOR(currentUnitOfWork);
			if (_createSite)
			{
				if (Name == null)
					Name = RandomName.Make("site");
				Site = SiteFactory.CreateSimpleSite(Name);
				var businessUnit = BusinessUnitRepository.DONT_USE_CTOR(currentUnitOfWork, null, null).LoadAll().Single(b => b.Name == BusinessUnit);
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
			
			Site.ClearOpenHourCollection();

			if (OpenHours != null)
			{
				var siteOpenHourRepository = new SiteOpenHourRepository(currentUnitOfWork);
				foreach (var openHour in OpenHours)
				{
					var siteOpenHour = new SiteOpenHour()
					{
						WeekDay = openHour.Key,
						TimePeriod = openHour.Value
					};
					if (Site.AddOpenHour(siteOpenHour))
					{
						siteOpenHourRepository.Add(siteOpenHour);
					}
				}
			}
		}
	}
}