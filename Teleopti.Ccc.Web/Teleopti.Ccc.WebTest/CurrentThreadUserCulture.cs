using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest
{
	public class CurrentThreadUserCulture : IUserCulture
	{
		public CultureInfo GetCulture()
		{
			return CultureInfo.CurrentCulture;
		}
	}
}