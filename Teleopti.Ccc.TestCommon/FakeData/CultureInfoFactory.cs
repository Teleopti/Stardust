using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public static class CultureInfoFactory
	{
		public static CultureInfo CreateSwedishCulture()
		{
			return new CultureInfo("sv-SE");
		}

		public static CultureInfo CreateFinnishCulture()
		{
			return new CultureInfo("fi-Fi");
		}

		public static CultureInfo CreateCatalanCulture()
		{
			return new CultureInfo("ca-ES");
		}

	}
}
