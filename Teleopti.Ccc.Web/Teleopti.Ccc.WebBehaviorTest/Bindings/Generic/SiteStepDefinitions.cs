using System;
using System.Collections.Generic;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;


namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class SiteStepDefinitions
	{
		[Given(@"there is a site named '(.*)'")]
		public void GivenThereIsASiteWith(string name)
		{
			DataMaker.Data().Apply(new SiteConfigurable {Name = name});
		}

		[Given(@"there is a site '(.*)' on business unit '(.*)'")]
		public void GivenThereIsASiteOnBusinessUnit(string site, string bu)
		{
			DataMaker.Data().Apply(new SiteConfigurable { Name = site, BusinessUnit = bu});
		}

		[Given(@"There are open hours '(.*)' for '(.*)', in site '(.*)'")]
		public void GivenThereAreOpenHoursForInSite(string period, string weedDayNames, string site)
		{
			var openHours = new Dictionary<DayOfWeek, TimePeriod>();
			foreach (var weedDayName in weedDayNames.Split(','))
			{
				DayOfWeek dayOfWeek;
				if (Enum.TryParse(weedDayName, out dayOfWeek))
				{
					openHours.Add(dayOfWeek, new TimePeriod(period));
				}
			}
			DataMaker.Data().Apply(new SiteConfigurable {Name = site, CreateSite = false, OpenHours = openHours});
		}

	}
}