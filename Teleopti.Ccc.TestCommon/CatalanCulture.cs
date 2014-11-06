using System.Globalization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class CatalanCulture : IUserCulture
	{
		public CultureInfo GetCulture()
		{
			return CultureInfoFactory.CreateCatalanCulture();
		}
	}
}