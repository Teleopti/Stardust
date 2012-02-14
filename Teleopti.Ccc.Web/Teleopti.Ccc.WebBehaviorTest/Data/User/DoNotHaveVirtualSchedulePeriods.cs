using System.Globalization;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public class DoNotHaveVirtualSchedulePeriods : IUserSetup
	{
		public void Apply(IPerson user, CultureInfo cultureInfo)
		{
			user.RemoveAllPersonPeriods();
			user.RemoveAllSchedulePeriods();
		}
	}
}