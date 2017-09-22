using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.DoNotUse
{
	public static class DefaultSite
	{
		private class TheSite : SiteConfigurable
		{
			public TheSite()
			{
				BusinessUnit = DefaultBusinessUnit.BusinessUnit.Name;
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