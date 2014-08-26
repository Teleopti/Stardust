using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.DoNotUse
{
	public static class DefaultTeam
	{
		private class TheTeam : GeneratedTeam
		{
			public TheTeam()
				: base(DefaultSite.Get())
			{
			}
		}

		public static ITeam Get()
		{
			if (!DataMaker.Data().HasSetup<TheTeam>())
				DataMaker.Data().Apply(new TheTeam());
			return DataMaker.Data().UserData<TheTeam>().TheTeam;
		}

	}
}