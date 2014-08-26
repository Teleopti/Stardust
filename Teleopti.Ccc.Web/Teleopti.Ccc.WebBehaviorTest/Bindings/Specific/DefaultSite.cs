using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Default;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Specific
{
	public static class DefaultSite
	{
		private class TheSite : SiteConfigurable
		{
			public TheSite()
			{
				BusinessUnit = DefaultBusinessUnit.BusinessUnitFromFakeState.Name;
				Name = "The site";
			}
		}

		public static ISite Get()
		{
			if (!DataMaker.Data().HasSetup<TheSite>())
				DataMaker.Data().Apply(new TheSite());
			return DataMaker.Data().UserData<TheSite>().Site;
		}
	}
}