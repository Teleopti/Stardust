using TechTalk.SpecFlow;
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
	}
}