using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class SwedishCulture : IUserCulture
	{
		public CultureInfo GetCulture()
		{
			return CultureInfoFactory.CreateSwedishCulture();
		}
	}
}